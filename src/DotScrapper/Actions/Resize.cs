using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DotScrapper.Actions
{
    public class Resize : IPostScrapAction
    {
        public Size SizeTo { get; set; }
            = new Size(1280, 720);

        public void Apply(Image image)
        {
            image.Mutate(x=>x.Resize(SizeTo, true));
        }
    }
}
