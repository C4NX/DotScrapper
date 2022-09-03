using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using DotScrapper.Viewing;
using OpenQA.Selenium.Chromium;
using Serilog;
using SixLabors.ImageSharp.Formats;
using SixLabors.ImageSharp.Formats.Bmp;
using SixLabors.ImageSharp.Formats.Gif;
using SixLabors.ImageSharp.Formats.Jpeg;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.Formats.Tga;
using SixLabors.ImageSharp.Formats.Webp;

namespace DotScrapper
{
    /// <summary>
    /// An instance contains everything that can make the scrapper work.
    /// </summary>
    public class ScrapperContext
    {
        /// <summary>
        /// Get the <see cref="ChromiumDriver"/> to use,it can be null.
        /// </summary>
        public ChromiumDriver? Driver { get; private set; }

        /// <summary>
        /// Get the <see cref="HttpClient"/> to use.
        /// </summary>
        public HttpClient Http { get; }

        /// <summary>
        /// Get the <see cref="ImageFormatManager"/> to use.
        /// </summary>
        public ImageFormatManager FormatManager { get; }

        /// <summary>
        /// Get the <see cref="ILogger"/> to use (for <see cref="IScrapper"/>).
        /// </summary>
        public ILogger Logger { get; }
        
        public Random Random { get; }

        /// <summary>
        /// Create a new instance of <see cref="ScrapperContext"/>
        /// </summary>
        /// <param name="driver">The <see cref="ChromiumDriver"/> to use. It can be null.</param>
        /// <param name="http">The <see cref="Http"/> to use.</param>
        public ScrapperContext(ChromiumDriver? driver, HttpClient http)
        {
            Driver = driver;
            Http = http;
            Random = new Random();
            
            FormatManager = new ImageFormatManager();
            FormatManager.AddImageFormatDetector(new PngImageFormatDetector());
            FormatManager.AddImageFormatDetector(new BmpImageFormatDetector());
            FormatManager.AddImageFormatDetector(new JpegImageFormatDetector());
            FormatManager.AddImageFormatDetector(new GifImageFormatDetector());
            FormatManager.AddImageFormatDetector(new WebpImageFormatDetector());
            FormatManager.AddImageFormatDetector(new TgaImageFormatDetector());

            FormatManager.AddImageFormat(PngFormat.Instance);
            FormatManager.AddImageFormat(BmpFormat.Instance);
            FormatManager.AddImageFormat(JpegFormat.Instance);
            FormatManager.AddImageFormat(GifFormat.Instance);
            FormatManager.AddImageFormat(WebpFormat.Instance);
            FormatManager.AddImageFormat(TgaFormat.Instance);

            Logger = Log.ForContext<ScrapperContext>();
        }

        public void UseChromium(ChromiumDriver? driver)
        {
            Driver = driver;
        }

        public IWebViewer CreateViewer(ScrapperDefinition def)
        {
            if (Driver != null && def.Flags.HasFlag(ScrapperDefFlag.RequireChromium))
                return new ChromiumWebViewer(Driver);
            if (def.Flags.HasFlag(ScrapperDefFlag.RequireHap))
                return new HtmlBasedWebViewer(this);

            Logger.Warning("{Name} does not contains a viewer require flag, default was used", def.Name);
            return new HtmlBasedWebViewer(this);
        }

        public void DisposeContext()
        {
            Driver?.Dispose();
            Http.Dispose();
        }
    }
}
