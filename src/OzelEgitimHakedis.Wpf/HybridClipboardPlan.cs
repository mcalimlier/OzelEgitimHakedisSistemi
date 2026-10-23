using System.Globalization;
using System.Text.RegularExpressions;

namespace OzelEgitimHakedis.Wpf;

/// <summary>
/// Pure, fail-closed plan for Excel TSV numeric paste. A plan never changes a
/// payment row; the UI applies it only after every pasted cell passes validation.
/// </summary>
internal static class HybridClipboardPlan
{
    internal sealed record Cell(int RowIndex, string FieldName, decimal Value);

    private static readonly HashSet<string> EditableFields = new(StringComparer.Ordinal)
    {
        "DailyPrice", "GuideDailyPrice", "WorkDays", "DiscountAmount", "PriceDifference",
        "GuideWorkDays", "GuideDiscountAmount", "GuidePriceDifference",
        "TaxDebt", "SocialSecurityDebt", "MinimumWageSupport", "InspectionPenalty"
    };
    private static readonly HashSet<string> NonNegativeFields = new(StringComparer.Ordinal)
    {
        "DailyPrice", "GuideDailyPrice", "WorkDays", "DiscountAmount", "GuideWorkDays", "GuideDiscountAmount",
        "TaxDebt", "SocialSecurityDebt", "MinimumWageSupport", "InspectionPenalty"
    };
    private static readonly Regex TurkishNumber = new(
        @"^[+-]?(?:\d+|\d{1,3}(?:\.\d{3})+)(?:,\d+)?$",
        RegexOptions.CultureInvariant | RegexOptions.Compiled);
    private static readonly CultureInfo TurkishCulture = CultureInfo.GetCultureInfo("tr-TR");

    public static bool TryCreate(
        string clipboardText,
        IReadOnlyList<string> visibleColumns,
        int firstRow, int firstColumn, int rowCount,
        Func<int, string, bool> isEditable,
        out IReadOnlyList<Cell> planned, out string error)
    {
        planned = Array.Empty<Cell>();
        error = string.Empty;
        ArgumentNullException.ThrowIfNull(visibleColumns);
        ArgumentNullException.ThrowIfNull(isEditable);
        if (string.IsNullOrWhiteSpace(clipboardText))
        {
            error = "Excel'den kopyalanmış sayısal hücre bulunamadı.";
            return false;
        }

        if (firstRow < 0 || firstColumn < 0 || firstRow >= rowCount || firstColumn >= visibleColumns.Count)
        {
            error = "Yapıştırmak için seçili bir hesap hücresi bulunamadı.";
            return false;
        }

        var lines = clipboardText.Replace("\r\n", "\n", StringComparison.Ordinal)
            .Replace('\r', '\n').TrimEnd('\n').Split('\n');
        var changes = new List<Cell>();
        for (var rowOffset = 0; rowOffset < lines.Length; rowOffset++)
        {
            var targetRow = firstRow + rowOffset;
            var values = lines[rowOffset].Split('\t');
            if (targetRow >= rowCount || firstColumn + values.Length > visibleColumns.Count)
            {
                error = "Yapıştırılan hücreler seçili hat tablosunun sınırlarını aşıyor; hiçbir hücre değiştirilmedi.";
                return false;
            }

            for (var offset = 0; offset < values.Length; offset++)
            {
                var targetColumn = firstColumn + offset;
                var field = visibleColumns[targetColumn];
                if (!EditableFields.Contains(field) || !isEditable(targetRow, field))
                {
                    error = $"Sütun '{field}' salt okunur, gizli veya bu hat için kapalı. İşlem iptal edildi.";
                    return false;
                }

                var valueText = values[offset].Trim()
                    .Replace("₺", string.Empty, StringComparison.Ordinal).Trim();
                if (!TurkishNumber.IsMatch(valueText)
                    || !decimal.TryParse(valueText, NumberStyles.Number, TurkishCulture, out var number))
                {
                    error = $"Hat satırı {targetRow + 1}, '{field}': '{values[offset]}' geçerli tr-TR sayısı değil. İşlem iptal edildi.";
                    return false;
                }

                if (NonNegativeFields.Contains(field) && number < 0m)
                {
                    error = $"Hat satırı {targetRow + 1}, '{field}': negatif değer kabul edilmez.";
                    return false;
                }

                changes.Add(new Cell(targetRow, field, number));
            }
        }

        planned = changes;
        return true;
    }
}
