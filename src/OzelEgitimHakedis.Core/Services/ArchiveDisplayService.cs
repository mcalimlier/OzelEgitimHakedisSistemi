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
            .OrderBy(row => row.FullPath, StringComparer.OrdinalIgnoreCase)
            .ToArray();
    }
}
