using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper
{
    public class ScrapSource
    {
        private readonly string _url;
        private readonly IScrapper _scrapper;
        private readonly string? _md5;

        public string Url
            => _url;

        public IScrapper Scrapper
            => _scrapper;

        public string? Md5 { get; init; }

        public ScrapSource(string url, IScrapper scrapper, string? md5 = null)
        {
            _url = url;
            _scrapper = scrapper;
            Md5 = md5;
        }
    }
}
