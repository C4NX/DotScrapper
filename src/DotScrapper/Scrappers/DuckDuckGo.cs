/*using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;

namespace DotScrapper.Scrappers
{
    public class DuckDuckGo : IScrapper
    {
        public ScrapperDefinition Definition { get; }
            = new("DuckDuckGo", true, "Use the DuckDuckGo service to retrieve pictures.");

        public void Initialize(ScrapperContext ctx)
        {
        }

        public IEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query)
        {
            var driver = ctx.Driver ?? throw  new ArgumentNullException(nameof(ctx.Driver), "Driver was not given");
            var logger = ctx.Logger;

            driver.Url = $"https://duckduckgo.com/?q={Uri.EscapeDataString(query.Query)}&iar=images&iax=images&ia=images";

            foreach (var webElement in GetAllImages(driver))
            {
                var dataSrc = webElement.GetDomAttribute("data-src");
                logger.Information("Found: {url}", dataSrc);
            }

            yield break;
        }

        public IEnumerable<IWebElement> GetAllImages(ChromiumDriver driver)
        {
            List<IWebElement> images = new List<IWebElement>();
            Thread.Sleep(1000);

            images.AddRange(driver.FindElements(By.ClassName("tile--img__img")));

            return images;
        }
    }
}
*/