using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper
{
    public class ScrapperQuery
    {
        public string Query { get; set; }

        public ScrapperQuery(string query)
        {
            Query = query;
        }
    }
}
