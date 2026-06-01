using bz1.stockscraper.Models.Data;
using bz1.stockscraper.Models.Scrapers;
using HtmlAgilityPack;

namespace bz1.stockscraper.Services;

public interface IScraperService
{
    Task<ScrapingResult> ScrapeAllAsync(IEnumerable<IScraper> scrapers);
}

public class ScraperService : IScraperService
{
    private readonly IBrowserService _browserService;
    private readonly IDataParsingService _dataParsingService;
    private readonly ILogger _logger;

    public ScraperService(
        IBrowserService browserService,
        IDataParsingService dataParsingService,
        ILogger logger)
    {
        _browserService = browserService;
        _dataParsingService = dataParsingService;
        _logger = logger;
    }

    public async Task<ScrapingResult> ScrapeAllAsync(IEnumerable<IScraper> scrapers)
    {
        _logger.LogInfo("Starting scraping process");

        await _browserService.InitializeAsync();

        var result = new ScrapingResult();

        try
        {
            foreach (var scraper in scrapers)
            {
                try
                {
                    var ticker = scraper.GetTicker();
                    _logger.LogInfo($"Scraping {ticker} from {scraper.GetEndpoint()}");

                    await _browserService.GoToPageAsync(scraper.GetEndpoint());

                    try
                    {
                        await _browserService.WaitForSelectorAsync(scraper.GetWaitForSelector());
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError($"Failed to load page for {ticker}: {ex.Message}");
                        continue; // Skip to next ticker
                    }

                    await _browserService.DelayAsync();

                    var html = await _browserService.GetPageContentAsync();
                    var htmlDocument = new HtmlDocument();
                    htmlDocument.LoadHtml(html);

                    var selectorList = scraper.GetSelectors().ToList();
                    var tickerData = _dataParsingService.ParseSelectorValues(htmlDocument, selectorList);

                    result.Tickers[ticker] = new TickerData { Values = tickerData };

                    foreach (var kvp in tickerData)
                    {
                        _logger.LogInfo($"  {kvp.Key}: {kvp.Value}");
                    }
                }
                catch (Exception ex)
                {
                    _logger.LogError($"Error scraping {scraper.GetTicker()}: {ex.Message}", ex);
                }
            }

            _logger.LogInfo("Scraping tickers completed");
        }
        finally
        {
            await _browserService.DisposeAsync();
        }

        return result;
    }
}
