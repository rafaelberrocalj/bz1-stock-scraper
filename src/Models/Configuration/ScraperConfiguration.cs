namespace bz1.stockscraper.Models.Configuration;

public class ScraperConfiguration
{
    public StatusInvestConfiguration StatusInvest { get; set; } = new();
    public StockAnalysisConfiguration StockAnalysis { get; set; } = new();
    public ExchangeRateConfiguration ExchangeRate { get; set; } = new();
}

public class StatusInvestConfiguration
{
    public string BaseEndpoint { get; set; } = "https://statusinvest.com.br/";
    public string WaitForSelector { get; set; } = "";
    public Dictionary<string, SelectorSet> SelectorSets { get; set; } = [];
}

public class StockAnalysisConfiguration
{
    public string BaseEndpoint { get; set; } = "https://stockanalysis.com/";
    public string WaitForSelector { get; set; } = "";
    public Dictionary<string, SelectorSet> SelectorSets { get; set; } = [];
}

public class SelectorSet
{
    public string Name { get; set; } = "";
    public string EndpointPath { get; set; } = "";
    public Dictionary<string, List<string>> Selectors { get; set; } = [];
}

public class ExchangeRateConfiguration
{
    public string Endpoint { get; set; } = "https://wise.com/br/currency-converter/dolar-hoje";
    public string ElementId { get; set; } = "target-input";
    public string AttributeName { get; set; } = "value";
}
