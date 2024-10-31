using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers
{
    public class StatusInvestComBrScraper : Scraper, IScraper
    {
        readonly string _endpointBasePath = "https://statusinvest.com.br/";
        readonly string _waitForSelector = "#main-header > div.container.pl-2.pr-1.pl-xs-3.pr-xs-3 > div > div:nth-child(1) > h1";

        string? EndpointPath { get; set; }

        public StatusInvestComBrScraper WithFii(string ticker)
        {
            Ticker = ticker;

            EndpointPath = $"fundos-imobiliarios/{GetTicker()}";

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
                .AddSelector("pvp", "/html/body/main/div[2]/div[5]/div/div[2]/div/div[1]/strong")
                .AddSelector("quote-value", "/html/body/main/div[2]/div[1]/div[1]/div/div[1]/strong")
                .AddSelector("dividend-yield", "/html/body/main/div[2]/div[1]/div[4]/div/div[1]/strong")
                .AddSelector("last-dividend", "/html/body/main/div[2]/div[8]/div/div[7]/div/div[2]/table/tbody/tr[1]/td[4]");

            return builder;
        }
    }
}
