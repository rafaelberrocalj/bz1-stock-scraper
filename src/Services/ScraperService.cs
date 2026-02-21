using bz1.stockscraper.Models.Configuration;
using bz1.stockscraper.Models.Data;
using bz1.stockscraper.Models.Scrapers;
using HtmlAgilityPack;

namespace bz1.stockscraper.Services;

public interface IScraperService
{
    Task<ScrapingResult> ScrapeAllAsync(IEnumerable<IScraper> scrapers);
    Task<DolarData> ScrapeExchangeRateAsync();
}

public class ScraperService : IScraperService
{
    private readonly IBrowserService _browserService;
    private readonly IDataParsingService _dataParsingService;
    private readonly ExchangeRateConfiguration _exchangeRateConfig;
    private readonly ILogger _logger;

    public ScraperService(
        IBrowserService browserService,
        IDataParsingService dataParsingService,
        ExchangeRateConfiguration exchangeRateConfig,
        ILogger logger)
    {
        _browserService = browserService;
        _dataParsingService = dataParsingService;
        _exchangeRateConfig = exchangeRateConfig;
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

            // Fetch exchange rate
            try
            {
                _logger.LogInfo("Fetching exchange rate...");
                result.DolarExchangeRate = await ScrapeExchangeRateAsync();
                _logger.LogInfo($"Exchange rate: {result.DolarExchangeRate.Value} on {result.DolarExchangeRate.Date}");
            }
            catch (Exception ex)
            {
                _logger.LogError("Failed to fetch exchange rate", ex);
                // Continue even if exchange rate fails
            }
        }
        finally
        {
            await _browserService.DisposeAsync();
        }

        return result;
    }

    public async Task<DolarData> ScrapeExchangeRateAsync()
    {
        try
        {
            await _browserService.GoToPageAsync(_exchangeRateConfig.Endpoint);
            var html = await _browserService.GetPageContentAsync();

            var htmlDoc = new HtmlDocument();
            htmlDoc.LoadHtml(html);

            var element = htmlDoc.GetElementbyId(_exchangeRateConfig.ElementId);
            if (element == null)
            {
                throw new InvalidOperationException($"Element with ID '{_exchangeRateConfig.ElementId}' not found");
            }

            var valueStr = element.GetAttributeValue(_exchangeRateConfig.AttributeName, string.Empty);
            if (string.IsNullOrWhiteSpace(valueStr))
            {
                throw new InvalidOperationException($"Attribute '{_exchangeRateConfig.AttributeName}' not found or empty");
            }

            var culturePtBr = System.Globalization.CultureInfo.CreateSpecificCulture("pt-BR");
            var numberStyles = System.Globalization.NumberStyles.AllowDecimalPoint | System.Globalization.NumberStyles.AllowThousands;

            if (!double.TryParse(valueStr, numberStyles, culturePtBr, out double value))
            {
                throw new InvalidOperationException($"Failed to parse exchange rate value: {valueStr}");
            }

            return new DolarData
            {
                Value = value,
                Date = DateTime.Today.ToString("yyyy/MM/dd")
            };
        }
        catch (Exception ex)
        {
            _logger.LogError("Error scraping exchange rate", ex);
            throw;
        }
    }
}
