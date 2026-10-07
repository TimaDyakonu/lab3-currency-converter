namespace CurrencyConverter.Core.Models;

public record Currency
{
    public string Code { get; }
    public string Name { get; }

    public Currency(string code, string name)
    {
        if (string.IsNullOrWhiteSpace(code))
            throw new ArgumentException("Currency code cannot be empty.", nameof(code));

        Code = code.Trim().ToUpperInvariant();
        Name = string.IsNullOrWhiteSpace(name) ? Code : name.Trim();
    }
}
