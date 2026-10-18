using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Legacy reference vectors from earlier workbook versions and independently
/// recalculated copies. They are regression fixtures, not golden-master vectors
/// for the latest XLSM files currently designated as authoritative sources.
/// </summary>
public sealed class ExcelSourceParityTests
{
    [Fact]
    public void BasicOrSecondary_LegacyRow8Vector_ShouldMatchTransportCalculationChain()
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
            ProgramType: TransportationProgramType.BasicOrSecondary,
            ManualAdditionalGuarantee: 94.2618m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(36530m, result.ServiceAmount);                 // G*H
        Assert.Equal(38101.03m, result.TransportNetAmount);          // K = G*H+I-J
        Assert.Equal(1571.03m, result.PriceDifferenceExcludingVat);  // I
        Assert.Equal(38101.03m, result.TotalAmount);                 // K
        Assert.Equal(3810.103m, result.TransportVat);                // L = K*10%
        Assert.Equal(1905.0515m, result.TransportWithholding);       // M = L/2
        Assert.Equal(40006.0815m, result.InvoiceAmountAfterWithholding); // N = K+M
        Assert.Equal(41911.133m, result.GrossAmount);                // O = K+L
        Assert.Equal(94.2618m, result.AdditionalGuarantee);
        Assert.Equal(361.1977644m, result.StampTax);
        Assert.Equal(2360.5110644m, result.TotalDeductions);
        Assert.Equal(39550.6219356m, result.NetPayable);
    }

    [Fact]
    public void SpecialEducation_LegacyRow4Vector_VatExempt_ShouldMatchTransportAndGuideChains()
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
    public void SpecialEducation_LegacyRow4VectorWithVatAndDiscounts_ShouldMatchRecalculatedCopy()
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

        Assert.Equal(52343.573m, result.TransportNetAmount);         // K = G*H+I-J
        Assert.Equal(40824.70m, result.GuideNetAmount);              // T = P*Q+R-S
        Assert.Equal(5234.3573m, result.TransportVat);                // L = K*10%
        Assert.Equal(8164.94m, result.GuideVat);                     // U = T*20%
        Assert.Equal(2617.17865m, result.TransportWithholding);      // M = L/2
        Assert.Equal(7348.446m, result.GuideWithholding);            // V = U*90%
        Assert.Equal(103133.89765m, result.InvoiceAmountAfterWithholding); // N+W
        Assert.Equal(84.21438m, result.AdditionalGuarantee);
        Assert.Equal(883.23522804m, result.StampTax);
        Assert.Equal(10933.07425804m, result.TotalDeductions);
        Assert.Equal(106567.5703m, result.GrossAmount);
        Assert.Equal(95634.49604196m, result.NetPayable);
    }


    [Fact]
    public void BasicOrSecondary_LegacyWorkbookRow5Vector_VatExempt_ShouldMatchRecordedCells()
    {
        // Legacy source: 03-Temel Eğitim Taşıma Ödeme.xlsm, Hesaplama Bilgileri row 5. This is not the latest authoritative upload.
        // Legacy source cells: G=13, H=6545, I=3659.2, K=88744.2,
        // T=219.552, U=841.295016, V=1060.847016, W=87683.352984.
        var input = new ProgressPaymentInput(
            WorkDays: 13, DailyPrice: 6545m, DiscountAmount: 0m,
            PriceDifference: 3659.2m, ContractAmount: 100000m,
            IsVatExempt: true, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary,
            ManualAdditionalGuarantee: 219.552m);
        var result = ProgressPaymentCalculator.Calculate(input);
        Assert.Equal(88744.2m, result.TransportNetAmount); // K
        Assert.Equal(0m, result.TransportVat); // L
        Assert.Equal(0m, result.TransportWithholding); // M
        Assert.Equal(88744.2m, result.GrossAmount); // O
        Assert.Equal(219.552m, result.AdditionalGuarantee); // T
        Assert.Equal(841.295016m, result.StampTax); // U
        Assert.Equal(1060.847016m, result.TotalDeductions); // V
        Assert.Equal(87683.352984m, result.NetPayable); // W
    }

    [Fact]
    public void SpecialEducation_LegacyWorkbookRow5Vector_VatExempt_ShouldMatchRecordedCells()
    {
        // Legacy source: 02-Özel Eğitim Hakediş Ödeme.xlsm, Hesaplama row 5. This is not the latest authoritative upload.
        // K=42813.848, T=40874.70, AC=68.75088, AD=793.36743504,
        // AE=862.11831504, AF=83688.548, AG=82826.42968496.
        var input = new ProgressPaymentInput(
            WorkDays: 22, DailyPrice: 1894m, DiscountAmount: 0m,
            PriceDifference: 1145.848m, ContractAmount: 100000m,
            IsVatExempt: true, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, InspectionPenalty: 0m,
            GuideWorkDays: 30, GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m, GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var result = ProgressPaymentCalculator.Calculate(input);
        Assert.Equal(42813.848m, result.TransportNetAmount); // K
        Assert.Equal(40874.70m, result.GuideNetAmount); // T
        Assert.Equal(68.75088m, result.AdditionalGuarantee); // AC
        Assert.Equal(793.36743504m, result.StampTax); // AD
        Assert.Equal(862.11831504m, result.TotalDeductions); // AE
        Assert.Equal(83688.548m, result.GrossAmount); // AF
        Assert.Equal(82826.42968496m, result.NetPayable); // AG
    }

    [Fact]
    public void SpecialEducation_LegacyWorkbookRow6Vector_VatExempt_ShouldMatchRecordedCells()
    {
        // Legacy source: 02-Özel Eğitim Hakediş Ödeme.xlsm, Hesaplama row 6. This is not the latest authoritative upload.
        // K=49369.2946, T=40874.70, AC=79.277676, AD=855.513068808,
        // AE=934.790744808, AF=90243.9946, AG=89309.203855192.
        var input = new ProgressPaymentInput(
            WorkDays: 22, DailyPrice: 2184m, DiscountAmount: 0m,
            PriceDifference: 1321.2946m, ContractAmount: 100000m,
            IsVatExempt: true, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, InspectionPenalty: 0m,
            GuideWorkDays: 30, GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m, GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var result = ProgressPaymentCalculator.Calculate(input);
        Assert.Equal(49369.2946m, result.TransportNetAmount); // K
        Assert.Equal(40874.70m, result.GuideNetAmount); // T
        Assert.Equal(79.277676m, result.AdditionalGuarantee); // AC
        Assert.Equal(855.513068808m, result.StampTax); // AD
        Assert.Equal(934.790744808m, result.TotalDeductions); // AE
        Assert.Equal(90243.9946m, result.GrossAmount); // AF
        Assert.Equal(89309.203855192m, result.NetPayable); // AG
    }

    [Fact]
    public void FractionalWorkDays_ShouldPreserveTwentyDecimalPlacesInCalculation()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1.12345678901234567890m,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(1123.45678901234567890m, result.ServiceAmount);
        Assert.Equal(1123.45678901234567890m, result.TotalAmount);
    }

}
