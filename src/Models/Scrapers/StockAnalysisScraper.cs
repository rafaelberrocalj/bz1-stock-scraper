using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers;

public class StockAnalysisScraper : IScraper
{
    readonly string _endpointBasePath = "https://stockanalysis.com/";
    readonly string _waitForSelector = @"#main > div.wrsb.mt-3.py-1.sm\:mt-4 > div > h1";

    string? Ticker { get; set; }
    string? EndpointPath { get; set; }

    public StockAnalysisScraper WithTicker(string ticker)
    {
        Ticker = ticker;

        return this;
    }

    public string GetTicker()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Ticker);

        return Ticker!;
    }

    public StockAnalysisScraper WithETF()
    {
        EndpointPath = $"etf/{GetTicker()}/dividend/";

        return this;
    }

    public IStockScraperBuilder Build()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(EndpointPath);

        var builder = new StockScraperBuilder(
            GetTicker(),
            string.Concat(_endpointBasePath, EndpointPath),
            _waitForSelector
        );

        builder
            .AddSelector("dividend",
                "/html/body/div/div[1]/div[2]/main/div[2]/div/div[3]/div[1]/div[2]/table/tbody/tr[1]/td[2]"
            );

        return builder;
    }
}
