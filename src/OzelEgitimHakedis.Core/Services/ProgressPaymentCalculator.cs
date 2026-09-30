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

        // Kaynak Excel hesap zinciri 2 ondalık basamakta ilerler.
        // Ara hücrelerdeki ROUND(...,2) davranışını korumak önemlidir;
        // aksi halde özellikle kesinti toplamında kuruş farkı oluşabilir.
        //
        // Taşıma:
        // K = (G*H) + I - J
        // L = IF(KDV uygulanır, K*10/100)
        // M = L/2
        // N = K+M
        // O = K+L
        var transportServiceAmount = Round(input.WorkDays * input.DailyPrice);
        var transportPriceDifference =
            Round(input.PriceDifference - input.DiscountAmount);
        var transportNetAmount =
            Round(transportServiceAmount + transportPriceDifference);

        // Rehber:
        // T = (P*Q) + R - S
        // U = IF(KDV uygulanır, T*20/100)
        // V = U*9/10
        // W = T+V
        // X = T+U
        var guideServiceAmount = Round(input.GuideWorkDays * input.GuideDailyPrice);
        var guidePriceDifference =
            Round(input.GuidePriceDifference - input.GuideDiscountAmount);
        var guideNetAmount =
            Round(guideServiceAmount + guidePriceDifference);

        var totalAmount = Round(transportNetAmount + guideNetAmount);

        var transportVat = input.IncludeVat
            ? Round(transportNetAmount * TransportVatRate)
            : 0m;
        var guideVat = input.IncludeVat
            ? Round(guideNetAmount * GuideVatRate)
            : 0m;

        var transportWithholding = input.IncludeVat
            ? Round(transportVat * 0.5m)
            : 0m;
        var guideWithholding = input.IncludeVat
            ? Round(guideVat * 0.9m)
            : 0m;

        var vat = Round(transportVat + guideVat);
        var withholding = Round(transportWithholding + guideWithholding);

        var transportInvoiceAmount = Round(transportNetAmount + transportWithholding);
        var guideInvoiceAmount = Round(guideNetAmount + guideWithholding);
        var invoiceAmountAfterWithholding =
            Round(transportInvoiceAmount + guideInvoiceAmount);

        // Damga vergisi taşıma + rehber hakedişleri üzerinden yürür.
        var transportStampTax = Round(transportNetAmount * StampTaxRate);
        var guideStampTax = Round(guideNetAmount * StampTaxRate);
        var stampTax = Round(transportStampTax + guideStampTax);

        // Excel AC hücresi doğrudan I ve R sütunlarını kullanır:
        // AC = (I*6/100) + (R*6/100).
        // I/R, iskonto düşülmeden önceki fiyat farkı alanlarıdır.
        // Bu nedenle ek kesin teminat, net fiyat farkından değil,
        // kaynak Excel'deki ham fiyat farkı tutarlarından hesaplanır.
        // Negatif değerler ayrıca sıfırlanmaz.
        var transportAdditionalGuarantee =
            Round(input.PriceDifference * AdditionalGuaranteeRate);
        var guideAdditionalGuarantee =
            Round(input.GuidePriceDifference * AdditionalGuaranteeRate);
        var additionalGuarantee =
            Round(transportAdditionalGuarantee + guideAdditionalGuarantee);

        // Excel: AE = M+V+Y+Z+AA+AB+AC+AD.
        // Kesinti toplamı, yukarıdaki yuvarlanmış hücrelerin toplamından oluşur.
        // Excel'deki kesinti hücreleri iki ondalık basamakla ilerler.
        // Kullanıcı üç veya daha fazla basamak girse bile toplam kesinti
        // aynı hücre yuvarlama sırasını izlemelidir.
        var totalDeductions = Round(
            withholding
            + stampTax
            + Round(input.TaxDebt)
            + Round(input.InspectionPenalty)
            + Round(input.SocialSecurityDebt)
            + Round(input.MinimumWageSupport)
            + additionalGuarantee);

        // Excel: AF = O+X; AG = AF-AE.
        var transportGrossAmount = Round(transportNetAmount + transportVat);
        var guideGrossAmount = Round(guideNetAmount + guideVat);
        var grossAmount = Round(transportGrossAmount + guideGrossAmount);
        var netPayable = Round(grossAmount - totalDeductions);

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
