using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ArchiveDisplayRow(
    string FullPath,
    string DisplayTitle,
    string DetailText,
    string NetPayableText,
    string RevisionText);

public static class ArchiveDisplayService
{
    private static DateOnly GetPeriod(ArchiveDisplayRow row)
    {
        try
        {
            var archive = new ProgressPaymentArchiveStore().Load(row.FullPath);
            return archive.ProgressPayment.PeriodDate ?? DateOnly.MaxValue;
        }
        catch (ProgressPaymentArchiveException)
        {
            return DateOnly.MaxValue;
        }
    }

    private static int GetProgressNumber(ArchiveDisplayRow row)
    {
        var prefix = "• Hakediş ";
        var index = row.DisplayTitle.IndexOf(prefix, StringComparison.Ordinal);
        if (index < 0)
            return int.MaxValue;

        var value = row.DisplayTitle[(index + prefix.Length)..];
        return int.TryParse(value, out var number) ? number : int.MaxValue;
    }

    private static int ParseRevision(string value) =>
        value.StartsWith("v", StringComparison.OrdinalIgnoreCase)
            && int.TryParse(value[1..], out var revision)
                ? revision
                : int.MaxValue;
    public static IReadOnlyList<ArchiveDisplayRow> BuildRows(ProgressPaymentArchiveStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var rows = new List<ArchiveDisplayRow>();
        foreach (var file in store.List())
        {
            try
            {
                var archive = store.Load(file.FullName);
                var progress = archive.ProgressPayment.ProgressPaymentNumber?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? "-";
                var period = archive.ProgressPayment.PeriodDate?.ToString("dd.MM.yyyy", System.Globalization.CultureInfo.CurrentCulture) ?? "-";
                rows.Add(new ArchiveDisplayRow(
                    file.FullName,
                    $"İhale/Dosya: {archive.ProgressPayment.TenderNumber ?? "-"} • Hat {archive.ProgressPayment.RouteNumber ?? "-"} • Hakediş {progress}",
                    $"Dönem: {period} • Yüklenici: {archive.Contractor.Name}",
                    archive.ProgressPayment.Calculation.NetPayable.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " ₺",
                    $"v{archive.Revision:00}"));
            }
            catch (ProgressPaymentArchiveException)
            {
                rows.Add(new ArchiveDisplayRow(
                    file.FullName,
                    Path.GetFileNameWithoutExtension(file.Name),
                    "Geçersiz veya okunamayan arşiv",
                    "-",
                    "GEÇERSİZ"));
            }
        }

        return rows
            .OrderBy(row => GetPeriod(row))
            .ThenBy(row => GetProgressNumber(row))
            .ThenBy(row => ParseRevision(row.RevisionText))
            .ThenBy(row => row.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
