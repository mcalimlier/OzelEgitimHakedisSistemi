using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentReportTotals(
    decimal CumulativeServiceAmount,
    decimal CurrentPriceDifference,
    decimal CumulativeTotal,
    decimal PreviousTotal,
    decimal CurrentTotal);

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
    /// Matches Module1.HakedişToplamlariniAktar: the workbook writes Hesaplama!K +
    /// Hesaplama!T directly into the selected month's D:N cells. K and T already
    /// apply the I/R price-difference inputs and J/S discounts; don't subtract I/R
    /// again when aggregating the monthly values in Hakediş Raporu!D6.
    /// </summary>
    public static decimal CalculateWorkbookMonthlyPaymentAmount(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Calculation.TotalAmount;
    }

    /// <summary>
    /// Excel's Hakediş Raporu!D7 and the İcmal Verileri price-difference columns
    /// use the raw I+R source inputs. They do not use I-J or R-S.
    /// </summary>
    public static decimal CalculateWorkbookPriceDifference(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);
        return payment.Input.PriceDifference + payment.Input.GuidePriceDifference;
    }

    public static ProgressPaymentReportTotals Calculate(
        ProgressPaymentHistoryData current,
        IEnumerable<ProgressPaymentHistoryData> payments)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(payments);

        var previous = payments
            .Where(x => x.Id != current.Id)
            .Where(x => IsBeforePayment(x, current))
            .Where(x => IsWithinCurrentFiscalYear(x, current))
            .ToArray();

        // Hakediş Raporu!D6: cumulative monthly service amounts, excluding the
        // separately stored raw price-difference cells.
        var previousServiceAmount = previous.Sum(CalculateWorkbookMonthlyPaymentAmount);
        var currentServiceAmount = CalculateWorkbookMonthlyPaymentAmount(current);
        var currentPriceDifference = CalculateWorkbookPriceDifference(current);
        var cumulativeServiceAmount = previousServiceAmount + currentServiceAmount;

        // Hakediş Raporu!D8 = D6 + D7.
        var cumulativeTotal = cumulativeServiceAmount + currentPriceDifference;

        // Hakediş Raporu!D10 reads the previous cumulative service-only value
        // from İcmal Verileri!O. Price differences are kept in their own columns.
        var previousTotal = previousServiceAmount;

        // The workbook's "Hesaplamayı Kontrol Et" macro is the authoritative
        // validation rule: D11 must equal D8 - D10. The visible E-row label also
        // states C - D. Do not add the price difference to Calculation.TotalAmount
        // again; it already includes it for the current period.
        var currentTotal = cumulativeTotal - previousTotal;

        return new ProgressPaymentReportTotals(
            cumulativeServiceAmount,
            currentPriceDifference,
            cumulativeTotal,
            previousTotal,
            currentTotal);
    }

    /// <summary>
    /// Excel Hakediş Raporu!D28 = ROUND(D13,2)-ROUND(D26,2).
    /// This report value is not interchangeable with Hesaplama!AG.
    /// </summary>
    public static decimal CalculateReportNetPayable(
        ProgressPaymentHistoryData current,
        ProgressPaymentReportTotals reportTotals)
    {
        ArgumentNullException.ThrowIfNull(current);
        ArgumentNullException.ThrowIfNull(reportTotals);

        var reportGross = reportTotals.CurrentTotal + current.Calculation.Vat;
        return decimal.Round(reportGross, 2, MidpointRounding.AwayFromZero)
               - decimal.Round(current.Calculation.TotalDeductions, 2, MidpointRounding.AwayFromZero);
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
