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
    public void Restore_ShouldKeepSameTenderContractorAndRouteSeparateAcrossPrograms()
    {
        var special = Archive();
        var basicContract = special.Contract with { ProgramType = TransportationProgramType.BasicOrSecondary };
        var basicRoute = special.Route with { ProgramType = TransportationProgramType.BasicOrSecondary };
        var basicContractor = special.Contractor with { ProgramType = TransportationProgramType.BasicOrSecondary };
        var basicInput = special.ProgressPayment.Input with { ProgramType = TransportationProgramType.BasicOrSecondary };
        var basicPayment = special.ProgressPayment with
        {
            Id = Guid.NewGuid(),
            Input = basicInput,
            Calculation = ProgressPaymentCalculator.Calculate(basicInput)
        };
        var basicArchive = special with
        {
            Contract = basicContract,
            Route = basicRoute,
            Contractor = basicContractor,
            ProgressPayment = basicPayment
        };
        var current = new OehsApplicationData(
            special.Contract,
            new[] { special.Route },
            new[] { special.Contractor },
            new[] { special.ProgressPayment },
            new[] { special.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, basicArchive);

        Assert.Equal(2, result.Data.Contracts!.Count);
        Assert.Equal(2, result.Data.Contractors.Count);
        Assert.Equal(2, result.Data.Routes.Count);
        Assert.Equal(2, result.Data.ProgressPayments!.Count);
        Assert.Contains(result.Data.Routes, x => x.ProgramType == TransportationProgramType.SpecialEducation);
        Assert.Contains(result.Data.Routes, x => x.ProgramType == TransportationProgramType.BasicOrSecondary);
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
    public void Restore_ShouldPromoteLegacyContractPropertyInsteadOfAddingArchiveDuplicate()
    {
        var archive = Archive();
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route },
            new[] { archive.Contractor },
            new[] { archive.ProgressPayment },
            Contracts: null);

        var result = ProgressPaymentArchiveRestoreService.Restore(current, archive);

        Assert.Single(result.Data.Contracts ?? Array.Empty<ContractDraftData>());
        Assert.Single(result.Data.Routes);
        Assert.Single(result.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>());
        Assert.False(result.AddedAnything);
    }

    [Fact]
    public void Restore_ShouldFillMissingContractNumberWhenPaymentGuidMatchesArchive()
    {
        var archive = Archive();
        var ambiguousExistingPayment = archive.ProgressPayment with { ContractNumber = string.Empty };
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route with { ContractNumber = archive.Contract.ContractNumber } },
            new[] { archive.Contractor },
            new[] { ambiguousExistingPayment },
            new[] { archive.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, archive);
        var restored = Assert.Single(result.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>());

        Assert.Equal(archive.ProgressPayment.ContractNumber, restored.ContractNumber);
        Assert.True(result.AddedAnything);
        Assert.Contains("Hakediş sözleşme bağlantısı", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_ShouldNotAddPaymentWithDuplicateBusinessKeyAndDifferentId()
    {
        var archive = Archive();
        var duplicatePayment = archive.ProgressPayment with { Id = Guid.NewGuid() };
        var duplicateArchive = archive with { ProgressPayment = duplicatePayment };
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route },
            new[] { archive.Contractor },
            new[] { archive.ProgressPayment },
            new[] { archive.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, duplicateArchive);

        Assert.Single(result.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>());
        Assert.False(result.AddedAnything);
        Assert.Contains("aynı ihale, hat, yüklenici, hakediş numarası ve dönem anahtarında", result.Message, StringComparison.Ordinal);
        Assert.DoesNotContain("UYARI:", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_ShouldWarnWhenSameBusinessKeyHasDifferentInputsOrAmountsWithoutOverwritingActivePayment()
    {
        var archive = Archive();
        var active = archive.ProgressPayment;
        var archiveInput = active.Input with { DailyPrice = active.Input.DailyPrice + 100m };
        var conflictingArchivedPayment = active with
        {
            Id = Guid.NewGuid(),
            Input = archiveInput,
            Calculation = ProgressPaymentCalculator.Calculate(archiveInput)
        };
        var conflictingArchive = archive with { ProgressPayment = conflictingArchivedPayment };
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route },
            new[] { archive.Contractor },
            new[] { active },
            new[] { archive.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, conflictingArchive);
        var restored = Assert.Single(result.Data.ProgressPayments!);

        Assert.Equal(active.Id, restored.Id);
        Assert.Equal(active.Input, restored.Input);
        Assert.Equal(active.Calculation.NetPayable, restored.Calculation.NetPayable);
        Assert.False(result.AddedAnything);
        Assert.Contains("UYARI:", result.Message, StringComparison.Ordinal);
        Assert.Contains("Aktif net ödeme:", result.Message, StringComparison.Ordinal);
        Assert.Contains("arşiv net ödeme:", result.Message, StringComparison.Ordinal);
        Assert.Contains("otomatik değiştirilmedi", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_ShouldWarnWhenSameIdArchiveHasDifferentInputsWithoutOverwritingActivePayment()
    {
        var archive = Archive();
        var active = archive.ProgressPayment;
        var archiveInput = active.Input with { DailyPrice = active.Input.DailyPrice + 100m };
        var changedSnapshot = active with
        {
            Input = archiveInput,
            Calculation = ProgressPaymentCalculator.Calculate(archiveInput)
        };
        var changedArchive = archive with { ProgressPayment = changedSnapshot };
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route },
            new[] { archive.Contractor },
            new[] { active },
            new[] { archive.Contract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, changedArchive);
        var restored = Assert.Single(result.Data.ProgressPayments!);

        Assert.Equal(active.Id, restored.Id);
        Assert.Equal(active.Input, restored.Input);
        Assert.False(result.AddedAnything);
        Assert.Contains("UYARI:", result.Message, StringComparison.Ordinal);
        Assert.Contains("aynı hakediş kimliğindeki", result.Message, StringComparison.OrdinalIgnoreCase);
        Assert.Contains("otomatik değiştirilmedi", result.Message, StringComparison.Ordinal);
    }

    [Fact]
    public void Restore_ShouldAllowSpecificContractPaymentWhenLegacyPaymentContractIsUnknown()
    {
        var archive = Archive();
        var ambiguousExistingPayment = archive.ProgressPayment with
        {
            Id = Guid.NewGuid(),
            ContractNumber = string.Empty
        };
        var siblingContract = archive.Contract with
        {
            ContractNumber = "SOZ-02",
            WorkName = "İkinci Kısım"
        };
        var current = new OehsApplicationData(
            archive.Contract,
            new[] { archive.Route with { ContractNumber = string.Empty } },
            new[] { archive.Contractor },
            new[] { ambiguousExistingPayment },
            new[] { archive.Contract, siblingContract });

        var result = ProgressPaymentArchiveRestoreService.Restore(current, archive);

        Assert.Equal(2, result.Data.ProgressPayments?.Count);
        Assert.Contains(result.Data.ProgressPayments!, x => x.Id == archive.ProgressPayment.Id);
        Assert.True(result.AddedAnything);
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
