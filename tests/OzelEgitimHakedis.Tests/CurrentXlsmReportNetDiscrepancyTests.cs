using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Program-specific source regression: Special Education D28 matches AG4 at
/// currency precision. The latest Basic source row shows D28 can differ from raw W4
/// by one kuruş because its report totals are rounded separately.
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
                ExpectedReportNet = 97595.37m,
                SourceReportD28 = 97595.37m
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
                ExpectedReportNet = 87543.81m,
                SourceReportD28 = 87543.81m
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

            Assert.Equal(decimal.Round(calculation.NetPayable, 2, MidpointRounding.AwayFromZero), reportNet);
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

        // Source D28 rounds the report gross and deduction totals separately.
        Assert.Equal(332516.08m, reportNet);
    }

    [Fact]
    public void UploadedSpecialEducationWorkbookRow4_ReportNetShouldMatchD28()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 16m,
            DailyPrice: 2320m,
            DiscountAmount: 1020m,
            PriceDifference: 7530m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 1200m,
            SocialSecurityDebt: 12560m,
            MinimumWageSupport: 1362.49m,
            InspectionPenalty: 2500m,
            GuideWorkDays: 23m,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 800m,
            GuidePriceDifference: 3500m,
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

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });
        var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);

        // Accepted source mapping: Hakediş Raporu!D28 = Hesaplama!AG4, at currency precision.
        Assert.Equal(decimal.Round(calculation.NetPayable, 2, MidpointRounding.AwayFromZero), reportNet);
    }

    [Fact]
    public void UploadedBasicEducationWorkbookRow4_ReportNetShouldMatchD28()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 16m,
            DailyPrice: 19539.1608m,
            DiscountAmount: 0m,
            PriceDifference: 7365.35m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);
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

        var totals = ProgressPaymentReportService.Calculate(current, new[] { current });
        var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);

        // Latest Basic source row: report D28 follows separately rounded report totals.
        Assert.Equal(332516.08m, reportNet);
    }
}
