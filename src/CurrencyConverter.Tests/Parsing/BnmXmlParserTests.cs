using CurrencyConverter.Core.Parsing;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Tests.Fixtures;

namespace CurrencyConverter.Tests.Parsing;

public class BnmXmlParserTests
{
    private readonly BnmXmlParser _parser = new();

    [Fact]
    public void Parse_ValidXml_ReturnsSuccessWithSnapshot()
    {
        var xml = TestSnapshotBuilder.CreateValidXml(new DateOnly(2026, 10, 7));

        var result = _parser.Parse(xml);

        Assert.True(result.IsSuccess);
        Assert.NotNull(result.Snapshot);
        Assert.Equal(new DateOnly(2026, 10, 7), result.Snapshot.RateDate);
        Assert.Equal(BnmConstants.SourceName, result.Snapshot.SourceName);
        Assert.True(result.Snapshot.HasCurrency("EUR"));
        Assert.True(result.Snapshot.HasCurrency("USD"));
        Assert.True(result.Snapshot.HasCurrency("RUB"));
    }

    [Fact]
    public void Parse_ValidXmlWithoutMdl_AutomaticallyInsertsMdlWithRateOne()
    {
        var xml = TestSnapshotBuilder.CreateValidXml();

        var result = _parser.Parse(xml);

        Assert.True(result.IsSuccess);
        var mdl = result.Snapshot!.GetRate("MDL");
        Assert.NotNull(mdl);
        Assert.Equal(1m, mdl.Nominal);
        Assert.Equal(1m, mdl.Value);
        Assert.Equal(1m, mdl.RatePerMdlUnit);
    }

    [Fact]
    public void Parse_NonUnitNominal_NormalizesValueDividedByNominal()
    {
        var xml = TestSnapshotBuilder.CreateValidXml();

        var result = _parser.Parse(xml);

        Assert.True(result.IsSuccess);
        var rub = result.Snapshot!.GetRate("RUB");
        Assert.NotNull(rub);
        Assert.Equal(100m, rub.Nominal);
        Assert.Equal(19.2000m, rub.Value);
        Assert.Equal(0.192000m, rub.RatePerMdlUnit);
    }

    [Fact]
    public void Parse_DecimalParsing_UsesInvariantCulture()
    {
        var xml = @"<ValCurs Date=""07.10.2026"">
            <Valute ID=""1""><CharCode>USD</CharCode><Nominal>1</Nominal><Value>17.8000</Value></Valute>
        </ValCurs>";

        var result = _parser.Parse(xml);

        Assert.True(result.IsSuccess);
        var usd = result.Snapshot!.GetRate("USD");
        Assert.NotNull(usd);
        Assert.Equal(17.8000m, usd.Value);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Parse_EmptyOrWhitespaceXml_ReturnsFailure(string? xml)
    {
        var result = _parser.Parse(xml!);

        Assert.False(result.IsSuccess);
        Assert.NotNull(result.ErrorMessage);
    }

    [Fact]
    public void Parse_MalformedXml_ReturnsFailureWithoutThrowing()
    {
        var xml = "<ValCurs Date=\"07.10.2026\"><Valute><CharCode>USD</Valute>";

        var result = _parser.Parse(xml);

        Assert.False(result.IsSuccess);
        Assert.Contains("Malformed XML", result.ErrorMessage);
    }

    [Fact]
    public void Parse_MissingDateAttribute_ReturnsFailure()
    {
        var xml = @"<ValCurs name=""Official exchange rates"">
            <Valute ID=""1""><CharCode>USD</CharCode><Nominal>1</Nominal><Value>17.80</Value></Valute>
        </ValCurs>";

        var result = _parser.Parse(xml);

        Assert.False(result.IsSuccess);
        Assert.Contains("Missing 'Date' attribute", result.ErrorMessage);
    }

    [Fact]
    public void Parse_MissingFields_ReturnsFailure()
    {
        var xml = @"<ValCurs Date=""07.10.2026"">
            <Valute ID=""1""><Nominal>1</Nominal><Value>17.80</Value></Valute>
        </ValCurs>";

        var result = _parser.Parse(xml);

        Assert.False(result.IsSuccess);
        Assert.Contains("Missing or empty <CharCode>", result.ErrorMessage);
    }

    [Fact]
    public void Parse_DuplicateCurrencyCodes_ReturnsFailure()
    {
        var xml = @"<ValCurs Date=""07.10.2026"">
            <Valute ID=""1""><CharCode>USD</CharCode><Nominal>1</Nominal><Value>17.80</Value></Valute>
            <Valute ID=""2""><CharCode>USD</CharCode><Nominal>1</Nominal><Value>17.85</Value></Valute>
        </ValCurs>";

        var result = _parser.Parse(xml);

        Assert.False(result.IsSuccess);
        Assert.Contains("Duplicate currency code", result.ErrorMessage);
    }

    [Theory]
    [InlineData("0", "17.80")]
    [InlineData("-1", "17.80")]
    [InlineData("1", "0")]
    [InlineData("1", "-17.80")]
    public void Parse_NonPositiveNominalOrValue_ReturnsFailure(string nominal, string value)
    {
        var xml = $@"<ValCurs Date=""07.10.2026"">
            <Valute ID=""1""><CharCode>USD</CharCode><Nominal>{nominal}</Nominal><Value>{value}</Value></Valute>
        </ValCurs>";

        var result = _parser.Parse(xml);

        Assert.False(result.IsSuccess);
        Assert.Contains("non-positive", result.ErrorMessage);
    }
}
