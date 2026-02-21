using System.Globalization;
using HtmlAgilityPack;

namespace bz1.stockscraper.Services;

public interface IDataParsingService
{
    Dictionary<string, object> ParseSelectorValues(HtmlDocument document, List<KeyValuePair<string, List<string>>> selectors);
    double? ParseDouble(string value);
}

public class DataParsingService : IDataParsingService
{
    private readonly CultureInfo _culturePtBr = CultureInfo.CreateSpecificCulture("pt-BR");
    private readonly NumberStyles _doubleDecimalStyle = NumberStyles.AllowDecimalPoint | NumberStyles.AllowThousands;

    public Dictionary<string, object> ParseSelectorValues(HtmlDocument document, List<KeyValuePair<string, List<string>>> selectors)
    {
        var result = new Dictionary<string, object>();

        foreach (var selectorPair in selectors)
        {
            var selectorName = selectorPair.Key;
            var selectorXpaths = selectorPair.Value;

            object scrapedValue = string.Empty;

            // Try each selector until one succeeds
            foreach (var xpath in selectorXpaths)
            {
                var node = document.DocumentNode.SelectSingleNode(xpath);
                if (node != null)
                {
                    var rawValue = node.InnerText.Trim();
                    scrapedValue = NormalizeValue(rawValue);

                    if (scrapedValue.ToString() != string.Empty)
                    {
                        break; // Use first successful selector
                    }
                }
            }

            result[selectorName] = scrapedValue;
        }

        return result;
    }

    public double? ParseDouble(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return null;

        var normalized = NormalizeValue(value);
        if (normalized is double d)
            return d;

        return null;
    }

    private object NormalizeValue(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return string.Empty;

        // Remove currency symbols and clean up
        var cleaned = value
            .Replace("R$", "")
            .Replace("$", "")
            .Trim()
            .Replace(".", ","); // Convert . to , for pt-BR parsing

        // Try to parse as double
        if (double.TryParse(cleaned, _doubleDecimalStyle, _culturePtBr, out double result))
        {
            return result;
        }

        // Return original value if not parseable as double
        return value;
    }
}
