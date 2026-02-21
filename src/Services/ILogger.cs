namespace bz1.stockscraper.Services;

public interface ILogger
{
    void LogInfo(string message);
    void LogError(string message, Exception? ex = null);
    void LogDebug(string message);
    void LogWarning(string message);
}
