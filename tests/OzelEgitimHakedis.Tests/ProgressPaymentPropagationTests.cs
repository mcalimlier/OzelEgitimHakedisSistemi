using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentPropagationTests
{
    [Fact]
    public void RouteDailyPriceChange_ShouldRecalculateWithoutChangingUserInputs()
    {
        var oldRoute = Route(1000m);
        var newRoute = oldRoute with { DailyPrice = 1500m };
        var history = History(new ProgressPaymentInput(
            10, 1000m, 50m, 100m, 100000m, false, 120m, 80m, 10m, 5m,
            5, 500m, 25m, 50m));

        var updated = ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[] { history }, oldRoute, newRoute, Contractor());

        Assert.Single(updated);
        var item = updated[0];

        Assert.Equal(10, item.Input.WorkDays);
        Assert.Equal(50m, item.Input.DiscountAmount);
        Assert.Equal(100m, item.Input.PriceDifference);
        Assert.Equal(120m, item.Input.TaxDebt);
        Assert.Equal(5m, item.Input.InspectionPenalty);
        Assert.Equal(1500m, item.Input.DailyPrice);
        Assert.True(item.Calculation.GrossAmount > history.Calculation.GrossAmount);
    }

    [Fact]
    public void ContractorRename_ShouldKeepStableIdentityAndUpdateDisplayName()
    {
        var oldContractor = Contractor();
        var newContractor = oldContractor with { Name = "Yeni Firma", BankAccountNumber = "TR-NEW" };
        var history = History(new ProgressPaymentInput(
            10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m));

        var updated = ProgressPaymentPropagationService.RecalculateForContractorChange(
            new[] { history }, oldContractor, newContractor);

        Assert.Single(updated);
        Assert.Equal("Yeni Firma", updated[0].ContractorName);
        Assert.Equal("TR-NEW", updated[0].BankAccountNumber);
        Assert.Equal("12345678901", updated[0].ContractorTaxOrIdentityNumber);
        Assert.Equal(history.Calculation, updated[0].Calculation);
    }

    [Fact]
    public void ContractorChange_ShouldUpdateContractReferenceFields()
    {
        var oldContractor = Contractor();
        var newContractor = oldContractor with
        {
            Name = "Yeni Firma",
            TaxOrIdentityNumber = "98765432109",
            BankAccountNumber = "TR-NEW"
        };

        var history = History(new ProgressPaymentInput(
            10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m));

        var updated = ProgressPaymentPropagationService.RecalculateForContractorChange(
            new[] { history }, oldContractor, newContractor);

        Assert.Equal("Yeni Firma", updated[0].ContractorName);
        Assert.Equal("98765432109", updated[0].ContractorTaxOrIdentityNumber);
        Assert.Equal("TR-NEW", updated[0].BankAccountNumber);
    }

    [Fact]
    public void VatExemptionChange_ShouldRecalculateVatAndWithholding()
    {
        var oldContract = Contract(vatExempt: false);
        var newContract = oldContract with { VatExempt = true };
        var history = History(new ProgressPaymentInput(
            10, 1000m, 0m, 100m, 100000m, false, 0m, 0m, 0m));

        var updated = ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { history }, Array.Empty<RouteData>(), oldContract, newContract);

        Assert.Single(updated);
        Assert.Equal(0m, updated[0].Calculation.Vat);
        Assert.Equal(0m, updated[0].Calculation.Withholding);
        Assert.Equal(newContract.ContractAmount, updated[0].Input.ContractAmount);
        Assert.True(updated[0].Input.IsVatExempt);
    }

    [Fact]
    public void ContractMetadataChange_ShouldRefreshOutputIdentity()
    {
        var oldContract = Contract(vatExempt: false);
        var newContract = oldContract with
        {
            WorkName = "Yeni İş",
            ContractNumber = "SOZ-NEW",
            ContractDate = new DateOnly(2026, 2, 2)
        };
        var history = History(new ProgressPaymentInput(
            10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m));

        var updated = ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { history }, Array.Empty<RouteData>(), oldContract, newContract);

        Assert.Equal("Yeni İş", updated[0].WorkName);
        Assert.Equal("SOZ-NEW", updated[0].ContractNumber);
        Assert.Equal(new DateOnly(2026, 2, 2), updated[0].ContractDate);
    }

    [Fact]
    public void ContractChange_ShouldNotRecalculateSiblingOrAmbiguousHistory()
    {
        var oldContract = Contract(vatExempt: false);
        var newContract = oldContract with { TransportDailyPrice = 1500m };
        var linked = History(new ProgressPaymentInput(10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m));
        var siblingInput = linked.Input with { DailyPrice = 2500m };
        var sibling = linked with
        {
            Id = Guid.NewGuid(),
            ContractNumber = "SOZ-02",
            WorkName = "İkinci Kısım",
            Input = siblingInput,
            Calculation = ProgressPaymentCalculator.Calculate(siblingInput)
        };
        var ambiguousLegacy = linked with { Id = Guid.NewGuid(), ContractNumber = "" };

        var updated = ProgressPaymentPropagationService.RecalculateForContractChange(
            new[] { linked, sibling, ambiguousLegacy },
            Array.Empty<RouteData>(),
            oldContract,
            newContract);

        Assert.Equal(1500m, updated[0].Input.DailyPrice);
        Assert.Equal(2500m, updated[1].Input.DailyPrice);
        Assert.Equal("İkinci Kısım", updated[1].WorkName);
        Assert.Equal(1000m, updated[2].Input.DailyPrice);
    }

    [Fact]
    public void SourceChangeCreatingDuplicateKey_ShouldBeRejected()
    {
        var oldRoute = Route(1000m);
        var newRoute = oldRoute with { RouteNumber = "2" };
        var first = History(new ProgressPaymentInput(10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m));
        var conflicting = History(new ProgressPaymentInput(10, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m)) with
        {
            Id = Guid.NewGuid(),
            RouteNumber = "2"
        };

        Assert.Throws<ProgressPaymentPropagationException>(() =>
            ProgressPaymentPropagationService.RecalculateForRouteChange(
                new[] { first, conflicting },
                oldRoute,
                newRoute,
                Contractor()));
    }


    [Fact]
    public void RouteChange_ShouldRecalculateWithCurrentSavedCalculationConstants()
    {
        var oldRoute = Route(1000m);
        var newRoute = oldRoute with { DailyPrice = 1500m };
        var history = History(new ProgressPaymentInput(
            10, 1000m, 50m, 100m, 100000m, false, 0m, 0m, 0m, 0m,
            5, 500m, 25m, 50m));
        var currentConstants = new ProgressPaymentCalculationConstants(
            TransportVatRate: 0.12m,
            GuideVatRate: 0.25m,
            TransportWithholdingShare: 0.40m,
            GuideWithholdingShare: 0.80m,
            StampTaxRate: 0.01m,
            AdditionalGuaranteeRate: 0.07m);

        var updated = ProgressPaymentPropagationService.RecalculateForRouteChange(
            new[] { history }, oldRoute, newRoute, Contractor(), currentConstants);

        var result = Assert.Single(updated).Calculation;
        Assert.Equal(0.12m, result.AppliedConstants!.TransportVatRate);
        Assert.Equal(0.25m, result.AppliedConstants.GuideVatRate);
        Assert.Equal(17575m * 0.01m, result.StampTax);
        Assert.Equal(10.5m, result.AdditionalGuarantee);
    }

    private static ContractDraftData Contract(bool vatExempt) =>
        new("TENDER", "SOZ-01", "İş", "Firma", 100000m,
            new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31), vatExempt,
            ContractorTaxOrIdentityNumber: "12345678901");

    private static Contractor Contractor() =>
        new("Firma", "12345678901", BankAccountNumber: "TR-OLD");

    private static RouteData Route(decimal price) =>
        new("1", "Hat 1", price, GuideWorkDays: 5, GuideDailyPrice: 500m,
            TransportWorkDays: 10, TenderNumber: "TENDER", ContractorTaxOrIdentityNumber: "12345678901");

    private static ProgressPaymentHistoryData History(ProgressPaymentInput input) =>
        new(Guid.NewGuid(), DateTime.UtcNow, input,
            ProgressPaymentCalculator.Calculate(input), 1, new DateOnly(2026, 10, 31),
            "1", "Hat 1", "İş", "Firma", "TENDER", "SOZ-01",
            ContractDate: new DateOnly(2026, 1, 2),
            ContractStartDate: new DateOnly(2026, 1, 1),
            ContractEndDate: new DateOnly(2026, 12, 31),
            BankAccountNumber: "TR-OLD",
            ContractorTaxOrIdentityNumber: "12345678901");
}
