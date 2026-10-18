using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// The latest Special Education workbook's report D28 and calculation AG use
/// separate formulas. The report path adds the raw current price difference and
/// rounds D13 and D26 separately to reproduce D28.
/// </summary>
public sealed class CurrentXlsmReportNetFormulaTests
{
    [Fact]
    public void CurrentSpecialEducationRows5And6_ShouldMatchD28Formula()
    {
        var examples = new[]
        {
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 19m,
                    DailyPrice: 2704.96m,
                    DiscountAmount: 250m,
                    PriceDifference: 1990m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    GuideWorkDays: 30m,
                    GuideDailyPrice: 1362.49m,
                    GuideDiscountAmount: 550m,
                    GuidePriceDifference: 1766m,
                    ProgramType: TransportationProgramType.SpecialEducation),
                ExpectedReportNet = 101351.37m,
                SourceReportD28 = 101351.37m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 19m,
                    DailyPrice: 2191.25m,
                    DiscountAmount: 350m,
                    PriceDifference: 2200m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 0m,
                    SocialSecurityDebt: 0m,
                    MinimumWageSupport: 0m,
                    InspectionPenalty: 0m,
                    GuideWorkDays: 30m,
                    GuideDailyPrice: 1362.49m,
                    GuideDiscountAmount: 650m,
                    GuidePriceDifference: 1875m,
                    ProgramType: TransportationProgramType.SpecialEducation),
                ExpectedReportNet = 91618.81m,
                SourceReportD28 = 91618.81m
            }
        };

        foreach (var example in examples)
        {
            var calculation = ProgressPaymentCalculator.Calculate(example.Input);
            var current = new ProgressPaymentHistoryData(
                Id: Guid.NewGuid(),
                CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
                Input: example.Input,
                Calculation: calculation,
                ProgressPaymentNumber: example.Input.PriceDifference == 1990m ? 2 : 3,
                PeriodDate: new DateOnly(2026, 10, 31),
                RouteNumber: example.Input.PriceDifference == 1990m ? "02" : "03",
                RouteName: example.Input.PriceDifference == 1990m ? "İkinci Güzergâh" : "Üçüncü Güzergâh",
                WorkName: "Örnek Güzergâh",
                ContractorName: "Örnek Yüklenici",
                TenderNumber: "TEST-IHALE",
                ContractNumber: "TEST-SOZ",
                ContractorTaxOrIdentityNumber: "12345678901");

            var totals = ProgressPaymentReportService.Calculate(current, new[] { current });
            var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);

            Assert.Equal(example.ExpectedReportNet, reportNet);
            Assert.Equal(example.SourceReportD28, reportNet);
        }
    }

    [Fact]
    public void CurrentSpecialEducationRow4_ReportNetShouldMatchD28()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19m,
            DailyPrice: 2608.26m,
            DiscountAmount: 150m,
            PriceDifference: 1850m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 30m,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 450m,
            GuidePriceDifference: 1530m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var current = new ProgressPaymentHistoryData(
            Id: Guid.NewGuid(),
            CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
            Input: input,
            Calculation: calculation,
            ProgressPaymentNumber: 1,
            PeriodDate: new DateOnly(2026, 10, 31),
            RouteNumber: "1",
            RouteName: "Örnek Güzergâh",
            WorkName: "Örnek Güzergâh",
            ContractorName: "Örnek Yüklenici",
            TenderNumber: "TEST-IHALE",
            ContractNumber: "TEST-SOZ",
            ContractorTaxOrIdentityNumber: "12345678901");

        var reportTotals = ProgressPaymentReportService.Calculate(current, new[] { current });
        var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, reportTotals);

        // Current XLSM recalculated in a disposable LibreOffice copy:
        // calculation AG4 = 95,527.1346528; report D28 = 98,907.13.
        Assert.Equal(98907.13m, reportNet);
    }
}
