namespace OzelEgitimHakedis.Core.Models;

public sealed record ProgressPaymentCalculation(
    decimal ServiceAmount,
    decimal PriceDifferenceExcludingVat,
    decimal TotalAmount,
    decimal Vat,
    decimal Withholding,
    decimal InvoiceAmountAfterWithholding,
    decimal StampTax,
    decimal GrossAmount,
    decimal TotalDeductions,
    decimal NetPayable,
    decimal AdditionalGuarantee,
    DeductionBreakdown Deductions,
    decimal GuideServiceAmount = 0m,
    decimal GuidePriceDifferenceExcludingVat = 0m,
    decimal TransportNetAmount = 0m,
    decimal GuideNetAmount = 0m,
    decimal TransportVat = 0m,
    decimal GuideVat = 0m,
    decimal TransportWithholding = 0m,
    decimal GuideWithholding = 0m)
{
    public decimal CombinedServiceAmount =>
        ServiceAmount + GuideServiceAmount;

    public decimal CombinedPriceDifferenceExcludingVat =>
        PriceDifferenceExcludingVat + GuidePriceDifferenceExcludingVat;

    public decimal TransportGrossAmount =>
        TransportNetAmount + TransportVat;

    public decimal GuideGrossAmount =>
        GuideNetAmount + GuideVat;
}
