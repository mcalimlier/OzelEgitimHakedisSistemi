using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
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

    [Theory]
    [InlineData("Contract.TenderNumber", null)]
    [InlineData("Contract.TenderNumber", "")]
    [InlineData("Contract.TenderNumber", " \t")]
    [InlineData("Contract.ContractNumber", null)]
    [InlineData("Contract.ContractNumber", "")]
    [InlineData("Contract.ContractNumber", " \t")]
    [InlineData("Route.RouteNumber", null)]
    [InlineData("Route.RouteNumber", "")]
    [InlineData("Route.RouteNumber", " \t")]
    [InlineData("ProgressPayment.PeriodDate", null)]
    [InlineData("Contract", null)]
    [InlineData("Contractor", null)]
    [InlineData("Route", null)]
    [InlineData("ProgressPayment", null)]
    public void Create_ShouldDescribeMissingArchiveField(string field, string? value)
    {
        var archive = CreateArchive();
        archive = field switch
        {
            "Contract.TenderNumber" => archive with { Contract = archive.Contract with { TenderNumber = value! } },
            "Contract.ContractNumber" => archive with { Contract = archive.Contract with { ContractNumber = value! } },
            "Route.RouteNumber" => archive with { Route = archive.Route with { RouteNumber = value! } },
            "ProgressPayment.PeriodDate" => archive with { ProgressPayment = archive.ProgressPayment with { PeriodDate = null } },
            "Contract" => archive with { Contract = null! },
            "Contractor" => archive with { Contractor = null! },
            "Route" => archive with { Route = null! },
            "ProgressPayment" => archive with { ProgressPayment = null! },
            _ => throw new ArgumentOutOfRangeException(nameof(field))
        };

        var exception = Assert.Throws<ProgressPaymentArchiveException>(() => ArchiveProgressEditorContext.Create(archive));

        Assert.Contains(field, exception.Message, StringComparison.Ordinal);
        Assert.Contains("Arşiv", exception.Message, StringComparison.Ordinal);
        Assert.Contains("eksik", exception.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Create_ShouldAcceptHistoricSnapshotWithOptionalLinkageAndMidMonthPeriod()
    {
        var archive = CreateArchive();
        archive = archive with
        {
            ProgressPayment = archive.ProgressPayment with { PeriodDate = new DateOnly(2026, 10, 15) }
        };

        var context = ArchiveProgressEditorContext.Create(archive);

        Assert.Equal(new DateOnly(2026, 10, 15), context.PeriodDate);
        Assert.Same(archive.Route, context.Route);
        Assert.Empty(context.Route.TenderNumber);
        Assert.Empty(context.Route.ContractNumber);
        Assert.Empty(context.Route.ContractorTaxOrIdentityNumber);
    }

    internal static ProgressPaymentArchive CreateArchive()
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
