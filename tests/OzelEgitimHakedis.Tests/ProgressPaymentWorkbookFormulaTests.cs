using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentWorkbookFormulaTests
{
    [Fact]
    public void ReportTotals_ShouldMatchExcelCurrentPriceDifferenceAndCumulativeTotals()
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

        // Excel D6 = (prior K+T + current K+T) - current I+R.
        // Excel D7 = current I+R only; D8 = D6 + D7.
        // A=310 (prior K+T 175 + current base work 135); B=40; C=350.
        Assert.Equal(310m, totals.CumulativeServiceAmount);
        Assert.Equal(40m, totals.CumulativePriceDifference);
        Assert.Equal(350m, totals.CumulativeTotal);
        Assert.Equal(175m, totals.PreviousTotal);
        Assert.Equal(175m, totals.CurrentTotal);
    }

    [Fact]
    public void BasicReportNetPayable_ShouldUseZeroStampTaxWhenD11IsNegativeLikeExcel()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: -200m,
            ContractAmount: 1000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);
        var current = History(1, new DateOnly(2026, 10, 31), input);
        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });

        Assert.Equal(-0.948m, current.Calculation.StampTax);
        Assert.Equal(0m, ProgressPaymentReportService.CalculateReportStampTax(
            current.Calculation, current.Input.ProgramType));
        // The workbook calculation sheet keeps T=I*6% signed; report D15 clamps
        // negative stamp tax to zero, but this does not erase the negative T deduction.
        Assert.Equal(-12m, current.Calculation.AdditionalGuarantee);
        Assert.Equal(-12m, ProgressPaymentReportService.CalculateReportTotalDeductions(
            current.Calculation, current.Input.ProgramType));
        // Do not assert a synthetic D28/net figure here: this input is not a captured Excel row.
        // Report D28 parity is covered only by source-derived vectors.
    }

    [Fact]
    public void SpecialEducationReport_ShouldUseCalculationSheetStampTaxAsIs()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: -200m,
            ContractAmount: 1000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var current = History(1, new DateOnly(2026, 10, 31), input);

        Assert.Equal(current.Calculation.StampTax,
            ProgressPaymentReportService.CalculateReportStampTax(
                current.Calculation, current.Input.ProgramType));
    }

    [Fact]
    public void BasicReportNetPayable_ShouldRoundReportTotalsSeparatelyLikeSourceD28()
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
            CumulativePriceDifference: 0m,
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

    [Fact]
    public void AdditionalGuarantee_ShouldApplySixPercentToEachSignedPriceDifference()
    {
        var scenarios = new (decimal TransportPriceDifference, decimal GuidePriceDifference, decimal ExpectedGuarantee)[]
        {
            (-100m, 50m, -3m),
            (-100m, -50m, -9m),
            (100m, -50m, 3m)
        };

        foreach (var scenario in scenarios)
        {
            var input = new ProgressPaymentInput(
                WorkDays: 0,
                DailyPrice: 0m,
                DiscountAmount: 0m,
                PriceDifference: scenario.TransportPriceDifference,
                ContractAmount: 1000m,
                IsVatExempt: true,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                GuideWorkDays: 0,
                GuideDailyPrice: 0m,
                GuideDiscountAmount: 0m,
                GuidePriceDifference: scenario.GuidePriceDifference,
                ProgramType: TransportationProgramType.SpecialEducation);

            var result = ProgressPaymentCalculator.Calculate(input);

            // Special Education XLSM AC = I*6% + R*6%; negative differences remain signed.
            Assert.Equal(scenario.ExpectedGuarantee, result.AdditionalGuarantee);
        }
    }

    [Fact]
    public void AdditionalGuarantee_ShouldNotIncludeTransportOrGuideDiscounts()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1,
            DailyPrice: 100m,
            DiscountAmount: 40m,
            PriceDifference: 50m,
            ContractAmount: 1000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 1,
            GuideDailyPrice: 100m,
            GuideDiscountAmount: 20m,
            GuidePriceDifference: 30m,
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        // Excel AC uses I and R only, not the net (I-J) and (R-S) values.
        Assert.Equal(4.8m, result.AdditionalGuarantee);
    }


    [Fact]
    public void ReportTotals_ShouldExcludePriorFiscalYearPayments()
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

        var previousFiscalYear = History(1, new DateOnly(2025, 9, 30), input);
        var current = History(2, new DateOnly(2026, 10, 31), input);

        var totals = ProgressPaymentReportService.Calculate(
            current, new[] { previousFiscalYear, current });

        // October 2026 belongs to the September 2026-July 2027 fiscal cycle.
        // September 2025 belongs to a different cycle and must not contribute.
        Assert.Equal(175m, totals.CurrentTotal);
        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(175m, totals.CumulativeTotal);
        Assert.Equal(135m, totals.CumulativeServiceAmount);
    }

    [Fact]
    public void ReportTotals_ShouldExcludePaymentsFromDifferentRoutes()
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

        var otherRoute = History(1, new DateOnly(2026, 9, 30), input) with
        {
            RouteNumber = "2"
        };
        var current = History(2, new DateOnly(2026, 10, 31), input);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { otherRoute, current });

        Assert.Equal(175m, totals.CurrentTotal);
        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(175m, totals.CumulativeTotal);
    }

}
