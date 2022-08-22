using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenQA.Selenium.Chromium;

namespace DotScrapper
{
    /// <summary>
    /// An interface to scrap.
    /// </summary>
    public interface IScrapper
    {
        ScrapperDefinition Definition { get; }

        /// <summary>
        /// Initialize that scrapper with a given <see cref="ScrapperContext"/>.
        /// </summary>
        /// <param name="ctx">That <see cref="ScrapperContext"/></param>
        void Initialize(ScrapperContext ctx);

        /// <summary>
        /// Scrap on the net with a given <see cref="ScrapperContext"/> and a <see cref="ScrapperQuery"/>.
        /// </summary>
        /// <param name="ctx">That <see cref="ScrapperContext"/></param>
        /// <param name="query">That <see cref="ScrapperQuery"/></param>
        /// <returns>An IEnumerable of <see cref="ScrapSource"/></returns>
        IAsyncEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query);
    }
}
