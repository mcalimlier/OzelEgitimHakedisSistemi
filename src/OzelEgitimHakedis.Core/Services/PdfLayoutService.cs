namespace OzelEgitimHakedis.Core.Services;

public static class PdfLayoutService
{
    // WPF FlowDocument page geometry uses device-independent pixels (96 DPI).
    // 210 mm => ~793.7008 DIPs, 297 mm => ~1122.5197 DIPs.
    public const double A4Width = 793.7008d;
    public const double A4Height = 1122.5197d;

    // Conservative printable content width. Leaves additional safety for
    // printer-driver/unprintable-area differences and table borders.
    public const double SafeContentWidth = 660d;

    public static IReadOnlyList<double> FitColumns(
        IReadOnlyList<double> widths)
    {
        ArgumentNullException.ThrowIfNull(widths);

        if (widths.Count == 0)
            return Array.Empty<double>();

        if (widths.Any(width => width <= 0))
            throw new ArgumentOutOfRangeException(nameof(widths), "Kolon genişlikleri sıfırdan büyük olmalıdır.");

        var total = widths.Sum();
        if (total <= SafeContentWidth)
            return widths.ToArray();

        var scale = SafeContentWidth / total;
        return widths.Select(width => width * scale).ToArray();
    }
}
