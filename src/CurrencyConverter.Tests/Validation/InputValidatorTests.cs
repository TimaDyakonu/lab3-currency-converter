using CurrencyConverter.Core.Validation;

namespace CurrencyConverter.Tests.Validation;

public class InputValidatorTests
{
    private readonly InputValidator _validator = new();

    [Fact]
    public void Validate_ValidDotSeparator_ReturnsSuccess()
    {
        var result = _validator.Validate("12.50", "USD", "MDL");

        Assert.True(result.IsValid);
        Assert.NotNull(result.Request);
        Assert.Equal(12.50m, result.Request.Amount);
        Assert.Equal("USD", result.Request.SourceCurrency);
        Assert.Equal("MDL", result.Request.TargetCurrency);
    }

    [Fact]
    public void Validate_ValidCommaSeparator_ReturnsSuccess()
    {
        var result = _validator.Validate("12,50", "USD", "MDL");

        Assert.True(result.IsValid);
        Assert.NotNull(result.Request);
        Assert.Equal(12.50m, result.Request.Amount);
    }

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyAmount_ReturnsFailure(string? amount)
    {
        var result = _validator.Validate(amount, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("12a")]
    [InlineData("$$")]
    public void Validate_AlphabeticOrInvalidCharacters_ReturnsFailure(string amount)
    {
        var result = _validator.Validate(amount, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.NotNull(result.ErrorMessage);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.0")]
    [InlineData("0,00")]
    public void Validate_ZeroAmount_ReturnsFailure(string amount)
    {
        var result = _validator.Validate(amount, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-12.50")]
    [InlineData("-0.01")]
    public void Validate_NegativeAmount_ReturnsFailure(string amount)
    {
        var result = _validator.Validate(amount, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Contains("greater than zero", result.ErrorMessage);
    }

    [Fact]
    public void Validate_MultipleSeparators_ReturnsFailure()
    {
        var result1 = _validator.Validate("12.3.4", "USD", "MDL");
        var result2 = _validator.Validate("12,3,4", "USD", "MDL");
        var result3 = _validator.Validate("12.3,4", "USD", "MDL");

        Assert.False(result1.IsValid);
        Assert.False(result2.IsValid);
        Assert.False(result3.IsValid);
    }

    [Fact]
    public void Validate_Overflow_ReturnsFailure()
    {
        var hugeNumber = "99999999999999999999999999999999999999999";
        var result = _validator.Validate(hugeNumber, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Contains("too large", result.ErrorMessage);
    }

    [Theory]
    [InlineData(null, "MDL")]
    [InlineData("", "MDL")]
    [InlineData("   ", "MDL")]
    public void Validate_MissingSourceCurrency_ReturnsFailure(string? source, string target)
    {
        var result = _validator.Validate("100", source, target);

        Assert.False(result.IsValid);
        Assert.Contains("source currency", result.ErrorMessage);
    }

    [Theory]
    [InlineData("USD", null)]
    [InlineData("USD", "")]
    [InlineData("USD", "   ")]
    public void Validate_MissingTargetCurrency_ReturnsFailure(string source, string? target)
    {
        var result = _validator.Validate("100", source, target);

        Assert.False(result.IsValid);
        Assert.Contains("target currency", result.ErrorMessage);
    }
}
