namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentAcceptancePeriod(
    DateOnly Start,
    DateOnly End);

public static class ProgressPaymentAcceptancePeriodService
{
    public static ProgressPaymentAcceptancePeriod BuildServicePeriod(DateOnly paymentPeriod) =>
        BuildServicePeriod(paymentPeriod, null, null);

    public static ProgressPaymentAcceptancePeriod BuildServicePeriod(
        DateOnly paymentPeriod,
        DateOnly? contractStartDate,
        DateOnly? contractEndDate)
    {
        var monthStart = new DateOnly(paymentPeriod.Year, paymentPeriod.Month, 1);
        var monthEnd = monthStart.AddMonths(1).AddDays(-1);

        var start = contractStartDate.HasValue && contractStartDate.Value > monthStart
            ? contractStartDate.Value
            : monthStart;
        var end = contractEndDate.HasValue && contractEndDate.Value < monthEnd
            ? contractEndDate.Value
            : monthEnd;

        if (end < start)
            throw new ArgumentOutOfRangeException(nameof(paymentPeriod), "Hakediş dönemi sözleşme tarih aralığıyla kesişmiyor.");

        return new ProgressPaymentAcceptancePeriod(start, end);
    }
}