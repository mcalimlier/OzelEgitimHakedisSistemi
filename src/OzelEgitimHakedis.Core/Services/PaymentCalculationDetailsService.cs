using System.Globalization;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record PaymentCalculationDetail(string Section, string Name, string Formula, decimal Value)
{
    public string ValueText => Value.ToString("#,##0.00##########################", CultureInfo.GetCultureInfo("tr-TR"));
}

/// <summary>Explains the saved snapshot. Never substitutes current settings or recalculates its amounts.</summary>
public static class PaymentCalculationDetailsService
{
    public static IReadOnlyList<PaymentCalculationDetail> Build(ProgressPaymentHistoryData payment,
        IEnumerable<ProgressPaymentHistoryData> history)
    {
        ArgumentNullException.ThrowIfNull(payment);
        ArgumentNullException.ThrowIfNull(history);
        var input = payment.Input;
        var result = payment.Calculation;
        var rates = result.AppliedConstants;
        var rows = new List<PaymentCalculationDetail>();
        string N(decimal number) => number.ToString("#,##0.############################", CultureInfo.GetCultureInfo("tr-TR"));
        string Rate(decimal? rate) => rate.HasValue ? $"%{N(rate.Value * 100m)}" : "oran kaydı yok";
        void Add(string section, string name, string formula, decimal value) => rows.Add(new(section, name, formula, value));
        void Input(string name, decimal value) => Add("Girişler", name, "Kaydedilmiş giriş", value);
        Input("Taşıma çalışma günü", input.WorkDays);
        Input("Taşıma günlük fiyatı", input.DailyPrice);
        Input("Taşıma iskonto", input.DiscountAmount);
        Input("Taşıma fiyat farkı (KDV hariç)", input.PriceDifference);
        Input("Rehber çalışma günü", input.GuideWorkDays);
        Input("Rehber günlük fiyatı", input.GuideDailyPrice);
        Input("Rehber iskonto", input.GuideDiscountAmount);
        Input("Rehber fiyat farkı (KDV hariç)", input.GuidePriceDifference);
        Input("Sözleşme bedeli", input.ContractAmount);
        Input("Vergi borcu", input.TaxDebt);
        Input("SGK borcu", input.SocialSecurityDebt);
        Input("Asgari ücret desteği", input.MinimumWageSupport);
        Input("Denetim / cezai işlem", input.InspectionPenalty);
        if (rates is not null)
        {
            Add("Kayıtlı oranlar", "Taşıma KDV oranı (%)", "Hakedişle saklanan oran × 100", rates.TransportVatRate * 100m);
            Add("Kayıtlı oranlar", "Rehber KDV oranı (%)", "Hakedişle saklanan oran × 100", rates.GuideVatRate * 100m);
            Add("Kayıtlı oranlar", "Taşıma KDV tevkifat payı (%)", "Hakedişle saklanan pay × 100", rates.TransportWithholdingShare * 100m);
            Add("Kayıtlı oranlar", "Rehber KDV tevkifat payı (%)", "Hakedişle saklanan pay × 100", rates.GuideWithholdingShare * 100m);
            Add("Kayıtlı oranlar", "Damga vergisi oranı (%)", "Hakedişle saklanan oran × 100", rates.StampTaxRate * 100m);
            Add("Kayıtlı oranlar", "Ek kesin teminat oranı (%)", "Hakedişle saklanan oran × 100", rates.AdditionalGuaranteeRate * 100m);
        }
        Add("Taşıma", "Taşıma hizmet tutarı", $"{N(input.WorkDays)} × {N(input.DailyPrice)}", result.ServiceAmount);
        Add("Taşıma", "Taşıma net fiyat farkı", $"{N(input.PriceDifference)} − {N(input.DiscountAmount)}", result.PriceDifferenceExcludingVat);
        Add("Taşıma", "Taşıma matrahı", "Taşıma hizmet tutarı + taşıma net fiyat farkı", result.TransportNetAmount);
        Add("Taşıma", "Taşıma KDV", input.IsVatExempt ? "KDV istisnası: 0" : $"Taşıma matrahı × {Rate(rates?.TransportVatRate)}", result.TransportVat);
        Add("Taşıma", "Taşıma tevkifatı", input.IsVatExempt ? "KDV istisnası: 0" : $"Taşıma KDV × {Rate(rates?.TransportWithholdingShare)}", result.TransportWithholding);
        Add("Taşıma", "Taşıma brütü", "Taşıma matrahı + taşıma KDV", result.TransportGrossAmount);
        Add("Rehber", "Rehber hizmet tutarı", input.ProgramType == TransportationProgramType.BasicOrSecondary ? "Bu programda rehber hizmeti yok" : $"{N(input.GuideWorkDays)} × {N(input.GuideDailyPrice)}", result.GuideServiceAmount);
        Add("Rehber", "Rehber net fiyat farkı", "Rehber fiyat farkı − rehber iskonto (temel/ortaöğretimde 0)", result.GuidePriceDifferenceExcludingVat);
        Add("Rehber", "Rehber matrahı", "Rehber hizmet tutarı + rehber net fiyat farkı", result.GuideNetAmount);
        Add("Rehber", "Rehber KDV", input.IsVatExempt ? "KDV istisnası: 0" : $"Rehber matrahı × {Rate(rates?.GuideVatRate)}", result.GuideVat);
        Add("Rehber", "Rehber tevkifatı", input.IsVatExempt ? "KDV istisnası: 0" : $"Rehber KDV × {Rate(rates?.GuideWithholdingShare)}", result.GuideWithholding);
        Add("Rehber", "Rehber brütü", "Rehber matrahı + rehber KDV", result.GuideGrossAmount);
        Add("Toplamlar", "Toplam hizmet", "Taşıma hizmet tutarı + rehber hizmet tutarı", result.CombinedServiceAmount);
        Add("Toplamlar", "Toplam net fiyat farkı", "Taşıma net fiyat farkı + rehber net fiyat farkı", result.CombinedPriceDifferenceExcludingVat);
        Add("Toplamlar", "Toplam matrah", "Taşıma matrahı + rehber matrahı", result.TotalAmount);
        Add("Toplamlar", "Toplam KDV", "Taşıma KDV + rehber KDV", result.Vat);
        Add("Toplamlar", "Toplam tevkifat", "Taşıma tevkifatı + rehber tevkifatı", result.Withholding);
        Add("Toplamlar", "Fatura tutarı (kaynak Excel zinciri)", "Toplam matrah + toplam tevkifat", result.InvoiceAmountAfterWithholding);
        Add("Toplamlar", "KDV dahil brüt", "Toplam matrah + toplam KDV", result.GrossAmount);
        Add("Kesintiler", "Damga vergisi", $"Toplam matrah × {Rate(rates?.StampTaxRate)}", result.StampTax);
        Add("Kesintiler", "Ek kesin teminat", $"Ham taşıma fiyat farkı{(input.ProgramType == TransportationProgramType.SpecialEducation ? " + ham rehber fiyat farkı" : "")} toplamı × {Rate(rates?.AdditionalGuaranteeRate)} (iskonto düşülmez)", result.AdditionalGuarantee);
        Add("Kesintiler", "Vergi borcu kesintisi", "Kaydedilmiş kesinti", result.Deductions.TaxDebt);
        Add("Kesintiler", "SGK borcu kesintisi", "Kaydedilmiş kesinti", result.Deductions.SocialSecurityDebt);
        Add("Kesintiler", "Asgari ücret desteği kesintisi", "Kaydedilmiş kesinti", result.Deductions.MinimumWageSupport);
        Add("Kesintiler", "Denetim / ceza kesintisi", "Kaydedilmiş kesinti", result.Deductions.InspectionPenalty);
        Add("Kesintiler", "Toplam kesinti", "Tevkifat + damga vergisi + ek teminat + vergi borcu + SGK borcu + asgari ücret desteği + ceza", result.TotalDeductions);
        Add("Sonuç", "Net ödenecek (hesaplama)", "KDV dahil brüt − toplam kesinti; ara adımlarda yuvarlama yapılmaz", result.NetPayable);
        var totals = ProgressPaymentReportService.Calculate(payment, history.Where(p => p.Id != payment.Id).Append(payment));
        Add("Resmî rapor", "Kümülatif hizmet tutarı", "Aynı hat / firma / ihale ve mali yılda seçili aya kadar matrah toplamı − bu ayın ham fiyat farkı", totals.CumulativeServiceAmount);
        Add("Resmî rapor", "Bu ayın fiyat farkı", "Kaydedilmiş ham taşıma + program kapsamındaki rehber fiyat farkı", totals.CurrentPriceDifference);
        Add("Resmî rapor", "Kümülatif toplam", "Kümülatif hizmet tutarı + bu ayın fiyat farkı", totals.CumulativeTotal);
        Add("Resmî rapor", "Önceki hakedişler toplamı", "Aynı kimliğe ait önceki ayların matrahı", totals.PreviousTotal);
        Add("Resmî rapor", "Bu hakediş tutarı", "Kümülatif toplam − önceki hakedişler toplamı", totals.CurrentTotal);
        Add("Resmî rapor", "Rapor damga vergisi", "Temel/ortaöğretimde negatif matrah 0 kabul edilir; özel eğitimde kayıtlı damga vergisi", ProgressPaymentReportService.CalculateReportStampTax(result, input.ProgramType));
        Add("Resmî rapor", "Rapor kesinti toplamı", "Toplam kesinti − hesaplama damga vergisi + rapor damga vergisi", ProgressPaymentReportService.CalculateReportTotalDeductions(result, input.ProgramType));
        Add("Resmî rapor", "PDF net ödeme", "YUVARLA(bu hakediş tutarı + KDV; 2) − YUVARLA(rapor kesinti toplamı; 2)", ProgressPaymentReportService.CalculateReportNetPayable(payment, totals));
        return rows;
    }
}
