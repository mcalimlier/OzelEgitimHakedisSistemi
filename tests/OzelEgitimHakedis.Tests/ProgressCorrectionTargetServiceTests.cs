using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressCorrectionTargetServiceTests
{
    [Fact]
    public void SelectTender_ShouldReturnTenderIdentity()
    {
        var selection = ProgressCorrectionTargetService.Select(
            ProgressCorrectionTarget.Tender, History());

        Assert.Equal(ProgressCorrectionTarget.Tender, selection.Target);
        Assert.Equal("TENDER", selection.TenderNumber);
    }

    [Fact]
    public void SelectContractor_ShouldReturnNormalizedContractorIdentity()
    {
        var selection = ProgressCorrectionTargetService.Select(
            ProgressCorrectionTarget.Contractor, History());

        Assert.Equal("12345678901", selection.ContractorTaxOrIdentityNumber);
    }

    [Fact]
    public void SelectRoute_ShouldReturnTenderRouteAndContractorIdentity()
    {
        var selection = ProgressCorrectionTargetService.Select(
            ProgressCorrectionTarget.Route, History());

        Assert.Equal("TENDER", selection.TenderNumber);
        Assert.Equal("1", selection.RouteNumber);
        Assert.Equal("12345678901", selection.ContractorTaxOrIdentityNumber);
    }

    [Fact]
    public void SelectProgressPayment_ShouldReturnExactHistoryId()
    {
        var history = History();
        var selection = ProgressCorrectionTargetService.Select(
            ProgressCorrectionTarget.ProgressPayment, history);

        Assert.Equal(history.Id, selection.ProgressPaymentId);
        Assert.Equal("TENDER", selection.TenderNumber);
    }

    [Fact]
    public void SelectCancel_ShouldReturnCancelWithoutMutationTarget()
    {
        var selection = ProgressCorrectionTargetService.Select(
            ProgressCorrectionTarget.Cancel, History());

        Assert.Equal(ProgressCorrectionTarget.Cancel, selection.Target);
        Assert.Null(selection.ProgressPaymentId);
    }

    private static ProgressPaymentHistoryData History() =>
        new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new ProgressPaymentInput(1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(
                1000m, 0m, 1000m, 100m, 50m, 1050m, 9.48m, 1100m, 59.48m, 1040.52m, 0m,
                new DeductionBreakdown(50m, 9.48m, 0m, 0m, 0m, 0m, 0m)),
            3,
            new DateOnly(2026, 11, 30),
            "1",
            "Hat 1",
            "İş",
            "Firma",
            "TENDER",
            "SOZ-01",
            ContractorTaxOrIdentityNumber: "123.456.789-01");
}
