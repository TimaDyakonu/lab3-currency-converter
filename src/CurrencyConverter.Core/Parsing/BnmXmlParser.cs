using System.Globalization;
using System.Xml;
using System.Xml.Linq;
using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;

namespace CurrencyConverter.Core.Parsing;

public class BnmXmlParser : IBnmXmlParser
{
    public BnmParseResult Parse(string xml)
    {
        if (string.IsNullOrWhiteSpace(xml))
        {
            return BnmParseResult.Failure("XML content is empty or whitespace.");
        }

        XDocument doc;
        try
        {
            doc = XDocument.Parse(xml);
        }
        catch (XmlException ex)
        {
            return BnmParseResult.Failure($"Malformed XML: {ex.Message}");
        }

        var root = doc.Root;
        if (root == null || !string.Equals(root.Name.LocalName, "ValCurs", StringComparison.OrdinalIgnoreCase))
        {
            return BnmParseResult.Failure("Root element must be <ValCurs>.");
        }

        var dateAttr = root.Attribute("Date");
        if (dateAttr == null || string.IsNullOrWhiteSpace(dateAttr.Value))
        {
            return BnmParseResult.Failure("Missing 'Date' attribute on <ValCurs>.");
        }

        if (!DateOnly.TryParseExact(
                dateAttr.Value.Trim(),
                BnmConstants.DateFormat,
                CultureInfo.InvariantCulture,
                DateTimeStyles.None,
                out var rateDate))
        {
            return BnmParseResult.Failure($"Invalid date format in 'Date' attribute: '{dateAttr.Value}'. Expected '{BnmConstants.DateFormat}'.");
        }

        var valuteElements = root.Elements().Where(e => string.Equals(e.Name.LocalName, "Valute", StringComparison.OrdinalIgnoreCase)).ToList();
        if (valuteElements.Count == 0)
        {
            return BnmParseResult.Failure("XML contains no <Valute> elements.");
        }

        var rates = new List<ExchangeRate>();
        var seenCodes = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

        foreach (var valute in valuteElements)
        {
            var charCodeElem = valute.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "CharCode", StringComparison.OrdinalIgnoreCase));
            if (charCodeElem == null || string.IsNullOrWhiteSpace(charCodeElem.Value))
            {
                return BnmParseResult.Failure("Missing or empty <CharCode> in <Valute>.");
            }

            var charCode = charCodeElem.Value.Trim().ToUpperInvariant();
            if (seenCodes.Contains(charCode))
            {
                return BnmParseResult.Failure($"Duplicate currency code found in XML: '{charCode}'.");
            }
            seenCodes.Add(charCode);

            var nameElem = valute.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "Name", StringComparison.OrdinalIgnoreCase));
            var name = nameElem?.Value?.Trim() ?? charCode;

            var nominalElem = valute.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "Nominal", StringComparison.OrdinalIgnoreCase));
            if (nominalElem == null || string.IsNullOrWhiteSpace(nominalElem.Value))
            {
                return BnmParseResult.Failure($"Missing or empty <Nominal> for currency '{charCode}'.");
            }

            if (!decimal.TryParse(nominalElem.Value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var nominal) || nominal <= 0m)
            {
                return BnmParseResult.Failure($"Invalid or non-positive <Nominal> for currency '{charCode}': '{nominalElem.Value}'.");
            }

            var valueElem = valute.Elements().FirstOrDefault(e => string.Equals(e.Name.LocalName, "Value", StringComparison.OrdinalIgnoreCase));
            if (valueElem == null || string.IsNullOrWhiteSpace(valueElem.Value))
            {
                return BnmParseResult.Failure($"Missing or empty <Value> for currency '{charCode}'.");
            }

            if (!decimal.TryParse(valueElem.Value.Trim(), NumberStyles.Number, CultureInfo.InvariantCulture, out var rateValue) || rateValue <= 0m)
            {
                return BnmParseResult.Failure($"Invalid or non-positive <Value> for currency '{charCode}': '{valueElem.Value}'.");
            }

            rates.Add(new ExchangeRate(charCode, name, nominal, rateValue));
        }

        var sourceName = BnmConstants.SourceName;

        try
        {
            var snapshot = new RateSnapshot(rateDate, sourceName, rates);
            return BnmParseResult.Success(snapshot);
        }
        catch (Exception ex)
        {
            return BnmParseResult.Failure($"Failed to construct rate snapshot: {ex.Message}");
        }
    }
}
