using System.Collections.Generic;
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
    public static IEnumerable<object[]> LatestSpecialRows =>
        new[]
        {
            new object[] { 16m, 2320m, 1020m, 7530m, 23m, 1362.49m, 800m, 3500m, 1200m, 2500m, 12560m, 1362.49m, 43630m, 34037.27m, 11170.454m, 8308.2086m, 661.8m, 736.2857196m, 27328.7843196m, 88837.724m, 61508.9396804m },
            new object[] { 16m, 1894m, 1200m, 5250m, 23m, 1362.49m, 680m, 3300m, 1500m, 3000m, 13100m, 1454.65m, 34354m, 33957.27m, 10226.854m, 7830.0086m, 513m, 647.5908396m, 28045.2494396m, 78538.124m, 50492.8745604m },
            new object[] { 16m, 2184m, 1300m, 3980m, 23m, 1362.49m, 930m, 5700m, 5500m, 3500m, 14850m, 1819.72m, 37624m, 36107.27m, 10983.854m, 8380.5086m, 580.8m, 698.9724396m, 35330.0010396m, 84715.124m, 49385.1229604m }
        };

    [Theory]
    [MemberData(nameof(LatestSpecialRows))]
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
