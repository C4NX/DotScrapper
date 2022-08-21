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
        private readonly List<IPostScrapAction> _postScrapActions = new();

        public void Add(IScrapper scrapper)
            => _scrappers.Add(scrapper);

        public void Add(IPostScrapAction postScrapAction)
            => _postScrapActions.Add(postScrapAction);

        public void ScanAssembly(Assembly assembly)
        {
            foreach (var exportedType in assembly.GetExportedTypes())
            {
                if(typeof(IScrapper).IsAssignableFrom(exportedType)
                   && exportedType != typeof(IScrapper))
                    _scrappers.Add((IScrapper)(Activator.CreateInstance(exportedType) ?? throw new NullReferenceException("CreateInstance --> Null")));
                else if (typeof(IPostScrapAction).IsAssignableFrom(exportedType)
                         && exportedType != typeof(IPostScrapAction))
                    _postScrapActions.Add((IPostScrapAction)(Activator.CreateInstance(exportedType) ?? throw new NullReferenceException("CreateInstance --> Null")));
            }
        }

        public IScrapper? GetByName(string? name)
        {
            return _scrappers
                .FirstOrDefault(x => x.GetType().Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IPostScrapAction? GetPostActionByName(string? name)
        {
            return _postScrapActions
                .FirstOrDefault(x => x.GetType().Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<IScrapper> AllScrappers()
            => _scrappers.AsEnumerable();

        public IEnumerable<IPostScrapAction> AllPostActions()
            => _postScrapActions.AsEnumerable();
    }
}
