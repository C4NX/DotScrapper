using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper.Scrappers
{
    public class ThrowException : IScrapper
    {
        public ScrapperDefinition Definition { get; }
            = new ("ThrowException", false, "DO NOT USE.");
        public void Initialize(ScrapperContext ctx)
        {
            throw new NotImplementedException();
        }

        public IAsyncEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query)
        {
            throw new NotImplementedException();
        }
    }
}
