using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using SixLabors.ImageSharp;

namespace DotScrapper
{
    public class ActionContext : IDisposable
    {
        private Image? _imageCache;

        public ScrapSource Source { get; }
        public string Path { get; }

        public ActionContext(ScrapSource source, string path)
        {
            Source = source;
            Path = path;
        }

        public async Task<Image> RequireImageAsync()
        {
            _imageCache = await Image.LoadAsync(Path);
            return _imageCache;
        }

        public async Task SaveAsync()
        {
            if (_imageCache != null)
            {
                await _imageCache.SaveAsync(Path);
            }
        }

        public void Move(string newFilename, bool overwrite = true)
            => File.Move(Path, newFilename, overwrite);

        public void Dispose()
        {
            _imageCache?.Dispose();
            _imageCache = null;
        }
    }
}
