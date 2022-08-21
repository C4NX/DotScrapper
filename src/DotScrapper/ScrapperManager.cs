using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace DotScrapper
{
    public class ScrapperManager
    {
        private readonly List<IScrapper> _scrappers = new();

        public void Add(IScrapper scrapper)
            => _scrappers.Add(scrapper);

        public void ScanAssembly(Assembly assembly)
        {
            _scrappers.AddRange(assembly.GetExportedTypes()
                .Where(x=> typeof(IScrapper).IsAssignableFrom(x) && x != typeof(IScrapper))
                .Select(x=>(IScrapper)Activator.CreateInstance(x)!));
        }

        public IScrapper? GetByName(string? name)
        {
            return _scrappers
                .FirstOrDefault(x => x.GetType().Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<IScrapper> All()
            => _scrappers.AsEnumerable();
    }
}
