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
    /// True when the month falls within the default Special Education icmal
    /// lookup range. Use the program-specific overload for Basic/Secondary, whose
    /// visible C/D month headers include July even though its E21 lookup does not.
    /// </summary>
    public static bool IsMonthInWorkbookLookupRange(DateOnly month) =>
        IsMonthInWorkbookLookupRange(month, TransportationProgramType.SpecialEducation);

    /// <summary>
    /// Basic/Secondary uses a dynamically searched month header for the visible
    /// icmal rows, so July is a valid row there. Special Education's current C/D
    /// lookup range ends in June and treats July as a zero placeholder. This method
    /// describes visible icmal rows, not the narrower E21 prior-deduction lookup.
    /// </summary>
    public static bool IsMonthInWorkbookLookupRange(
        DateOnly month,
        TransportationProgramType programType)
    {
        if (!Enum.IsDefined(programType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(programType),
                programType,
                "Taşımalı eğitim program türü tanımlı bir değer olmalıdır.");
        }

        return programType == TransportationProgramType.BasicOrSecondary
            ? month.Month >= 9 || month.Month <= 7
            : month.Month >= 9 || month.Month <= 6;
    }

    /// <summary>
    /// Returns the prior calendar month's deduction from payment history.
    /// The source workbooks use different cells and monthly columns: Basic/Secondary
    /// uses Anasayfa!E4 with İcmal Verileri!AB:AL; Special Education uses E5 with AN:AX.
    /// Both source E21 formulas match the selected month against a September-June list.
    /// The visible icmal row cycle can include July for Basic, but that does not make July
    /// valid for the E21 previous-deduction lookup. Keep those two ranges distinct.
    /// </summary>
    public static decimal FindPreviousDeduction(
        IEnumerable<ProgressPaymentHistoryData> payments,
        ProgressPaymentHistoryData current)
    {
        ArgumentNullException.ThrowIfNull(payments);
        ArgumentNullException.ThrowIfNull(current);

        if (!current.PeriodDate.HasValue || !HasCompleteIdentity(current))
            return 0m;

        var sourcePreviousPeriod = ResolveSourcePreviousDeductionPeriod(
            current.PeriodDate.Value,
            current.Input.ProgramType);
        if (!sourcePreviousPeriod.HasValue)
            return 0m;

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
            .Where(x => x.PeriodDate!.Value.Year == sourcePreviousPeriod.Value.Year
                        && x.PeriodDate.Value.Month == sourcePreviousPeriod.Value.Month)
            .OrderByDescending(x => x.CreatedAt)
            .Select(x => x.Calculation.TotalDeductions)
            .FirstOrDefault();
    }

    // Reproduce the corrected Special Education source month map. The accepted
    // workbook correction sets İcmal Verileri!AP3 = Anasayfa!I8 (Kasım) and
    // AQ3 = Anasayfa!I9 (Aralık). December and January therefore follow the ordinary
    // previous-calendar-month mapping; the old duplicate-December workaround is obsolete.
    private static DateOnly? ResolveSourcePreviousDeductionPeriod(
        DateOnly currentPeriod,
        TransportationProgramType programType)
    {
        if (!Enum.IsDefined(programType))
        {
            throw new ArgumentOutOfRangeException(
                nameof(programType),
                programType,
                "Taşımalı eğitim program türü tanımlı bir değer olmalıdır.");
        }

        if (currentPeriod.Month is 7 or 8)
            return null;

        if (currentPeriod.Month == 9)
            return null;

        return currentPeriod.AddMonths(-1);
    }

    // Missing identity components must not collapse into an empty composite key:
    // otherwise unrelated incomplete records could inherit a previous deduction.
    private static bool HasCompleteIdentity(ProgressPaymentHistoryData payment) =>
        !string.IsNullOrWhiteSpace(payment.TenderNumber) &&
        !string.IsNullOrWhiteSpace(payment.ContractNumber) &&
        !string.IsNullOrWhiteSpace(payment.RouteNumber) &&
        !string.IsNullOrWhiteSpace(OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber));
}
