using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Reference vectors transcribed from populated source workbook rows and
/// independently recalculated workbook copies. These are regression fixtures;
/// they do not replace desktop Excel/VBA acceptance.
/// </summary>
public sealed class ExcelSourceParityTests
{
    [Fact]
    public void BasicOrSecondary_ExcelRow8_ShouldMatchTransportCalculationChain()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 13,
            DailyPrice: 2810m,
            DiscountAmount: 0m,
            PriceDifference: 1571.03m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(38101.03m, result.ServiceAmount);
        Assert.Equal(1571.03m, result.PriceDifferenceExcludingVat);
        Assert.Equal(38101.03m, result.TotalAmount);
        Assert.Equal(3810.103m, result.Vat);
        Assert.Equal(1905.0515m, result.Withholding);
        Assert.Equal(41911.133m, result.GrossAmount);
        Assert.Equal(94.2618m, result.AdditionalGuarantee);
        Assert.Equal(361.1977644m, result.StampTax);
        Assert.Equal(2360.5110644m, result.TotalDeductions);
        Assert.Equal(39550.6219356m, result.NetPayable);
    }

    [Fact]
    public void SpecialEducation_ExcelRow4_VatExempt_ShouldMatchTransportAndGuideChains()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 22,
            DailyPrice: 2320m,
            DiscountAmount: 0m,
            PriceDifference: 1403.573m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 30,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(52443.573m, result.TransportNetAmount);
        Assert.Equal(40874.70m, result.GuideNetAmount);
        Assert.Equal(0m, result.TransportVat);
        Assert.Equal(0m, result.GuideVat);
        Assert.Equal(84.21438m, result.AdditionalGuarantee);
        Assert.Equal(884.65722804m, result.StampTax);
        Assert.Equal(968.87160804m, result.TotalDeductions);
        Assert.Equal(93318.273m, result.GrossAmount);
        Assert.Equal(92349.40139196m, result.NetPayable);
    }

    [Fact]
    public void SpecialEducation_ExcelRow4WithVatAndDiscounts_ShouldMatchRecalculatedCopy()
    {
        // Test-only workbook copy: E4 changed to "Hayır", J4=100 and S4=50.
        // The source XLSM uploaded by the user is never modified.
        var input = new ProgressPaymentInput(
            WorkDays: 22,
            DailyPrice: 2320m,
            DiscountAmount: 100m,
            PriceDifference: 1403.573m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 30,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 50m,
            GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(52343.573m, result.TransportNetAmount);
        Assert.Equal(40824.70m, result.GuideNetAmount);
        Assert.Equal(5234.3573m, result.TransportVat);
        Assert.Equal(8164.94m, result.GuideVat);
        Assert.Equal(2617.17865m, result.TransportWithholding);
        Assert.Equal(7348.446m, result.GuideWithholding);
        Assert.Equal(84.21438m, result.AdditionalGuarantee);
        Assert.Equal(883.23522804m, result.StampTax);
        Assert.Equal(10933.07425804m, result.TotalDeductions);
        Assert.Equal(106567.5703m, result.GrossAmount);
        Assert.Equal(95634.49604196m, result.NetPayable);
    }
}
