using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DotScrapper.Actions
{
    public class ToThumbnail : IPostScrapAction
    {
        public void Apply(Image image)
        {
            image.Mutate(x=>x.Resize(new Size(1280, 720), true));
        }
    }
}
