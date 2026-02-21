namespace bz1.stockscraper.Services;

public interface IBrowserService : IAsyncDisposable
{
    Task InitializeAsync();
    Task GoToPageAsync(string url);
    Task WaitForSelectorAsync(string selector);
    Task<string> GetPageContentAsync();
    Task DelayAsync();
}
