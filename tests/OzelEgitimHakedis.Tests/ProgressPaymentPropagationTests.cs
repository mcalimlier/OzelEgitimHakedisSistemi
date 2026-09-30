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
