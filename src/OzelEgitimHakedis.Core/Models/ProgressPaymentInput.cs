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
    decimal InspectionPenaltyRatePerThousand = 2m);
