namespace bz1.stockscraper.Models.Builders
{
    public class StockScraperBuilder : IStockScraperBuilder
    {
        readonly string _ticker;
        readonly string _endpoint;
        readonly string _waitForSelector;
        readonly Dictionary<string, string> _selectors = new Dictionary<string, string>();

        public StockScraperBuilder(
            string ticker,
            string endpoint,
            string waitForSelector)
        {
            _ticker = ticker;
            _endpoint = endpoint;
            _waitForSelector = waitForSelector;
        }

        public string GetTicker()
        {
            return _ticker;
        }

        public string GetEndpoint()
        {
            return _endpoint;
        }

        public string GetWaitForSelector()
        {
            return _waitForSelector;
        }

        public Dictionary<string, string> GetSelectors()
        {
            return _selectors;
        }

        public StockScraperBuilder AddSelector(string name, string selector)
        {
            _selectors.Add(name, selector);
            return this;
        }
    }
}
