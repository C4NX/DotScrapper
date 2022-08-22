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
        private readonly ILogger _logger;
        private readonly ScrapperContext _context;

        public IList<IScrapper> Scrappers { get; init; }
        public IList<IPostScrapAction?> PostScrapsActions { get; set; } 
            = new List<IPostScrapAction?>();

        public ScrapperDownloader(ScrapperContext ctx, IScrapper? scrapper = null)
        {
            _logger = Log.ForContext<ScrapperDownloader>();

            Scrappers = new List<IScrapper>();
            _context = ctx;

            if(scrapper != null)
                Scrappers.Add(scrapper);
        }

        public ScrapperDownloader(ScrapperContext ctx, IEnumerable<IScrapper> scrappers)
        {
            _logger = Log.ForContext<ScrapperDownloader>();

            Scrappers = new List<IScrapper>(scrappers);
            _context = ctx;
        }

        public async Task<uint> DownloadAsync(ScrapperQuery query, string outputDirectory, CancellationToken cancellationToken = default)
        {
            uint completeCount = 0;

            foreach (var scrapper in Scrappers)
            {
                _logger.Information("Downloading from {name}...", scrapper.Definition.Name);

                await Parallel.ForEachAsync(
                    scrapper.Perform(_context, query)
                    ,cancellationToken
                    ,async (x, token) =>
                    {
                        try
                        {
                            using var resp = await _context.Http.GetAsync(x.Url, token);

                            if (!resp.IsSuccessStatusCode)
                            {
                                _logger.Warning("Scrapping failed for {url} (code: {code})", x.Url, resp.StatusCode);
                                
                            }
                            else
                            {
                                var filePath = Path.Combine(outputDirectory,
                                    GetFilenameFor(x, resp));

                                _logger.Verbose("[{url}] Downloading...", x.Url);

                                await using (var respStream = await resp.Content.ReadAsStreamAsync(token))
                                {
                                    await using (var fStream = File.Create(filePath))
                                    {
                                        await respStream.CopyToAsync(fStream, token);
                                    }
                                }

                                // do custom scrap actions.
                                if (PostScrapsActions.Count > 0)
                                {
                                    using var image = await Image.LoadAsync(filePath, token);

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

                                        await image.SaveAsync(filePath, cancellationToken: token);
                                    }
                                }

                                _logger.Verbose("[{url}] Downloaded.", x.Url);
                                completeCount++;
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.Error(ex, "An error has occurred while downloading.");
                        }
                    });
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
                    var imageFormat = _context.FormatManager.FindFormatByMimeType(contentType.MediaType);
                    uriExt = imageFormat == null ? string.Empty : '.' + imageFormat.FileExtensions.First();
                }
                else // if no header is found, put the extension to an empty string.
                    uriExt = string.Empty;
            }

            return (source.Md5 ?? Path.GetFileNameWithoutExtension(localPath)) + uriExt;
        }
    }
}
