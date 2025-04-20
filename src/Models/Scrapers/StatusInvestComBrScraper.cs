using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers;

public class StatusInvestComBrScraper : IScraper
{
    readonly string _endpointBasePath = "https://statusinvest.com.br/";
    readonly string _waitForSelector = "#earning-section > div.d-md-flex.justify-between.align-items-center.mb-2 > div.card-title > a > h3";

    string? Ticker { get; set; }
    string? EndpointPath { get; set; }

    public StatusInvestComBrScraper WithTicker(string ticker)
    {
        Ticker = ticker;

        return this;
    }

    public string GetTicker()
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(Ticker);

        return Ticker!;
    }

    public StatusInvestComBrScraper WithFIIs()
    {
        EndpointPath = $"fundos-imobiliarios/{GetTicker()}";

        return this;
    }

    public StatusInvestComBrScraper WithFIInfras()
    {
        EndpointPath = $"fiinfras/{GetTicker()}";

        return this;
    }

    public StatusInvestComBrScraper WithFIAgros()
    {
        EndpointPath = $"fiagros/{GetTicker()}";

        return this;
    }

    public StatusInvestComBrScraper WithETFEUA()
    {
        EndpointPath = $"etf/eua/{GetTicker()}";

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
                "/html/body/main/div[2]/div[8]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]",
                "/html/body/main/div[2]/div[7]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]",
                "/html/body/main/div[3]/div[1]/div[2]/div[7]/div/div[2]/table/tbody/tr[1]/td[4]"
            );

        return builder;
    }
}
