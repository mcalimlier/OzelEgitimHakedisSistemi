#pragma warning disable CA1707 // xUnit test adlarında alt çizgi kullanımı okunabilirlik için bilinçlidir.

using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentCalculatorTests
{
    [Fact]
    public void ExcelReferenceTransportOnly_ShouldMatchWorkbook()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19, DailyPrice: 8135m, DiscountAmount: 0m, PriceDifference: 28327.94m,
            ContractAmount: 1138900m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m, InspectionPenalty: 2277.80m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(154565.00m, result.ServiceAmount);
        Assert.Equal(28327.94m, result.PriceDifferenceExcludingVat);
        Assert.Equal(182892.94m, result.TotalAmount);
        Assert.Equal(18289.294m, result.Vat);
        Assert.Equal(9144.647m, result.Withholding);
        Assert.Equal(192037.587m, result.InvoiceAmountAfterWithholding);
        Assert.Equal(201182.234m, result.GrossAmount);
        Assert.Equal(1733.8250712m, result.StampTax);
        Assert.Equal(1699.6764m, result.AdditionalGuarantee);
        Assert.Equal(14855.9484712m, result.TotalDeductions);
        Assert.Equal(186326.2855288m, result.NetPayable);
    }

    [Fact]
    public void ExcelTransportAndGuideComposition_ShouldMatchWorkbook()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 29, DailyPrice: 2608.26m, DiscountAmount: 0m, PriceDifference: 1500m,
            ContractAmount: 816805.03m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m, InspectionPenalty: 0m,
            GuideWorkDays: 30, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 1500m);

        var result = ProgressPaymentCalculator.Calculate(input);

        // Bu senaryo sentetik bir regresyon testidir; Excel zincirindeki
        // T = (P*Q)+R-S ve ortak kesinti kurallarının doğrudan uygulanmasını doğrular.
        Assert.Equal(75639.54m, result.ServiceAmount);
        Assert.Equal(40874.70m, result.GuideServiceAmount);
        Assert.Equal(1500m, result.PriceDifferenceExcludingVat);
        Assert.Equal(1500m, result.GuidePriceDifferenceExcludingVat);
        Assert.Equal(3000m, result.CombinedPriceDifferenceExcludingVat);
        Assert.Equal(119514.24m, result.TotalAmount);
        Assert.Equal(16188.894m, result.Vat);
        Assert.Equal(11484.423m, result.Withholding);
        Assert.Equal(180m, result.AdditionalGuarantee);
        Assert.Equal(1132.9949952m, result.StampTax);
        Assert.Equal(12797.4179952m, result.TotalDeductions);
        Assert.Equal(122905.7160048m, result.NetPayable);
    }

    [Fact]
    public void ExcelRealScenario_Row4_ShouldMatchWorkbook()
    {
        // Kaynak Excel Hesaplama 4. satır senaryosu:
        // Taşıma: 29 gün × 2.608,26 + 1.500,00 fiyat farkı.
        // Rehber: 30 gün × 1.362,49; fiyat farkı ve iskonto 0.
        // E4 = "Hayır" olduğundan KDV ve tevkifat hesaplanır.
        var input = new ProgressPaymentInput(
            WorkDays: 29, DailyPrice: 2608.26m, DiscountAmount: 0m, PriceDifference: 1500m,
            ContractAmount: 816805.03m, IsVatExempt: false, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m, InspectionPenalty: 0m,
            GuideWorkDays: 30, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(75639.54m, result.ServiceAmount);
        Assert.Equal(77139.54m, result.TransportNetAmount);
        Assert.Equal(40874.70m, result.GuideServiceAmount);
        Assert.Equal(40874.70m, result.GuideNetAmount);
        Assert.Equal(1500m, result.PriceDifferenceExcludingVat);
        Assert.Equal(0m, result.GuidePriceDifferenceExcludingVat);
        Assert.Equal(118014.24m, result.TotalAmount);
        Assert.Equal(15888.894m, result.Vat);
        Assert.Equal(11214.423m, result.Withholding);
        Assert.Equal(129228.663m, result.InvoiceAmountAfterWithholding);
        Assert.Equal(90m, result.AdditionalGuarantee);
        Assert.Equal(1118.7749952m, result.StampTax);
        Assert.Equal(12423.1979952m, result.TotalDeductions);
        Assert.Equal(133903.134m, result.GrossAmount);
        Assert.Equal(121479.9360048m, result.NetPayable);
    }

    [Fact]
    public void PdfScenario_HakedisRaporu_ShouldMatchDisplayedTotals()
    {
        // deneme.pdf: 22.500,00 hizmet + 255,00 fiyat farkı,
        // KDV muaf, 180,00 denetim cezası.
        var input = new ProgressPaymentInput(
            WorkDays: 3, DailyPrice: 7500m, DiscountAmount: 0m, PriceDifference: 255m,
            ContractAmount: 90000m, IsVatExempt: true, TaxDebt: 0m,
            SocialSecurityDebt: 0m, MinimumWageSupport: 0m, InspectionPenalty: 180m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(22500m, result.ServiceAmount);
        Assert.Equal(255m, result.PriceDifferenceExcludingVat);
        Assert.Equal(22755m, result.TotalAmount);
        Assert.Equal(0m, result.Vat);
        Assert.Equal(0m, result.Withholding);
        Assert.Equal(22755m, result.GrossAmount);
        Assert.Equal(215.7174m, result.StampTax);
        Assert.Equal(15.30m, result.AdditionalGuarantee);
        Assert.Equal(411.0174m, result.TotalDeductions);
        Assert.Equal(22343.9826m, result.NetPayable);
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
    public void NegativePriceDifference_ShouldFlowThroughLikeExcel()
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

}
