#pragma warning disable CA1707 // xUnit test adlarında alt çizgi kullanımı okunabilirlik için bilinçlidir.

using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentCalculatorTests
{
    [Fact]
    public void BasicOrSecondaryProgram_ShouldExcludeGuideButIncludeMinimumWageSupport()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 250m,
            GuideWorkDays: 20,
            GuideDailyPrice: 200m,
            GuideDiscountAmount: 10m,
            GuidePriceDifference: 50m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input, ProgressPaymentCalculationConstants.Defaults);

        Assert.Equal(0m, result.GuideServiceAmount);
        Assert.Equal(0m, result.GuideNetAmount);
        Assert.Equal(0m, result.GuideVat);
        Assert.Equal(0m, result.GuideWithholding);
        Assert.Equal(1000m, result.TransportNetAmount);
        Assert.Equal(100m, result.TransportVat);
        Assert.Equal(50m, result.TransportWithholding);
        Assert.Equal(250m, result.Deductions.MinimumWageSupport);
        // Temel/Ortaöğretim modunda girilmiş rehber fiyat farkı yok sayılır;
        // ek kesin teminat hesabına da taşınmamalıdır.
        Assert.Equal(0m, result.AdditionalGuarantee);
        Assert.Equal(309.48m, result.TotalDeductions);
        Assert.Equal(790.52m, result.NetPayable);
        Assert.Equal(0m, result.AppliedConstants!.GuideVatRate);
        Assert.Equal(0m, result.AppliedConstants.GuideWithholdingShare);
    }

    [Fact]
    public void BasicOrSecondaryProgram_ShouldCalculateAdditionalGuaranteeFromWorkbookPriceDifference()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 100m, DiscountAmount: 0m, PriceDifference: 20m,
            ContractAmount: 1000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m, ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(1.20m, result.AdditionalGuarantee);
        Assert.Equal(1.20m, result.Deductions.AdditionalGuarantee);
        // Nihai Temel Eğitim Excel'i: U = K*9.48/1000; T = I*6%.
        Assert.Equal(1.1376m, result.StampTax);
        Assert.Equal(8.3376m, result.TotalDeductions);
        Assert.Equal(123.6624m, result.NetPayable);
    }

    [Fact]
    public void SpecialEducationProgram_ShouldCalculateAdditionalGuaranteeFromTransportAndGuidePriceDifferences()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 100m, DiscountAmount: 0m, PriceDifference: 100m,
            ContractAmount: 1000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuidePriceDifference: 50m, ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(9m, result.AdditionalGuarantee);
    }

    [Fact]
    public void SpecialEducationProgram_ShouldReduceAdditionalGuaranteeWhenGuidePriceDifferenceIsZero()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 100m, DiscountAmount: 0m, PriceDifference: 100m,
            ContractAmount: 1000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuidePriceDifference: 0m, ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(6m, result.AdditionalGuarantee);
    }

    [Fact]
    public void SpecialEducationProgram_ShouldKeepGuideAndMinimumWageSupportInCalculation()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 250m,
            GuideWorkDays: 20,
            GuideDailyPrice: 200m,
            GuideDiscountAmount: 10m,
            GuidePriceDifference: 50m,
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input, ProgressPaymentCalculationConstants.Defaults);

        Assert.Equal(4000m, result.GuideServiceAmount);
        Assert.Equal(4040m, result.GuideNetAmount);
        Assert.Equal(808m, result.GuideVat);
        Assert.Equal(727.2m, result.GuideWithholding);
        Assert.Equal(250m, result.Deductions.MinimumWageSupport);
        Assert.Equal(0.20m, result.AppliedConstants!.GuideVatRate);
        Assert.Equal(0.90m, result.AppliedConstants.GuideWithholdingShare);
    }

    [Fact]
    public void ExcelDiscountIsSubtractedFromPriceDifference()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 100m, PriceDifference: 200m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 5, GuideDailyPrice: 500m, GuideDiscountAmount: 50m,
            GuidePriceDifference: 100m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(10100m, result.TransportNetAmount);
        Assert.Equal(2550m, result.GuideNetAmount);
        Assert.Equal(12650m, result.TotalAmount);
        // AC = I*6% + R*6%; iskonto AC hesabından düşülmez.
        Assert.Equal(18m, result.AdditionalGuarantee);
    }

    [Fact]
    public void ExcelAdditionalGuarantee_ShouldUsePriceDifferenceColumns()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: 500m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 5, GuideDailyPrice: 500m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 100m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(36m, result.AdditionalGuarantee);
    }

    [Fact]
    public void VatExempt_ShouldNotCalculateVatOrWithholding()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: 0m,
            ContractAmount: 10000m, IsVatExempt: true, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(10000m, result.TotalAmount);
        Assert.Equal(0m, result.Vat);
        Assert.Equal(0m, result.Withholding);
        Assert.Equal(9905.20000m, result.NetPayable);
    }

    [Fact]
    public void ExcelDebtsAndDeductions_ShouldFlowIntoNetPayable()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19, DailyPrice: 8135m, DiscountAmount: 0m, PriceDifference: 28327.94m,
            ContractAmount: 1138900m, IsVatExempt: false, TaxDebt: 1200m,
            SocialSecurityDebt: 800m, MinimumWageSupport: 300m, InspectionPenalty: 2277.80m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(1699.6764m, result.AdditionalGuarantee);
        Assert.Equal(17155.9484712m, result.TotalDeductions);
        Assert.Equal(184026.2855288m, result.NetPayable);
    }

    [Fact]
    public void NegativePriceDifference_ShouldMatchExcelSignedGuaranteeFormula()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: -500m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(9500m, result.TotalAmount);
        Assert.Equal(950m, result.Vat);
        Assert.Equal(-30m, result.AdditionalGuarantee);
        Assert.Equal(90.06m, result.StampTax);
        Assert.Equal(475m, result.Withholding);
        Assert.Equal(10450m, result.GrossAmount);
        Assert.Equal(535.06m, result.TotalDeductions);
        Assert.Equal(9914.94m, result.NetPayable);
    }

    [Fact]
    public void MixedSignedPriceDifferences_ShouldCalculateEachExcelPriceDifferenceAtSixPercent()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: -500m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 5, GuideDailyPrice: 500m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 1000m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(-500m, result.PriceDifferenceExcludingVat);
        Assert.Equal(1000m, result.GuidePriceDifferenceExcludingVat);
        Assert.Equal(30m, result.AdditionalGuarantee);
        Assert.Equal(13000m, result.TotalAmount);
    }

    [Fact]
    public void NegativeWorkDays_ShouldThrow()
    {
        var input = new ProgressPaymentInput(
            WorkDays: -1, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: 0m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m);

        Assert.Throws<ArgumentOutOfRangeException>(() => ProgressPaymentCalculator.Calculate(input));
    }

    [Fact]
    public void IntermediatePrecision_ShouldNotRoundAtTwoDecimals()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 1.005m, DiscountAmount: 0m, PriceDifference: 0m,
            ContractAmount: 1.005m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(1.005m, result.ServiceAmount);
        Assert.Equal(1.1055m, result.GrossAmount);
        Assert.Equal(0.0095274m, result.StampTax);
        Assert.Equal(0.0597774m, result.TotalDeductions);
        Assert.Equal(1.0457226m, result.NetPayable);
    }


    [Fact]
    public void GuideWithholding_ShouldBeNineTenthsOfGuideVat()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 0, DailyPrice: 0m, DiscountAmount: 0m, PriceDifference: 0m,
            ContractAmount: 10000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 10, GuideDailyPrice: 1000m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(2000m, result.GuideVat);
        Assert.Equal(1800m, result.GuideWithholding);
        Assert.Equal(1800m, result.Withholding);
    }

    [Fact]
    public void CustomCalculationConstants_ShouldDriveEveryRateBasedFormulaAndBeSnapshotted()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10, DailyPrice: 1000m, DiscountAmount: 0m, PriceDifference: 1000m,
            ContractAmount: 100000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            GuideWorkDays: 5, GuideDailyPrice: 500m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 1000m);
        var constants = new ProgressPaymentCalculationConstants(
            TransportVatRate: 0.12m,
            GuideVatRate: 0.25m,
            TransportWithholdingShare: 0.40m,
            GuideWithholdingShare: 0.80m,
            StampTaxRate: 0.01m,
            AdditionalGuaranteeRate: 0.07m);

        var result = ProgressPaymentCalculator.Calculate(input, constants);

        Assert.Equal(11000m, result.TransportNetAmount);
        Assert.Equal(3500m, result.GuideNetAmount);
        Assert.Equal(2195m, result.Vat);
        Assert.Equal(1228m, result.Withholding);
        Assert.Equal(145m, result.StampTax);
        Assert.Equal(140m, result.AdditionalGuarantee);
        Assert.Equal(1513m, result.TotalDeductions);
        Assert.Equal(16695m, result.GrossAmount);
        Assert.Equal(15182m, result.NetPayable);
        Assert.Equal(constants, result.AppliedConstants);
    }

    [Fact]
    public void InvalidCalculationConstants_ShouldBeRejectedBeforeCalculation()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1, DailyPrice: 100m, DiscountAmount: 0m, PriceDifference: 0m,
            ContractAmount: 1000m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m);
        var invalid = ProgressPaymentCalculationConstants.Defaults with { StampTaxRate = 1.1m };

        Assert.Throws<ArgumentOutOfRangeException>(
            () => ProgressPaymentCalculator.Calculate(input, invalid));
    }


}
