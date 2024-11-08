using bz1.stockscraper.Models.Builders;
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

Console.WriteLine($"BrowserFetcher.DownloadAsync");
await new BrowserFetcher().DownloadAsync();

Console.WriteLine($"Puppeteer.LaunchAsync");
await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true,
    Args = [
        //"--start-maximized",
        "--no-sandbox",
        "--disable-setuid-sandbox"
    ]
});

Console.WriteLine($"browser.NewPageAsync");
await using var page = await browser.NewPageAsync();

Console.WriteLine($"page.SetViewportAsync");
await page.SetViewportAsync(new ViewPortOptions
{
    Width = 1280,
    Height = 1024
});

Console.WriteLine($"page.SetUserAgentAsync");
await page.SetUserAgentAsync("Mozilla/5.0 (Windows NT 10.0; Win64; x64) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/130.0.0.0 Safari/537.36");

var statusInvestComBrFIIsScrapers = tickersConfigurationListFIIs.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIIs().Build());
var statusInvestComBrFIInfrasScrapers = tickersConfigurationListFIInfras.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIInfras().Build());
var statusInvestComBrFIAgrosScrapers = tickersConfigurationListFIAgros.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIAgros().Build());

var stockScraperBuilders =
    statusInvestComBrFIIsScrapers
    .Union(statusInvestComBrFIInfrasScrapers)
    .Union(statusInvestComBrFIAgrosScrapers);

var tickersData = new Dictionary<string, Dictionary<string, object>>();

var emptyScrapedValue = "-";

foreach (var stockScraperBuilder in stockScraperBuilders)
{
    var currentTicker = stockScraperBuilder.GetTicker();

    Console.WriteLine();
    Console.WriteLine($"ticker:{currentTicker} endpoint:{stockScraperBuilder.GetEndpoint()}");

    Console.WriteLine($"page.GoToAsync");
    await page.GoToAsync(stockScraperBuilder.GetEndpoint());
    Console.WriteLine($"page.WaitForSelectorAsync");
    await page.WaitForSelectorAsync(stockScraperBuilder.GetWaitForSelector());

    Console.WriteLine($"page.GetContentAsync");
    var html = await page.GetContentAsync();
    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(html);

    var tickerData = new Dictionary<string, object>();

    var selectors = stockScraperBuilder.GetSelectors();

    foreach (var selector in selectors)
    {
        object scrapedSelectorValue = emptyScrapedValue;

        var htmlDocumentSingleNode = htmlDocument.DocumentNode.SelectSingleNode(selector.Value);
        if (htmlDocumentSingleNode != null)
        {
            if (double.TryParse(htmlDocumentSingleNode.InnerText, out double val))
            {
                scrapedSelectorValue = val;
            }
            else
            {
                scrapedSelectorValue = htmlDocumentSingleNode.InnerText;
            }
        }

        if (!tickerData.ContainsKey(selector.Key) || scrapedSelectorValue.ToString() != emptyScrapedValue)
        {
            tickerData[selector.Key] = scrapedSelectorValue;
        }

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
