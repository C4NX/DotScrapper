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
        public bool RequireChromium { get; }

        public ScrapperDefinition(string name, bool requireChromium, string? description = null)
        {
            Name = name;
            Description = description;
            RequireChromium = requireChromium;
        }
    }
}
