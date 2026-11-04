using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Latest uploaded Special Education XLSM source rows 4-6.
/// SHA-256: c31cb024ac1eb59f3b88d2ee12234be98d1f07056884374d8dad108f674d7d1e.
/// Cached-cell regression only; not desktop Excel/VBA execution.
/// </summary>
public sealed class CurrentXlsmSourceParityTests
{
    [Fact]
    public void LatestSpecialEducationRows4To6_ShouldMatchWorkbookCalculationCells()
    {
        var rows = new[]
        {
            (Input: new ProgressPaymentInput(
                WorkDays: 16m, DailyPrice: 2320m, DiscountAmount: 1020m, PriceDifference: 7530m,
                ContractAmount: 100000m, IsVatExempt: false, TaxDebt: 1200m,
                SocialSecurityDebt: 12560m, MinimumWageSupport: 1362.49m, InspectionPenalty: 2500m,
                GuideWorkDays: 23m, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 800m,
                GuidePriceDifference: 3500m, ProgramType: TransportationProgramType.SpecialEducation),
                TransportNet: 43630m, GuideNet: 34037.27m, Vat: 11170.454m, Withholding: 8308.2086m,
                Guarantee: 661.8m, StampTax: 736.2857196m, Deductions: 27328.7843196m,
                Gross: 88837.724m, Net: 61508.9396804m),
            (Input: new ProgressPaymentInput(
                WorkDays: 16m, DailyPrice: 1894m, DiscountAmount: 1200m, PriceDifference: 5250m,
                ContractAmount: 100000m, IsVatExempt: false, TaxDebt: 1500m,
                SocialSecurityDebt: 13100m, MinimumWageSupport: 1454.65m, InspectionPenalty: 3000m,
                GuideWorkDays: 23m, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 680m,
                GuidePriceDifference: 3300m, ProgramType: TransportationProgramType.SpecialEducation),
                TransportNet: 34354m, GuideNet: 33957.27m, Vat: 10226.854m, Withholding: 7830.0086m,
                Guarantee: 513m, StampTax: 647.5908396m, Deductions: 28045.2494396m,
                Gross: 78538.124m, Net: 50492.8745604m),
            (Input: new ProgressPaymentInput(
                WorkDays: 16m, DailyPrice: 2184m, DiscountAmount: 1300m, PriceDifference: 3980m,
                ContractAmount: 100000m, IsVatExempt: false, TaxDebt: 5500m,
                SocialSecurityDebt: 14850m, MinimumWageSupport: 1819.72m, InspectionPenalty: 3500m,
                GuideWorkDays: 23m, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 930m,
                GuidePriceDifference: 5700m, ProgramType: TransportationProgramType.SpecialEducation),
                TransportNet: 37624m, GuideNet: 36107.27m, Vat: 10983.854m, Withholding: 8380.5086m,
                Guarantee: 580.8m, StampTax: 698.9724396m, Deductions: 35330.0010396m,
                Gross: 84715.124m, Net: 49385.1229604m)
        };

        foreach (var row in rows)
        {
            var result = ProgressPaymentCalculator.Calculate(row.Input);
            Assert.Equal(row.TransportNet, result.TransportNetAmount);
            Assert.Equal(row.GuideNet, result.GuideNetAmount);
            Assert.Equal(row.Vat, result.Vat);
            Assert.Equal(row.Withholding, result.Withholding);
            Assert.Equal(row.Guarantee, result.AdditionalGuarantee);
            Assert.Equal(row.StampTax, result.StampTax);
            Assert.Equal(row.Deductions, result.TotalDeductions);
            Assert.Equal(row.Gross, result.GrossAmount);
            Assert.Equal(row.Net, result.NetPayable);
        }
    }
}
