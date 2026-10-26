using System.Globalization;
using System.Text.RegularExpressions;

namespace OzelEgitimHakedis.Wpf;

internal static class TurkishMoneyInput
{
    private static readonly CultureInfo Culture = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly Regex Pattern = new(@"^[+-]?(?:[0-9]+|[0-9]{1,3}(?:\.[0-9]{3})+)(?:,[0-9]+)?$", RegexOptions.CultureInvariant);

    public static bool TryParse(string? text, out decimal value)
    {
        value = 0m;
        var input = text?.Trim();
        return input is not null && Pattern.IsMatch(input)
            && decimal.TryParse(input, NumberStyles.AllowLeadingSign | NumberStyles.AllowThousands | NumberStyles.AllowDecimalPoint, Culture, out value);
    }

    public static string Format(decimal value) => value.ToString("#,##0.00##########################", Culture);

    public static bool TryParseCents(string? text, out decimal value)
        => TryParsePrecision(text, 2, out value);

    public static bool TryParseCalculationAmount(string? text, out decimal value)
        => TryParsePrecision(text, 4, out value);

    private static bool TryParsePrecision(string? text, int fractionalDigits, out decimal value)
    {
        value = 0m;
        if (!TryParse(text, out var parsed)) return false;
        var trimmed = text!.Trim();
        var comma = trimmed.IndexOf(',', StringComparison.Ordinal);
        if (comma >= 0 && trimmed.Length - comma - 1 > fractionalDigits) return false;
        value = parsed;
        return true;
    }
}
