using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

/// <summary>
/// Source-cell vectors transcribed from the latest uploaded Basic/Secondary XLSM:
/// 03-Temel Eğitim Taşıma Ödeme.xlsmdüzeltildi (1).xlsm
/// SHA-256 37a7818881aae52e8b2fcf95f30a75e1f302dfce1e91e2345f3473668a4d5296.
/// Cached values are asserted; this is not desktop Excel/VBA execution.
/// </summary>
public sealed class CurrentBasicWorkbookParityTests
{
    [Theory]
    [InlineData(16, 19539.1608, 7365.35, 319991.9228, 31999.19228, 15999.59614, 351991.11508, 441.921, 3033.523428144, 19475.040568144, 332516.074511856)]
    [InlineData(16, 6545, 7366.35, 112086.35, 11208.635, 5604.3175, 123294.985, 441.981, 1062.578598, 7108.877098, 116186.107902)]
    [InlineData(16, 6135, 7367.35, 105527.35, 10552.735, 5276.3675, 116080.085, 442.041, 1000.399278, 6718.807778, 109361.277222)]
    [InlineData(16, 4857.1601, 7368.35, 85082.9116, 8508.29116, 4254.14558, 93591.20276, 442.101, 806.586001968, 5502.832581968, 88088.370178032)]
    [InlineData(16, 2810, 7369.35, 52329.35, 5232.935, 2616.4675, 57562.285, 442.161, 496.082238, 3554.710738, 54007.574262)]
    public void LatestBasicRows4To8_ShouldMatchWorkbookCalculationCells(
        decimal workDays, decimal dailyPrice, decimal priceDifference,
        decimal sourceTransportNet, decimal sourceVat, decimal sourceWithholding,
        decimal sourceGross, decimal sourceGuarantee, decimal sourceStampTax,
        decimal sourceDeductions, decimal sourceNet)
    {
        var input = new ProgressPaymentInput(
            WorkDays: workDays, DailyPrice: dailyPrice, DiscountAmount: 0m,
            PriceDifference: priceDifference, ContractAmount: 100000m, IsVatExempt: false,
            TaxDebt: 0m, SocialSecurityDebt: 0m, MinimumWageSupport: 0m,
            InspectionPenalty: 0m, ProgramType: TransportationProgramType.BasicOrSecondary);
        var result = ProgressPaymentCalculator.Calculate(input);
        Assert.Equal(sourceTransportNet, result.TransportNetAmount);
        Assert.Equal(sourceVat, result.Vat);
        Assert.Equal(sourceWithholding, result.Withholding);
        Assert.Equal(sourceGross, result.GrossAmount);
        Assert.Equal(sourceGuarantee, result.AdditionalGuarantee);
        Assert.Equal(sourceStampTax, result.StampTax);
        Assert.Equal(sourceDeductions, result.TotalDeductions);
        Assert.Equal(sourceNet, result.NetPayable);
    }
}
