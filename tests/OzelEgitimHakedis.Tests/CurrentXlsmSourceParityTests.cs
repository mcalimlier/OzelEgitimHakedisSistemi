using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Current source vectors transcribed from the latest uploaded
/// "Özel Eğitim Hakediş Ödeme Programı En Güncel.xlsm" cached cells.
/// These cover the calculation-sheet chain, not Excel desktop/VBA execution.
/// </summary>
public sealed class CurrentXlsmSourceParityTests
{
    [Fact]
    public void CurrentSpecialEducationRow4_ShouldMatchLatestWorkbookCalculationCells()
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

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(51256.94m, result.TransportNetAmount);       // Hesaplama!K4
        Assert.Equal(41954.70m, result.GuideNetAmount);           // Hesaplama!T4
        Assert.Equal(13516.634m, result.Vat);                     // L4 + U4
        Assert.Equal(10114.693m, result.Withholding);             // M4 + V4
        Assert.Equal(202.80m, result.AdditionalGuarantee);         // AC4
        Assert.Equal(883.6463472m, result.StampTax);              // AD4
        Assert.Equal(11201.1393472m, result.TotalDeductions);      // AE4
        Assert.Equal(106728.274m, result.GrossAmount);             // AF4
        Assert.Equal(95527.1346528m, result.NetPayable);           // AG4
    }

    [Fact]
    public void CurrentSpecialEducationRow5_ShouldMatchLatestWorkbookCalculationCells()
    {
        var input = new ProgressPaymentInput(
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
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(53134.24m, result.TransportNetAmount);        // Hesaplama!K5
        Assert.Equal(42090.70m, result.GuideNetAmount);            // Hesaplama!T5
        Assert.Equal(225.36m, result.AdditionalGuarantee);         // AC5
        Assert.Equal(902.7324312m, result.StampTax);               // AD5
        Assert.Equal(11361.1304312m, result.TotalDeductions);      // AE5
        Assert.Equal(108956.504m, result.GrossAmount);             // AF5
        Assert.Equal(97595.3735688m, result.NetPayable);            // AG5
    }

    [Fact]
    public void CurrentSpecialEducationRow6_ShouldMatchLatestWorkbookCalculationCells()
    {
        var input = new ProgressPaymentInput(
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
            ProgramType: TransportationProgramType.SpecialEducation);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(43483.75m, result.TransportNetAmount);        // Hesaplama!K6
        Assert.Equal(42099.70m, result.GuideNetAmount);            // Hesaplama!T6
        Assert.Equal(244.50m, result.AdditionalGuarantee);          // AC6
        Assert.Equal(811.331106m, result.StampTax);                 // AD6
        Assert.Equal(10807.964606m, result.TotalDeductions);        // AE6
        Assert.Equal(98351.765m, result.GrossAmount);               // AF6
        Assert.Equal(87543.800394m, result.NetPayable);              // AG6
    }

    [Fact]
    public void UploadedSpecialEducationWorkbookRow4_ShouldMatchCachedCalculationChain()
    {
        // Read from the user's 2026-10-19 upload:
        // 02-Özel Eğitim Hakediş Ödeme (1)(1).xlsm, Hesaplama row 4.
        // These are cached workbook results; this is not a claim that Excel/VBA
        // was executed in this environment.
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

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(43630m, result.TransportNetAmount);          // K4
        Assert.Equal(34037.27m, result.GuideNetAmount);            // T4
        Assert.Equal(11170.454m, result.Vat);                      // L4 + U4
        Assert.Equal(8308.2086m, result.Withholding);              // M4 + V4
        Assert.Equal(661.8m, result.AdditionalGuarantee);           // AC4 = 6%*(I4+R4)
        Assert.Equal(736.2857196m, result.StampTax);                // AD4
        Assert.Equal(27328.7843196m, result.TotalDeductions);       // AE4
        Assert.Equal(88837.724m, result.GrossAmount);               // AF4
        Assert.Equal(61508.9396804m, result.NetPayable);            // AG4
    }


    [Fact]
    public void UploadedBasicEducationWorkbookRow4_ShouldUsePriceDifferenceTimesSixPercent()
    {
        // Source: 03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi(2).xlsm
        // Hesaplama Bilgileri!T4 = I4*6/100; cached T4 = 441.921.
        // I4 = 7365.35 in this source vector. Discount J4 does not participate in this formula.
        var input = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 500m,
            PriceDifference: 7365.35m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(441.921m, result.AdditionalGuarantee);
        Assert.Equal(441.921m, result.Deductions.AdditionalGuarantee);
    }


    [Fact]
    public void UploadedBasicEducationWorkbookRow4_ShouldMatchFullCalculationChain()
    {
        // Source: 03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi(2).xlsm
        // Values are from the recalculated workbook copy; LibreOffice recalculation
        // was used here, not desktop Excel/VBA execution.
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

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(319991.9228m, result.TransportNetAmount); // K4
        Assert.Equal(31999.19228m, result.Vat);                // L4
        Assert.Equal(15999.59614m, result.Withholding);        // M4
        Assert.Equal(335991.51894m, result.InvoiceAmountAfterWithholding); // N4
        Assert.Equal(351991.11508m, result.GrossAmount);        // O4
        Assert.Equal(441.921m, result.AdditionalGuarantee);     // T4
        Assert.Equal(3033.523428144m, result.StampTax);        // U4
        Assert.Equal(19475.040568144m, result.TotalDeductions);// V4
        Assert.Equal(332516.074511856m, result.NetPayable);    // W4
    }

    [Fact]
    public void HistoricalBasicWorkbookRows5And8_ShouldMatchRecalculatedSourceCells()
    {
        // Source copies reviewed 2026-10-17:
        // 03-Temel Eğitim Taşıma Ödeme(1).xlsm, Hesaplama Bilgileri rows 5 and 8.
        // These are formula-recalculation parity vectors, not desktop Excel/VBA execution.
        var exemptRow5 = ProgressPaymentCalculator.Calculate(new ProgressPaymentInput(
            WorkDays: 13m,
            DailyPrice: 6545m,
            DiscountAmount: 0m,
            PriceDifference: 3659.2m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary));

        Assert.Equal(88744.2m, exemptRow5.TransportNetAmount);       // K5
        Assert.Equal(0m, exemptRow5.TransportVat);                   // L5
        Assert.Equal(0m, exemptRow5.TransportWithholding);           // M5
        Assert.Equal(88744.2m, exemptRow5.GrossAmount);             // O5
        Assert.Equal(219.552m, exemptRow5.AdditionalGuarantee);     // T5 = I5*6%
        Assert.Equal(841.295016m, exemptRow5.StampTax);              // U5 = K5*9.48/1000
        Assert.Equal(1060.847016m, exemptRow5.TotalDeductions);      // V5
        Assert.Equal(87683.352984m, exemptRow5.NetPayable);          // W5

        var taxableRow8 = ProgressPaymentCalculator.Calculate(new ProgressPaymentInput(
            WorkDays: 13m,
            DailyPrice: 2810m,
            DiscountAmount: 0m,
            PriceDifference: 1571.03m,
            ContractAmount: 100000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary));

        Assert.Equal(38101.03m, taxableRow8.TransportNetAmount);     // K8
        Assert.Equal(3810.103m, taxableRow8.TransportVat);           // L8
        Assert.Equal(1905.0515m, taxableRow8.TransportWithholding);  // M8
        Assert.Equal(41911.133m, taxableRow8.GrossAmount);           // O8
        Assert.Equal(94.2618m, taxableRow8.AdditionalGuarantee);     // T8 = I8*6%
        Assert.Equal(361.1977644m, taxableRow8.StampTax);            // U8 = K8*9.48/1000
        Assert.Equal(2360.5110644m, taxableRow8.TotalDeductions);    // V8
        Assert.Equal(39550.6219356m, taxableRow8.NetPayable);        // W8
    }

    [Fact]
    public void AdditionalGuarantee_ShouldFollowProgramSpecificSourceFormulaAndIgnoreDiscounts()
    {
        var basic = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: 7365.35m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var special = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: 7365.35m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 1m,
            GuideDailyPrice: 100m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 3500m,
            ProgramType: TransportationProgramType.SpecialEducation);

        var basicBaseline = ProgressPaymentCalculator.Calculate(basic);
        var basicWithDiscount = ProgressPaymentCalculator.Calculate(basic with { DiscountAmount = 500m });
        var specialBaseline = ProgressPaymentCalculator.Calculate(special);
        var specialWithDiscount = ProgressPaymentCalculator.Calculate(special with
        {
            DiscountAmount = 500m,
            GuideDiscountAmount = 800m
        });

        // Basic/Secondary source formula: T = I * 6%; discount J is not part of T.
        Assert.Equal(441.921m, basicBaseline.AdditionalGuarantee);
        Assert.Equal(basicBaseline.AdditionalGuarantee, basicWithDiscount.AdditionalGuarantee);

        // Special Education source formula: AC = (I * 6%) + (R * 6%); discounts J/S are not inputs.
        Assert.Equal(651.921m, specialBaseline.AdditionalGuarantee);
        Assert.Equal(specialBaseline.AdditionalGuarantee, specialWithDiscount.AdditionalGuarantee);
    }


    [Fact]
    public void AdditionalGuarantee_ShouldPreserveSignedSourceFormulaForNegativePriceDifferences()
    {
        // The source formula is a direct percentage expression and does not apply MAX(0, ...).
        // This locks current Excel-formula parity while keeping the legal/contract interpretation
        // as a documented review risk rather than silently changing the official calculation.
        var basic = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: -100m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);

        var special = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: -100m,
            ContractAmount: 100000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 1m,
            GuideDailyPrice: 100m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: -50m,
            ProgramType: TransportationProgramType.SpecialEducation);

        Assert.Equal(-6m, ProgressPaymentCalculator.Calculate(basic).AdditionalGuarantee);
        Assert.Equal(-9m, ProgressPaymentCalculator.Calculate(special).AdditionalGuarantee);
    }

}
