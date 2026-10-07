using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Parsing;

public interface IBnmXmlParser
{
    BnmParseResult Parse(string xml);
}
