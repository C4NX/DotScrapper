using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper.Viewing
{
    /// <summary>
    /// An interface of an <see cref="IWebViewer"/> DOM element.
    /// </summary>
    public interface IDomViewElement
    {
        /// <summary>
        /// Get or set the dom TagName of this element
        /// <exception cref="NotSupportedException">Tag Name switching operation is not supported</exception>
        /// </summary>
        public string Name { get; set; }

        string? GetDomAttribute(string key);

        IDomViewElement? FindElement(BySelector selector);
        IEnumerable<IDomViewElement> FindElements(BySelector selector);
    }
}
