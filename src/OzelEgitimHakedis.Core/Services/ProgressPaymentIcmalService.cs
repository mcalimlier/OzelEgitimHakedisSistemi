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
    /// The source workbooks use different period/range conventions: Basic/Secondary
    /// uses Anasayfa!E4 and monthly deductions in İcmal Verileri!AB:AL (confirmed by
    /// KesintiToplamlariniAktar), while Special Education uses E5 and AN:AX.
    /// OEHS resolves the equivalent prior-month row from history, scoped by program,
    /// tender, contract, route, and contractor instead of reusing either workbook's bad lookup.
    /// </summary>
    public static decimal FindPreviousDeduction(
        IEnumerable<ProgressPaymentHistoryData> payments,
        ProgressPaymentHistoryData current)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(current);

        if (!current.PeriodDate.HasValue ||
            !IsMonthInWorkbookLookupRange(current.PeriodDate.Value, current.Input.ProgramType) ||
            !HasCompleteIdentity(current))
        {
            return 0m;
        }

        if (current.PeriodDate.Value.Month == 9)
            return 0m;

        var previousMonth = current.PeriodDate.Value.AddMonths(-1);

        var currentRouteKey = OehsIdentityService.BuildRouteKey(
            current.TenderNumber ?? string.Empty,
            current.RouteNumber ?? string.Empty,
            current.ContractorTaxOrIdentityNumber ?? string.Empty,
            current.ContractNumber ?? string.Empty,
            current.Input.ProgramType);

        return payments
            .Where(x => x.Id != current.Id)
            .Where(HasCompleteIdentity)
            .Where(x => string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber ?? string.Empty,
                    x.RouteNumber ?? string.Empty,
                    x.ContractorTaxOrIdentityNumber ?? string.Empty,
                    x.ContractNumber ?? string.Empty,
                    x.Input.ProgramType),
                currentRouteKey,
                StringComparison.Ordinal))
            .Where(x => x.PeriodDate.HasValue)
            .Where(x => x.PeriodDate!.Value.Year == previousMonth.Year
                        && x.PeriodDate.Value.Month == previousMonth.Month)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Calculation.TotalDeductions)
            .FirstOrDefault();
    }

    // Missing identity components must not collapse into an empty composite key:
    // otherwise unrelated incomplete records could inherit a previous deduction.
    private static bool HasCompleteIdentity(ProgressPaymentHistoryData payment) =>
        !string.IsNullOrWhiteSpace(payment.TenderNumber) &&
        !string.IsNullOrWhiteSpace(payment.ContractNumber) &&
        !string.IsNullOrWhiteSpace(payment.RouteNumber) &&
        !string.IsNullOrWhiteSpace(OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber));
}
