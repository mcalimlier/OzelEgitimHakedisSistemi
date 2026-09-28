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
    DeductionBreakdown Deductions);
