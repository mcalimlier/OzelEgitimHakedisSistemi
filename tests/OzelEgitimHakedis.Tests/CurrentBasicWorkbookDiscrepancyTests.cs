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
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input);

        // Latest workbook's Hesaplama Bilgileri row 4, recalculated in a disposable
        // LibreOffice copy (not Excel/VBA acceptance): K=220490, L=22049,
        // M=11024.5, T=1500 (literal), U=2104.4652, V=20628.9652, W=221910.0348.
        Assert.Equal(220490m, result.TransportNetAmount);
        Assert.Equal(22049m, result.TransportVat);
        Assert.Equal(11024.5m, result.TransportWithholding);
        Assert.Equal(242539m, result.GrossAmount);

        // OEHS currently derives T from I*6% and calculates stamp tax on K alone.
        // The source uses a literal T=1500 and includes T in U's stamp-tax formula.
        Assert.Equal(90m, result.AdditionalGuarantee);
        Assert.Equal(2090.2452m, result.StampTax);
        Assert.Equal(19204.7452m, result.TotalDeductions);
        Assert.Equal(223334.2548m, result.NetPayable);

        Assert.NotEqual(1500m, result.AdditionalGuarantee);
        Assert.NotEqual(2104.4652m, result.StampTax);
        Assert.NotEqual(20628.9652m, result.TotalDeductions);
        Assert.NotEqual(221910.0348m, result.NetPayable);

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

        // Latest source Hakediş Raporu!D28 = 223,410.03, while OEHS's
        // report service currently yields 223,334.25 for this source-derived row.
        Assert.Equal(223334.25m, reportNet);
        Assert.NotEqual(223410.03m, reportNet);
    }
}
