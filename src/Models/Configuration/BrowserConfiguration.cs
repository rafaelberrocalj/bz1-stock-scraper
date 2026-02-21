namespace bz1.stockscraper.Models.Configuration;

public class BrowserConfiguration
{
    public bool Headless { get; set; } = true;
    public int ViewportWidth { get; set; } = 1280;
    public int ViewportHeight { get; set; } = 1024;
    public List<string> UserAgents { get; set; } = [];
    public List<string> LaunchArgs { get; set; } = ["--no-sandbox", "--disable-setuid-sandbox"];
    public int SelectorTimeoutMs { get; set; } = 30000;
    public int MinDelayMs { get; set; } = 500;
    public int MaxDelayMs { get; set; } = 5000;
}

public class LoggingConfiguration
{
    public LogLevel Level { get; set; } = LogLevel.Information;
    public bool IncludeTimestamp { get; set; } = true;
}

public enum LogLevel
{
    Debug,
    Information,
    Warning,
    Error
}

public class OutputConfiguration
{
    public string FilePath { get; set; } = "tickersData.json";
    public bool Indented { get; set; } = true;
}
