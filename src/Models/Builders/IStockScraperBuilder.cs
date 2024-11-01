namespace bz1.stockscraper.Models.Builders
{
    public interface IStockScraperBuilder
    {
        public string GetTicker();
        public string GetEndpoint();
        public string GetWaitForSelector();
        public List<KeyValuePair<string, string>> GetSelectors();
    }
}
