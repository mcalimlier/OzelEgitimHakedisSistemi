using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentIcmalServiceTests
{
    [Fact]
    public void BuildPeriods_ShouldMatchXlsmTenMonthCycle()
    {
        var periods = ProgressPaymentIcmalService.BuildPeriods(new DateOnly(2026, 10, 31));

        Assert.Equal(10, periods.Count);
        Assert.Equal(new DateOnly(2026, 9, 1), periods[0]);
        Assert.Equal(new DateOnly(2027, 6, 1), periods[^1]);
    }

    [Fact]
    public void FindPreviousDeduction_ShouldUseImmediatePreviousPaymentOnly()
    {
        var older = History(1, new DateOnly(2026, 9, 30), 100m);
        var immediatePrevious = History(2, new DateOnly(2026, 10, 31), 200m);
        var current = History(3, new DateOnly(2026, 11, 30), 300m);

        var result = ProgressPaymentIcmalService.FindPreviousDeduction(
            new[] { older, immediatePrevious, current }, current);

        Assert.Equal(200m, result);
    }

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
