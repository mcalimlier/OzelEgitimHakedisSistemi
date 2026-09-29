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
                "İskonto, kesinti ve denetim cezası tutarları negatif olamaz.");
        }

        var transportServiceAmount = input.WorkDays * input.DailyPrice;
        // Excel: Ek Fiyat Farkı KDV Hariç = İskonto Tutarı + Ek Fiyat Farkı.
        // İskonto alanı Excel'de işaretli bir bileşendir; negatif değer girilmesi
        // gerçek bir iskonto olarak desteklenir.
        var transportPriceDifference =
            input.DiscountAmount + input.PriceDifference;
        var transportNetAmount =
            transportServiceAmount + transportPriceDifference;

        var guideServiceAmount = input.GuideWorkDays * input.GuideDailyPrice;
        var guidePriceDifference =
            input.GuideDiscountAmount + input.GuidePriceDifference;
        var guideNetAmount =
            guideServiceAmount + guidePriceDifference;

        var totalAmount = transportNetAmount + guideNetAmount;

        var transportVat = input.IncludeVat
            ? transportNetAmount * TransportVatRate
            : 0m;
        var guideVat = input.IncludeVat
            ? guideNetAmount * GuideVatRate
            : 0m;

        var transportWithholding = input.IncludeVat
            ? transportVat * 0.5m
            : 0m;
        var guideWithholding = input.IncludeVat
            ? guideVat * 0.9m
            : 0m;

        var vat = transportVat + guideVat;
        var withholding = transportWithholding + guideWithholding;

        var invoiceAmountAfterWithholding =
            totalAmount + vat - withholding;

        var stampTax =
            Math.Max(totalAmount, 0m) * StampTaxRate;

        var inspectionPenalty = input.InspectionPenalty;

        // Excel'de iki sürümün formülü bulunuyor:
        // 1) Taşıma-only V3: Ek Kesin Teminat = max(H, 0) × %6.
        // 2) Rehber personel eklenmiş tabloda AC hücresi:
        //    =(I4*6/100)+R4*6/100
        //    I4 = Taşıma Toplam Tutarı, R4 = Rehber Fiyat Farkı KDV Hariç.
        // Rehber bileşeni kullanılıyorsa genişletilmiş Excel formülünü,
        // rehber yoksa mevcut V3 taşıma formülünü koruyoruz.
        var hasGuideComponent =
            input.GuideWorkDays > 0
            || input.GuideDailyPrice != 0m
            || guidePriceDifference != 0m;

        var additionalGuarantee = hasGuideComponent
            ? (transportNetAmount + guidePriceDifference) * AdditionalGuaranteeRate
            : Math.Max(transportPriceDifference, 0m) * AdditionalGuaranteeRate;

        var deductions = new DeductionBreakdown(
            Round(withholding),
            Round(stampTax),
            Round(input.TaxDebt),
            Round(inspectionPenalty),
            Round(input.SocialSecurityDebt),
            Round(input.MinimumWageSupport),
            Round(additionalGuarantee));

        var grossAmount = totalAmount + vat;
        // Excel'deki hücreler ara sonuçları tam hassasiyetle toplar; son tutar
        // ekranda iki ondalığa yuvarlanır.
        var exactTotalDeductions =
            withholding
            + stampTax
            + input.TaxDebt
            + inspectionPenalty
            + input.SocialSecurityDebt
            + input.MinimumWageSupport
            + additionalGuarantee;
        var totalDeductions = Round(exactTotalDeductions);
        var netPayable = grossAmount - exactTotalDeductions;

        return new ProgressPaymentCalculation(
            Round(transportServiceAmount),
            Round(transportPriceDifference),
            Round(totalAmount),
            Round(vat),
            Round(withholding),
            Round(invoiceAmountAfterWithholding),
            Round(stampTax),
            Round(grossAmount),
            Round(totalDeductions),
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
