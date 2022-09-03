using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Processing;

namespace DotScrapper.Actions
{
    public class Resize : IScrapAction
    {
        public Size SizeTo { get; set; }
            = new Size(1280, 720);

        public async Task Apply(ActionContext ctx)
        {
            using var image = await ctx.RequireImageAsync();
            image.Mutate(x=>x.Resize(SizeTo, true));
        }
    }
}
