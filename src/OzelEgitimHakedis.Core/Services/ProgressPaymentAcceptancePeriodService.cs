namespace OzelEgitimHakedis.Core.Services;

public sealed record ProgressPaymentAcceptancePeriod(
    DateOnly Start,
    DateOnly End);

public static class ProgressPaymentAcceptancePeriodService
{
    public static ProgressPaymentAcceptancePeriod BuildServicePeriod(DateOnly paymentPeriod)
    {
        var start = new DateOnly(paymentPeriod.Year, paymentPeriod.Month, 1);
        return new ProgressPaymentAcceptancePeriod(
            start,
            start.AddMonths(1).AddDays(-1));
    }
}
