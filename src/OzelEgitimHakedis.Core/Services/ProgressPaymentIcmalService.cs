using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public static class ProgressPaymentIcmalService
{
    /// <summary>
    /// The output form displays 11 rows (September through July). The source
    /// workbook's live month lookup, however, contains only September-June
    /// (Anasayfa!I6:I15); its July row resolves through IFERROR(...,0).
    /// Preserve the printed placeholder while matching the workbook's value behavior.
    /// </summary>
    public static IReadOnlyList<DateOnly> BuildPeriods(DateOnly periodDate)
    {
        var baseYear = periodDate.Month >= 9 ? periodDate.Year : periodDate.Year - 1;
        return Enumerable.Range(0, 11)
            .Select(index => new DateOnly(baseYear, 9, 1).AddMonths(index))
            .ToArray();
    }

    /// <summary>
    /// True when the month can be matched in the source workbook's Anasayfa!I6:I15
    /// lookup range. July remains on the printed form but returns zero in the
    /// source workbook's current C/D month formulas.
    /// </summary>
    public static bool IsMonthInWorkbookLookupRange(DateOnly month) =>
        month.Month >= 9 || month.Month <= 6;

    /// <summary>
    /// Mirrors the source formula in Hakediş İcmali!E21:
    /// current month is matched in Anasayfa!I6:I15; the previous month's name
    /// is then matched in İcmal Verileri!AN3:AX3. That header range currently
    /// omits November and contains December twice, so a December payment's
    /// previous-November lookup falls through IFERROR to zero. Do not silently
    /// return a different amount until the reference workbook is corrected.
    /// </summary>
    public static decimal FindPreviousDeduction(
        IEnumerable<ProgressPaymentHistoryData> payments,
        ProgressPaymentHistoryData current)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(current);

        if (!current.PeriodDate.HasValue ||
            !IsMonthInWorkbookLookupRange(current.PeriodDate.Value))
        {
            return 0m;
        }

        if (current.PeriodDate.Value.Month == 9)
            return 0m;

        var previousMonth = current.PeriodDate.Value.AddMonths(-1);

        // Source workbook header defect: "Kasım" is missing from AN3:AX3,
        // so MATCH("Kasım", ...) errors and outer IFERROR(...,0) returns zero.
        if (previousMonth.Month == 11)
            return 0m;

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
