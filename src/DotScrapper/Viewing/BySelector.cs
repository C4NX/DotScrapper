using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DotScrapper.Viewing
{
    /// <summary>
    /// An extension to the selenium By system that allows to make css selector but to be compatible with all <see cref="IWebViewer"/>.
    /// </summary>
    public class BySelector
    {
        private string _selector;

        private bool IsByClass
            => _selector[0] == '.';
        private bool IsById
            => _selector[0] == '#';

        public string Selector
        {
            get => _selector;
            set => _selector = value;
        }

        public BySelector(string cssSelector)
        {
            _selector = cssSelector;
        }

        public By ToSeleniumBy()
        {
            if(IsByClass)
                return By.ClassName(_selector[1..]);
            if(IsById)
                return By.Id(_selector[1..]);

            return By.CssSelector(_selector);
        }

        public static BySelector ClassName(string className)
            => new ($".{className}");

        public static BySelector Id(string id)
            => new($"#{id}");
    }
}
