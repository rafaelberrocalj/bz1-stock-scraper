namespace bz1.stockscraper.Models.Configuration;

public class ApplicationSettings
{
    public Dictionary<string, List<string>> Tickers { get; set; } = [];
    public ScraperConfiguration Scrapers { get; set; } = new();
    public BrowserConfiguration Browser { get; set; } = new();
    public LoggingConfiguration Logging { get; set; } = new();
    public OutputConfiguration Output { get; set; } = new();
}
