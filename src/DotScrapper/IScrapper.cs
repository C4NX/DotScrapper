using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;

namespace DotScrapper
{
    public interface IScrapper
    {
        void Initialize(ChromiumDriver driver);
        IEnumerable<ScrapSource> ScrapWithChromium(ChromiumDriver driver, ScrapperQuery query);
    }
}
