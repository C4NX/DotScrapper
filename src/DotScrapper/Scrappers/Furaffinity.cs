using OpenQA.Selenium;

namespace DotScrapper.Scrappers;

public class Furaffinity : IScrapper
{
    public ScrapperDefinition Definition { get; }
        = new("Furaffinity", true, "Use the furaffinity.net image service.");
    
    public void Initialize(ScrapperContext ctx)
    {
        // no initialize.
    }

    public async IAsyncEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query)
    {
        if (ctx.Driver == null)
            throw new ArgumentNullException(nameof(ctx.Driver));
        
        ctx.Driver.Url = $"https://www.furaffinity.net/search/?q={Uri.EscapeDataString(query.Query)}";

        // wait a little.
        await Task.Delay(ctx.Random.Next(100, 250));

        foreach (var postId in ctx.Driver.FindElements(By.ClassName("t-image"))
                     .Select(x => x.GetDomAttribute("id")[4..])
                     .ToArray()/*compute all in an array before.*/)
        {
            ctx.Driver.Url = $"https://www.furaffinity.net/view/{postId}/";
            
            // wait a little.
            await Task.Delay(ctx.Random.Next(100, 250));
            
            var imgElement = ctx.Driver.FindElementOrNull(By.Id("submissionImg"));
            var srcAttr = imgElement?.GetDomAttribute("data-fullview-src");
            
            if (srcAttr != null)
            {
                ctx.Logger.Verbose("{Id} -> Resolve: {Url}", postId, srcAttr);
                yield return new ScrapSource($"https:{srcAttr}", this);
            }
        }
    }
}