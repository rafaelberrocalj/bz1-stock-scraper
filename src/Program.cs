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

bool.TryParse(configuration["BROWSER_HEADLESS"] ?? "true", out bool BROWSER_HEADLESS);

Console.WriteLine($"bz1-stock-scraper");

var tickersConfigurationSectionFIIs = configuration.GetSection("Tickers:FIIs");
var tickersConfigurationSectionFIInfras = configuration.GetSection("Tickers:FIInfras");
var tickersConfigurationSectionFIAgros = configuration.GetSection("Tickers:FIAgros");

var tickersConfigurationListFIIs = tickersConfigurationSectionFIIs.Get<List<string>>()!;
var tickersConfigurationListFIInfras = tickersConfigurationSectionFIInfras.Get<List<string>>()!;
var tickersConfigurationListFIAgros = tickersConfigurationSectionFIAgros.Get<List<string>>()!;

try
{
    Console.WriteLine($"BrowserFetcher.DownloadAsync");
    await new BrowserFetcher().DownloadAsync();
}
catch (Exception ex)
{
    Console.WriteLine($"error:{ex.Message}");
}

Console.WriteLine($"Puppeteer.LaunchAsync");
await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = BROWSER_HEADLESS,
    Args = [
        "--no-sandbox",
        "--disable-setuid-sandbox",
        "--disable-dev-shm-usage",
        "--disable-gpu"
    ]
});

Console.WriteLine($"browser.NewPageAsync");
await using var page = await browser.NewPageAsync();

//Console.WriteLine($"browser.PagesAsync");
//var page = (await browser.PagesAsync())[0];

Console.WriteLine($"page.SetViewportAsync");
await page.SetViewportAsync(new ViewPortOptions
{
    Width = 1920,
    Height = 768
});

Console.WriteLine($"page.SetUserAgentAsync");
await page.SetUserAgentAsync("Mozilla/5.0 (iPhone; CPU iPhone OS 17_7_2 like Mac OS X) AppleWebKit/605.1.15 (KHTML, like Gecko) Version/17.4.1 Mobile/15E148 Safari/604.1");

Console.WriteLine($"page.GoToAsync=init");
await page.GoToAsync("https://bot.sannysoft.com/", null, [WaitUntilNavigation.Networkidle0, WaitUntilNavigation.Networkidle2]);

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
    var pageResponse = await page.GoToAsync(stockScraperBuilder.GetEndpoint(), null, [WaitUntilNavigation.Networkidle0, WaitUntilNavigation.Networkidle2]);
    Console.WriteLine($"page.WaitForSelectorAsync");
    //await page.WaitForSelectorAsync(stockScraperBuilder.GetWaitForSelector());

    Console.WriteLine($"pageResponse={pageResponse.Ok}");

    Console.WriteLine($"page.GetContentAsync");
    var html = await page.GetContentAsync();
    Console.WriteLine($"html=${html}");

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

var jsonSerializerOptions = new JsonSerializerOptions
{
    WriteIndented = true
};

var tickersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "tickersData.json");
var tickersFileContent = JsonSerializer.Serialize(tickersData, jsonSerializerOptions);

File.WriteAllText(tickersFilePath, tickersFileContent);

Console.WriteLine($"tickersData.json={tickersFileContent}");

Console.WriteLine();
Console.WriteLine($"scraper done, exiting");
