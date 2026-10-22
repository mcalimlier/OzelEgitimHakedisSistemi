using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class ArchiveProgressEditorContextTests
{
    [Fact]
    public void Create_ShouldUseSnapshotDataWhenNoLiveRecordExists()
    {
        var archive = CreateArchive();

        var context = ArchiveProgressEditorContext.Create(archive);

        Assert.Equal(archive.Contract, context.Contract);
        Assert.Equal(archive.Contractor, context.Contractor);
        Assert.Equal(archive.Route.RouteNumber, context.Route.RouteNumber);
        Assert.Equal(archive.ProgressPayment.PeriodDate, context.PeriodDate);
    }

    private static ProgressPaymentArchive CreateArchive()
    {
        var input = new ProgressPaymentInput(20m, 1_000m, 0m, 0m, 0m, false, 0m, 0m, 0m);
        var calculation = new ProgressPaymentCalculation(
            20_000m, 0m, 20_000m, 4_000m, 2_000m, 22_000m, 0m, 24_000m, 2_000m,
            22_000m, 0m, new DeductionBreakdown(2_000m, 0m, 0m, 0m, 0m, 0m, 0m));

        return new ProgressPaymentArchive(
            "1.0",
            1,
            DateTime.UtcNow,
            "ActiveRecord",
            new ContractDraftData(
                "IHALE-01", "SOZ-01", "Özel Eğitim Taşıma", "Firma", 100_000m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false),
            new Contractor("Firma", "12345678901"),
            new RouteData("01", "Hat 01", 1_000m, TransportWorkDays: 20m),
            new ProgressPaymentHistoryData(
                Guid.NewGuid(), DateTime.UtcNow, input, calculation, 1,
                new DateOnly(2026, 10, 31), "01", "Hat 01"),
            Array.Empty<ProgressPaymentHistoryData>(),
            "archive.pdf",
            Array.Empty<string>());
    }
}
