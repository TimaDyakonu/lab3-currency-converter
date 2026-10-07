using CurrencyConverter.Core.Models;
using CurrencyConverter.Core.Services;
using CurrencyConverter.Core.Validation;

namespace CurrencyConverter.Tests.Validation;

public class InputValidationEdgeCaseTests
{
    private readonly InputValidator _validator = new();

    [Theory]
    [InlineData("")]
    [InlineData("   ")]
    [InlineData(null)]
    public void Validate_EmptyAmount_ReturnsClearErrorWithoutThrowing(string? emptyInput)
    {
        var result = _validator.Validate(emptyInput, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Null(result.Request);
        Assert.Equal("Please enter an amount.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("abc")]
    [InlineData("twelve")]
    [InlineData("12.34xyz")]
    public void Validate_AlphabeticInput_ReturnsClearErrorWithoutThrowing(string textInput)
    {
        var result = _validator.Validate(textInput, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Null(result.Request);
        Assert.Equal("Amount must be a valid numeric value.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("0")]
    [InlineData("0.0")]
    [InlineData("0,00")]
    [InlineData("-0")]
    public void Validate_ZeroAmount_ReturnsClearErrorWithoutThrowing(string zeroInput)
    {
        var result = _validator.Validate(zeroInput, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Null(result.Request);
        Assert.Equal("Amount must be greater than zero.", result.ErrorMessage);
    }

    [Theory]
    [InlineData("-1")]
    [InlineData("-12.50")]
    [InlineData("-0.01")]
    [InlineData("-100,5")]
    public void Validate_NegativeAmount_ReturnsClearErrorWithoutThrowing(string negativeInput)
    {
        var result = _validator.Validate(negativeInput, "USD", "MDL");

        Assert.False(result.IsValid);
        Assert.Null(result.Request);
        Assert.Equal("Amount must be greater than zero.", result.ErrorMessage);
    }
}
