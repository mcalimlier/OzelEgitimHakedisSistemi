using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentMonthServiceTests
{
    [Fact]
    public void SeptemberBeginningMidMonth_ShouldClipPeriodToContractStart()
    {
        var options = ProgressPaymentMonthService.BuildOptions(new DateOnly(2026, 9, 14), new DateOnly(2027, 6, 30));
        var september = Assert.Single(options.Where(x => x.StartDate.Month == 9 && x.StartDate.Year == 2026));

        Assert.Equal(new DateOnly(2026, 9, 14), september.StartDate);
        Assert.Equal(new DateOnly(2026, 9, 30), september.EndDate);
        Assert.Equal(new DateOnly(2026, 9, 30), september.PeriodDate);
        Assert.Contains("Eylül 2026", september.DisplayText, StringComparison.Ordinal);
        Assert.Equal(new DateOnly(2026, 10, 1), options[1].StartDate);
        Assert.Equal(new DateOnly(2026, 10, 31), options[1].EndDate);
    }

    [Fact]
    public void FinalMonth_ShouldClipPeriodToContractEnd()
    {
        var options = ProgressPaymentMonthService.BuildOptions(new DateOnly(2026, 9, 14), new DateOnly(2026, 10, 15));
        Assert.Equal(2, options.Count);
        Assert.Equal(new DateOnly(2026, 10, 1), options[1].StartDate);
        Assert.Equal(new DateOnly(2026, 10, 15), options[1].EndDate);
    }

    [Fact]
    public void FullCalendarYear_ShouldOfferOneOptionPerMonth()
    {
        var options = ProgressPaymentMonthService.BuildOptions(new DateOnly(2026, 1, 1), new DateOnly(2026, 12, 31));
        Assert.Equal(12, options.Count);
        Assert.Equal(new DateOnly(2026, 1, 31), options[0].PeriodDate);
        Assert.Equal(new DateOnly(2026, 12, 31), options[^1].PeriodDate);
    }

    [Fact]
    public void FindForPeriod_ShouldSelectMatchingMonthAndYear()
    {
        var options = ProgressPaymentMonthService.BuildOptions(new DateOnly(2026, 9, 14), new DateOnly(2027, 9, 30));
        var selected = ProgressPaymentMonthService.FindForPeriod(options, new DateOnly(2027, 9, 30));

        Assert.NotNull(selected);
        Assert.Equal(new DateOnly(2027, 9, 1), selected!.StartDate);
    }
}
