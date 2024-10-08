﻿using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers
{
    public class MundoFiiComScraper : IScraper
    {
        readonly string _endpointBasePath = "https://mundofii.com/";
        readonly string _waitForSelector = "#card_vermelho--sigla";

        string? EndpointPath { get; set; }

        public MundoFiiComScraper WithFii(string ticker)
        {
            EndpointPath = $"fundos/{ticker}";
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
                .AddSelector("pvp", "/html/body/span[2]/span[4]/span/span[1]/span[2]/span[2]/span[1]/b")
                .AddSelector("quote-value", "/html/body/span[2]/span[4]/span/span[1]/span[2]/span[1]/span[1]/b");

            return builder;
        }
    }
}
