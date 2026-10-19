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
        // I4 = 7365.35 in this source vector. Discount J4 and the legacy manual
        // input do not participate in this formula.
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
            ProgramType: TransportationProgramType.BasicOrSecondary,
            ManualAdditionalGuarantee: 1500m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(441.921m, result.AdditionalGuarantee);
        Assert.Equal(441.921m, result.Deductions.AdditionalGuarantee);
    }

}
