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
        Assert.Equal(18289.29m, result.Vat);
        Assert.Equal(9144.65m, result.Withholding);
        Assert.Equal(192037.59m, result.InvoiceAmountAfterWithholding);
        Assert.Equal(201182.23m, result.GrossAmount);
        Assert.Equal(1733.83m, result.StampTax);
        Assert.Equal(1699.68m, result.AdditionalGuarantee);
        Assert.Equal(14855.96m, result.TotalDeductions);
        Assert.Equal(186326.27m, result.NetPayable);
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

        Assert.Equal(75639.54m, result.ServiceAmount);
        Assert.Equal(42374.70m, result.GuideServiceAmount);
        Assert.Equal(1500m, result.PriceDifferenceExcludingVat);
        Assert.Equal(1500m, result.GuidePriceDifferenceExcludingVat);
        Assert.Equal(3000m, result.CombinedPriceDifferenceExcludingVat);
        Assert.Equal(121014.24m, result.TotalAmount);
        Assert.Equal(16488.89m, result.Vat);
        Assert.Equal(11754.42m, result.Withholding);
        Assert.Equal(180m, result.AdditionalGuarantee);
        Assert.Equal(1147.21m, result.StampTax);
        Assert.Equal(13081.63m, result.TotalDeductions);
        Assert.Equal(124421.50m, result.NetPayable);
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
        Assert.Equal(215.72m, result.StampTax);
        Assert.Equal(15.30m, result.AdditionalGuarantee);
        Assert.Equal(411.02m, result.TotalDeductions);
        Assert.Equal(22343.98m, result.NetPayable);
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
        Assert.Equal(10000m, result.NetPayable);
    }

    [Fact]
    public void ExcelDebtsAndDeductions_ShouldFlowIntoNetPayable()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19, DailyPrice: 8135m, DiscountAmount: 0m, PriceDifference: 28327.94m,
            ContractAmount: 1138900m, IsVatExempt: false, TaxDebt: 1200m,
            SocialSecurityDebt: 800m, MinimumWageSupport: 300m, InspectionPenalty: 2277.80m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(1699.68m, result.AdditionalGuarantee);
        Assert.Equal(17155.96m, result.TotalDeductions);
        Assert.Equal(184026.27m, result.NetPayable);
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
}
