using DotScrapper.Viewing;
using Serilog;

namespace DotScrapper.Scrappers;

public class Furaffinity : IScrapper
{
    private HtmlBasedWebViewer? viewer;

    public ScrapperDefinition Definition { get; }
        = new("Furaffinity", ScrapperDefFlag.RequireHap, "Use the furaffinity.net image service.");
    
    public void Initialize(ScrapperContext ctx)
    {
        viewer = (HtmlBasedWebViewer)ctx.CreateViewer(Definition);
    }

    public async IAsyncEnumerable<ScrapSource> Perform(ScrapperContext ctx, ScrapperQuery query)
    {
        if (viewer == null)
            throw new InvalidOperationException("Use of a non-initialized scrapper.");

        foreach (var postId in await GetQueryIds(viewer, query))
        {
            await viewer.Goto($"https://www.furaffinity.net/view/{postId}/");
            
            // wait a little.
            await Task.Delay(ctx.Random.Next(100, 250));
            
            var imgElement = viewer.FindElement(BySelector.Id("submissionImg"));
            var srcAttr = imgElement?.GetDomAttribute("data-fullview-src");
            
            if (srcAttr != null)
            {
                ctx.Logger.Verbose("{Id} -> Resolve: {Url}", postId, srcAttr);
                yield return new ScrapSource($"https:{srcAttr}", this);
            }
        }
    }

    private static async Task<IReadOnlyCollection<string>> GetQueryIds(HtmlBasedWebViewer viewer, ScrapperQuery query)
    {
        if (query.MaxResults == null)
        {
            return BaseGetIds(viewer)
                .ToList();
        }

        List<string> ids = new List<string>();
        int page = 0;
        FuraffinityRequestValues requestValues = new FuraffinityRequestValues(query, page);
        while (ids.Count < query.MaxResults)
        {
            page++;

            await viewer.GotoPost("https://www.furaffinity.net/search/", requestValues.CreateContent());
            var idsToAdd = BaseGetIds(viewer);
            if (idsToAdd.Count == 0)
                break;

            ids.AddRange(idsToAdd);
        }

        return ids;
    }

    private static IReadOnlyCollection<string> BaseGetIds(IWebViewer webViewer)
        => webViewer.FindElements(BySelector.ClassName("t-image"))
            .Select(x => (x.GetDomAttribute("id") ?? new string('0',4))[4..])
            .ToList() /*compute all in an array before.*/;
}

public class FuraffinityRequestValues
{
    public FuraffinityRequestValues(ScrapperQuery query, int? page = null)
    {
        Query = query.Query;
        Page = page.GetValueOrDefault(1);
    }

    public int Page { get; set; }
    public string NextPage { get; set; } = "Next";
    public string Query { get; set; }
    public string OrderBy { get; set; } = "relevancy";
    public string OrderDirection { get; set; } = "desc";
    public string Range { get; set; } = "5years";
    public string RangeFrom { get; set; } = string.Empty;
    public string RangeTo { get; set; } = string.Empty;
    public int RatingGeneral { get; set; } = 1;

    public HttpContent CreateContent()
        => new FormUrlEncodedContent(new Dictionary<string, string>()
        {
            { "page", Page.ToString() },
            { "next_page", NextPage },
            { "q", Query },
            { "order-by", OrderBy },
            { "order-direction", OrderDirection },
            { "range", Range },
            { "range_from", RangeFrom },
            { "range_to", RangeTo },
            { "rating-general", RatingGeneral.ToString() },
            { "type-art", "1" },
            { "type-music", "1" },
            { "type-flash", "1" },
            { "type-story", "1" },
            { "type-photo", "1" },
            { "type-poetry", "1" },
            { "mode", "extended" },
        });
}