using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentReportTotals(
    decimal CumulativeServiceAmount,
    decimal CumulativePriceDifference,
    decimal CumulativeTotal,
    decimal PreviousTotal,
    decimal CurrentTotal)
{
    public decimal CurrentPriceDifference { get; init; }
}

public static class ProgressPaymentReportService
{
    public static IReadOnlyList<string> RequiredDocumentNames { get; } =
        new[]
        {
            "Hakediş Raporu",
            "Hizmet İşleri Hakediş Raporu",
            "Hizmet İşleri Kabul Tutanağı",
            "Hakediş İcmali"
        };

    public static IReadOnlyList<ProgressPaymentHistoryData> OrderByPaymentPeriod(
        IEnumerable<ProgressPaymentHistoryData> payments)
    {
        ArgumentNullException.ThrowIfNull(payments);

        return payments
            .OrderBy(x => x.PeriodDate ?? DateOnly.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .ToArray();
    }

    /// <summary>
    /// Calculates the base service after discounts for one route/month:
    /// G*H-J plus P*Q-S. The official report's D6 is not simply the sum of this
    /// value across months; it is cumulative K+T less the current month's raw I+R.
    /// See Calculate for the exact report-cell mapping.
    /// </summary>
    public static decimal CalculateWorkbookMonthlyPaymentAmount(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        var transportAmount = payment.Input.WorkDays * payment.Input.DailyPrice
                              - payment.Input.DiscountAmount;

        // Temel/Ortaöğretim Excel'inde rehber personel alanı bulunmaz.
        // Arşivde eski/elle değiştirilmiş rehber değerleri bulunsa bile rapor
        // geçmişine sızmamalıdır.
        if (payment.Input.ProgramType == TransportationProgramType.BasicOrSecondary)
            return transportAmount;

        return transportAmount
               + payment.Input.GuideWorkDays * payment.Input.GuideDailyPrice
               - payment.Input.GuideDiscountAmount;
    }

    /// <summary>
    /// Excel Hakediş Raporu!D7 uses only the current month's raw I+R inputs.
    /// Monthly history also stores these raw price differences separately from
    /// K+T totals; discounts J/S are not subtracted from I/R for this field.
    /// </summary>
    public static decimal CalculateWorkbookPriceDifference(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Input.ProgramType == TransportationProgramType.BasicOrSecondary
            ? payment.Input.PriceDifference
            : payment.Input.PriceDifference + payment.Input.GuidePriceDifference;
    }

    /// <summary>
    /// Mirrors the XLSM's monthly K+T value: contract-price work after discount
    /// plus the signed transport and guide price differences for that month.
    /// Used for completed prior periods, whose price differences are already part
    /// of the previous cumulative payment.
    /// </summary>
    public static decimal CalculateWorkbookMonthlyPaymentTotal(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return CalculateWorkbookMonthlyPaymentAmount(payment)
               + CalculateWorkbookPriceDifference(payment);
    }

    public static ProgressPaymentReportTotals Calculate(
        ProgressPaymentHistoryData current,
        IEnumerable<ProgressPaymentHistoryData> payments)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(payments);

        // The official workbook report is route-specific. Never let a different
        // route, contractor, contract, tender, or transport program contribute to
        // the selected route's cumulative cells—even when names/numbers coincide.
        var currentHasCompleteIdentity = HasCompleteIdentity(current);
        var currentRouteKey = OehsIdentityService.BuildRouteKey(
            current.TenderNumber ?? string.Empty,
            current.RouteNumber ?? string.Empty,
            current.ContractorTaxOrIdentityNumber ?? string.Empty,
            current.ContractNumber ?? string.Empty,
            current.Input.ProgramType);

        var previous = payments
            .Where(x => x.Id != current.Id)
            .Where(_ => currentHasCompleteIdentity)
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
            .Where(x => IsBeforePayment(x, current))
            .Where(x => IsWithinCurrentFiscalYear(x, current))
            .ToArray();

        // XLSM Hakediş Raporu formül zinciri:
        // D6 = toplam aylık K+T tutarı (önceki + mevcut) - mevcut ayın ham I+R fiyat farkı.
        // D7 = yalnızca mevcut ayın ham I+R fiyat farkı.
        // D8 = D6 + D7 = önceki toplamlar + mevcut ayın K+T tutarı.
        // D10 = önceki aylara ait toplam K+T.
        // D11 = IF(D7<0,H2-ABS(D7),H2+D7); iki dal da H2+D7 sonucunu verir.
        // D11 bu nedenle H2'ye eşitlenmez; ham fiyat farkı D7 rapor zincirine ayrıca girer.
        // Gelecek aylardaki olası önceden girilmiş değerleri rapora katmamak için
        // yalnızca seçili aya kadar olan uygulama geçmişini topluyoruz.
        var currentMonthlyTotal = CalculateWorkbookMonthlyPaymentTotal(current);
        var previousTotal = previous.Sum(CalculateWorkbookMonthlyPaymentTotal);
        var currentPriceDifference = CalculateWorkbookPriceDifference(current);
        var cumulativeTotal = previousTotal + currentMonthlyTotal;
        var cumulativeServiceAmount = cumulativeTotal - currentPriceDifference;
        var currentTotal = currentMonthlyTotal;

        return new ProgressPaymentReportTotals(
            cumulativeServiceAmount,
            currentPriceDifference,
            cumulativeTotal,
            previousTotal,
            currentTotal)
        {
            CurrentPriceDifference = currentPriceDifference
        };
    }

