namespace bz1.stockscraper.Models.Builders
{
    public interface IStockScraperBuilder
    {
        public string GetEndpoint();
        public string GetWaitForSelector();
        public Dictionary<string, string> GetSelectors();
    }
}
