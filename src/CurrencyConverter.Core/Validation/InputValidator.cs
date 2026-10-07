using System.Globalization;
using CurrencyConverter.Core.Models;

namespace CurrencyConverter.Core.Validation;

public class InputValidator : IInputValidator
{
    public ValidationResult Validate(string? amountText, string? sourceCode, string? targetCode)
    {
        if (string.IsNullOrWhiteSpace(sourceCode))
        {
            return ValidationResult.Failure("Please select a source currency.");
        }

        if (string.IsNullOrWhiteSpace(targetCode))
        {
            return ValidationResult.Failure("Please select a target currency.");
        }

        if (string.IsNullOrWhiteSpace(amountText))
        {
            return ValidationResult.Failure("Please enter an amount.");
        }

        var trimmed = amountText.Trim();

        // Check for multiple separators or both separators
        var commaCount = trimmed.Count(c => c == ',');
        var dotCount = trimmed.Count(c => c == '.');

        if ((commaCount > 0 && dotCount > 0) || commaCount > 1 || dotCount > 1)
        {
            return ValidationResult.Failure("Amount format is invalid. Use a single dot or comma as the decimal separator.");
        }

        var normalized = trimmed.Replace(',', '.');

        // Check if there are illegal characters
        if (!decimal.TryParse(normalized, NumberStyles.AllowDecimalPoint | NumberStyles.AllowLeadingSign, CultureInfo.InvariantCulture, out var amount))
        {
            // Check if it's an overflow
            if (normalized.All(c => char.IsDigit(c) || c == '.' || c == '-'))
            {
                return ValidationResult.Failure("Amount is too large.");
            }

            return ValidationResult.Failure("Amount must be a valid numeric value.");
        }

        if (amount <= 0m)
        {
            return ValidationResult.Failure("Amount must be greater than zero.");
        }

        try
        {
            var request = new ConversionRequest(amount, sourceCode, targetCode);
            return ValidationResult.Success(request);
        }
        catch (Exception ex)
        {
            return ValidationResult.Failure(ex.Message);
        }
    }
}
