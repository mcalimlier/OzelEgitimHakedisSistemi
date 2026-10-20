using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Boundary regression tests for input guards shared by both program-specific
/// calculation paths. These tests do not change official workbook formulas.
/// </summary>
public sealed class ProgressPaymentInputBoundaryTests
{
    public static TheoryData<ProgressPaymentInput> InvalidInputs()
    {
        var valid = new ProgressPaymentInput(
            WorkDays: 0m,
            DailyPrice: 0m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 0m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 0m,
            GuideDailyPrice: 0m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);

        return new TheoryData<ProgressPaymentInput>
        {
            valid with { WorkDays = -1m },
            valid with { GuideWorkDays = -1m },
            valid with { DailyPrice = -0.01m },
            valid with { GuideDailyPrice = -0.01m },
            valid with { ContractAmount = -0.01m },
            valid with { TaxDebt = -0.01m },
            valid with { SocialSecurityDebt = -0.01m },
            valid with { MinimumWageSupport = -0.01m },
            valid with { InspectionPenalty = -0.01m }
        };
    }

    [Theory]
    [MemberData(nameof(InvalidInputs))]
    public void Calculate_ShouldRejectNegativeValuesForFieldsWithNonNegativeContract(
        ProgressPaymentInput input)
    {
        Assert.Throws<ArgumentOutOfRangeException>(
            () => ProgressPaymentCalculator.Calculate(input));
    }

    public static TheoryData<TransportationProgramType> ProgramTypes() => new()
    {
        TransportationProgramType.BasicOrSecondary,
        TransportationProgramType.SpecialEducation
    };

    [Theory]
    [MemberData(nameof(ProgramTypes))]
    public void ZeroAmountsAndZeroWorkDays_ShouldRemainAValidBoundary(
        TransportationProgramType programType)
    {
        var input = new ProgressPaymentInput(
            WorkDays: 0m,
            DailyPrice: 0m,
            DiscountAmount: 0m,
            PriceDifference: 0m,
            ContractAmount: 0m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 0m,
            GuideDailyPrice: 0m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m,
            ProgramType: programType);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(0m, result.TransportNetAmount);
        Assert.Equal(0m, result.GuideNetAmount);
        Assert.Equal(0m, result.TotalDeductions);
        Assert.Equal(0m, result.NetPayable);
    }

    [Theory]
    [MemberData(nameof(ProgramTypes))]
    public void NegativePriceDifference_ShouldRemainSignedRatherThanBeClamped(
        TransportationProgramType programType)
    {
        var input = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 100m,
            DiscountAmount: 0m,
            PriceDifference: -10m,
            ContractAmount: 1000m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            ProgramType: programType);

        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(90m, result.TransportNetAmount);
        Assert.Equal(-0.6m, result.AdditionalGuarantee);
    }
}
