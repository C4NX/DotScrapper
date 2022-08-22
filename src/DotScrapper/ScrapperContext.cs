using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;
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
    public class ScrapperContext : IDisposable
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
        /// Create a new instance of <see cref="ScrapperContext"/>
        /// </summary>
        /// <param name="driver">The <see cref="ChromiumDriver"/> to use. It can be null.</param>
        /// <param name="http">The <see cref="Http"/> to use.</param>
        public ScrapperContext(ChromiumDriver? driver, HttpClient http)
        {
            Driver = driver;
            Http = http;

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
        }

        public void UseChromium(ChromiumDriver? driver)
            => Driver = driver;

        public void Dispose()
        {
            Driver?.Dispose();
            Http.Dispose();
        }
    }
}
