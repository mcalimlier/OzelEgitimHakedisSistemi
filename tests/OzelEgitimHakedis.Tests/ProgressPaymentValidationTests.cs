using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentValidationTests
{
    [Fact]
    public void NormalizeIdentity_ShouldCollapseFormattingDifferences()
    {
        Assert.Equal("12345678901", OehsIdentityService.NormalizeIdentity("123.456.789-01"));
        Assert.Equal("12345678901", OehsIdentityService.NormalizeIdentity(" 123 456 789 01 "));
    }

    [Fact]
    public void RouteKey_ShouldIncludeTenderNumberAndNormalizedIdentity()
    {
        var a = OehsIdentityService.BuildRouteKey("IH-A", "1", "123.456.789-01");
        var b = OehsIdentityService.BuildRouteKey("IH-B", "1", "12345678901");

        Assert.NotEqual(a, b);
        Assert.Equal("IH-A|1|12345678901", a);
    }

    [Fact]
    public void DuplicateProgressPaymentKey_ShouldRejectNewRecordButAllowCurrentEdit()
    {
        var existing = History(1, new DateOnly(2026, 10, 31));
        var candidate = History(1, new DateOnly(2026, 10, 31));

        var data = Data(existing);

        var duplicateErrors = ProgressPaymentValidationService.ValidateCreation(
            data,
            Contract(),
            Route(),
            candidate,
            editingExisting: false);

        var editErrors = ProgressPaymentValidationService.ValidateCreation(
            data,
            Contract(),
            Route(),
            candidate with { Id = existing.Id },
            editingExisting: true);

        Assert.Contains(duplicateErrors, x => x.Contains("Mükerrer", StringComparison.OrdinalIgnoreCase));
        Assert.DoesNotContain(editErrors, x => x.Contains("Mükerrer", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SameRouteMonth_ShouldRejectDifferentProgressPaymentNumber()
    {
        var existing = History(1, new DateOnly(2026, 10, 5));
        var candidate = History(2, new DateOnly(2026, 10, 31));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            Data(existing),
            Contract(),
            Route(),
            candidate,
            editingExisting: false);

        Assert.Contains(errors, x => x.Contains("aynı ayda birden fazla", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SameRouteMonth_ShouldAllowEditingOnlyExistingMonthlyClaim()
    {
        var existing = History(1, new DateOnly(2026, 10, 31));
        var candidate = existing with { };

        var errors = ProgressPaymentValidationService.ValidateCreation(
            Data(existing),
            Contract(),
            Route(),
            candidate,
            editingExisting: true);

        Assert.DoesNotContain(errors, x => x.Contains("aynı ayda birden fazla", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MissingContractSource_ShouldBeRejected()
    {
        var data = new OehsApplicationData(
            null,
            new[] { Route() },
            new[] { new Contractor("Firma", "12345678901") },
            Array.Empty<ProgressPaymentHistoryData>(),
            Array.Empty<ContractDraftData>());

        var candidate = History(1, new DateOnly(2026, 10, 31));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            data, Contract(), Route(), candidate, false);

        Assert.Contains(errors, x => x.Contains("ihale / dosya", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MissingRouteSource_ShouldBeRejected()
    {
        var data = new OehsApplicationData(
            Contract(),
            Array.Empty<RouteData>(),
            new[] { new Contractor("Firma", "12345678901") },
            Array.Empty<ProgressPaymentHistoryData>(),
            new[] { Contract() });

        var candidate = History(1, new DateOnly(2026, 10, 31));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            data, Contract(), Route(), candidate, false);

        Assert.Contains(errors, x => x.Contains("hat kaydı", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void MissingContractorSource_ShouldBeRejected()
    {
        var data = new OehsApplicationData(
            Contract(),
            new[] { Route() },
            Array.Empty<Contractor>(),
            Array.Empty<ProgressPaymentHistoryData>(),
            new[] { Contract() });

        var candidate = History(1, new DateOnly(2026, 10, 31));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            data, Contract(), Route(), candidate, false);

        Assert.Contains(errors, x => x.Contains("yüklenici firma kaydı", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void SuggestNextProgressPaymentNumber_ShouldUseSameTenderRouteIdentity()
    {
        var history = new[]
        {
            History(2, new DateOnly(2026, 11, 30)),
            History(5, new DateOnly(2026, 12, 31)),
            History(9, new DateOnly(2026, 12, 31), tender:"OTHER")
        };

        var suggested = ProgressPaymentValidationService.SuggestNextProgressPaymentNumber(
            history, "TENDER", "1", "123.456.789-01");

        Assert.Equal(6, suggested);
    }

    [Fact]
    public void PeriodBeforePrevious_ShouldBeRejected()
    {
        var data = Data(History(1, new DateOnly(2026, 11, 30)));
        var candidate = History(2, new DateOnly(2026, 10, 31));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            data, Contract(), Route(), candidate, false);

        Assert.Contains(errors, x => x.Contains("önceki", StringComparison.OrdinalIgnoreCase));
    }

    [Fact]
    public void PeriodOutsideContract_ShouldBeRejected()
    {
        var candidate = History(1, new DateOnly(2027, 2, 1));

        var errors = ProgressPaymentValidationService.ValidateCreation(
            Data(), Contract(), Route(), candidate, false);

        Assert.Contains(errors, x => x.Contains("sözleşme", StringComparison.OrdinalIgnoreCase));
    }

    private static OehsApplicationData Data(params ProgressPaymentHistoryData[] history) =>
        new(Contract(), new[] { Route() }, new[] { new Contractor("Firma", "12345678901") }, history, new[] { Contract() });

    private static ContractDraftData Contract() =>
        new("TENDER", "SOZ-01", "Özel Eğitim Taşıma", "Firma", 100000m,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), false,
            ContractorTaxOrIdentityNumber: "12345678901");

    private static RouteData Route() =>
        new("1", "Hat 1", 1000m, TenderNumber: "TENDER", ContractorTaxOrIdentityNumber: "12345678901");

    private static ProgressPaymentHistoryData History(
        int number,
        DateOnly period,
        string tender = "TENDER") =>
        new(
            Guid.NewGuid(),
            DateTime.Today,
            new ProgressPaymentInput(1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(1000m, 0m, 1000m, 100m, 50m, 1050m, 9.48m, 1100m, 59.48m, 1040.52m, 0m,
                new DeductionBreakdown(50m, 9.48m, 0m, 0m, 0m, 0m, 0m)),
            number,
            period,
            "1",
            "Hat 1",
            "Özel Eğitim Taşıma",
            "Firma",
            tender,
            "SOZ-01",
            ContractorTaxOrIdentityNumber: "12345678901");
}
