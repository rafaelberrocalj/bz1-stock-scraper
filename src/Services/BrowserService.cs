using bz1.stockscraper.Models.Configuration;
using PuppeteerSharp;
using PuppeteerExtraSharp;
using PuppeteerExtraSharp.Plugins.ExtraStealth;

namespace bz1.stockscraper.Services;

public class BrowserService : IBrowserService
{
    private readonly BrowserConfiguration _config;
    private readonly ILogger _logger;
    private IBrowser? _browser;
    private IPage? _page;
    private Random? _random;

    public BrowserService(BrowserConfiguration config, ILogger logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task InitializeAsync()
    {
        _logger.LogDebug("Initializing browser...");

        _random = new Random();

        await new BrowserFetcher().DownloadAsync();

        var puppeteerExtra = new PuppeteerExtra();
        puppeteerExtra.Use(new StealthPlugin());

        var launchOpts = new LaunchOptions
        {
            Headless = _config.Headless,
            Args = _config.LaunchArgs.ToArray(),
            DefaultViewport = new ViewPortOptions
            {
                Width = _config.ViewportWidth,
                Height = _config.ViewportHeight
            }
        };

        _browser = await puppeteerExtra.LaunchAsync(launchOpts);
        _page = (await _browser.PagesAsync()).Single();

        SetRandomUserAgent();
        _logger.LogInfo("Browser initialized successfully");
    }

    public async Task GoToPageAsync(string url)
    {
        if (_page == null) throw new InvalidOperationException("Browser not initialized");

        _logger.LogDebug($"Navigating to {url}");
        await _page.GoToAsync(url, new NavigationOptions
        {
            WaitUntil = [WaitUntilNavigation.DOMContentLoaded]
        });
    }

    public async Task WaitForSelectorAsync(string selector)
    {
        if (_page == null) throw new InvalidOperationException("Browser not initialized");

        _logger.LogDebug($"Waiting for selector: {selector}");
        try
        {
            await _page.WaitForSelectorAsync(selector, new WaitForSelectorOptions
            {
                Timeout = _config.SelectorTimeoutMs
            });
        }
        catch (WaitTaskTimeoutException ex)
        {
            _logger.LogError($"Timeout waiting for selector '{selector}' after {_config.SelectorTimeoutMs}ms", ex);
            throw;
        }
    }

    public async Task<string> GetPageContentAsync()
    {
        if (_page == null) throw new InvalidOperationException("Browser not initialized");
        return await _page.GetContentAsync();
    }

    public async Task DelayAsync()
    {
        if (_random == null) throw new InvalidOperationException("Browser not initialized");
        var delay = _random.Next(_config.MinDelayMs, _config.MaxDelayMs);
        await Task.Delay(delay);
    }

    private void SetRandomUserAgent()
    {
        if (_page == null || _random == null) return;
        if (_config.UserAgents.Count == 0) return;

        var userAgent = _config.UserAgents[_random.Next(_config.UserAgents.Count)];
        _page.SetUserAgentAsync(userAgent).GetAwaiter().GetResult();
        _logger.LogDebug($"User agent set: {userAgent.Substring(0, Math.Min(50, userAgent.Length))}...");
    }

    async ValueTask IAsyncDisposable.DisposeAsync()
    {
        if (_page != null)
        {
            await _page.CloseAsync();
            await _page.DisposeAsync();
        }

        if (_browser != null)
        {
            await _browser.CloseAsync();
            await _browser.DisposeAsync();
        }

        _logger.LogDebug("Browser disposed");
    }
}
