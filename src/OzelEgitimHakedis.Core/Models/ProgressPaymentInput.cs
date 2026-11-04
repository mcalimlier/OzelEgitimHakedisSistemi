namespace OzelEgitimHakedis.Core.Models;

public sealed record ProgressPaymentInput(
    decimal WorkDays,
    decimal DailyPrice,
    decimal DiscountAmount,
    decimal PriceDifference,
    decimal ContractAmount,
    bool IsVatExempt,
    decimal TaxDebt,
    decimal SocialSecurityDebt,
    decimal MinimumWageSupport,
    decimal InspectionPenalty = 0m,
    decimal GuideWorkDays = 0m,
    decimal GuideDailyPrice = 0m,
    decimal GuideDiscountAmount = 0m,
    decimal GuidePriceDifference = 0m,
    TransportationProgramType ProgramType = TransportationProgramType.SpecialEducation,
    IReadOnlyList<PaymentRatePeriod>? TransportPeriods = null,
    IReadOnlyList<PaymentRatePeriod>? GuidePeriods = null,
    bool HasPeriodChanges = false)
{
    public bool IncludeVat => !IsVatExempt;
}

public sealed record PaymentRatePeriod(decimal WorkDays, decimal DailyPrice, decimal PriceDifference = 0m, string Description = "");
