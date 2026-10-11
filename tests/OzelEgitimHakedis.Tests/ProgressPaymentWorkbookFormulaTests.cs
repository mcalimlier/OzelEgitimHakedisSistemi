using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentWorkbookFormulaTests
{
    [Fact]
    public void ReportTotals_ShouldMatchRawPriceDifferenceCellsAndDiscountedServiceBase()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1,
            DailyPrice: 100m,
            DiscountAmount: 10m,
            PriceDifference: 25m,
            ContractAmount: 1000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 1,
            GuideDailyPrice: 50m,
            GuideDiscountAmount: 5m,
            GuidePriceDifference: 15m);

        var previous = History(1, new DateOnly(2026, 9, 30), input);
        var current = History(2, new DateOnly(2026, 10, 31), input);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        // K+T = 175; service amount excluding raw I+R is 135 after J/S discounts.
        // D7 = I+R = 40; source D11 = H2 + D7 = 215.
        Assert.Equal(270m, totals.CumulativeServiceAmount);
        Assert.Equal(40m, totals.CurrentPriceDifference);
        Assert.Equal(310m, totals.CumulativeTotal);
        Assert.Equal(135m, totals.PreviousTotal);
        Assert.Equal(215m, totals.CurrentTotal);
    }

    [Fact]
    public void ReportNetPayable_ShouldRoundGrossAndDeductionsSeparatelyLikeExcelD28()
    {
        var current = History(
            1,
            new DateOnly(2026, 10, 31),
            new ProgressPaymentInput(1, 100m, 0m, 0m, 1000m, true, 0m, 0m, 0m));

        current = current with
        {
            Calculation = current.Calculation with
            {
                Vat = 0.005m,
                TotalDeductions = 10.005m
            }
        };

        var totals = new ProgressPaymentReportTotals(
            CumulativeServiceAmount: 0m,
            CurrentPriceDifference: 0m,
            CumulativeTotal: 0m,
            PreviousTotal: 0m,
            CurrentTotal: 200.005m);

        Assert.Equal(190.00m, ProgressPaymentReportService.CalculateReportNetPayable(current, totals));
    }

    private static ProgressPaymentHistoryData History(
        int number,
        DateOnly period,
        ProgressPaymentInput input) =>
        new(
            Guid.NewGuid(),
            period.ToDateTime(TimeOnly.MinValue),
            input,
            ProgressPaymentCalculator.Calculate(input),
            number,
            period,
            "1",
            "Hat 1",
            "İş",
            "Firma",
            "TENDER",
            "SOZ",
            ContractorTaxOrIdentityNumber: "TEST-VKN-01");
}
