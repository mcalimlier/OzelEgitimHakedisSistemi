using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Regression vectors for the latest Basic/Secondary XLSM. Calculation-sheet
/// W4 and report D28 are separate workbook formula chains and are asserted separately.
/// </summary>
public sealed class CurrentBasicWorkbookParityTests
{
    [Fact]
    public void CurrentBasicRows5And6_ShouldMatchCalculationAndReportCells()
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
                SourceStampTax = 942.15648m,
                SourceTotalDeductions = 12003.51648m,
                SourceNet = 97365.08352m,
                SourceReportD28 = 98851.45m,
                ExpectedReportNet = 98851.45m
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
                    ProgramType: TransportationProgramType.BasicOrSecondary,
                    ManualAdditionalGuarantee: 1500m),
                SourceTransportNet = 93197m,
                SourceGross = 102516.7m,
                SourceStampTax = 883.50756m,
                SourceTotalDeductions = 11633.47756m,
                SourceNet = 90883.22244m,
                SourceReportD28 = 92370.98m,
                ExpectedReportNet = 92370.98m
            }
        };

        foreach (var example in examples)
        {
            var result = ProgressPaymentCalculator.Calculate(example.Input);

            Assert.Equal(example.SourceTransportNet, result.TransportNetAmount);
            Assert.Equal(example.SourceGross, result.GrossAmount);

            // Latest Basic XLSM: T=I*6%; U=K*9.48/1000. The legacy manual input is ignored.
            Assert.Equal(example.Input.PriceDifference * 0.06m, result.AdditionalGuarantee);
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
            Assert.Equal(example.SourceReportD28, reportNet);
        }
    }

    [Fact]
    public void CurrentBasicRow4_ShouldMatchCalculationSheetAndReportD28()
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

        // Latest workbook's Hesaplama Bilgileri row 4: K=220490, L=22049,
        // M=11024.5, T=I*6%=90, U=K*9.48/1000=2090.2452.
        // LibreOffice cached/recalculated values are not Excel/VBA acceptance.
        Assert.Equal(220490m, result.TransportNetAmount);
        Assert.Equal(22049m, result.TransportVat);
        Assert.Equal(11024.5m, result.TransportWithholding);
        Assert.Equal(242539m, result.GrossAmount);

        // Source formulas T=I*6%, U=K*9.48/1000, V includes T+U+M, W=O-V.
        Assert.Equal(90m, result.AdditionalGuarantee);
        Assert.Equal(2090.2452m, result.StampTax);
        Assert.Equal(19204.7452m, result.TotalDeductions);
        Assert.Equal(223334.2548m, result.NetPayable);

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

        // Hakediş Raporu D11=K+I, D13=D11+D12, D28=D13-D26.
        Assert.Equal(224820.03m, reportNet);
    }
}
