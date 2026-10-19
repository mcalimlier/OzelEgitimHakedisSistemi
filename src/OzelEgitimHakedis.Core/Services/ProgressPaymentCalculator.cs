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

    public static ProgressPaymentCalculation Calculate(ProgressPaymentInput input) =>
        Calculate(input, ProgressPaymentCalculationConstants.Defaults);

    public static ProgressPaymentCalculation Calculate(
        ProgressPaymentInput input,
        ProgressPaymentCalculationConstants constants)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(constants);
        if (!constants.TryValidate(out var constantsError))
            throw new ArgumentOutOfRangeException(nameof(constants), constantsError);

        // Temel/ortaöğretim kaynak Excel'inde rehber personel kalemi yoktur,
        // ancak Asgari Ücret Destek Tutarı kesinti toplamına dahildir.
        // Bu nedenle yalnızca rehber alanları program kapsamında sıfırlanır.
        if (input.ProgramType == TransportationProgramType.BasicOrSecondary)
        {
            input = input with
            {
                GuideWorkDays = 0,
                GuideDailyPrice = 0m,
                GuideDiscountAmount = 0m,
                GuidePriceDifference = 0m
            };
            constants = constants with
            {
                GuideVatRate = 0m,
                GuideWithholdingShare = 0m
            };
        }

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
            ? transportNetAmount * constants.TransportVatRate
            : 0m;
        var guideVat = input.IncludeVat
            ? guideNetAmount * constants.GuideVatRate
            : 0m;

        var transportWithholding = input.IncludeVat
            ? transportVat * constants.TransportWithholdingShare
            : 0m;
        var guideWithholding = input.IncludeVat
            ? guideVat * constants.GuideWithholdingShare
            : 0m;

        var vat = transportVat + guideVat;
        var withholding = transportWithholding + guideWithholding;

        // Excel'deki N + W:
        // Taşıma N=K+M, Rehber W=T+V.
        var transportInvoiceAmount = transportNetAmount + transportWithholding;
        var guideInvoiceAmount = guideNetAmount + guideWithholding;
        var invoiceAmountAfterWithholding =
            transportInvoiceAmount + guideInvoiceAmount;

        // Nihai kaynak XLSM formülleri:
        // Temel Eğitim: U = K*9.48/1000.
        // Özel Eğitim: AD = (K*9.48/1000) + (T*9.48/1000).
        // Hakediş Raporu'nun ayrı D15 formülü rapor servisi içinde uygulanır.
        var transportStampTax = transportNetAmount * constants.StampTaxRate;
        var guideStampTax = input.ProgramType == TransportationProgramType.BasicOrSecondary
            ? 0m
            : guideNetAmount * constants.StampTaxRate;
        // Hesaplama sayfası kaynak formülü negatif değerleri aynen korur.
        // Temel Eğitim raporundaki D15 için negatif matrahın sıfırlanması ayrı rapor formülüdür.
        var stampTax = transportStampTax + guideStampTax;

        // Onaylanan kaynak davranışı: yeni yüklenen XLSM formülleri esas alınır.
        // Temel Eğitim: Hesaplama Bilgileri!T4 = I4*6/100; I4, input.PriceDifference alanıdır.
        // Özel Eğitim: Hesaplama!AC4 = (I4*6/100) + (R4*6/100).
        // Her iki formülde de iskonto J/S kullanılmaz.
        var additionalGuarantee = input.ProgramType == TransportationProgramType.BasicOrSecondary
            ? input.PriceDifference * constants.AdditionalGuaranteeRate
            : (input.PriceDifference * constants.AdditionalGuaranteeRate)
              + (input.GuidePriceDifference * constants.AdditionalGuaranteeRate);

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

        // Hesaplama sayfası neti: W = O - V / AG = AF - AE.
        // Hakediş Raporu D28 ayrı bir hücre zinciridir ve rapor servisinde hesaplanır.
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
            guideWithholding,
            AppliedConstants: constants);
    }
}
