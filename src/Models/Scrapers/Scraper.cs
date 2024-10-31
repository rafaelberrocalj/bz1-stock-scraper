namespace bz1.stockscraper.Models.Scrapers
{
    public abstract class Scraper
    {
        internal string Ticker { get; set; }

        public string GetTicker()
        {
            ArgumentException.ThrowIfNullOrWhiteSpace(Ticker);

            return Ticker!;
        }
    }
}
