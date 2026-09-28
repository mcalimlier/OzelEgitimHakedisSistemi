using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class ProgressPaymentCalculator
{
    public const decimal VatRate = 0.10m;
    public const decimal WithholdingRate = 0.05m;
    public const decimal StampTaxRate = 0.00948m;
    public const decimal AdditionalGuaranteeRate = 0.06m;

    public static ProgressPaymentCalculation Calculate(ProgressPaymentInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.WorkDays < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "İş günü negatif olamaz.");
        }

        if (input.DailyPrice < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Günlük fiyat negatif olamaz.");
        }

        if (input.ContractAmount < 0)
        {
            throw new ArgumentOutOfRangeException(nameof(input), "Sözleşme bedeli negatif olamaz.");
        }

        if (input.InspectionPenaltyRatePerThousand < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                "Denetim cezası oranı negatif olamaz.");
        }

        var serviceAmount = Round(input.WorkDays * input.DailyPrice);
        var priceDifferenceExcludingVat = Round(input.DiscountAmount + input.PriceDifference);
        var totalAmount = Round(serviceAmount + priceDifferenceExcludingVat);

        var vat = input.IsVatExempt
            ? 0m
            : Round(totalAmount * VatRate);

        var withholding = input.IsVatExempt || totalAmount <= 0m
            ? 0m
            : Round(totalAmount * WithholdingRate);

        var invoiceAmountAfterWithholding =
            Round(totalAmount + vat - withholding);

        var stampTax =
            Round(Math.Max(totalAmount, 0m) * StampTaxRate);

        var inspectionPenalty = input.HasInspectionPenalty
            ? Round(input.ContractAmount * input.InspectionPenaltyRatePerThousand / 1000m)
            : 0m;

        var additionalGuarantee =
            Round(Math.Max(priceDifferenceExcludingVat, 0m) * AdditionalGuaranteeRate);

        var deductions = new DeductionBreakdown(
            withholding,
            stampTax,
            input.TaxDebt,
            inspectionPenalty,
            input.SocialSecurityDebt,
            input.MinimumWageSupport,
            additionalGuarantee);

        var grossAmount = Round(totalAmount + vat);
        var totalDeductions = Round(deductions.Total);
        var netPayable = Round(grossAmount - totalDeductions);

        return new ProgressPaymentCalculation(
            serviceAmount,
            priceDifferenceExcludingVat,
            totalAmount,
            vat,
            withholding,
            invoiceAmountAfterWithholding,
            stampTax,
            grossAmount,
            totalDeductions,
            netPayable,
            additionalGuarantee,
            deductions);
    }

    private static decimal Round(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
