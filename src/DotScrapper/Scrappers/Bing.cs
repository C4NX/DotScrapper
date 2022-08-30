﻿using OpenQA.Selenium;
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

        public ScrapperDefinition Definition { get; } 
            = new("Bing", true, "Use the bing image service.");

        public Bing()
        {
            _logger = Log.Logger
                .ForContext<Bing>();
        }

        public void Initialize(ScrapperContext ctx)
        {
            // no initialize for bing.
        }

        public async IAsyncEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query)
        {
            var bingSearchUrl = $"https://www.bing.com/images/search?q={Uri.EscapeDataString(query.Query)}";

            var driver = ctx.Driver
                         ?? throw new ArgumentNullException(nameof(ctx),
                             "Driver was not given in this ScrapperContext");

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
                    _logger.Information("Adult content is now enabled");

                    // keep calm, that was fast.
                    await Task.Delay(AdultUpdateSleepTime);
                }
                else
                {
                    _logger.Error("Failed to enable adult content");
                }

            }

            foreach (var divElement in await GetAllImages(driver, query.MaxResults))
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
                        _logger.Debug("Found: {src}", murl);

                        yield return new ScrapSource(murl!, this, md5);
                    }
                }
            }
        }

        private async Task<IList<IWebElement>> GetAllImages(ChromiumDriver driver, int? maxResults)
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

                        // if there are more element in the imagesElement that the maxResults, end.
                        if (imagesElements.Count >= maxResults)
                        {
                            wasAdded = 0; // tell: no more images.
                            break;
                        }
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
                await Task.Delay(UpdateSleepTime);

                // update elements
                webElements = driver.FindElements(byImgPt);
            }

            return imagesElements;
        }
    }
}
