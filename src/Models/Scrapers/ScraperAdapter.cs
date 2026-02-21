using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers;

public class ScraperAdapter : IScraper
{
    private readonly IStockScraperBuilder _builder;

    public ScraperAdapter(IStockScraperBuilder builder)
    {
        _builder = builder;
    }

    public string GetTicker() => _builder.GetTicker();
    public string GetEndpoint() => _builder.GetEndpoint();
    public string GetWaitForSelector() => _builder.GetWaitForSelector();
    public IEnumerable<KeyValuePair<string, List<string>>> GetSelectors() => _builder.GetSelectors();

    public IStockScraperBuilder Build() => _builder;
}

public static class ScraperExtensions
{
    public static IEnumerable<KeyValuePair<string, List<string>>> GetSelectors(this IScraper scraper)
    {
        if (scraper is ScraperAdapter adapter)
            return adapter.GetSelectors();
        throw new InvalidOperationException("IScraper must be a ScraperAdapter to use GetSelectors");
    }

    public static string GetEndpoint(this IScraper scraper)
    {
        if (scraper is ScraperAdapter adapter)
            return adapter.GetEndpoint();
        throw new InvalidOperationException("IScraper must be a ScraperAdapter to use GetEndpoint");
    }

    public static string GetWaitForSelector(this IScraper scraper)
    {
        if (scraper is ScraperAdapter adapter)
            return adapter.GetWaitForSelector();
        throw new InvalidOperationException("IScraper must be a ScraperAdapter to use GetWaitForSelector");
    }
}
