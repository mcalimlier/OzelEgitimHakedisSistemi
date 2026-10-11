using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class ProgressPaymentIcmalService
{
    /// <summary>
    /// The reference workbook has 11 printed periods: September through June
    /// in Anasayfa!I6:I15, plus the separate July column in İcmal Verileri.
    /// </summary>
    public static IReadOnlyList<DateOnly> BuildPeriods(DateOnly periodDate)
    {
        var baseYear = periodDate.Month >= 9 ? periodDate.Year : periodDate.Year - 1;
        return Enumerable.Range(0, 11)
            .Select(index => new DateOnly(baseYear, 9, 1).AddMonths(index))
            .ToArray();
    }

    /// <summary>
    /// Excel Hakediş İcmali!E21 uses the immediately preceding calendar month's
    /// deduction cell. If that month has no payment, its value is zero; do not
    /// substitute an older payment from a different month.
    /// </summary>
    public static decimal FindPreviousDeduction(
        IEnumerable<ProgressPaymentHistoryData> payments,
        ProgressPaymentHistoryData current)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(current);

        if (!current.PeriodDate.HasValue)
            return 0m;

        var previousMonth = current.PeriodDate.Value.AddMonths(-1);
        return payments
            .Where(x => x.Id != current.Id)
            .Where(x => x.PeriodDate.HasValue)
            .Where(x => x.PeriodDate!.Value.Year == previousMonth.Year
                        && x.PeriodDate.Value.Month == previousMonth.Month)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Calculation.TotalDeductions)
            .FirstOrDefault();
    }
}
