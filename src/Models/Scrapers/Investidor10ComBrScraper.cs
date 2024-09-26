using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers
{
    public class Investidor10ComBrScraper : IScraper
    {
        readonly string _endpointBasePath = "https://investidor10.com.br/";
        readonly string _waitForSelector = "#header_action > div.action > div.name-ticker > h1";

        string? EndpointPath { get; set; }

        public Investidor10ComBrScraper WithFii(string ticker)
        {
            EndpointPath = $"fiis/{ticker}";
            return this;
        }

        public IStockScraperBuilder Build()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(EndpointPath);

            var builder = new StockScraperBuilder(
                string.Concat(_endpointBasePath, EndpointPath),
                _waitForSelector
            );

            builder
                .AddSelector("pvp", "/html/body/div[4]/main/section/div/section[1]/div[3]/div[2]/span")
                .AddSelector("quote-value", "/html/body/div[4]/main/section/div/section[1]/div[1]/div[2]/div/span");

            return builder;
        }
    }
}
