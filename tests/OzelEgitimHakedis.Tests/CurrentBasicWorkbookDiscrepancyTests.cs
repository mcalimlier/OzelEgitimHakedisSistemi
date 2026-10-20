using System.Collections.Generic;
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
    public static IEnumerable<object[]> LatestBasicRows =>
        new[]
        {
            new object[] { 16m, 19539.1608m, 7365.35m, 319991.9228m, 31999.19228m, 15999.59614m, 351991.11508m, 441.921m, 3033.523428144m, 19475.040568144m, 332516.074511856m },
            new object[] { 16m, 6545m, 7366.35m, 112086.35m, 11208.635m, 5604.3175m, 123294.985m, 441.981m, 1062.578598m, 7108.877098m, 116186.107902m },
            new object[] { 16m, 6135m, 7367.35m, 105527.35m, 10552.735m, 5276.3675m, 116080.085m, 442.041m, 1000.399278m, 6718.807778m, 109361.277222m },
            new object[] { 16m, 4857.1601m, 7368.35m, 85082.9116m, 8508.29116m, 4254.14558m, 93591.20276m, 442.101m, 806.586001968m, 5502.832581968m, 88088.37017803201m },
            new object[] { 16m, 2810m, 7369.35m, 52329.35m, 5232.935m, 2616.4675m, 57562.285m, 442.161m, 496.082238m, 3554.710738m, 54007.574262m }
        };

    [Theory]
    [MemberData(nameof(LatestBasicRows))]
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
