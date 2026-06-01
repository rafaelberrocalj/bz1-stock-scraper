using bz1.stockscraper.Models.Data;
using System.Text.Json;

namespace bz1.stockscraper.Services;

public interface IFileService
{
    Task SaveResultsAsync(ScrapingResult result, string outputPath);
    Task<ScrapingResult> LoadResultsAsync(string inputPath);
}

public class FileService : IFileService
{
    private readonly ILogger _logger;

    public FileService(ILogger logger)
    {
        _logger = logger;
    }

    public async Task SaveResultsAsync(ScrapingResult result, string outputPath)
    {
        _logger.LogInfo($"Saving results to {outputPath}");

        try
        {
            var outputFormat = result.ToOutputFormat();
            var json = JsonSerializer.Serialize(outputFormat, new JsonSerializerOptions
            {
                WriteIndented = true
            });

            await File.WriteAllTextAsync(outputPath, json);
            _logger.LogInfo("Results saved successfully");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to save results to {outputPath}", ex);
            throw;
        }
    }

    public async Task<ScrapingResult> LoadResultsAsync(string inputPath)
    {
        _logger.LogDebug($"Loading results from {inputPath}");

        try
        {
            var json = await File.ReadAllTextAsync(inputPath);
            var data = JsonSerializer.Deserialize<Dictionary<string, Dictionary<string, object>>>(json)
                ?? throw new InvalidOperationException("Failed to deserialize JSON");

            var result = new ScrapingResult();

            foreach (var kvp in data)
            {
                result.Tickers[kvp.Key] = new TickerData { Values = kvp.Value };
            }

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError($"Failed to load results from {inputPath}", ex);
            throw;
        }
    }
}
