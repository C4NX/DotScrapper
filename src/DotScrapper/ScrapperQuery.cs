namespace DotScrapper
{
    public class ScrapperQuery
    {
        public readonly static ScrapperQuery Empty
            = new (string.Empty);

        public string Query { get; set; }
        public int? MaxResults { get; set; }

        public ScrapperQuery(string query, int? maxResults = null)
        {
            Query = query;
            MaxResults = maxResults;
        }

        /// <summary>
        /// Parse like (Query)@[Max Results]
        /// </summary>
        /// <param name="value">The value to parse.</param>
        /// <returns>The parsed <see cref="ScrapperQuery"/></returns>
        public static ScrapperQuery Parse(string value)
        {
            if (string.IsNullOrWhiteSpace(value))
                throw new FormatException("Empty query");

            var spl = value.Split(new []{'@'}, 2, StringSplitOptions.RemoveEmptyEntries);

            return spl.Length == 1 ? new ScrapperQuery(spl[0]) : new ScrapperQuery(spl[0], int.Parse(spl[1]));
        }
    }
}
