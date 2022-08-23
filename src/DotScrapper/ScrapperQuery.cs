namespace DotScrapper
{
    public class ScrapperQuery
    {
        public string Query { get; set; }
        public int? MaxResults { get; set; }

        public ScrapperQuery(string query, int? maxResults)
        {
            Query = query;
            MaxResults = maxResults;
        }
    }
}
