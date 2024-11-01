using bz1.stockscraper.Models.Scrapers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using System.Reflection;
using System.Text.Json;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Build();

Console.WriteLine($"bz1-stock-scraper");

var tickersConfigurationSectionFIIs = configuration.GetSection("Tickers:FIIs");
var tickersConfigurationSectionFIInfras = configuration.GetSection("Tickers:FIInfras");
var tickersConfigurationSectionFIAgros = configuration.GetSection("Tickers:FIAgros");

var tickersConfigurationListFIIs = tickersConfigurationSectionFIIs.Get<List<string>>()!;
var tickersConfigurationListFIInfras = tickersConfigurationSectionFIInfras.Get<List<string>>()!;
var tickersConfigurationListFIAgros = tickersConfigurationSectionFIAgros.Get<List<string>>()!;

await new BrowserFetcher().DownloadAsync();

await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = false,
    Args = ["--start-maximized"]
});

await using var page = await browser.NewPageAsync();

await page.SetViewportAsync(new ViewPortOptions
{
    Width = 1280,
    Height = 1024
});

var statusInvestComBrFIIsScrapers = tickersConfigurationListFIIs.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIIs().Build());
var statusInvestComBrFIInfrasScrapers = tickersConfigurationListFIInfras.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIInfras().Build());
var statusInvestComBrFIAgrosScrapers = tickersConfigurationListFIAgros.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIAgros().Build());

var stockScraperBuilders =
    statusInvestComBrFIIsScrapers
    .Union(statusInvestComBrFIInfrasScrapers)
    .Union(statusInvestComBrFIAgrosScrapers);

var tickersData = new Dictionary<string, Dictionary<string, string>>();

foreach (var stockScraperBuilder in stockScraperBuilders)
{
    var currentTicker = stockScraperBuilder.GetTicker();

    Console.WriteLine();
    Console.WriteLine($"ticker:{currentTicker} endpoint:{stockScraperBuilder.GetEndpoint()}");

    await page.GoToAsync(stockScraperBuilder.GetEndpoint());
    await page.WaitForSelectorAsync(stockScraperBuilder.GetWaitForSelector());

    var html = await page.GetContentAsync();
    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(html);

    var tickerData = new Dictionary<string, string>();

    var selectors = stockScraperBuilder.GetSelectors();

    foreach (var selector in selectors)
    {
        string scrapedSelectorValue = "-";

        var singleNode = htmlDocument.DocumentNode.SelectSingleNode(selector.Value);
        if (singleNode != null)
        {
            scrapedSelectorValue = singleNode.InnerText;
        }

        tickerData.Add(selector.Key, scrapedSelectorValue);

        Console.WriteLine($"{selector.Key}:{scrapedSelectorValue}");
    }

    tickersData.Add(currentTicker, tickerData);
}

await page.CloseAsync();
await page.DisposeAsync();

await browser.CloseAsync();
await browser.DisposeAsync();

var tickersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "tickersData.json");
var tickersFileContent = JsonSerializer.Serialize(tickersData);

File.WriteAllText(tickersFilePath, tickersFileContent);

Console.WriteLine();
Console.WriteLine($"scraper done, exiting");
