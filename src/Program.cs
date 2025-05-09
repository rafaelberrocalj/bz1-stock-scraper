using bz1.stockscraper.Models.Scrapers;
using HtmlAgilityPack;
using Microsoft.Extensions.Configuration;
using PuppeteerSharp;
using System.Globalization;
using System.Reflection;
using System.Text.Json;

var builder = new ConfigurationBuilder()
    .SetBasePath(Directory.GetCurrentDirectory())
    .AddJsonFile("appsettings.json")
    .AddEnvironmentVariables()
    .AddUserSecrets(Assembly.GetExecutingAssembly());
var configuration = builder.Build();

Console.WriteLine($"bz1-stock-scraper");

var culturePtBr = CultureInfo.CreateSpecificCulture("pt-BR");
var doubleDecimalStylePtBr = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

var tickersConfigurationSectionFIIs = configuration.GetSection("Tickers:FIIs");
var tickersConfigurationSectionFIInfras = configuration.GetSection("Tickers:FIInfras");
var tickersConfigurationSectionFIAgros = configuration.GetSection("Tickers:FIAgros");
var tickersConfigurationSectionETFEUA = configuration.GetSection("Tickers:ETFEUA");

var tickersConfigurationListFIIs = tickersConfigurationSectionFIIs.Get<List<string>>()!;
var tickersConfigurationListFIInfras = tickersConfigurationSectionFIInfras.Get<List<string>>()!;
var tickersConfigurationListFIAgros = tickersConfigurationSectionFIAgros.Get<List<string>>()!;
var tickersConfigurationListETFEUA = tickersConfigurationSectionETFEUA.Get<List<string>>()!;

var scraperFIIs = tickersConfigurationListFIIs.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIIs().Build());
var scraperFIInfras = tickersConfigurationListFIInfras.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIInfras().Build());
var scraperFIAgros = tickersConfigurationListFIAgros.Select(ticker => new StatusInvestComBrScraper().WithTicker(ticker).WithFIAgros().Build());
var scraperETFEUA = tickersConfigurationListETFEUA.Select(ticker => new StockAnalysisScraper().WithTicker(ticker).WithETF().Build());

var stockScraperBuilders =
    scraperFIIs
    .Union(scraperFIInfras)
    .Union(scraperFIAgros)
    .Union(scraperETFEUA);

await new BrowserFetcher().DownloadAsync();
var executablePath = configuration["PUPPETEER_EXECUTABLE_PATH"];
Console.WriteLine();
Console.WriteLine($"ENV:PUPPETEER_EXECUTABLE_PATH:{executablePath}");

await using var browser = await Puppeteer.LaunchAsync(new LaunchOptions
{
    Headless = true,
    Args = ["--no-sandbox", "--disable-setuid-sandbox"],
    //ExecutablePath = executablePath,
    //ExecutablePath = "/Applications/Google Chrome.app/Contents/MacOS/Google Chrome",
    //UserDataDir = "~/Library/Application Support/Google/Chrome/Default",
    DefaultViewport = new ViewPortOptions
    {
        Width = 1280,
        Height = 1024
    }
});

var page = (await browser.PagesAsync()).Single();

await page.SetUserAgentAsync("Mozilla/5.0 (Macintosh; Intel Mac OS X 10_15_7) AppleWebKit/537.36 (KHTML, like Gecko) Chrome/135.0.0.0 Safari/537.36");

var tickersData = new Dictionary<string, Dictionary<string, object>>();

var random = new Random();

foreach (var stockScraperBuilder in stockScraperBuilders)
{
    var currentTicker = stockScraperBuilder.GetTicker();

    Console.WriteLine();
    Console.WriteLine($"ticker:{currentTicker} endpoint:{stockScraperBuilder.GetEndpoint()}");

    try
    {
        await page.GoToAsync(stockScraperBuilder.GetEndpoint().ToLower());
        //await Task.Delay(random.Next(1000, 2000));


        await page.WaitForSelectorAsync(stockScraperBuilder.GetWaitForSelector());
        //await Task.Delay(random.Next(1000, 2000));

    }
    catch (Exception exception)
    {
        Console.Error.WriteLine($"Error navigating to {stockScraperBuilder.GetEndpoint()}: {exception.Message}");

        var rawHtml = await page.GetContentAsync();
        if (rawHtml.Contains("cloudflare.com"))
        {

            await page.EvaluateExpressionAsync("document.querySelector('input[name=cf-turnstile-response]').click();");
            //await page.ClickAsync("input[name=cf-turnstile-response]");

            await page.Mouse.MoveAsync(100, 100);
            await page.Mouse.DownAsync();
            await page.Mouse.MoveAsync(200, 200);
            await page.Mouse.UpAsync();

            await Task.Delay(random.Next(5000, 9000));
            Console.WriteLine($"html:{await page.GetContentAsync()}");

            var inputs = await page.QuerySelectorAllAsync("input");
            foreach (var input in inputs)
            {

                var name = await input.GetPropertyAsync("name");

                Console.WriteLine($"input:{name}");

            }



        }

        break;
    }

    var pageHtml = await page.GetContentAsync();
    var htmlDocument = new HtmlDocument();
    htmlDocument.LoadHtml(pageHtml);

    var tickerData = new Dictionary<string, object>();

    var selectors = stockScraperBuilder.GetSelectors();

    foreach (var selector in selectors)
    {
        object scrapedSelectorValue = string.Empty;

        var htmlDocumentSingleNode = htmlDocument.DocumentNode.SelectSingleNode(selector.Value);
        if (htmlDocumentSingleNode != null)
        {
            var actualValue = htmlDocumentSingleNode.InnerText
                .Trim()
                .Replace("R$", "")
                .Replace("$", "")
                .Replace(".", ",");

            if (double.TryParse(actualValue, doubleDecimalStylePtBr, culturePtBr, out double val))
            {
                scrapedSelectorValue = val;
            }
            else
            {
                scrapedSelectorValue = actualValue;
            }
        }

        if (!tickerData.ContainsKey(selector.Key) || scrapedSelectorValue.ToString() != string.Empty)
        {
            tickerData[selector.Key] = scrapedSelectorValue;
        }

        Console.WriteLine($"{selector.Key}:{scrapedSelectorValue}");
    }

    tickersData.Add(currentTicker, tickerData);
}

Console.WriteLine();

await page.GoToAsync("https://wise.com/br/currency-converter/dolar-hoje");
var htmlDolar = await page.GetContentAsync();
var htmlDocumentDolar = new HtmlDocument();
htmlDocumentDolar.LoadHtml(htmlDolar);
var dolarString = htmlDocumentDolar.GetElementbyId("target-input").GetAttributeValue("value", string.Empty);
double.TryParse(dolarString, doubleDecimalStylePtBr, culturePtBr, out double dolarValue);
tickersData["DOLAR"] = new Dictionary<string, object>
{
    ["valueBR"] = dolarValue,
    ["date"] = DateTime.UtcNow.Date.ToString("yyy/MM/dd")
};
Console.WriteLine($"DOLAR:valueBR:{dolarValue}");

await page.CloseAsync();
await page.DisposeAsync();

await browser.CloseAsync();
await browser.DisposeAsync();

var tickersFilePath = Path.Combine(Directory.GetCurrentDirectory(), "tickersData.json");
var tickersFileContent = JsonSerializer.Serialize(tickersData, new JsonSerializerOptions
{
    WriteIndented = true
});

File.WriteAllText(tickersFilePath, tickersFileContent);

Console.WriteLine();
Console.WriteLine($"scraper done, exiting");
