using System.Globalization;
using System.Text.Json;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record PaymentIssue(string Message, string Target);
public sealed record PaymentDifference(string Field, string Before, string After);
public sealed record PaymentFilter(string Search = "", DateOnly? Month = null, string Tender = "",
    string Contractor = "", TransportationProgramType? Program = null, bool UndatedOnly = false);
public sealed record MonthlyPaymentRow(RouteData Route, ContractDraftData? Contract, Contractor? Contractor,
    ProgressPaymentHistoryData? Payment, string Status, IReadOnlyList<PaymentIssue> Issues)
{
    public string Tender => Route.TenderNumber;
    public string Firm => Contractor?.Name ?? Payment?.ContractorName ?? "Firma bağlantısı eksik";
    public string RouteTitle => $"{Route.RouteNumber} — {Route.Name}";
    public string NetText => Payment is null ? "—" : Payment.Calculation.NetPayable.ToString("N2", CultureInfo.GetCultureInfo("tr-TR")) + " ₺";
    public string Group => Status == "Tamamlandı" ? "Tamamlanan" : Payment is null || Issues.Count > 0 ? "Bekleyen" : "Kontrole hazır";
}

public static class PaymentOperationsService
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    public static string RouteKey(RouteData r) => OehsIdentityService.BuildRouteKey(r.TenderNumber,
        r.RouteNumber, r.ContractorTaxOrIdentityNumber, r.ContractNumber, r.ProgramType);
    public static string RouteKey(ProgressPaymentHistoryData p) => OehsIdentityService.BuildRouteKey(p.TenderNumber ?? "",
        p.RouteNumber ?? "", p.ContractorTaxOrIdentityNumber ?? "", p.ContractNumber ?? "", p.Input.ProgramType);
    public static bool SameMonth(DateOnly a, DateOnly b) => a.Year == b.Year && a.Month == b.Month;
    public static bool Contains(string? value, string? search) => string.IsNullOrWhiteSpace(search) ||
        Turkish.CompareInfo.IndexOf(value ?? "", search.Trim(), CompareOptions.IgnoreCase) >= 0;

    public static bool Matches(ProgressPaymentHistoryData p, PaymentFilter f) =>
        (!f.Program.HasValue || p.Input.ProgramType == f.Program) &&
        (!f.Month.HasValue || p.PeriodDate.HasValue && SameMonth(p.PeriodDate.Value, f.Month.Value)) &&
        (!f.UndatedOnly || !p.PeriodDate.HasValue) && Contains(p.TenderNumber, f.Tender) &&
        Contains(p.ContractorName, f.Contractor) && Contains(string.Join(" ", p.TenderNumber, p.RouteNumber,
            p.RouteName, p.ContractorName), f.Search);

    public static IReadOnlyList<PaymentIssue> Validate(ProgressPaymentHistoryData p, ContractDraftData? c,
        RouteData? r, Contractor? firm)
    {
        var issues = new List<PaymentIssue>();
        if (string.IsNullOrWhiteSpace(p.TenderNumber) || c is null)
            issues.Add(new("İKN / sözleşme bağlantısını tamamlayın.", "Contract"));
        if (r is null || string.IsNullOrWhiteSpace(r.Name) || r is not null && RouteKey(r) != RouteKey(p))
            issues.Add(new("Hat bağlantısını ve adını kontrol edin.", "Lines"));
        if (firm is null || string.IsNullOrWhiteSpace(firm.Name))
            issues.Add(new("Yüklenici firma bağlantısını tamamlayın.", "Contractors"));
        if (!p.PeriodDate.HasValue || p.ProgressPaymentNumber is null or <= 0)
            issues.Add(new("Hakediş dönemi ve numarasını tamamlayın.", "Progress"));
        var start = p.ContractStartDate ?? c?.StartDate;
        var end = p.ContractEndDate ?? c?.EndDate;
        if (start is null || end is null || end < start)
            issues.Add(new("Sözleşme başlangıç/bitiş tarihlerini kontrol edin.", "Contract"));
        else if (p.PeriodDate is DateOnly month && (end < new DateOnly(month.Year, month.Month, 1) ||
                 start > new DateOnly(month.Year, month.Month, DateTime.DaysInMonth(month.Year, month.Month))))
            issues.Add(new("Hakediş ayı sözleşme tarihleri dışında.", "Progress"));
        if (p.Input.WorkDays < 0 || p.Input.DailyPrice < 0 || p.Input.GuideWorkDays < 0 || p.Input.GuideDailyPrice < 0)
            issues.Add(new("Çalışma günleri ve günlük fiyatları kontrol edin.", "Progress"));
        return issues;
    }

    public static IReadOnlyList<MonthlyPaymentRow> BuildMonth(OehsApplicationData data, TransportationProgramType program,
        DateOnly month, IReadOnlyList<ProgressPaymentArchive>? archives = null, Func<ProgressPaymentHistoryData, bool>? currentOutput = null)
    {
        var first = new DateOnly(month.Year, month.Month, 1);
        var last = first.AddMonths(1).AddDays(-1);
        var contracts = (data.Contracts ?? (data.Contract is null ? Array.Empty<ContractDraftData>() : new[] { data.Contract })).ToArray();
        var rows = new List<MonthlyPaymentRow>();
        foreach (var route in data.Routes.Where(r => r.ProgramType == program))
        {
            var c = contracts.FirstOrDefault(c => c.ProgramType == program &&
                OehsIdentityService.BuildContractKey(c.TenderNumber, c.ContractNumber, program) ==
                OehsIdentityService.BuildContractKey(route.TenderNumber, route.ContractNumber, program));
            if (c is not null && (c.EndDate < first || c.StartDate > last)) continue;
            var firm = data.Contractors.FirstOrDefault(f => f.ProgramType == program &&
                OehsIdentityService.NormalizeIdentity(f.TaxOrIdentityNumber) == OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber));
            var payments = (data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>()).Where(p =>
                p.PeriodDate.HasValue && SameMonth(p.PeriodDate.Value, month) && RouteKey(p) == RouteKey(route))
                .DistinctBy(p => p.Id).OrderByDescending(p => p.CreatedAt).ToArray();
            var payment = payments.FirstOrDefault();
            var issues = payment is null ? new List<PaymentIssue>() : Validate(payment, c, route, firm).ToList();
            if (payment is null && c is null) issues.Add(new("Sözleşme bağlantısı eksik.", "Contract"));
            if (payment is null && firm is null) issues.Add(new("Firma bağlantısı eksik.", "Contractors"));
            if (payments.Length > 1) issues.Add(new("Birden fazla hakediş var; dönem kayıtlarını kontrol edin.", "History"));
            var archive = archives?.Where(a => a.ProgressPayment.Id == payment?.Id)
                .OrderByDescending(a => a.Revision).FirstOrDefault();
            var archived = archive is not null && archive.ProgressPayment == payment;
            var status = payment is null ? "Hakediş yok" : issues.Count > 0 ? "Bilgi eksik" :
                !archived ? "Kontrole hazır" : currentOutput?.Invoke(payment!) == true ? "Tamamlandı" : "Arşivlendi · PDF bekliyor";
            rows.Add(new(route, c, firm, payment, status, issues));
        }
        return rows.OrderBy(r => r.Firm, StringComparer.Create(Turkish, true)).ThenBy(r => r.Route.RouteNumber, StringComparer.Ordinal).ToArray();
    }

    public static IReadOnlyList<PaymentDifference> Compare(ProgressPaymentHistoryData before, ProgressPaymentHistoryData after)
    {
        var result = new List<PaymentDifference>();
        void Add(string field, object? oldValue, object? newValue)
        {
            if (!Equals(oldValue, newValue)) result.Add(new(field, Display(oldValue), Display(newValue)));
        }
        Add("İKN", before.TenderNumber, after.TenderNumber);
        Add("Yüklenici", before.ContractorName, after.ContractorName);
        Add("Vergi / kimlik no", before.ContractorTaxOrIdentityNumber, after.ContractorTaxOrIdentityNumber);
        Add("Hat no", before.RouteNumber, after.RouteNumber); Add("Hattın adı", before.RouteName, after.RouteName);
        Add("Dönem", before.PeriodDate, after.PeriodDate); Add("Hakediş no", before.ProgressPaymentNumber, after.ProgressPaymentNumber);
        Add("Başlangıç tarihi", before.ContractStartDate, after.ContractStartDate);
        Add("Bitiş tarihi", before.ContractEndDate, after.ContractEndDate);
        Add("Banka hesabı", before.BankAccountNumber, after.BankAccountNumber);
        var labels = new Dictionary<string, string>(StringComparer.Ordinal)
        {
            ["WorkDays"] = "Taşıma iş günü", ["DailyPrice"] = "Taşıma günlük fiyatı", ["GuideWorkDays"] = "Rehber iş günü",
            ["GuideDailyPrice"] = "Rehber günlük fiyatı", ["DiscountAmount"] = "Taşıma iskontosu", ["PriceDifference"] = "Taşıma fiyat farkı",
            ["GuideDiscountAmount"] = "Rehber iskontosu", ["GuidePriceDifference"] = "Rehber fiyat farkı", ["ContractAmount"] = "Sözleşme bedeli",
            ["IsVatExempt"] = "KDV istisnası", ["TaxDebt"] = "Vergi borcu", ["SocialSecurityDebt"] = "SGK borcu",
            ["MinimumWageSupport"] = "Asgari ücret desteği", ["InspectionPenalty"] = "Denetim cezası", ["ProgramType"] = "Program"
        };
        foreach (var prop in typeof(ProgressPaymentInput).GetProperties().Where(p => labels.ContainsKey(p.Name)))
            Add(labels[prop.Name], prop.GetValue(before.Input), prop.GetValue(after.Input));
        Add("Net ödeme", before.Calculation.NetPayable, after.Calculation.NetPayable);
        Add("Toplam kesinti", before.Calculation.TotalDeductions, after.Calculation.TotalDeductions);
        if (before.Calculation.AppliedConstants != after.Calculation.AppliedConstants)
            result.Add(new("Hesaplama oranları", JsonSerializer.Serialize(before.Calculation.AppliedConstants),
                JsonSerializer.Serialize(after.Calculation.AppliedConstants)));
        return result;
    }
    public static IReadOnlyList<PaymentDifference> CompareFields(object before, object after, string prefix)
    {
        var result = new List<PaymentDifference>();
        foreach (var p in before.GetType().GetProperties().Where(p => p.CanRead && p.GetIndexParameters().Length == 0))
        {
            if (p.Name is "DisplayText" or "ContractNumber") continue;
            var oldValue = p.GetValue(before); var newValue = p.GetValue(after);
            if (!Equals(oldValue, newValue)) result.Add(new($"{prefix} · {p.Name}", Display(oldValue), Display(newValue)));
        }
        return result;
    }
    private static string Display(object? value) => value switch
    {
        null => "—", DateOnly date => date.ToString("dd.MM.yyyy", Turkish),
        decimal number => number.ToString("0.############################", Turkish),
        bool boolean => boolean ? "Evet" : "Hayır", _ => Convert.ToString(value, Turkish) ?? "—"
    };
}
