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

    // Reproduce the source E21 lookup chain, not the broader visible icmal row cycle.
    // Both current-month lists contain September-June. Basic's AB:AL header map has
    // September-July, so its previous-month lookup works for the supported current months.
    // Special's AN:AX header row is defective in the latest XLSM: November is missing
    // and December appears twice (AP and AQ). Consequently, December's previous-month
    // lookup returns zero; January's MATCH finds the first December (AP), whose data
    // column is mapped to November by the source transfer macro. Preserve this source
    // behavior explicitly until the workbook itself is corrected and re-accepted.
    private static DateOnly? ResolveSourcePreviousDeductionPeriod(
        DateOnly currentPeriod,
        TransportationProgramType programType)
    {
        if (currentPeriod.Month < 9 && currentPeriod.Month > 6)
            return null;

        if (currentPeriod.Month == 9)
            return null;

        if (programType == TransportationProgramType.SpecialEducation)
        {
            if (currentPeriod.Month == 12)
                return null; // November header is missing from AN:AX.
            if (currentPeriod.Month == 1)
                return currentPeriod.AddMonths(-2); // first "December" header is AP, mapped to November data.
        }

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
