using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Source-cell vectors transcribed from the latest uploaded Special Education XLSM:
/// 02-Özel Eğitim Hakediş Ödeme (1)(2).xlsm
/// SHA-256 c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e.
/// Cached values are asserted; this is not desktop Excel/VBA execution.
/// </summary>
public sealed class CurrentXlsmSourceParityTests
{
    [Theory]
    [InlineData(16, 2320, 1020, 7530, 23, 1362.49, 800, 3500, 1200, 2500, 12560, 1362.49,
        43630, 34037.27, 11170.454, 8308.2086, 661.8, 736.2857196, 27328.7843196, 88837.724, 61508.9396804)]
    [InlineData(16, 1894, 1200, 5250, 23, 1362.49, 680, 3300, 1500, 3000, 13100, 1454.65,
        34354, 33957.27, 10226.854, 7829.??, 513, 647.5908396, 28045.2494396, 78538.124, 50492.8745604)]
    [InlineData(16, 2184, 1300, 3980, 23, 1362.49, 930, 5700, 5500, 3500, 14850, 1819.72,
        37624, 36107.27, 10983.854, 8380.5086, 580.8, 698.9724396, 35330.0010396, 84715.124, 49385.1229604)]
    public void LatestSpecialEducationRows4To6_ShouldMatchWorkbookCalculationCells(
        decimal workDays, decimal dailyPrice, decimal discount, decimal priceDifference,
        decimal guideDays, decimal guideDailyPrice, decimal guideDiscount, decimal guidePriceDifference,
        decimal taxDebt, decimal inspectionPenalty, decimal socialSecurityDebt, decimal minimumWageSupport,
        decimal sourceTransportNet, decimal sourceGuideNet, decimal sourceVat, decimal sourceWithholding,
        decimal sourceGuarantee, decimal sourceStampTax, decimal sourceDeductions,
        decimal sourceGross, decimal sourceNet)
    {
        var input = new ProgressPaymentInput(
            WorkDays: workDays, DailyPrice: dailyPrice, DiscountAmount: discount,
            PriceDifference: priceDifference, ContractAmount: 100000m, IsVatExempt: false,
            TaxDebt: taxDebt, SocialSecurityDebt: socialSecurityDebt,
            MinimumWageSupport: minimumWageSupport, InspectionPenalty: inspectionPenalty,
            GuideWorkDays: guideDays, GuideDailyPrice: guideDailyPrice,
            GuideDiscountAmount: guideDiscount, GuidePriceDifference: guidePriceDifference,
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);
        Assert.Equal(sourceTransportNet, result.TransportNetAmount);
        Assert.Equal(sourceGuideNet, result.GuideNetAmount);
        Assert.Equal(sourceVat, result.Vat);
        Assert.Equal(sourceWithholding, result.Withholding);
        Assert.Equal(sourceGuarantee, result.AdditionalGuarantee);
        Assert.Equal(sourceStampTax, result.StampTax);
        Assert.Equal(sourceDeductions, result.TotalDeductions);
        Assert.Equal(sourceGross, result.GrossAmount);
        Assert.Equal(sourceNet, result.NetPayable);
    }
}
