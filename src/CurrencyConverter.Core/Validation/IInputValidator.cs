using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Validation;

public interface IInputValidator
{
    ValidationResult Validate(string? amountText, string? sourceCode, string? targetCode);
}
