using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers
{
    public class Investidor10ComBrScraper : Scraper, IScraper
    {
        readonly string _endpointBasePath = "https://investidor10.com.br/";
        readonly string _waitForSelector = "#header_action > div.action > div.name-ticker > h1";

        string? EndpointPath { get; set; }

        public Investidor10ComBrScraper WithFii(string ticker)
        {
            Ticker = ticker;

            EndpointPath = $"fiis/{GetTicker()}";

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
                .AddSelector("pvp", "/html/body/div[4]/main/section/div/section[1]/div[3]/div[2]/span")
                .AddSelector("quote-value", "/html/body/div[4]/main/section/div/section[1]/div[1]/div[2]/div/span")
                .AddSelector("dividend-yield", "/html/body/div[4]/main/section/div/section[1]/div[2]/div[2]/div/span")
                .AddSelector("last-dividend", "/html/body/div[4]/main/section/div/div[9]/div[2]/div/div/div[2]/div/div/div[2]/table/tbody/tr[1]/td[4]");

            return builder;
        }
    }
}
