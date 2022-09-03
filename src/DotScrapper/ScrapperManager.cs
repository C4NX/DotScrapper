using System.Reflection;

namespace DotScrapper
{
    public class ScrapperManager
    {
        private readonly List<IScrapper> _scrappers = new();
        private readonly List<IScrapAction> _postScrapActions = new();

        public void Add(IScrapper scrapper)
            => _scrappers.Add(scrapper);

        public void Add(IScrapAction scrapAction)
            => _postScrapActions.Add(scrapAction);

        public void ScanAssembly(Assembly assembly)
        {
            foreach (var exportedType in assembly.GetExportedTypes())
            {
                if(typeof(IScrapper).IsAssignableFrom(exportedType)
                   && exportedType != typeof(IScrapper))
                    _scrappers.Add((IScrapper)(Activator.CreateInstance(exportedType) 
                                               ?? throw new NullReferenceException("CreateInstance --> Null")));
                else if (typeof(IScrapAction).IsAssignableFrom(exportedType)
                         && exportedType != typeof(IScrapAction))
                    _postScrapActions.Add((IScrapAction)(Activator.CreateInstance(exportedType) 
                                                             ?? throw new NullReferenceException("CreateInstance --> Null")));
            }
        }

        public IScrapper? GetByName(string? name)
        {
            return _scrappers
                .FirstOrDefault(x => x.Definition.Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IScrapAction? GetActionByName(string? name)
        {
            return _postScrapActions
                .FirstOrDefault(x => x.GetType().Name.Equals(name, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<IScrapper> AllScrappers()
            => _scrappers.AsEnumerable();

        public IEnumerable<IScrapAction> AllActions()
            => _postScrapActions.AsEnumerable();

        public static ScrapperManager FromAssembly(Assembly assembly)
        {
            var r = new ScrapperManager();
            r.ScanAssembly(assembly);
            return r;
        }
    }
}
