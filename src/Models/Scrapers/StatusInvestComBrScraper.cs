using bz1.stockscraper.Models.Builders;
using bz1.stockscraper.Models.Configuration;

namespace bz1.stockscraper.Models.Scrapers;

public class StatusInvestComBrScraper : IScraper
{
    private readonly StatusInvestConfiguration _config;
    private readonly string _selectorSetName;
    private string? _ticker;

    public StatusInvestComBrScraper(StatusInvestConfiguration config, string selectorSetName)
    {
        _config = config;
        _selectorSetName = selectorSetName;
    }

    public StatusInvestComBrScraper WithTicker(string ticker)
    {
        _ticker = ticker;
        return this;
    }

    public string GetTicker()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_ticker);
        return _ticker!;
    }

    public IStockScraperBuilder Build()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(_ticker);

        if (!_config.SelectorSets.TryGetValue(_selectorSetName, out var selectorSet))
        {
            throw new InvalidOperationException($"Selector set '{_selectorSetName}' not found in configuration");
        }

        var endpoint = $"{_config.BaseEndpoint}{selectorSet.EndpointPath}".Replace("{0}", _ticker);

        var builder = new StockScraperBuilder(
            _ticker,
            endpoint,
            _config.WaitForSelector
        );

        // Add selectors from configuration
        foreach (var selector in selectorSet.Selectors)
        {
            builder.AddSelector(selector.Key, selector.Value.ToArray());
        }

        return builder;
    }
}
