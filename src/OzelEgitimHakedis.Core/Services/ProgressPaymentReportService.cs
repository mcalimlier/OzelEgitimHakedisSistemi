using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentReportTotals(
    decimal CumulativeServiceAmount,
    decimal CumulativePriceDifference,
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
    /// Calculates the contract-price work amount for the report's A line.
    /// The source workbook's K/T cells include I/R price-difference inputs,
    /// while the report adds raw I+R separately in B. Therefore this monthly
    /// amount must subtract I/R from K+T (equivalently: work-day amounts less J/S
    /// discounts) to prevent the current month's price difference being counted twice.
    /// </summary>
    public static decimal CalculateWorkbookMonthlyPaymentAmount(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Input.WorkDays * payment.Input.DailyPrice
               - payment.Input.DiscountAmount
               + payment.Input.GuideWorkDays * payment.Input.GuideDailyPrice
               - payment.Input.GuideDiscountAmount;
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

        var previous = payments
            .Where(x => x.Id != current.Id)
            .Where(x => IsBeforePayment(x, current))
            .Where(x => IsWithinCurrentFiscalYear(x, current))
            .ToArray();

        // XLSM Hakediş Raporu formül zinciri:
        // D6 = toplam aylık K+T tutarı (önceki + mevcut) - mevcut ayın ham I+R fiyat farkı.
        // D7 = yalnızca mevcut ayın ham I+R fiyat farkı.
        // D8 = D6 + D7 = önceki toplamlar + mevcut ayın K+T tutarı.
        // D10 = önceki aylara ait toplam K+T.
        // D11 = IF(D7<0,H2-ABS(D7),H2+D7)-D7; her iki dal da cebirsel olarak H2'ye,
        // yani mevcut satırın K+T tutarına sadeleşir.
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
            cumulativePriceDifference,
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
