namespace bz1.stockscraper.Models.Builders
{
    public class StockScraperBuilder : IStockScraperBuilder
    {
        readonly string _ticker;
        readonly string _endpoint;
        readonly string _waitForSelector;
        readonly List<KeyValuePair<string, string>> _selectors = new List<KeyValuePair<string, string>>();

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

        public List<KeyValuePair<string, string>> GetSelectors()
        {
            return _selectors;
        }

        public StockScraperBuilder AddSelector(string name, params string[] selectors)
        {
            foreach (var selector in selectors)
            {
                _selectors.Add(new KeyValuePair<string, string>(name, selector));
            }

            return this;
        }
    }
}
