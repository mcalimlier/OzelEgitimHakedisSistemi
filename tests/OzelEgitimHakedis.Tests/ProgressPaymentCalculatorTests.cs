#pragma warning disable CA1707 // xUnit test adlarında alt çizgi kullanımı okunabilirlik için bilinçlidir.

using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class ProgressPaymentCalculatorTests
{
    [Fact]
    public void ExcelReferenceExample_ShouldMatch()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19,
            DailyPrice: 8135m,
            DiscountAmount: 0m,
            PriceDifference: 28327.94m,
            ContractAmount: 1138900m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 2277.80m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(154565.00m, result.ServiceAmount);
        Assert.Equal(28327.94m, result.PriceDifferenceExcludingVat);
        Assert.Equal(182892.94m, result.TotalAmount);
        Assert.Equal(18289.29m, result.Vat);
        Assert.Equal(9144.65m, result.Withholding);
        Assert.Equal(192037.58m, result.InvoiceAmountAfterWithholding);
        Assert.Equal(201182.23m, result.GrossAmount);
        Assert.Equal(1733.83m, result.StampTax);
        Assert.Equal(9144.65m, result.Deductions.Withholding);
        Assert.Equal(1733.83m, result.Deductions.StampTax);
        Assert.Equal(2277.80m, result.Deductions.InspectionPenalty);
        Assert.Equal(1699.68m, result.AdditionalGuarantee);
        Assert.Equal(14855.96m, result.TotalDeductions);
        Assert.Equal(186326.27m, result.NetPayable);
    }


    [Fact]
    public void TransportAndGuideComposition_ShouldMatch()
    {
        // Taşıma + rehber personel bileşimi regresyon senaryosu:
        // Taşıma 29 gün x 2608,26 + 1.500 fiyat farkı
        // Rehber 30 gün x 1362,49 + 1.500 fiyat farkı
        // Taşıma KDV %10, rehber KDV %20, tevkifat sırasıyla %5 ve %18.
        var input = new ProgressPaymentInput(
            WorkDays: 29,
            DailyPrice: 2608.26m,
            DiscountAmount: 0m,
            PriceDifference: 1500m,
            ContractAmount: 816805.03m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 30,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m,
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
        // Excel AC = (Taşıma Toplam Tutarı × %6) + (Rehber Fiyat Farkı × %6)
        // = (77.139,54 × %6) + (1.500 × %6) = 4.718,37.
        Assert.Equal(4718.37m, result.AdditionalGuarantee);
        Assert.Equal(1147.21m, result.StampTax);
        Assert.Equal(17620.01m, result.TotalDeductions);
        Assert.Equal(119883.12m, result.NetPayable);
    }

    [Fact]
    public void ExpandedExcelAdditionalGuarantee_ShouldUseTransportTotalPlusGuidePriceDifference()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: 500m,
            ContractAmount: 10000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 5,
            GuideDailyPrice: 500m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 100m);

        var result = ProgressPaymentCalculator.Calculate(input);

        // Excel AC: =(I*6/100)+(R*6/100) = (10500*6%) + (100*6%).
        Assert.Equal(636m, result.AdditionalGuarantee);
    }

    [Fact]
    public void DiscountAndPriceDifference_ShouldComposeTransportAndGuideBases()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 1000m,
            DiscountAmount: 100m,
            PriceDifference: 200m,
            ContractAmount: 10000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            GuideWorkDays: 5,
            GuideDailyPrice: 500m,
            GuideDiscountAmount: 50m,
            GuidePriceDifference: 100m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(10300m, result.TransportNetAmount);
        Assert.Equal(2650m, result.GuideNetAmount);
        Assert.Equal(12950m, result.TotalAmount);
    }

    [Fact]
    public void VatExempt_ShouldNotCalculateVatOrWithholding()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 10000m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(10000m, result.TotalAmount);
        Assert.Equal(0m, result.Vat);
        Assert.Equal(0m, result.Withholding);
        Assert.Equal(0m, result.Deductions.InspectionPenalty);
        Assert.Equal(9905.20m, result.NetPayable);
    }

    [Fact]
    public void InspectionPenalty_WhenPresent_ShouldBeCalculated()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 10000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 20m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(20m, result.Deductions.InspectionPenalty);
    }

    [Fact]
    public void ExcelRules_ShouldApplyDebtsAndAdditionalGuarantee()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 19,
            DailyPrice: 8135m,
            DiscountAmount: 0m,
            PriceDifference: 28327.94m,
            ContractAmount: 1138900m,
            IsVatExempt: false,
            TaxDebt: 1200m,
            SocialSecurityDebt: 800m,
            MinimumWageSupport: 300m,
            InspectionPenalty: 2277.80m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(154565.00m, result.ServiceAmount);
        Assert.Equal(182892.94m, result.TotalAmount);
        Assert.Equal(18289.29m, result.Vat);
        Assert.Equal(9144.65m, result.Withholding);
        Assert.Equal(1733.83m, result.StampTax);
        Assert.Equal(2277.80m, result.Deductions.InspectionPenalty);
        Assert.Equal(1200m, result.Deductions.TaxDebt);
        Assert.Equal(800m, result.Deductions.SocialSecurityDebt);
        Assert.Equal(300m, result.Deductions.MinimumWageSupport);
        Assert.Equal(1699.68m, result.AdditionalGuarantee);
        Assert.Equal(17155.96m, result.TotalDeductions);
        Assert.Equal(184026.27m, result.NetPayable);
    }

    [Fact]
    public void NegativePriceDifference_ShouldNotCreateAdditionalGuarantee()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 10,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: -500m,
            ContractAmount: 10000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(9500m, result.TotalAmount);
        Assert.Equal(950m, result.Vat);
        Assert.Equal(0m, result.AdditionalGuarantee);
        Assert.Equal(90.06m, result.StampTax);
        Assert.Equal(475m, result.Withholding);
        Assert.Equal(10450m, result.GrossAmount);
        Assert.Equal(565.06m, result.TotalDeductions);
        Assert.Equal(9884.94m, result.NetPayable);
    }

    [Fact]
    public void NegativeWorkDays_ShouldThrow()
    {
        var input = new ProgressPaymentInput(
            WorkDays: -1,
            DailyPrice: 1000m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 10000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m);

        Assert.Throws<ArgumentOutOfRangeException>(() => ProgressPaymentCalculator.Calculate(input));
    }
}
