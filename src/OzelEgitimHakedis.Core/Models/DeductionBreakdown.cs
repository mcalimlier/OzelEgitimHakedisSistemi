namespace OzelEgitimHakedis.Core.Models;

public sealed record DeductionBreakdown(
    decimal Withholding,
    decimal StampTax,
    decimal TaxDebt,
    decimal InspectionPenalty,
    decimal SocialSecurityDebt,
    decimal MinimumWageSupport,
    decimal AdditionalGuarantee)
{
    public decimal Total =>
        Withholding
        + StampTax
        + TaxDebt
        + InspectionPenalty
        + SocialSecurityDebt
        + MinimumWageSupport
        + AdditionalGuarantee;
}
