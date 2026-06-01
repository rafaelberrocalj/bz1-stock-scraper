namespace bz1.stockscraper.Models.Data;

public class TickerData
{
    public Dictionary<string, object> Values { get; set; } = [];
}

public class ScrapingResult
{
    public Dictionary<string, TickerData> Tickers { get; set; } = [];

    public Dictionary<string, Dictionary<string, object>> ToOutputFormat()
    {
        var result = new Dictionary<string, Dictionary<string, object>>();

        foreach (var ticker in Tickers)
        {
            result[ticker.Key] = ticker.Value.Values;
        }

        return result;
    }
}
