using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class ProgressPaymentCalculator
{
    public const decimal TransportVatRate = 0.10m;
    public const decimal GuideVatRate = 0.20m;
    public const decimal TransportWithholdingRate = 0.05m;
    public const decimal GuideWithholdingRate = 0.18m;
    public const decimal StampTaxRate = 0.00948m;
    public const decimal AdditionalGuaranteeRate = 0.06m;

    // Geriye dönük kod uyumluluğu için eski sabit isimleri koruyoruz.
    public const decimal VatRate = TransportVatRate;
    public const decimal WithholdingRate = TransportWithholdingRate;

    public static ProgressPaymentCalculation Calculate(ProgressPaymentInput input)
    {
        ArgumentNullException.ThrowIfNull(input);

        if (input.WorkDays < 0 || input.GuideWorkDays < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "İş günü negatif olamaz.");

        if (input.DailyPrice < 0 || input.GuideDailyPrice < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "Günlük fiyat negatif olamaz.");

        if (input.ContractAmount < 0)
            throw new ArgumentOutOfRangeException(nameof(input), "Sözleşme bedeli negatif olamaz.");

        if (input.TaxDebt < 0 ||
            input.SocialSecurityDebt < 0 ||
            input.MinimumWageSupport < 0 ||
            input.InspectionPenalty < 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(input),
                "Kesinti ve denetim cezası tutarları negatif olamaz.");
        }

        // Kaynak Excel Hesaplama:
        // K = (G * H) + I - J
        // I = Taşıma Fiyat Farkı KDV Hariç, J = Taşıma İskonto.
        var transportServiceAmount = input.WorkDays * input.DailyPrice;
        var transportPriceDifference =
            input.PriceDifference - input.DiscountAmount;
        var transportNetAmount =
            transportServiceAmount + transportPriceDifference;

        // Kaynak Excel Hesaplama:
        // T = (P * Q) + R - S
        // R = Rehber Personel Fiyat Farkı KDV Hariç, S = Rehber Personel İskonto.
        var guideServiceAmount = input.GuideWorkDays * input.GuideDailyPrice;
        var guidePriceDifference =
            input.GuidePriceDifference - input.GuideDiscountAmount;
        var guideNetAmount =
            guideServiceAmount + guidePriceDifference;

        var totalAmount = transportNetAmount + guideNetAmount;

        // Excel: L = K*10%, U = T*20%.
        var transportVat = input.IncludeVat
            ? transportNetAmount * TransportVatRate
            : 0m;
        var guideVat = input.IncludeVat
            ? guideNetAmount * GuideVatRate
            : 0m;

        // Excel: M = L/2 (%5), V = U*9/10 (%18).
        var transportWithholding = input.IncludeVat
            ? transportVat * 0.5m
            : 0m;
        var guideWithholding = input.IncludeVat
            ? guideVat * 0.9m
            : 0m;

        var vat = transportVat + guideVat;
        var withholding = transportWithholding + guideWithholding;

        // Excel: N = K+M, W = T+V.
        var invoiceAmountAfterWithholding = totalAmount + withholding;

        // Excel: AD = (K*9.48/1000)+(T*9.48/1000).
        var stampTax = totalAmount * StampTaxRate;

        // Excel: AC = (I*6/100)+(R*6/100).
        // I/R fiyat farkı KDV hariç alanlarıdır; Excel'deki gibi negatif değerler
        // ayrıca sıfırlanmaz.
        var additionalGuarantee =
            (transportPriceDifference + guidePriceDifference)
            * AdditionalGuaranteeRate;

        // Excel: AE = M+V+Y+Z+AA+AB+AC+AD.
        var exactTotalDeductions =
            withholding
            + stampTax
            + input.TaxDebt
            + input.InspectionPenalty
            + input.SocialSecurityDebt
            + input.MinimumWageSupport
            + additionalGuarantee;

        var totalDeductions = Round(exactTotalDeductions);

        // Excel: AF = O+X = (K+L)+(T+U).
        // Excel: AG = AF-AE.
        var grossAmount = transportNetAmount
            + transportVat
            + guideNetAmount
            + guideVat;
        var netPayable = grossAmount - exactTotalDeductions;

        var deductions = new DeductionBreakdown(
            Round(transportWithholding + guideWithholding),
            Round(stampTax),
            Round(input.TaxDebt),
            Round(input.InspectionPenalty),
            Round(input.SocialSecurityDebt),
            Round(input.MinimumWageSupport),
            Round(additionalGuarantee));

        return new ProgressPaymentCalculation(
            Round(transportServiceAmount),
            Round(transportPriceDifference),
            Round(totalAmount),
            Round(vat),
            Round(withholding),
            Round(invoiceAmountAfterWithholding),
            Round(stampTax),
            Round(grossAmount),
            totalDeductions,
            Round(netPayable),
            Round(additionalGuarantee),
            deductions,
            Round(guideServiceAmount),
            Round(guidePriceDifference),
            Round(transportNetAmount),
            Round(guideNetAmount),
            Round(transportVat),
            Round(guideVat),
            Round(transportWithholding),
            Round(guideWithholding));
    }

    private static decimal Round(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
