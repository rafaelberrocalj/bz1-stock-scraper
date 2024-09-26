using bz1.stockscraper.Models.Builders;
using bz1.stockscraper.Models.Scrapers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using System.Reflection;

var builder = new ConfigurationBuilder()
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Build();

Console.WriteLine($"bz1-stock-scraper");

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

var stockScraperBuilders = new IStockScraperBuilder[]
{
    new Investidor10ComBrScraper().WithFii("BARI11").Build(),
    new MundoFiiComScraper().WithFii("BARI11").Build()
};

foreach (var stockScraperBuilder in stockScraperBuilders)
{
    await page.GoToAsync(stockScraperBuilder.GetEndpoint());
    await page.WaitForSelectorAsync(stockScraperBuilder.GetWaitForSelector());

    var html = await page.GetContentAsync();
    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(html);

    var selectors = stockScraperBuilder.GetSelectors();

    foreach (var selector in selectors)
    {
        var scrapedSelectorValue = htmlDocument.DocumentNode.SelectSingleNode(selector.Value).InnerText;

        Console.WriteLine($"endpoint:{stockScraperBuilder.GetEndpoint()} {selector.Key}:{scrapedSelectorValue}");
    }
}

await page.CloseAsync();
await page.DisposeAsync();

await browser.CloseAsync();
await browser.DisposeAsync();
