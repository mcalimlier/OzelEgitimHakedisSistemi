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
    public void CalendarCapacity_ShouldRejectImpossibleDaysWithoutAssumingSchoolPuantaj()
    {
        var partialMonth = new ProgressPaymentMonthOption(
            new DateOnly(2026, 9, 14), new DateOnly(2026, 9, 30));
        Assert.Null(ProgressPaymentMonthService.GetCalendarCapacityError(partialMonth, 17m, 17m));
        Assert.Null(ProgressPaymentMonthService.GetCalendarCapacityError(partialMonth, 0m, 0m));

        var tooManyTransport = ProgressPaymentMonthService.GetCalendarCapacityError(
            partialMonth, 18m, 0m);
        Assert.Contains("17 takvim", tooManyTransport, StringComparison.Ordinal);
        var tooManyGuide = ProgressPaymentMonthService.GetCalendarCapacityError(
            partialMonth, 1m, 18m);
        Assert.Contains("Rehber", tooManyGuide, StringComparison.Ordinal);

        var february = new ProgressPaymentMonthOption(
            new DateOnly(2027, 2, 1), new DateOnly(2027, 2, 28));
        Assert.Null(ProgressPaymentMonthService.GetCalendarCapacityError(february, 28m, 28m));
        Assert.NotNull(ProgressPaymentMonthService.GetCalendarCapacityError(february, 29m, 0m));
        Assert.NotNull(ProgressPaymentMonthService.GetCalendarCapacityError(february, -1m, 0m));
    }

    [Fact]
    public void CalendarCapacity_ShouldRejectInvalidCrossMonthPeriod()
    {
        var period = new ProgressPaymentMonthOption(
            new DateOnly(2026, 9, 14), new DateOnly(2026, 10, 2));
        Assert.NotNull(ProgressPaymentMonthService.GetCalendarCapacityError(period, 1m, 0m));
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
