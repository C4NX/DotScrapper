using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper
{
    public class ScrapperDefinition
    {
        public string Name { get; }
        public string? Description { get; }
        public ScrapperDefFlag Flags { get; }

        public ScrapperDefinition(string name, ScrapperDefFlag flags, string? description = null)
        {
            Name = name;
            Description = description;
            Flags = flags;
        }
    }

    [Flags]
    public enum ScrapperDefFlag
    {
        None = 0,
        /// <summary>
        /// This scrapper require chromium driver.
        /// </summary>
        RequireChromium = 1<<0,
        /// <summary>
        /// This scrapper require HtmlAgilityPack, used for CreateViewer.
        /// </summary>
        RequireHap=1<<1,
    }
}
