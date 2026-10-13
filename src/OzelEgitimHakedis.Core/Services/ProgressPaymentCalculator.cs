using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed class ProgressPaymentCalculator
{
    public const decimal TransportVatRate = 0.10m;
    public const decimal GuideVatRate = 0.20m;
    public const decimal TransportWithholdingShare = 0.50m;
    public const decimal GuideWithholdingShare = 0.90m;
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

        // Nihai XLSM'nin Hesaplama sayfasındaki zincir, ara hücrelerde
        // gösterilen bir ROUND işlemi bulunmadığı sürece tam decimal hassasiyetinde
        // korunur. UI ve resmi çıktılar görüntüleme aşamasında iki ondalık gösterir.
        //
        // Taşıma:
        // K = (G*H) + I - J
        // L = IF(E="Hayır", K*10%, 0)
        // M = L/2
        // N = K+M
        // O = K+L
        var transportServiceAmount = input.WorkDays * input.DailyPrice;
        var transportPriceDifference = input.PriceDifference - input.DiscountAmount;
        var transportNetAmount = transportServiceAmount + transportPriceDifference;

        // Rehber:
        // T = (P*Q) + R - S
        // U = IF(E="Hayır", T*20%, 0)
        // V = IF(E="Hayır", U*9/10, 0)
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
            ? transportVat * TransportWithholdingShare
            : 0m;
        var guideWithholding = input.IncludeVat
            ? guideVat * GuideWithholdingShare
            : 0m;

        var vat = transportVat + guideVat;
        var withholding = transportWithholding + guideWithholding;

        // Excel'deki N + W:
        // Taşıma N=K+M, Rehber W=T+V.
        var transportInvoiceAmount = transportNetAmount + transportWithholding;
        var guideInvoiceAmount = guideNetAmount + guideWithholding;
        var invoiceAmountAfterWithholding =
            transportInvoiceAmount + guideInvoiceAmount;

        // Excel AD:
        // AD = (K*9.48/1000) + (T*9.48/1000)
        var transportStampTax = transportNetAmount * StampTaxRate;
        var guideStampTax = guideNetAmount * StampTaxRate;
        var stampTax = transportStampTax + guideStampTax;

        // 4735 sayılı Kanun'un 12. maddesindeki ek kesin teminat,
        // fiyat farkının sözleşme bedelinde artış meydana getirdiği tutar üzerinden
        // alınır. Negatif fiyat farkı artış doğurmadığından negatif teminat kesintisi
        // (yüklenici lehine bir ek ödeme) oluşturulmamalıdır. Kaynak Excel'deki AC
        // formülü ham I/R girdilerini kullanır; burada yalnızca negatif tutarı sıfırla
        // sınırlıyoruz, iskonto J/S'yi teminat tabanına dahil etmiyoruz.
        var combinedPriceDifference =
            input.PriceDifference + input.GuidePriceDifference;
        var additionalGuarantee =
            Math.Max(0m, combinedPriceDifference) * AdditionalGuaranteeRate;

        // Excel AE = M+V+Y+Z+AA+AB+AC+AD.
        var totalDeductions =
            transportWithholding
            + guideWithholding
            + input.TaxDebt
            + input.InspectionPenalty
            + input.SocialSecurityDebt
            + input.MinimumWageSupport
            + additionalGuarantee
            + stampTax;

        // Excel AF = O+X; AG = AF-AE.
        var transportGrossAmount = transportNetAmount + transportVat;
        var guideGrossAmount = guideNetAmount + guideVat;
        var grossAmount = transportGrossAmount + guideGrossAmount;
        var netPayable = grossAmount - totalDeductions;

        var deductions = new DeductionBreakdown(
            withholding,
            stampTax,
            input.TaxDebt,
            input.InspectionPenalty,
            input.SocialSecurityDebt,
            input.MinimumWageSupport,
            additionalGuarantee);

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
}
