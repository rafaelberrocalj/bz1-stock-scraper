using bz1.stockscraper.Models.Builders;

namespace bz1.stockscraper.Models.Scrapers;

public interface IScraper
{
    public IStockScraperBuilder Build();
    public string GetTicker();
}
