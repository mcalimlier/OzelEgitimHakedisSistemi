using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentAcceptancePeriodServiceTests
{
    [Fact]
    public void BuildServicePeriod_ShouldUseSameMonthAsPaymentPeriod()
    {
        var period = ProgressPaymentAcceptancePeriodService.BuildServicePeriod(
            new DateOnly(2026, 10, 31));

        Assert.Equal(new DateOnly(2026, 10, 1), period.Start);
        Assert.Equal(new DateOnly(2026, 10, 31), period.End);
    }

    [Fact]
    public void BuildServicePeriod_ShouldClipFirstAndLastMonthsToContractDates()
    {
        var first = ProgressPaymentAcceptancePeriodService.BuildServicePeriod(
            new DateOnly(2026, 9, 30),
            new DateOnly(2026, 9, 14),
            new DateOnly(2026, 11, 20));
        var last = ProgressPaymentAcceptancePeriodService.BuildServicePeriod(
            new DateOnly(2026, 11, 20),
            new DateOnly(2026, 9, 14),
            new DateOnly(2026, 11, 20));

        Assert.Equal(new DateOnly(2026, 9, 14), first.Start);
        Assert.Equal(new DateOnly(2026, 9, 30), first.End);
        Assert.Equal(new DateOnly(2026, 11, 1), last.Start);
        Assert.Equal(new DateOnly(2026, 11, 20), last.End);
    }

}
