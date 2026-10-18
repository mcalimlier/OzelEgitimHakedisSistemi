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
    decimal ManualAdditionalGuarantee = 0m)
{
    public bool IncludeVat => !IsVatExempt;
}
