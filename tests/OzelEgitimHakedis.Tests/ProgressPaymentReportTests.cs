using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentReportTests
{
    private static readonly string[] DocumentNames =
    {
        "Hakediş Raporu",
        "Hizmet İşleri Hakediş Raporu",
        "Hizmet İşleri Kabul Tutanağı",
        "Hakediş İcmali"
    };
    [Fact]
    public void ReportHistory_ShouldOrderByPeriodDateNotCreatedAt()
    {
        var first = History(1, new DateOnly(2026, 10, 31), new DateTime(2026, 11, 5), 100m, 5m);
        var second = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 10, 1), 150m, 10m);

        var ordered = ProgressPaymentReportService.OrderByPaymentPeriod(new[] { second, first });

        Assert.Equal(first.Id, ordered[0].Id);
        Assert.Equal(second.Id, ordered[1].Id);
    }

    [Fact]
    public void CurrentPeriodPriceDifference_ShouldRemainInCurrentReport()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 5m);
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 12, 10), 150m, 10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(250m, totals.CumulativeServiceAmount);
        Assert.Equal(10m, totals.CurrentPriceDifference);
        Assert.Equal(260m, totals.CumulativeTotal);
        Assert.Equal(100m, totals.PreviousTotal);
        Assert.Equal(160m, totals.CurrentTotal);
    }

    [Fact]
    public void ReportTotals_ShouldExcludePaymentsFromPriorFiscalYear()
    {
        var priorFiscalYear = History(
            1,
            new DateOnly(2026, 8, 31),
            new DateTime(2026, 8, 31),
            500m,
            40m);
        var currentSeptember = History(
            2,
            new DateOnly(2026, 9, 30),
            new DateTime(2026, 9, 30),
            100m,
            5m);

        var totals = ProgressPaymentReportService.Calculate(
            currentSeptember,
            new[] { priorFiscalYear, currentSeptember });

        Assert.Equal(100m, totals.CumulativeServiceAmount);
        Assert.Equal(5m, totals.CurrentPriceDifference);
        Assert.Equal(105m, totals.CumulativeTotal);
        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(105m, totals.CurrentTotal);
    }

    [Fact]
    public void OfficialDocumentNames_ShouldContainTheFourRequiredDocuments()
    {
        var names = ProgressPaymentReportService.RequiredDocumentNames;

        Assert.Equal(
            new[]
            {
                "Hakediş Raporu",
                "Hizmet İşleri Hakediş Raporu",
                "Hizmet İşleri Kabul Tutanağı",
                "Hakediş İcmali"
            },
            names);
    }

    private static ProgressPaymentHistoryData History(
        int number,
        DateOnly period,
        DateTime createdAt,
        decimal serviceAmount,
        decimal priceDifference) =>
        new(
            Guid.NewGuid(),
            createdAt,
            new ProgressPaymentInput(1, serviceAmount, 0m, priceDifference, 100000m, false, 0m, 0m, 0m),
            new ProgressPaymentCalculation(
                serviceAmount,
                priceDifference,
                serviceAmount + priceDifference,
                0m,
                0m,
                serviceAmount + priceDifference,
                0m,
                serviceAmount + priceDifference,
                0m,
                serviceAmount + priceDifference,
                0m,
                new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)),
            number,
            period,
            "1",
            "Hat 1",
            "İş",
            "Firma",
            "TENDER",
            "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");

    [Fact]
    public void FinalXlsmRoute1Report_ShouldMatchWorkbookReference()
    {
        var previous = History(
            1,
            new DateOnly(2026, 9, 30),
            new DateTime(2026, 10, 1),
            116514.24m,
            0m);

        var current = History(
            2,
            new DateOnly(2026, 10, 31),
            new DateTime(2026, 11, 1),
            116514.24m,
            1500m);

        var totals = ProgressPaymentReportService.Calculate(
            current,
            new[] { previous, current });

        Assert.Equal(233028.48m, totals.CumulativeServiceAmount);
        Assert.Equal(1500m, totals.CurrentPriceDifference);
        Assert.Equal(234528.48m, totals.CumulativeTotal);
        Assert.Equal(116514.24m, totals.PreviousTotal);
        Assert.Equal(118014.24m, totals.CurrentTotal);
    }


    [Fact]
    public void CurrentTotal_ShouldEqualCumulativeTotalMinusPreviousTotal()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 25m);
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 1), 100m, 10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(210m, totals.CumulativeTotal); // C = cumulative contract work + current F
        Assert.Equal(100m, totals.PreviousTotal);   // D = prior cumulative contract work
        Assert.Equal(110m, totals.CurrentTotal);    // E = current contract work + current F
        Assert.Equal(totals.CumulativeTotal - totals.PreviousTotal, totals.CurrentTotal);
    }


    [Fact]
    public void NegativeCurrentPriceDifference_ShouldBeAppliedOnceAndRemainNegative()
    {
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 1), 100m, -10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });

        Assert.Equal(90m, totals.CurrentTotal);
    }

}