    /// <summary>
    /// Maps the official report's D28 to the selected program's calculation-sheet
    /// net result, per the accepted source mapping: Basic W4 and Special Education AG4.
    /// Keep this mapping independent from cumulative report subtotals.
    /// </summary>
    public static decimal CalculateReportStampTax(
        ProgressPaymentCalculation calculation,
        TransportationProgramType programType,
        decimal? rawTransportPriceDifference = null)
    {
        ArgumentNullException.ThrowIfNull(calculation);

        // Temel Eğitim Hakediş Raporu D15 = IF(D11<0,0,D11*9.48/1000).
        // D11 is the current calculation-sheet K value. Do not add raw I again:
        // K already includes I. This is distinct from the Special Education
        // calculation-sheet stamp-tax formula, which is returned unchanged.
        if (programType != TransportationProgramType.BasicOrSecondary)
            return calculation.StampTax;

        var reportBase = calculation.TransportNetAmount;
        var stampTaxRate = calculation.AppliedConstants?.StampTaxRate
                           ?? ProgressPaymentCalculator.StampTaxRate;
        return reportBase < 0m ? 0m : reportBase * stampTaxRate;
    }

    public static decimal CalculateReportTotalDeductions(
        ProgressPaymentCalculation calculation,
        TransportationProgramType programType,
        decimal? rawTransportPriceDifference = null)
    {
        ArgumentNullException.ThrowIfNull(calculation);

        return calculation.TotalDeductions
               - calculation.StampTax
               + CalculateReportStampTax(calculation, programType, rawTransportPriceDifference);
    }

    public static decimal CalculateReportNetPayable(
        ProgressPaymentHistoryData current,
        ProgressPaymentReportTotals reportTotals)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(reportTotals);

        // Accepted user-approved source mapping:
        // Basic: Hakediş Raporu!D28 = Hesaplama Bilgileri!W4.
        // Special: Hakediş Raporu!D28 = Hesaplama!AG4.
        // Calculation.NetPayable represents W4 (O4-V4) or AG4 (AF4-AE4).
        // Do not recalculate D28 from separately rounded report subtotals.
        _ = reportTotals;
        return current.Calculation.NetPayable;
    }

    private static bool IsWithinCurrentFiscalYear(
        ProgressPaymentHistoryData candidate,
        ProgressPaymentHistoryData current)
    {
        // The source workbook's monthly lookup spans the current September-July
        // cycle. Records from a prior fiscal year must not leak into D6 or D10.
        // Workbook monthly cells are keyed by the fiscal period. A legacy
        // record without a period date cannot be matched to those cells safely.
        if (!candidate.PeriodDate.HasValue || !current.PeriodDate.HasValue)
            return false;

        static int FiscalStartYear(DateOnly date) =>
            date.Month >= 9 ? date.Year : date.Year - 1;

        return FiscalStartYear(candidate.PeriodDate.Value)
               == FiscalStartYear(current.PeriodDate.Value);
    }

    private static bool HasCompleteIdentity(ProgressPaymentHistoryData payment) =>
        !string.IsNullOrWhiteSpace(payment.TenderNumber) &&
        !string.IsNullOrWhiteSpace(payment.ContractNumber) &&
        !string.IsNullOrWhiteSpace(payment.RouteNumber) &&
        !string.IsNullOrWhiteSpace(OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber));

    private static bool IsBeforePayment(
        ProgressPaymentHistoryData candidate,
        ProgressPaymentHistoryData current)
    {
        if (candidate.PeriodDate.HasValue && current.PeriodDate.HasValue)
            return candidate.PeriodDate.Value < current.PeriodDate.Value;

        if (candidate.PeriodDate.HasValue && !current.PeriodDate.HasValue)
            return true;

        if (!candidate.PeriodDate.HasValue && current.PeriodDate.HasValue)
            return false;

        return candidate.CreatedAt < current.CreatedAt;
    }
}
