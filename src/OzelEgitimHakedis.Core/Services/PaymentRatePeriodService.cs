using System.Globalization;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class PaymentRatePeriodService
{
    public static decimal ServiceAmount(IReadOnlyList<PaymentRatePeriod>? periods, decimal days, decimal price) =>
        periods is null ? days * price : periods.Sum(p => p.WorkDays * p.DailyPrice);

    public static string Formula(IReadOnlyList<PaymentRatePeriod>? periods, decimal days, decimal price)
    {
        var culture = CultureInfo.GetCultureInfo("tr-TR");
        string N(decimal value) => value.ToString("#,##0.############################", culture);
        return periods is null ? $"{N(days)} × {N(price)}" :
            string.Join(" + ", periods.Select(p => $"{N(p.WorkDays)} × {N(p.DailyPrice)}"));
    }

    public static void Validate(ProgressPaymentInput input)
    {
        ValidatePart(input.TransportPeriods, input.WorkDays, input.DailyPrice, input.PriceDifference, "Taşıma");
        if (input.ProgramType == TransportationProgramType.SpecialEducation)
            ValidatePart(input.GuidePeriods, input.GuideWorkDays, input.GuideDailyPrice, input.GuidePriceDifference, "Rehber");
    }

    private static void ValidatePart(IReadOnlyList<PaymentRatePeriod>? periods, decimal days, decimal price, decimal difference, string title)
    {
        if (periods is null) return;
        if (periods.Count == 0 || periods.Any(p => p is null || p.WorkDays < 0 || p.DailyPrice < 0))
            throw new ArgumentException($"{title}: değişiklik bölümlerinde gün ve günlük fiyat sıfır veya daha büyük olmalıdır.");
        if (periods.Sum(p => p.WorkDays) != days || periods[^1].DailyPrice != price || periods.Sum(p => p.PriceDifference) != difference)
            throw new ArgumentException($"{title}: bölüm toplamları ile gün, son günlük fiyat ve fiyat farkı uyumlu değil. Dönem içi değişiklik penceresini kullanın.");
    }

    public static ProgressPaymentInput Apply(ProgressPaymentInput input, IReadOnlyList<PaymentRatePeriod> transport,
        IReadOnlyList<PaymentRatePeriod>? guide)
    {
        ArgumentNullException.ThrowIfNull(input);
        ArgumentNullException.ThrowIfNull(transport);
        if (transport.Count == 0 || guide is { Count: 0 }) throw new ArgumentException("En az bir hesap bölümü gerekir.");
        if (transport.Concat(guide ?? Array.Empty<PaymentRatePeriod>()).Any(p => p is null || p.WorkDays < 0 || p.DailyPrice < 0))
            throw new ArgumentException("Bölüm günleri ve günlük fiyatlar negatif olamaz.");
        var result = input with
        {
            WorkDays = transport.Sum(p => p.WorkDays), DailyPrice = transport[^1].DailyPrice,
            PriceDifference = transport.Sum(p => p.PriceDifference), TransportPeriods = transport.Count > 1 ? transport.ToArray() : null,
            HasPeriodChanges = true
        };
        if (input.ProgramType == TransportationProgramType.SpecialEducation && guide is not null)
            result = result with { GuideWorkDays = guide.Sum(p => p.WorkDays), GuideDailyPrice = guide[^1].DailyPrice,
                GuidePriceDifference = guide.Sum(p => p.PriceDifference), GuidePeriods = guide.Count > 1 ? guide.ToArray() : null };
        if (input.ProgramType == TransportationProgramType.BasicOrSecondary)
            result = result with { GuideWorkDays = 0, GuideDailyPrice = 0, GuidePriceDifference = 0, GuidePeriods = null };
        Validate(result);
        return result;
    }
}
