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
        IsMonthInWorkbookLookupRange(month, TransportationProgramType.SpecialEducation);

    /// <summary>
    /// Basic/Secondary uses a dynamically searched month header in its icmal
    /// transfer macros, so July is a valid period there. Special Education's
    /// current C/D lookup range ends in June and treats July as a zero placeholder.
    /// </summary>
    public static bool IsMonthInWorkbookLookupRange(
        DateOnly month,
        TransportationProgramType programType) =>
        programType == TransportationProgramType.BasicOrSecondary
            ? month.Month >= 9 || month.Month <= 7
            : month.Month >= 9 || month.Month <= 6;

    /// <summary>
    /// Returns the prior calendar month's deduction from payment dates.
    /// The source workbook's AN3:AX3 header range currently omits November and
    /// duplicates December; using dates avoids losing a valid November carry-forward.
    /// </summary>
    public static decimal FindPreviousDeduction(
        IEnumerable<ProgressPaymentHistoryData> payments,
        ProgressPaymentHistoryData current)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(current);

        if (!current.PeriodDate.HasValue ||
            !IsMonthInWorkbookLookupRange(current.PeriodDate.Value, current.Input.ProgramType))
        {
            return 0m;
        }

        if (current.PeriodDate.Value.Month == 9)
            return 0m;

        var previousMonth = current.PeriodDate.Value.AddMonths(-1);

        return payments
            .Where(x => x.Id != current.Id)
            .Where(x => x.Input.ProgramType == current.Input.ProgramType)
            .Where(x => string.Equals(x.TenderNumber, current.TenderNumber, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(x.ContractNumber, current.ContractNumber, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(x.RouteNumber, current.RouteNumber, StringComparison.OrdinalIgnoreCase))
            .Where(x => string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                OehsIdentityService.NormalizeIdentity(current.ContractorTaxOrIdentityNumber),
                StringComparison.Ordinal))
            .Where(x => x.PeriodDate.HasValue)
            .Where(x => x.PeriodDate!.Value.Year == previousMonth.Year
                        && x.PeriodDate.Value.Month == previousMonth.Month)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Calculation.TotalDeductions)
            .FirstOrDefault();
    }
}
