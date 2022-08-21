using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Metadata.Ecma335;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;
using Serilog;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;

namespace DotScrapper
{
    public class ScrapperDownloader
    {
        private readonly HttpClient _httpClient;
        private readonly ILogger _logger;
        private readonly ImageFormatManager _formatManager;

        public IList<IScrapper> Scrappers { get; init; }
        public ChromiumDriver Driver { get; set; }
        public IList<IPostScrapAction?> PostScrapsActions { get; set; } 
            = new List<IPostScrapAction?>();

        public ScrapperDownloader(ChromiumDriver driver, HttpClient? client = null)
        {
            _logger = Log.ForContext<ScrapperDownloader>();

            Scrappers = new List<IScrapper>();
            Driver = driver;
            _httpClient = client ?? new HttpClient(new SocketsHttpHandler()
            {
                AllowAutoRedirect = true
            });

            _formatManager = new ImageFormatManager();
            _formatManager.AddImageFormatDetector(new PngImageFormatDetector());
            _formatManager.AddImageFormatDetector(new BmpImageFormatDetector());
            _formatManager.AddImageFormatDetector(new JpegImageFormatDetector());
            _formatManager.AddImageFormatDetector(new GifImageFormatDetector());
            _formatManager.AddImageFormatDetector(new WebpImageFormatDetector());
            _formatManager.AddImageFormatDetector(new TgaImageFormatDetector());

            _formatManager.AddImageFormat(PngFormat.Instance);
            _formatManager.AddImageFormat(BmpFormat.Instance);
            _formatManager.AddImageFormat(JpegFormat.Instance);
            _formatManager.AddImageFormat(GifFormat.Instance);
            _formatManager.AddImageFormat(WebpFormat.Instance);
            _formatManager.AddImageFormat(TgaFormat.Instance);
        }

        public ScrapperDownloader(ChromiumDriver driver, IEnumerable<IScrapper> scrappers, HttpClient? client, ImageFormatManager formatManager)
        {
            _logger = Log.ForContext<ScrapperDownloader>();

            Scrappers = new List<IScrapper>(scrappers);
            Driver = driver;
            _formatManager = formatManager;
            _httpClient = client ?? new HttpClient(new SocketsHttpHandler()
            {
                AllowAutoRedirect = true
            });
        }

        public async Task<uint> DownloadAsync(ScrapperQuery query, string outputDirectory, bool validateContent)
        {
            Queue<ScrapSource> sources = new Queue<ScrapSource>(Scrappers.SelectMany(x=>x.ScrapWithChromium(Driver, query)));

            uint completeCount = 0;

            int startCount = sources.Count;

            while (sources.TryDequeue(out var currentSource))
            {
                try
                {
                    using var resp = await _httpClient.GetAsync(currentSource.Url);

                    if (!resp.IsSuccessStatusCode)
                    {
                        _logger.Warning("Scrapping failed for {url} (code: {code})", currentSource.Url, resp.StatusCode);
                        continue; // skip that fail.
                    }

                    var filePath = Path.Combine(outputDirectory,
                        GetFilenameFor(currentSource, resp));

                    await using (var respStream = await resp.Content.ReadAsStreamAsync())
                    {
                        await using (var fStream = File.Create(filePath))
                        {
                            await respStream.CopyToAsync(fStream);
                        }
                    }

                    // do custom scrap actions.
                    if (PostScrapsActions.Count > 0)
                    {
                        using var image = await Image.LoadAsync(filePath);

                        foreach (var postScrapsAction in PostScrapsActions)
                        {
                            _logger.Information("Post Action: {action}", postScrapsAction?.GetType().Name);

                            try
                            {
                                postScrapsAction?.Apply(image);
                            }
                            catch (Exception ex)
                            {
                                _logger.Error(ex, "Post action error.");
                            }

                            await image.SaveAsync(filePath);
                        }
                    }

                    _logger.Information("Downloading all... {p}%", (startCount - sources.Count)* 100 / startCount);
                    completeCount++;
                }
                catch (Exception ex)
                {
                    _logger.Error(ex, "An error has occurred while downloading.");
                }
            }

            _logger.Information("Downloaded {count} files in {dir}", completeCount, outputDirectory);
            return completeCount;
        }

        public ScrapperDownloader Using(IScrapper? scrapper)
        {
            if(scrapper != null)
                Scrappers.Add(scrapper);
            return this;
        }

        public ScrapperDownloader UsingPost(IPostScrapAction? postScrapAction)
        {
            if(postScrapAction != null)
                PostScrapsActions.Add(postScrapAction);

            return this;
        }

        private string GetFilenameFor(ScrapSource source, HttpResponseMessage responseMessage)
        {
            //get the extension relative to the local path.
            string localPath = new Uri(source.Url).LocalPath;
            string uriExt = Path.GetExtension(localPath);

            if (string.IsNullOrWhiteSpace(uriExt))
            {
                // try using the Content-Type header.
                var contentType = responseMessage.Content.Headers.ContentType;
                if (contentType != null)
                {
                    var imageFormat = _formatManager.FindFormatByMimeType(contentType.MediaType);
                    uriExt = imageFormat == null ? string.Empty : '.' + imageFormat.FileExtensions.First();
                }
                else // if no header is found, put the extension to an empty string.
                    uriExt = string.Empty;
            }

            return (source.Md5 ?? Path.GetFileNameWithoutExtension(localPath)) + uriExt;
        }
    }
}
