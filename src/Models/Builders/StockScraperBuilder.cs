namespace bz1.stockscraper.Models.Builders;

public class StockScraperBuilder : IStockScraperBuilder
{
    readonly string _ticker;
    readonly string _endpoint;
    readonly string _waitForSelector;
    readonly Dictionary<string, List<string>> _selectors = new();

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

    public IEnumerable<KeyValuePair<string, List<string>>> GetSelectors()
    {
        return _selectors.Select(x => new KeyValuePair<string, List<string>>(x.Key, x.Value));
    }

    public StockScraperBuilder AddSelector(string name, params string[] selectors)
    {
        if (!_selectors.ContainsKey(name))
        {
            _selectors[name] = new List<string>();
        }

        _selectors[name].AddRange(selectors);

        return this;
    }
}
