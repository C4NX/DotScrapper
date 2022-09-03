using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium;

namespace DotScrapper.Viewing
{
    /// <summary>
    /// An interface to make a link between the html data/browser and the scrapper.
    /// </summary>
    public interface IWebViewer
    {
        /// <summary>
        /// Navigate to an url.
        /// </summary>
        /// <param name="url">The url to go.</param>
        /// <returns>An async task</returns>
        Task Goto(string url);

        /// <summary>
        /// Find an element using <see cref="BySelector"/>
        /// </summary>
        /// <param name="by">The <see cref="BySelector"/> to use.</param>
        /// <returns><see cref="IDomViewElement"/> or null.</returns>
        IDomViewElement? FindElement(BySelector by);

        /// <summary>
        /// Find multiple element using <see cref="BySelector"/>
        /// </summary>
        /// <param name="by">The <see cref="BySelector"/> to use.</param>
        /// <returns>An IEnumerable of <see cref="IDomViewElement"/></returns>
        IEnumerable<IDomViewElement> FindElements(BySelector by);
    }
}
