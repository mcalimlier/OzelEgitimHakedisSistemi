using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Source-cell regression vectors for the proven icmal duplicate-price-difference
/// risk. Values are from the latest XLSM cached cells documented in
/// docs/ICMAL_MUKERRER_FIYAT_FARKI_INCELEMESI_2026-10-21.md.
/// </summary>
public sealed class IcmalDuplicatePriceDifferenceRegressionTests
{
    [Fact]
    public void BasicSourceVector_ShouldSeparateIFromK_AndRecomposeKExactlyOnce()
    {
        // Hesaplama Bilgileri!K4 = 319,991.9228; I4 = 7,365.35.
        var payment = History(
            new ProgressPaymentInput(
                WorkDays: 1m,
                DailyPrice: 312626.5728m,
                DiscountAmount: 0m,
                PriceDifference: 7365.35m,
                ContractAmount: 500000m,
                IsVatExempt: true,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                ProgramType: TransportationProgramType.BasicOrSecondary),
            new ProgressPaymentCalculation(
                ServiceAmount: 319991.9228m,
                PriceDifferenceExcludingVat: 7365.35m,
                TotalAmount: 319991.9228m,
                Vat: 0m,
                Withholding: 0m,
                InvoiceAmountAfterWithholding: 319991.9228m,
                StampTax: 0m,
                GrossAmount: 319991.9228m,
                TotalDeductions: 19475.040568144m,
                NetPayable: 300516.882231856m,
                AdditionalGuarantee: 0m,
                Deductions: new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)));

        var serviceBase = ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentAmount(payment);
        var priceDifference = ProgressPaymentReportService.CalculateWorkbookPriceDifference(payment);
        var monthlyTotal = ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentTotal(payment);

        Assert.Equal(312626.5728m, serviceBase);
        Assert.Equal(7365.35m, priceDifference);
        Assert.Equal(319991.9228m, monthlyTotal);
        Assert.Equal(319991.9228m, serviceBase + priceDifference);
        // Preserve the historical workbook's erroneous sums as discrepancy evidence;
        // the icmal must use priceDifference and monthlyTotal above, not these values.
        Assert.Equal(26840.390568144m, priceDifference + payment.Calculation.TotalDeductions);
        Assert.Equal(339466.963368144m, serviceBase + priceDifference + payment.Calculation.TotalDeductions);
        Assert.NotEqual(346832.313368144m, serviceBase + priceDifference + payment.Calculation.TotalDeductions);
    }

    [Fact]
    public void SpecialEducationSourceVector_ShouldSeparateIAndRFromKPlusT_AndRecomposeOnce()
    {
        // Hesaplama!K4 + T4 = 77,667.27; İcmal Verileri!I4 + R4 = 11,030.
        var payment = History(
            new ProgressPaymentInput(
                WorkDays: 1m,
                DailyPrice: 43630m,
                DiscountAmount: 0m,
                PriceDifference: 7530m,
                ContractAmount: 500000m,
                IsVatExempt: true,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                GuideWorkDays: 1m,
                GuideDailyPrice: 23007.27m,
                GuideDiscountAmount: 0m,
                GuidePriceDifference: 3500m,
                ProgramType: TransportationProgramType.SpecialEducation),
            new ProgressPaymentCalculation(
                ServiceAmount: 77667.27m,
                PriceDifferenceExcludingVat: 11030m,
                TotalAmount: 77667.27m,
                Vat: 0m,
                Withholding: 0m,
                InvoiceAmountAfterWithholding: 77667.27m,
                StampTax: 0m,
                GrossAmount: 77667.27m,
                TotalDeductions: 0m,
                NetPayable: 77667.27m,
                AdditionalGuarantee: 0m,
                Deductions: new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)));

        var serviceBase = ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentAmount(payment);
        var priceDifference = ProgressPaymentReportService.CalculateWorkbookPriceDifference(payment);
        var monthlyTotal = ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentTotal(payment);

        Assert.Equal(66637.27m, serviceBase);
        Assert.Equal(11030m, priceDifference);
        Assert.Equal(77667.27m, monthlyTotal);
        Assert.Equal(77667.27m, serviceBase + priceDifference);
        var duplicatedPriceDifferenceTotal = serviceBase + priceDifference + priceDifference;
        Assert.Equal(88697.27m, duplicatedPriceDifferenceTotal);
        Assert.NotEqual(88697.27m, monthlyTotal);
    }

    private static ProgressPaymentHistoryData History(
        ProgressPaymentInput input,
        ProgressPaymentCalculation calculation) =>
        new(
            Id: Guid.NewGuid(),
            CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
            Input: input,
            Calculation: calculation,
            ProgressPaymentNumber: 1,
            PeriodDate: new DateOnly(2026, 10, 31),
            RouteNumber: "1",
            RouteName: "Kaynak Parite Test Hattı",
            WorkName: "Kaynak Parite Test İşi",
            ContractorName: "Kaynak Parite Test Yüklenicisi",
            TenderNumber: "OEHS-ICMAL-REGRESSION",
            ContractNumber: "OEHS-ICMAL-CONTRACT",
            ContractorTaxOrIdentityNumber: "12345678901");
}
