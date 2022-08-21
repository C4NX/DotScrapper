using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;
using Serilog;
using System.Text.Json;
using OpenQA.Selenium.Interactions;

namespace DotScrapper.Scrappers
{
    /// <summary>
    /// Bing image scrapper.
    /// </summary>
    public class Bing : IScrapper
    {
        private readonly ILogger _logger;

        /// <summary>
        /// Get or Set the waiting time (in ms) when new images are loaded, we recommend a longer time if your connection is bad, by default the value is 1000ms.  
        /// </summary>
        public int UpdateSleepTime { get; set; } = 1000;
        /// <summary>
        /// Get or Set the waiting time after adult settings was changed, by default the value is 500ms.
        /// </summary>
        public int AdultUpdateSleepTime { get; set; } = 500;

        public Bing()
        {
            _logger = Log.Logger
                .ForContext<Bing>();
        }

        public void Initialize(ChromiumDriver driver)
        {
            // no initialize for bing.
        }

        public IEnumerable<ScrapSource> ScrapWithChromium(ChromiumDriver driver, ScrapperQuery query)
        {
            var bingSearchUrl = $"https://www.bing.com/images/search?q={Uri.EscapeDataString(query.Query)}";

            driver.Url = bingSearchUrl;

            // delete cookie banner
            driver.DeleteElementById("bnp_cookie_banner");

            // check for adult block.
            if (driver.FindElementOrNull(By.ClassName("adltblkqmsg")) != null)
            {
                _logger.Warning("Adult content is disabled.");

                var updateParamBtn = driver.FindElementOrNull(By.CssSelector("a[class*='cbtn b_noTarget b_highlighted']"));

                if (updateParamBtn != null)
                {
                    _logger.Information("Updating parameters...");

                    updateParamBtn.Click();

                    driver.FindElementOrNull(By.CssSelector("label[for*='adlt_set_off']"))?.Click();
                    driver.FindElementOrNull(By.Id("sv_btn"))?.Click();
                    driver.FindElementOrNull(By.Id("adlt_confirm"))?.Click();

                    driver.Url = bingSearchUrl;
                    _logger.Information("Adult content is now enabled.");

                    // keep calm, that was fast.
                    Thread.Sleep(AdultUpdateSleepTime);
                }
                else
                {
                    _logger.Error("Failed to enable adult content.");
                }

            }

            foreach (var divElement in GetAllImages(driver))
            {
                // get json data with md5 & url.
                var aElement = divElement.FindElement(By.ClassName("iusc"));

                var jsonData = JsonSerializer.Deserialize<Dictionary<string, string?>>(aElement.GetDomAttribute("m"));

                if (jsonData != null)
                {
                    var md5 = jsonData.GetValueOrDefault("md5", null);
                    var murl = jsonData.GetValueOrDefault("murl", null);

                    if (md5 != null)
                    {
                        _logger.Information("Found: {src}", murl);

                        yield return new ScrapSource(murl!, this, md5);
                    }
                }
            }
        }

        private IEnumerable<IWebElement> GetAllImages(ChromiumDriver driver)
        {
            var byImgPt = By.ClassName("imgpt");

            var webElements = driver.FindElements(byImgPt);

            var imagesElements = new List<IWebElement>();

            int? lastAdded = null;
            while (lastAdded != 0)
            {
                int wasAdded = 0;
                foreach (var x in webElements)
                {
                    if (!imagesElements.Contains(x))
                    {
                        imagesElements.Add(x);
                        wasAdded++;
                    }
                }
                lastAdded = wasAdded;

                // move to the last added element.
                new OpenQA.Selenium.Interactions.Actions(driver)
                    .MoveToElement(imagesElements[^1])
                    .Perform();

                // remove cluster
                driver.DeleteElementById("inline_cluster");

                //let images data load.
                Thread.Sleep(UpdateSleepTime);

                // update elements
                webElements = driver.FindElements(byImgPt);
            }

            return imagesElements;
        }
    }
}
