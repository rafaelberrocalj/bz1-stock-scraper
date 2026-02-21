using bz1.stockscraper.Models.Configuration;

namespace bz1.stockscraper.Services;

public class LoggingService : ILogger
{
    private readonly LoggingConfiguration _config;

    public LoggingService(LoggingConfiguration config)
    {
        _config = config;
    }

    public void LogInfo(string message) => Log(LogLevel.Information, message);
    public void LogError(string message, Exception? ex = null) => Log(LogLevel.Error, message, ex);
    public void LogDebug(string message) => Log(LogLevel.Debug, message);
    public void LogWarning(string message) => Log(LogLevel.Warning, message);

    private void Log(LogLevel level, string message, Exception? ex = null)
    {
        if (ShouldLog(level))
        {
            var prefix = _config.IncludeTimestamp ? $"[{DateTime.Now:HH:mm:ss}] " : "[";
            var levelStr = level.ToString().ToUpper();

            Console.WriteLine($"{prefix}[{levelStr}] {message}");

            if (ex != null && level == LogLevel.Error)
            {
                Console.WriteLine($"Exception: {ex.Message}");
                if (!string.IsNullOrEmpty(ex.StackTrace))
                {
                    Console.WriteLine($"StackTrace: {ex.StackTrace}");
                }
            }
        }
    }

    private bool ShouldLog(LogLevel level) => level >= _config.Level;
}
