using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using HtmlAgilityPack;
using HtmlAgilityPack.CssSelectors.NetCore;
using OpenQA.Selenium;
using Serilog;

namespace DotScrapper.Viewing
{
    /// <summary>
    /// An implementation based on the HtmlAgilityPack html document.
    /// </summary>
    public class HtmlBasedWebViewer : IWebViewer
    {
        private readonly ScrapperContext _context;
        private HtmlDocument? _document;

        public HtmlBasedWebViewer(ScrapperContext context)
        {
            _context = context;
        }

        public async Task Goto(string url)
        {
            _document = new HtmlDocument();
            _document.LoadHtml(await _context.Http.GetStringAsync(url));
        }

        public async Task GotoPost(string url, HttpContent? content)
        {
            _document = new HtmlDocument();
            using var postResult = await _context.Http.PostAsync(url, content);
            _document.LoadHtml(await postResult.Content.ReadAsStringAsync());
        }

        public IDomViewElement? FindElement(BySelector selector)
        {
            var nodeElement = _document?.QuerySelector(selector.Selector);
            return nodeElement != null ? new HtmlBasedViewElement(nodeElement) : null;
        }

        public IEnumerable<IDomViewElement> FindElements(BySelector selector)
        {
            return _document?.QuerySelectorAll(selector.Selector)
                .Select(x=>new HtmlBasedViewElement(x)) 
                   ?? Enumerable.Empty<IDomViewElement>();
        }
    }

    public class HtmlBasedViewElement : IDomViewElement
    {
        private readonly HtmlNode _node;

        public HtmlBasedViewElement(HtmlNode node)
        {
            _node = node;
        }

        public string Name
        {
            get => _node.Name;
            set => _node.Name = value;
        }

        public string? GetDomAttribute(string key)
            => (string?)_node.GetAttributeValue<string>(key, null!);

        public IDomViewElement? FindElement(BySelector selector)
        {
            var nodeElement = _node?.QuerySelector(selector.Selector);
            return nodeElement != null ? new HtmlBasedViewElement(nodeElement) : null;
        }

        public IEnumerable<IDomViewElement> FindElements(BySelector selector)
        {
            return _node?.QuerySelectorAll(selector.Selector)
                       .Select(x => new HtmlBasedViewElement(x))
                   ?? Enumerable.Empty<IDomViewElement>();
        }
    }
}
