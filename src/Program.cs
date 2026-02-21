using bz1.stockscraper.Models.Configuration;
using bz1.stockscraper.Models.Builders;
using bz1.stockscraper.Models.Scrapers;
using bz1.stockscraper.Services;
using Microsoft.Extensions.Configuration;
using System.Reflection;

// Build configuration
var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Build();

// Load app settings
var appSettings = configuration.Get<ApplicationSettings>()
    ?? throw new InvalidOperationException("Failed to load application settings");

Console.WriteLine("bz1-stock-scraper");

// Create services
var logger = new LoggingService(appSettings.Logging);
var dataParsingService = new DataParsingService();
var fileService = new FileService(logger);
var browserService = new BrowserService(appSettings.Browser, logger);

try
{
    // Build scrapers from configuration
    var scrapers = BuildScrapers(appSettings);

    logger.LogInfo($"Configured to scrape {scrapers.Count} tickers");

    // Create scraper service and execute
    var scraperService = new ScraperService(
        browserService,
        dataParsingService,
        appSettings.Scrapers.ExchangeRate,
        logger);

    var result = await scraperService.ScrapeAllAsync(scrapers);

    // Save results
    var outputPath = Path.Combine(Directory.GetCurrentDirectory(), appSettings.Output.FilePath);
    await fileService.SaveResultsAsync(result, outputPath);

    logger.LogInfo("Scraper completed successfully");
}
catch (Exception ex)
{
    logger.LogError("Scraper failed with error", ex);
    throw;
}
finally
{
    await ((IAsyncDisposable)browserService).DisposeAsync();
}

Console.WriteLine();
Console.WriteLine("scraper done, exiting");

// Helper method to build scrapers from configuration
static List<IScraper> BuildScrapers(ApplicationSettings appSettings)
{
    var scrapers = new List<IScraper>();

    // StatusInvest scrapers
    var statusInvestConfig = appSettings.Scrapers.StatusInvest;

    // FIIs
    if (appSettings.Tickers.TryGetValue("FIIs", out var fiis))
    {
        foreach (var ticker in fiis)
        {
            var builder = new StatusInvestComBrScraper(statusInvestConfig, "FIIs")
                .WithTicker(ticker)
                .Build();
            scrapers.Add(new ScraperAdapter(builder));
        }
    }

    // FIInfras
    if (appSettings.Tickers.TryGetValue("FIInfras", out var fiinfras))
    {
        foreach (var ticker in fiinfras)
        {
            var builder = new StatusInvestComBrScraper(statusInvestConfig, "FIInfras")
                .WithTicker(ticker)
                .Build();
            scrapers.Add(new ScraperAdapter(builder));
        }
    }

    // FIAgros
    if (appSettings.Tickers.TryGetValue("FIAgros", out var fiagros))
    {
        foreach (var ticker in fiagros)
        {
            var builder = new StatusInvestComBrScraper(statusInvestConfig, "FIAgros")
                .WithTicker(ticker)
                .Build();
            scrapers.Add(new ScraperAdapter(builder));
        }
    }

    // StockAnalysis scrapers
    var stockAnalysisConfig = appSettings.Scrapers.StockAnalysis;

    // ETFs
    if (appSettings.Tickers.TryGetValue("ETFEUA", out var etfs))
    {
        foreach (var ticker in etfs)
        {
            var builder = new StockAnalysisScraper(stockAnalysisConfig)
                .WithTicker(ticker)
                .Build();
            scrapers.Add(new ScraperAdapter(builder));
        }
    }

    return scrapers;
}
