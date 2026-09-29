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
