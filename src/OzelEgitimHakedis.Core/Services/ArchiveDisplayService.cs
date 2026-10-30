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
    private sealed record LoadedRow(
        ArchiveDisplayRow Row,
        DateOnly PeriodDate,
        int ProgressPaymentNumber,
        int Revision);

    public static IReadOnlyList<ArchiveDisplayRow> BuildRows(ProgressPaymentArchiveStore store)
    {
        ArgumentNullException.ThrowIfNull(store);

        var validRows = new List<LoadedRow>();
        var invalidRows = new List<ArchiveDisplayRow>();

        foreach (var file in store.List())
        {
            try
            {
                var archive = ProgressPaymentArchiveStore.Load(file.FullName);
                var progress = archive.ProgressPayment.ProgressPaymentNumber ?? int.MaxValue;
                var progressDisplay = archive.ProgressPayment.ProgressPaymentNumber?.ToString(System.Globalization.CultureInfo.CurrentCulture) ?? "-";
                var period = archive.ProgressPayment.PeriodDate ?? DateOnly.MaxValue;

                var monthLabel = period == DateOnly.MaxValue
                    ? "Dönem belirtilmemiş"
                    : System.Globalization.CultureInfo.GetCultureInfo("tr-TR").TextInfo.ToTitleCase(
                        period.ToString("MMMM yyyy", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")));
                var title = !string.IsNullOrWhiteSpace(archive.ProgressPayment.BatchDisplayName)
                    ? $"{archive.ProgressPayment.BatchDisplayName} • Hat {archive.ProgressPayment.RouteNumber ?? "-"} • Hakediş {progressDisplay}"
                    : $"İhale/Dosya: {archive.ProgressPayment.TenderNumber ?? "-"} • Hat {archive.ProgressPayment.RouteNumber ?? "-"} • Hakediş {progressDisplay}";

                validRows.Add(new LoadedRow(
                    new ArchiveDisplayRow(
                        file.FullName,
                        title,
                        $"Dönem: {monthLabel} • Yüklenici: {archive.Contractor.Name}",
                        archive.ProgressPayment.Calculation.NetPayable.ToString("N2", System.Globalization.CultureInfo.GetCultureInfo("tr-TR")) + " ₺",
                        $"v{archive.Revision:00}"),
                    period,
                    progress,
                    archive.Revision));
            }
            catch (ProgressPaymentArchiveException)
            {
                invalidRows.Add(new ArchiveDisplayRow(
                    file.FullName,
                    Path.GetFileNameWithoutExtension(file.Name),
                    "Geçersiz veya okunamayan arşiv",
                    "-",
                    "GEÇERSİZ"));
            }
        }

        return validRows
            .OrderBy(x => x.PeriodDate)
            .ThenBy(x => x.ProgressPaymentNumber)
            .ThenBy(x => x.Revision)
            .ThenBy(x => x.Row.FullPath, StringComparer.OrdinalIgnoreCase)
            .Select(x => x.Row)
            .Concat(invalidRows.OrderBy(x => x.FullPath, StringComparer.OrdinalIgnoreCase))
            .ToArray();
    }
}
