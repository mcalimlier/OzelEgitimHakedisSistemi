namespace OzelEgitimHakedis.Core.Models;

public sealed record ProgressPaymentInput(
    int WorkDays,
    decimal DailyPrice,
    decimal DiscountAmount,
    decimal PriceDifference,
    decimal ContractAmount,
    bool IsVatExempt,
    decimal TaxDebt,
    decimal SocialSecurityDebt,
    decimal MinimumWageSupport,
    decimal InspectionPenalty = 0m,
    int GuideWorkDays = 0,
    decimal GuideDailyPrice = 0m,
    decimal GuideDiscountAmount = 0m,
    decimal GuidePriceDifference = 0m)
{
    public bool IncludeVat => !IsVatExempt;
}
