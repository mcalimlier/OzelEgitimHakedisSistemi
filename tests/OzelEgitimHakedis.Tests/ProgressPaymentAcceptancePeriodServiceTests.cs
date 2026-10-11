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
}
