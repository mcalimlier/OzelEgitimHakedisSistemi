using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Explicitly records a known mismatch between the latest Basic/Secondary XLSM
/// row 4 and the current calculator. This is a blocker, not a parity-success test.
/// </summary>
public sealed class CurrentBasicWorkbookDiscrepancyTests
{
    [Fact]
    public void CurrentBasicRows5And6_ShouldRepeatTheSourceGuaranteeStampAndNetGap()
    {
        var examples = new[]
        {
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 15m,
                    DailyPrice: 6545m,
                    DiscountAmount: 250m,
                    PriceDifference: 1501m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 1500m,
                    SocialSecurityDebt: 1500m,
                    MinimumWageSupport: 1500m,
                    InspectionPenalty: 1500m,
                    ProgramType: TransportationProgramType.BasicOrSecondary,
                    ManualAdditionalGuarantee: 1500m),
                SourceTransportNet = 99426m,
                SourceGross = 109368.6m,
                SourceStampTax = 956.77848m,
                SourceTotalDeductions = 13428.07848m,
                SourceNet = 95940.52152m,
                SourceReportD28 = 97441.51m,
                ExpectedReportNet = 95940.52m
            },
            new
            {
                Input = new ProgressPaymentInput(
                    WorkDays: 15m,
                    DailyPrice: 6135m,
                    DiscountAmount: 330m,
                    PriceDifference: 1502m,
                    ContractAmount: 100000m,
                    IsVatExempt: false,
                    TaxDebt: 1500m,
                    SocialSecurityDebt: 1500m,
                    MinimumWageSupport: 1500m,
                    InspectionPenalty: 1500m,
                    ProgramType: TransportationProgramType.BasicOrSecondary),
                SourceTransportNet = 93197m,
                SourceGross = 102516.7m,
                SourceStampTax = 897.72756m,
                SourceTotalDeductions = 13057.57756m,
                SourceNet = 89459.12244m,
                SourceReportD28 = 90961.10m,
                ExpectedReportNet = 89459.12m
            }
        };

        foreach (var example in examples)
        {
            var result = ProgressPaymentCalculator.Calculate(example.Input);

            Assert.Equal(example.SourceTransportNet, result.TransportNetAmount);
            Assert.Equal(example.SourceGross, result.GrossAmount);

            // Current workbook: T is manually entered as 1500; U=(K*9.48/1000)+(T*9.48/1000).
            Assert.Equal(1500m, result.AdditionalGuarantee);
            Assert.Equal(example.SourceStampTax, result.StampTax);
            Assert.Equal(example.SourceTotalDeductions, result.TotalDeductions);
            Assert.Equal(example.SourceNet, result.NetPayable);

            var current = new ProgressPaymentHistoryData(
                Id: Guid.NewGuid(),
                CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
                Input: example.Input,
                Calculation: result,
                ProgressPaymentNumber: example.Input.PriceDifference == 1501m ? 2 : 3,
                PeriodDate: new DateOnly(2026, 10, 31),
                RouteNumber: example.Input.PriceDifference == 1501m ? "02" : "03",
                RouteName: "Örnek Güzergâh",
                WorkName: "Örnek Güzergâh",
                ContractorName: "Örnek Yüklenici",
                TenderNumber: "TEST-IHALE",
                ContractNumber: "TEST-SOZ",
                ContractorTaxOrIdentityNumber: "12345678901");
            var reportTotals = ProgressPaymentReportService.Calculate(current, new[] { current });
            var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, reportTotals);

            Assert.Equal(example.ExpectedReportNet, reportNet);
            Assert.NotEqual(example.SourceReportD28, reportNet);
        }
    }

    [Fact]
    public void CurrentBasicRow4_ShouldKeepGuaranteeAndStampTaxGapVisibleUntilDecision()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 15m,
            DailyPrice: 14606m,
            DiscountAmount: 100m,
            PriceDifference: 1500m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 1500m,
            SocialSecurityDebt: 1500m,
            MinimumWageSupport: 1500m,
            InspectionPenalty: 1500m,
            ProgramType: TransportationProgramType.BasicOrSecondary,
            ManualAdditionalGuarantee: 1500m);

        var result = ProgressPaymentCalculator.Calculate(input);

        // Latest workbook's Hesaplama Bilgileri row 4, recalculated in a disposable
        // LibreOffice copy (not Excel/VBA acceptance): K=220490, L=22049,
        // M=11024.5, T=1500 (literal), U=2104.4652, V=20628.9652, W=221910.0348.
        Assert.Equal(220490m, result.TransportNetAmount);
        Assert.Equal(22049m, result.TransportVat);
        Assert.Equal(11024.5m, result.TransportWithholding);
        Assert.Equal(242539m, result.GrossAmount);

        // The manually entered T=1500 is used as-is and included in the source stamp-tax base.
        Assert.Equal(1500m, result.AdditionalGuarantee);
        Assert.Equal(2104.4652m, result.StampTax);
        Assert.Equal(20628.9652m, result.TotalDeductions);
        Assert.Equal(221910.0348m, result.NetPayable);

        var current = new ProgressPaymentHistoryData(
            Id: Guid.NewGuid(),
            CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
            Input: input,
            Calculation: result,
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

        // Hesaplama Bilgileri!W4 rounds to 221,910.03; Hakediş Raporu!D28 remains 223,410.03.
        // This unresolved source-report discrepancy is preserved, not silently normalized.
        Assert.Equal(221910.03m, reportNet);
        Assert.NotEqual(223410.03m, reportNet);
    }
}
