using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentIcmalIdentityCompletenessTests
{
    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryWhenTenderNumberIsMissing() =>
        AssertIncompleteIdentityDoesNotCarry("TenderNumber");

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryWhenContractNumberIsMissing() =>
        AssertIncompleteIdentityDoesNotCarry("ContractNumber");

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryWhenRouteNumberIsMissing() =>
        AssertIncompleteIdentityDoesNotCarry("RouteNumber");

    [Fact]
    public void FindPreviousDeduction_ShouldNotCarryWhenContractorIdentityIsMissing() =>
        AssertIncompleteIdentityDoesNotCarry("ContractorTaxOrIdentityNumber");

    [Fact]
    public void FindPreviousDeduction_ShouldIgnorePreviousRecordWithIncompleteIdentity()
    {
        var incompletePrevious = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            ContractNumber = ""
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m);

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { incompletePrevious, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotMatchIdentityNumberThatNormalizesToEmpty()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), 200m) with
        {
            ContractorTaxOrIdentityNumber = "----"
        };
        var current = History(2, new DateOnly(2026, 11, 30), 300m) with
        {
            ContractorTaxOrIdentityNumber = "----"
        };

        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotMatchEmptyTenderIdentity()
    {
        var incompletePrevious = History(1, new DateOnly(2026, 10, 31), 200m) with { TenderNumber = "" };
        var current = History(2, new DateOnly(2026, 11, 30), 300m);
        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { incompletePrevious, current }, current));
    }

    [Fact]
    public void FindPreviousDeduction_ShouldNotMatchWhitespaceTenderIdentity()
    {
        var incompletePrevious = History(1, new DateOnly(2026, 10, 31), 200m) with { TenderNumber = "   " };
        var current = History(2, new DateOnly(2026, 11, 30), 300m);
        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { incompletePrevious, current }, current));
    }

    private static void AssertIncompleteIdentityDoesNotCarry(string field)
    {
        var previous = WithBlankIdentity(History(1, new DateOnly(2026, 10, 31), 200m), field);
        var current = WithBlankIdentity(History(2, new DateOnly(2026, 11, 30), 300m), field);
        Assert.Equal(0m, ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { previous, current }, current));
    }

    private static ProgressPaymentHistoryData WithBlankIdentity(
        ProgressPaymentHistoryData payment,
        string field) =>
        field switch
        {
            "TenderNumber" => payment with { TenderNumber = "" },
            "ContractNumber" => payment with { ContractNumber = "" },
            "RouteNumber" => payment with { RouteNumber = "" },
            "ContractorTaxOrIdentityNumber" => payment with { ContractorTaxOrIdentityNumber = "" },
            _ => throw new ArgumentOutOfRangeException(nameof(field), field, "Unknown identity field.")
        };

    private static ProgressPaymentHistoryData History(
        int number,
        DateOnly period,
        decimal deduction) =>
        new(
            Guid.NewGuid(),
            DateTime.UtcNow,
            new ProgressPaymentInput(1, 1000m, 0m, 0m, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(
                1000m, 0m, 1000m, 0m, 0m, 1000m, 0m, 1000m, deduction, 1000m - deduction, 0m,
                new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)),
            number,
            period,
            "1", "Hat 1", "İş", "Firma", "TENDER", "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");
}
