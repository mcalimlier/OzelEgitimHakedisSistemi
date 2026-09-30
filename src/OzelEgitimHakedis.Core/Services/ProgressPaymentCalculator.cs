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

        // Kaynak Excel'deki Hesaplama hücreleri ara adımlarda ROUND kullanmıyor.
        // Bu nedenle zincir tam hassasiyetle ilerler; 2 ondalık yuvarlama yalnızca
        // ekranda/raporda gösterim sınırında yapılır. Bu, özellikle tevkifat,
        // damga vergisi ve net ödemede kuruş farkını önler.

        // Taşıma:
        // K = (G*H) + I - J
        // L = IF(KDV uygulanır, K*10/100)
        // M = L/2
        // N = K+M
        // O = K+L
        var transportServiceAmount = input.WorkDays * input.DailyPrice;
        var transportPriceDifference = input.PriceDifference - input.DiscountAmount;
        var transportNetAmount = transportServiceAmount + transportPriceDifference;

        // Rehber:
        // T = (P*Q) + R - S
        // U = IF(KDV uygulanır, T*20/100)
        // V = U*9/10
        // W = T+V
        // X = T+U
        var guideServiceAmount = input.GuideWorkDays * input.GuideDailyPrice;
        var guidePriceDifference = input.GuidePriceDifference - input.GuideDiscountAmount;
        var guideNetAmount = guideServiceAmount + guidePriceDifference;

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

        var transportInvoiceAmount = transportNetAmount + transportWithholding;
        var guideInvoiceAmount = guideNetAmount + guideWithholding;
        var invoiceAmountAfterWithholding =
            transportInvoiceAmount + guideInvoiceAmount;

        // Damga vergisi:
        // AD = (K*9.48/1000) + (T*9.48/1000)
        var transportStampTax = transportNetAmount * StampTaxRate;
        var guideStampTax = guideNetAmount * StampTaxRate;
        var stampTax = transportStampTax + guideStampTax;

        // Excel AC hücresi doğrudan I ve R sütunlarını kullanır:
        // AC = (I*6/100) + (R*6/100).
        // I/R, iskonto düşülmeden önceki fiyat farkı alanlarıdır.
        // Bu nedenle ek kesin teminat, net fiyat farkından değil,
        // kaynak Excel'deki ham fiyat farkı tutarlarından hesaplanır.
        var transportAdditionalGuarantee =
            input.PriceDifference * AdditionalGuaranteeRate;
        var guideAdditionalGuarantee =
            input.GuidePriceDifference * AdditionalGuaranteeRate;
        var additionalGuarantee =
            transportAdditionalGuarantee + guideAdditionalGuarantee;

        // Excel: AE = M+V+Y+Z+AA+AB+AC+AD.
        // Kesinti toplamı ara hücreler yuvarlanmadan oluşur.
        var totalDeductions =
            withholding
            + stampTax
            + input.TaxDebt
            + input.InspectionPenalty
            + input.SocialSecurityDebt
            + input.MinimumWageSupport
            + additionalGuarantee;

        // Excel: AF = O+X; AG = AF-AE.
        var transportGrossAmount = transportNetAmount + transportVat;
        var guideGrossAmount = guideNetAmount + guideVat;
        var grossAmount = transportGrossAmount + guideGrossAmount;
        var netPayable = grossAmount - totalDeductions;

        return new ProgressPaymentCalculation(
            transportServiceAmount,
            transportPriceDifference,
            totalAmount,
            vat,
            withholding,
            invoiceAmountAfterWithholding,
            stampTax,
            grossAmount,
            totalDeductions,
            netPayable,
            additionalGuarantee,
            deductions,
            guideServiceAmount,
            guidePriceDifference,
            transportNetAmount,
            guideNetAmount,
            transportVat,
            guideVat,
            transportWithholding,
            guideWithholding);
    }

    private static decimal Round(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);
}
