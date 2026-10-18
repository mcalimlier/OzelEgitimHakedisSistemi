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
    public void CurrentPriceDifference_ShouldMatchExcelReportFormula()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 5m);
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 12, 10), 150m, 10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(255m, totals.CumulativeServiceAmount);
        Assert.Equal(10m, totals.CumulativePriceDifference);
        Assert.Equal(265m, totals.CumulativeTotal);
        Assert.Equal(105m, totals.PreviousTotal);
        Assert.Equal(160m, totals.CurrentTotal);
    }

    [Fact]
    public void ReportTotals_ShouldIgnoreOtherRoutesProgramsContractsAndContractors()
    {
        var matchingPrevious = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 5m);
        var otherRoute = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 900m, 90m)
            with { RouteNumber = "Q3" };
        var otherProgram = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 800m, 80m)
            with
            {
                Input = new ProgressPaymentInput(
                    1, 800m, 0m, 80m, 100000m, false, 0m, 0m, 0m,
                    ProgramType: TransportationProgramType.BasicOrSecondary)
            };
        var otherContract = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 700m, 70m)
            with { ContractNumber = "OTHER-CONTRACT" };
        var otherTender = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 600m, 60m)
            with { TenderNumber = "OTHER-TENDER" };
        var otherContractor = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 500m, 50m)
            with { ContractorTaxOrIdentityNumber = "DIFFERENT-VKN" };
        var current = History(
            2, new DateOnly(2026, 11, 30), new DateTime(2026, 12, 10), 150m, 10m);

        var totals = ProgressPaymentReportService.Calculate(
            current,
            new[]
            {
                matchingPrevious, otherRoute, otherProgram, otherContract,
                otherTender, otherContractor, current
            });

        Assert.Equal(255m, totals.CumulativeServiceAmount);
        Assert.Equal(10m, totals.CumulativePriceDifference);
        Assert.Equal(265m, totals.CumulativeTotal);
        Assert.Equal(105m, totals.PreviousTotal);
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
        Assert.Equal(5m, totals.CumulativePriceDifference);
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
    public void FinalXlsmRoute1Report_ShouldSeparateContractWorkAndPriceDifference()
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
        Assert.Equal(1500m, totals.CumulativePriceDifference);
        Assert.Equal(234528.48m, totals.CumulativeTotal);
        Assert.Equal(116514.24m, totals.PreviousTotal);
        Assert.Equal(118014.24m, totals.CurrentTotal);
    }



    [Fact]
    public void SpecialEducation_SourceWorkbookExample_ShouldMatchHakedişRaporuCellsD6ToD11()
    {
        // Source XLSM values for a populated Special Education route:
        // previous K+T = 54,725.903; current K=52,443.573, T=40,874.70,
        // current raw I+R=1,403.573.
        var previousInput = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 54725.903m, DiscountAmount: 0m,
            PriceDifference: 0m, ContractAmount: 100000m, IsVatExempt: true,
            TaxDebt: 0m, SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var previousCalculation = ProgressPaymentCalculator.Calculate(previousInput);
        var previous = new ProgressPaymentHistoryData(
            Guid.NewGuid(), new DateTime(2026, 9, 30),
            previousInput, previousCalculation, 1, new DateOnly(2026, 9, 30),
            "1", "Hat 1", "İş", "Firma", "TENDER", "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");

        var currentInput = new ProgressPaymentInput(
            WorkDays: 22, DailyPrice: 2320m, DiscountAmount: 0m,
            PriceDifference: 1403.573m, ContractAmount: 100000m, IsVatExempt: true,
            TaxDebt: 0m, SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 30, GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m, GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var currentCalculation = ProgressPaymentCalculator.Calculate(currentInput);
        var current = new ProgressPaymentHistoryData(
            Guid.NewGuid(), new DateTime(2026, 10, 31),
            currentInput, currentCalculation, 2, new DateOnly(2026, 10, 31),
            "1", "Hat 1", "İş", "Firma", "TENDER", "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(146640.603m, totals.CumulativeServiceAmount); // D6
        Assert.Equal(1403.573m, totals.CumulativePriceDifference); // D7
        Assert.Equal(148044.176m, totals.CumulativeTotal);         // D8 = D6+D7
        Assert.Equal(54725.903m, totals.PreviousTotal);            // D10
        Assert.Equal(93318.273m, totals.CurrentTotal);             // D11 = current K+T

        // Source workbook cached values for the same example:
        // D13 = D11 + D12 (VAT-exempt example, so D12=0),
        // D26 = SUM(D14:F25), and D28 = ROUND(D13,2)-ROUND(D26,2).
        var reportGross = totals.CurrentTotal + currentCalculation.Vat;
        var reportDeductions = ProgressPaymentReportService.CalculateReportTotalDeductions(
            currentCalculation,
            TransportationProgramType.SpecialEducation);
        var reportNet = decimal.Round(reportGross, 2, MidpointRounding.AwayFromZero)
                        - decimal.Round(reportDeductions, 2, MidpointRounding.AwayFromZero);

        Assert.Equal(93318.273m, reportGross);                 // D13
        Assert.Equal(968.87160804m, reportDeductions);         // D26
        Assert.Equal(92349.40m, reportNet);                    // D28
    }


    [Fact]
    public void BasicOrSecondary_SyntheticTransportVector_ShouldMapReportCells()
    {
        // Synthetic regression vector for the Basic/Secondary report mapping.
        // It checks service-cell arithmetic only; it is NOT represented as a
        // reconstructed source-workbook row or a golden-master Excel result.
        var input = new ProgressPaymentInput(
            WorkDays: 13,
            DailyPrice: 6545m,
            DiscountAmount: 0m,
            PriceDifference: 3659.2m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var current = new ProgressPaymentHistoryData(
            Guid.NewGuid(),
            new DateTime(2026, 10, 31),
            input,
            calculation,
            1,
            new DateOnly(2026, 10, 31),
            "1",
            "Hat 1",
            "İş",
            "Firma",
            "TENDER",
            "SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });

        Assert.Equal(88744.2m, calculation.TransportNetAmount); // Hesaplama!K
        Assert.Equal(85085.0m, totals.CumulativeServiceAmount); // report D6 = K - I
        Assert.Equal(3659.2m, totals.CumulativePriceDifference); // report D7 = I
        Assert.Equal(88744.2m, totals.CumulativeTotal); // report D8 = D6 + D7
        Assert.Equal(0m, totals.PreviousTotal); // report D10
        Assert.Equal(88744.2m, totals.CurrentTotal); // report D11 / current K+T
    }

    [Fact]
    public void CurrentTotal_ShouldEqualCumulativeTotalMinusPreviousTotal()
    {
        var previous = History(1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 25m);
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 1), 100m, 10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(225m, totals.CumulativeServiceAmount);     // prior K+T plus current base work
        Assert.Equal(10m, totals.CumulativePriceDifference);    // current month's raw I+R only
        Assert.Equal(235m, totals.CumulativeTotal);             // C = A + B
        Assert.Equal(125m, totals.PreviousTotal);           // D = prior K+T
        Assert.Equal(110m, totals.CurrentTotal);            // E = C - D
        Assert.Equal(totals.CumulativeTotal - totals.PreviousTotal, totals.CurrentTotal);
    }


    [Fact]
    public void NegativeCumulativePriceDifference_ShouldBeAppliedOnceAndRemainNegative()
    {
        var current = History(2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 1), 100m, -10m);

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });

        Assert.Equal(100m, totals.CumulativeServiceAmount);
        Assert.Equal(-10m, totals.CumulativePriceDifference);
        Assert.Equal(90m, totals.CumulativeTotal);
        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(90m, totals.CurrentTotal);
    }


    [Fact]
    public void BasicOrSecondaryReportTotals_ShouldIgnoreLegacyGuideFields()
    {
        var current = History(
            1,
            new DateOnly(2026, 10, 31),
            new DateTime(2026, 10, 31),
            100m,
            10m) with
        {
            Input = new ProgressPaymentInput(
                WorkDays: 1,
                DailyPrice: 100m,
                DiscountAmount: 0m,
                PriceDifference: 10m,
                ContractAmount: 100000m,
                IsVatExempt: false,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary,
                GuideWorkDays: 1,
                GuideDailyPrice: 999m,
                GuideDiscountAmount: 0m,
                GuidePriceDifference: 500m)
        };

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });

        Assert.Equal(100m, ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentAmount(current));
        Assert.Equal(10m, ProgressPaymentReportService.CalculateWorkbookPriceDifference(current));
        Assert.Equal(100m, totals.CumulativeServiceAmount);
        Assert.Equal(10m, totals.CumulativePriceDifference);
        Assert.Equal(110m, totals.CumulativeTotal);
        Assert.Equal(110m, totals.CurrentTotal);
    }


    [Fact]
    public void ReportTotals_ShouldNormalizeIdentityFieldsWhenMatchingHistory()
    {
        var previous = History(
            1,
            new DateOnly(2026, 10, 31),
            new DateTime(2026, 10, 31),
            100m,
            5m) with
        {
            TenderNumber = " TENDER ",
            ContractNumber = " soz ",
            RouteNumber = " 1 ",
            ContractorTaxOrIdentityNumber = "123-456-789-01"
        };
        var current = History(
            2,
            new DateOnly(2026, 11, 30),
            new DateTime(2026, 11, 30),
            150m,
            10m) with
        {
            TenderNumber = "tender",
            ContractNumber = "SOZ",
            RouteNumber = "1",
            ContractorTaxOrIdentityNumber = "12345678901"
        };

        var totals = ProgressPaymentReportService.Calculate(
            current,
            new[] { previous, current });

        Assert.Equal(255m, totals.CumulativeServiceAmount);
        Assert.Equal(10m, totals.CumulativePriceDifference);
        Assert.Equal(265m, totals.CumulativeTotal);
        Assert.Equal(105m, totals.PreviousTotal);
        Assert.Equal(160m, totals.CurrentTotal);
    }


    [Fact]
    public void ReportTotals_ShouldNotCarryPreviousAmountsWhenCurrentIdentityIsIncomplete()
    {
        var previous = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 25m) with
        {
            ContractorTaxOrIdentityNumber = ""
        };
        var current = History(
            2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 30), 100m, 10m) with
        {
            ContractorTaxOrIdentityNumber = ""
        };

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(110m, totals.CurrentTotal);
        Assert.Equal(110m, totals.CumulativeTotal);
    }

    [Fact]
    public void ReportTotals_ShouldNotCarryPreviousAmountsWhenIdentityNormalizesToEmpty()
    {
        var previous = History(
            1, new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31), 100m, 25m) with
        {
            ContractorTaxOrIdentityNumber = "----"
        };
        var current = History(
            2, new DateOnly(2026, 11, 30), new DateTime(2026, 11, 30), 100m, 10m) with
        {
            ContractorTaxOrIdentityNumber = "----"
        };

        var totals = ProgressPaymentReportService.Calculate(current, new[] { previous, current });

        Assert.Equal(0m, totals.PreviousTotal);
        Assert.Equal(110m, totals.CurrentTotal);
        Assert.Equal(110m, totals.CumulativeTotal);
    }
}
