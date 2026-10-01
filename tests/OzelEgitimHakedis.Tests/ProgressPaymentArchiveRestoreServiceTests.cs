using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentArchiveRestoreServiceTests
{
    [Fact]
    public void Restore_ShouldCreateMissingTenderContractorAndRouteFromSnapshot()
    {
        var archive = Archive();

        var result = ProgressPaymentArchiveRestoreService.Restore(
            new OehsApplicationData(
                null,
                Array.Empty<RouteData>(),
                Array.Empty<Contractor>(),
                Array.Empty<ProgressPaymentHistoryData>(),
                Array.Empty<ContractDraftData>()),
            archive);

        var contracts = result.Data.Contracts ?? Array.Empty<ContractDraftData>();
        var contractors = result.Data.Contractors ?? Array.Empty<Contractor>();
        var routes = result.Data.Routes;
        var payments = result.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>();

        Assert.Single(contracts);
        Assert.Single(contractors);
        Assert.Single(routes);
        Assert.Single(payments);

        Assert.Equal("IHALE-01", contracts[0].TenderNumber);
        Assert.Equal("12345678901", contractors[0].TaxOrIdentityNumber);
        Assert.Equal("01", routes[0].RouteNumber);
        Assert.Equal(archive.ProgressPayment.Id, payments[0].Id);
    }

    [Fact]
    public void Restore_ShouldNotDuplicateExistingSnapshotRecords()
    {
        var archive = Archive();
        var baseData = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route },
            new[] { archive.Contractor },
            new[] { archive.ProgressPayment },
            new[] { archive.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(baseData, archive);

        Assert.Single(result.Data.Contracts ?? Array.Empty<ContractDraftData>());
        Assert.Single(result.Data.Contractors ?? Array.Empty<Contractor>());
        Assert.Single(result.Data.Routes);
        Assert.Single(result.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>());
        Assert.False(result.AddedAnything);
    }

    [Fact]
    public void Restore_ShouldReportExistingAndAddedItems()
    {
        var archive = Archive();
        var result = ProgressPaymentArchiveRestoreService.Restore(
            new OehsApplicationData(
                archive.Contract,
                Array.Empty<RouteData>(),
                new[] { archive.Contractor },
                Array.Empty<ProgressPaymentHistoryData>(),
                new[] { archive.Contract }),
            archive);

        Assert.True(result.AddedAnything);
        Assert.Contains("Hat", result.Message, StringComparison.Ordinal);
        Assert.Contains("Hakediş", result.Message, StringComparison.Ordinal);
    }

    private static ProgressPaymentArchive Archive()
    {
        var input = new ProgressPaymentInput(
            20, 1000m, 10m, 100m, 100000m, false, 0m, 0m, 0m);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var history = new ProgressPaymentHistoryData(
            Guid.NewGuid(), DateTime.UtcNow, input, calculation, 1,
            new DateOnly(2026, 10, 31), "01", "Hat 01", "İş", "Firma",
            "IHALE-01", "SOZ-01",
            ContractorTaxOrIdentityNumber: "12345678901");

        return new ProgressPaymentArchive(
            "1.0", 1, DateTime.UtcNow, "Archive",
            new ContractDraftData(
                "IHALE-01", "SOZ-01", "İş", "Firma", 100000m,
                new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false,
                ContractorTaxOrIdentityNumber: "12345678901"),
            new Contractor("Firma", "12345678901", BankAccountNumber: "TR-01"),
            new RouteData(
                "01", "Hat 01", 1000m, 0, 0, 20,
                "IHALE-01", "12345678901"),
            history,
            Array.Empty<ProgressPaymentHistoryData>(),
            "OEHS_IHALE-01_Hat-01_Hakediş-01_2026-10-31.pdf",
            ProgressPaymentReportService.RequiredDocumentNames);
    }
}
