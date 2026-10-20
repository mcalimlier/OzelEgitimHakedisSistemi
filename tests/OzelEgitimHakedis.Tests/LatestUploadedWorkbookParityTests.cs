using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Cached formula-result vectors from the latest uploaded XLSM files.
/// These vectors do not claim desktop Excel/VBA execution.
/// </summary>
public sealed class LatestUploadedWorkbookParityTests
{
    public static IEnumerable<object[]> BasicRows => new[]
    {
        new object[] { 5, 6545m, 7366.35m, 112086.35m, 11208.635m, 5604.3175m, 117690.6675m, 123294.985m, 441.981m, 1062.578598m, 7108.877098m, 116186.107902m },
        new object[] { 6, 6135m, 7367.35m, 105527.35m, 10552.735m, 5276.3675m, 110803.7175m, 116080.085m, 442.041m, 1000.399278m, 6718.807778m, 109361.277222m },
        new object[] { 7, 4857.1601m, 7368.35m, 85082.9116m, 8508.29116m, 4254.14558m, 89337.05718m, 93591.20276m, 442.101m, 806.586001968m, 5502.832581968m, 88088.370178032m },
        new object[] { 8, 2810m, 7369.35m, 52329.35m, 5232.935m, 2616.4675m, 54945.8175m, 57562.285m, 442.161m, 496.082238m, 3554.710738m, 54007.574262m }
    };

    public static IEnumerable<object[]> SpecialRows => new[]
    {
        new object[] { 5, 1894m, 5250m, 1200m, 23m, 1362.49m, 3300m, 680m, 1500m, 3000m, 13100m, 1454.65m, 34354m, 33957.27m, 513m, 647.5908396m, 28045.2494396m, 78538.124m, 50492.8745604m },
        new object[] { 6, 2184m, 3980m, 1300m, 23m, 1362.49m, 5700m, 930m, 5500m, 3500m, 14850m, 1819.72m, 37624m, 36107.27m, 580.8m, 698.9724396m, 35330.0010396m, 84715.124m, 49385.1229604m }
    };

    [Theory]
    [MemberData(nameof(BasicRows))]
    public void LatestBasicWorkbookRows5To8_ShouldMatchCachedFormulaChain(
        int row, decimal dailyPrice, decimal priceDifference, decimal transportNet,
        decimal vat, decimal withholding, decimal invoiceAfterWithholding, decimal gross,
        decimal additionalGuarantee, decimal stampTax, decimal totalDeductions, decimal netPayable)
    {
        var input = new ProgressPaymentInput(
            WorkDays: 16m, DailyPrice: dailyPrice, DiscountAmount: 0m,
            PriceDifference: priceDifference, ContractAmount: 100000m, IsVatExempt: false,
            TaxDebt: 0m, SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            InspectionPenalty: 0m, ProgramType: TransportationProgramType.BasicOrSecondary);
        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(transportNet, result.TransportNetAmount);
        Assert.Equal(vat, result.TransportVat);
        Assert.Equal(withholding, result.TransportWithholding);
        Assert.Equal(invoiceAfterWithholding, result.InvoiceAmountAfterWithholding);
        Assert.Equal(gross, result.GrossAmount);
        Assert.Equal(additionalGuarantee, result.AdditionalGuarantee);
        Assert.Equal(stampTax, result.StampTax);
        Assert.Equal(totalDeductions, result.TotalDeductions);
        Assert.Equal(netPayable, result.NetPayable);
    }

    [Theory]
    [MemberData(nameof(SpecialRows))]
    public void LatestSpecialEducationWorkbookRows5And6_ShouldMatchCachedFormulaChain(
        int row, decimal transportDailyPrice, decimal transportPriceDifference, decimal transportDiscount,
        decimal guideDays, decimal guideDailyPrice, decimal guidePriceDifference, decimal guideDiscount,
        decimal taxDebt, decimal socialSecurityDebt, decimal minimumWageSupport, decimal inspectionPenalty,
        decimal transportNet, decimal guideNet, decimal additionalGuarantee, decimal stampTax,
        decimal totalDeductions, decimal gross, decimal netPayable)
    {
        var input = new ProgressPaymentInput(
            WorkDays: 16m, DailyPrice: transportDailyPrice, DiscountAmount: transportDiscount,
            PriceDifference: transportPriceDifference, ContractAmount: 100000m, IsVatExempt: false,
            TaxDebt: taxDebt, SocialSecurityDebt: socialSecurityDebt, MinimumWageSupport: minimumWageSupport,
            InspectionPenalty: inspectionPenalty, GuideWorkDays: guideDays, GuideDailyPrice: guideDailyPrice,
            GuideDiscountAmount: guideDiscount, GuidePriceDifference: guidePriceDifference,
            ProgramType: TransportationProgramType.SpecialEducation);
        var result = ProgressPaymentCalculator.Calculate(input);

        Assert.Equal(transportNet, result.TransportNetAmount);
        Assert.Equal(guideNet, result.GuideNetAmount);
        Assert.Equal(transportNet * 0.1m, result.TransportVat);
        Assert.Equal(transportNet * 0.05m, result.TransportWithholding);
        Assert.Equal(guideNet * 0.2m, result.GuideVat);
        Assert.Equal(guideNet * 0.18m, result.GuideWithholding);
        Assert.Equal(additionalGuarantee, result.AdditionalGuarantee);
        Assert.Equal(stampTax, result.StampTax);
        Assert.Equal(totalDeductions, result.TotalDeductions);
        Assert.Equal(gross, result.GrossAmount);
        Assert.Equal(netPayable, result.NetPayable);
    }
}
