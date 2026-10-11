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
    /// Maps a saved payment to the Excel workbook's monthly service-amount area.
    /// The workbook keeps the price-difference cells separately, so subtract the
    /// raw I/R inputs from K+T. This also preserves J/S discounts in the service base.
    /// </summary>
    public static decimal CalculateWorkbookServiceAmount(ProgressPaymentHistoryData payment)
    {
        ArgumentNullException.ThrowIfNull(payment);

        return payment.Calculation.TotalAmount
               - payment.Input.PriceDifference
               - payment.Input.GuidePriceDifference;
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
            .ToArray();

        // Hakediş Raporu!D6: cumulative monthly service amounts, excluding the
        // separately stored raw price-difference cells.
        var previousServiceAmount = previous.Sum(CalculateWorkbookServiceAmount);
        var currentServiceAmount = CalculateWorkbookServiceAmount(current);
        var currentPriceDifference = CalculateWorkbookPriceDifference(current);
        var cumulativeServiceAmount = previousServiceAmount + currentServiceAmount;

        // Hakediş Raporu!D8 = D6 + D7.
        var cumulativeTotal = cumulativeServiceAmount + currentPriceDifference;

        // Hakediş Raporu!D10 reads the previous cumulative service-only value
        // from İcmal Verileri!O. Price differences are kept in their own columns.
        var previousTotal = previousServiceAmount;

        // Excel formula in Hakediş Raporu!D11, despite the row label saying C-D:
        // H2 = K + T; D7 = raw I + R; D11 = IF(D7<0,H2-ABS(D7),H2+D7).
        // Keep the source formula exactly; do not silently replace it with C-D.
        var currentTotal = currentPriceDifference < 0m
            ? current.Calculation.TotalAmount - Math.Abs(currentPriceDifference)
            : current.Calculation.TotalAmount + currentPriceDifference;

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
