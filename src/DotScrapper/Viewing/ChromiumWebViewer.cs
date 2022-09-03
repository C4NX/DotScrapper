using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;
using OpenQA.Selenium.Chromium;

namespace DotScrapper.Viewing
{
    /// <summary>
    /// An implementation for chromium.
    /// </summary>
    public class ChromiumWebViewer : IWebViewer
    {
        private readonly ChromiumDriver _driver;

        public ChromiumWebViewer(ChromiumDriver driver)
        {
            _driver = driver;
        }

        public Task Goto(string url)
        {
            _driver.Url = url;

            return Task.CompletedTask;
        }

        public IDomViewElement? FindElement(BySelector selector)
        {
            var element = _driver.FindElementOrNull(selector.ToSeleniumBy());

            return element == null ? null : new SeleniumDomViewElement(element);
        }

        public IEnumerable<IDomViewElement> FindElements(BySelector selector)
            => _driver.FindElements(selector.ToSeleniumBy())
                .Select(x=>new SeleniumDomViewElement(x));
    }

    /// <summary>
    /// An <see cref="IWebElement"/> selenium element rebuilt for an <see cref="IDomViewElement"/>
    /// </summary>
    public class SeleniumDomViewElement : IDomViewElement
    {
        private readonly IWebElement _element;

        public SeleniumDomViewElement(IWebElement element)
        {
            _element = element;
        }

        public string Name
        {
            get => _element.TagName;
            set => throw new NotSupportedException();
        }

        public string? GetDomAttribute(string key)
            => _element.GetDomAttribute(key);

        public IDomViewElement? FindElement(BySelector by)
        {
            var elmt = _element.FindElementOrNull(by.ToSeleniumBy());
            return elmt == null ? null : new SeleniumDomViewElement(elmt);
        }

        public IEnumerable<IDomViewElement> FindElements(BySelector by)
            => _element.FindElements(by.ToSeleniumBy())
                .Select(x => new SeleniumDomViewElement(x));
    }
}
