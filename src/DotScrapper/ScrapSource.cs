using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper
{
    /// <summary>
    /// A Class that represent a result of a <see cref="IScrapper"/>.
    /// </summary>
    public class ScrapSource
    {
        /// <summary>
        /// Get the <see cref="ScrapSource"/> file url.
        /// </summary>
        public string Url { get; }

        /// <summary>
        /// Get the <see cref="IScrapper"/> who have created this instance.
        /// </summary>
        public IScrapper Scrapper { get; }

        /// <summary>
        /// Get the md5 hash or null.
        /// </summary>
        public string? Md5 { get; init; }

        /// <summary>
        /// Create a new <see cref="ScrapSource"/> instance with an url, scrapper and md5.
        /// </summary>
        /// <param name="url">The file url.</param>
        /// <param name="scrapper">The scrapper instance.</param>
        /// <param name="md5">The MD5 hash of the file.</param>
        public ScrapSource(string url, IScrapper scrapper, string? md5 = null)
        {
            Url = url;
            Scrapper = scrapper;
            Md5 = md5;
        }
    }
}
