using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;

namespace DotScrapper
{
    /// <summary>
    /// An interface to scrap images.
    /// </summary>
    public interface IScrapper
    {
        /// <summary>
        /// Initialize that scrapper, it mostly configure the <see cref="ChromiumDriver"/>.
        /// </summary>
        /// <param name="driver"></param>
        void Initialize(ChromiumDriver driver);

        /// <summary>
        /// Scrap images with a <see cref="ChromiumDriver"/> and a <see cref="ScrapperQuery"/>.
        /// </summary>
        /// <param name="driver">That <see cref="ChromiumDriver"/></param>
        /// <param name="query">That <see cref="ScrapperQuery"/></param>
        /// <returns>An IEnumerable of <see cref="ScrapSource"/></returns>
        IEnumerable<ScrapSource> ScrapWithChromium(ChromiumDriver driver, ScrapperQuery query);
    }
}
