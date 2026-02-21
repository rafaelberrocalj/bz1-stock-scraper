namespace bz1.stockscraper.Models.Data;

public class TickerData
{
    public Dictionary<string, object> Values { get; set; } = [];
}

public class DolarData
{
    public double Value { get; set; }
    public string Date { get; set; } = "";
}

public class ScrapingResult
{
    public Dictionary<string, TickerData> Tickers { get; set; } = [];
    public DolarData DolarExchangeRate { get; set; } = new();

    public Dictionary<string, Dictionary<string, object>> ToOutputFormat()
    {
        var result = new Dictionary<string, Dictionary<string, object>>();

        foreach (var ticker in Tickers)
        {
            result[ticker.Key] = ticker.Value.Values;
        }

        result["DOLAR"] = new Dictionary<string, object>
        {
            ["value"] = DolarExchangeRate.Value,
            ["date"] = DolarExchangeRate.Date
        };

        return result;
    }
}
