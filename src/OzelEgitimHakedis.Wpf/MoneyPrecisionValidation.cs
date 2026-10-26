using System;
using System.Globalization;
using System.Windows.Controls;
using System.Windows.Data;

namespace OzelEgitimHakedis.Wpf;

internal sealed class MoneyPrecisionValidation : ValidationRule
{
    public override ValidationResult Validate(object value, CultureInfo cultureInfo) => ValidationResult.ValidResult;

    public override ValidationResult Validate(object value, CultureInfo cultureInfo, BindingExpressionBase owner)
    {
        if (value is not string text || !TurkishMoneyInput.TryParse(text, out var amount))
            return new ValidationResult(false, "Geçerli bir TL tutarı girin.");

        var normalized = text.Trim();
        var comma = normalized.IndexOf(',', StringComparison.Ordinal);
        if (comma < 0 || normalized.Length - comma - 1 <= 4)
            return ValidationResult.ValidResult;

        if (owner is BindingExpression binding && binding.ResolvedSource is { } source &&
            source.GetType().GetProperty(binding.ResolvedSourcePropertyName)?.GetValue(source) is decimal original &&
            decimal.Round(original, 4) != original && original == amount)
            return ValidationResult.ValidResult;

        return new ValidationResult(false, "Hesap tutarlarında en fazla dört ondalık basamak kullanın.");
    }
}
