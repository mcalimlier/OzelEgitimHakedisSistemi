using System.Text.Json;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Tests;

public sealed class PaymentRatePeriodTests
{
    [Fact]
    public void Sections_ShouldSumExactServiceAndDifferencesAndApplyDeductionsOnce()
    {
        foreach (var program in new[] { TransportationProgramType.BasicOrSecondary, TransportationProgramType.SpecialEducation })
        {
        var input = new ProgressPaymentInput(18, 1200, 100, 300, 100000, false, 50, 20, 0,
            GuideWorkDays: program == TransportationProgramType.SpecialEducation ? 18 : 0,
            GuideDailyPrice: program == TransportationProgramType.SpecialEducation ? 150 : 0,
            GuideDiscountAmount: program == TransportationProgramType.SpecialEducation ? 20 : 0,
            GuidePriceDifference: program == TransportationProgramType.SpecialEducation ? 80 : 0, ProgramType: program,
            TransportPeriods: new[] { new PaymentRatePeriod(10, 1000, 100), new PaymentRatePeriod(8, 1200, 200) },
            GuidePeriods: program == TransportationProgramType.SpecialEducation
                ? new[] { new PaymentRatePeriod(6, 100, 30), new PaymentRatePeriod(12, 150, 50) } : null);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var equivalent = input with { WorkDays = 1, DailyPrice = 19600, TransportPeriods = null,
            GuideWorkDays = program == TransportationProgramType.SpecialEducation ? 1 : 0,
            GuideDailyPrice = program == TransportationProgramType.SpecialEducation ? 2400 : 0, GuidePeriods = null };
        Assert.Equal(ProgressPaymentCalculator.Calculate(equivalent), calculation);
        var restored = JsonSerializer.Deserialize<ProgressPaymentInput>(JsonSerializer.Serialize(input))!;
        Assert.Equal(calculation, ProgressPaymentCalculator.Calculate(restored));
        Assert.Equal(input.TransportPeriods, restored.TransportPeriods);
        var payment = new ProgressPaymentHistoryData(Guid.NewGuid(), DateTime.UtcNow, restored, calculation);
        Assert.Equal(19600 - 100 + (program == TransportationProgramType.SpecialEducation ? 2400 - 20 : 0),
            ProgressPaymentReportService.CalculateWorkbookMonthlyPaymentAmount(payment));
        Assert.Contains(PaymentCalculationDetailsService.Build(payment, new[] { payment }),
            d => d.Name == "Taşıma hizmet tutarı" && d.Formula.Contains(" + ", StringComparison.Ordinal));
        }
    }

    [Fact]
    public void Sections_ShouldRejectMismatchedTotalsAndNegativeDays()
    {
        var original = new ProgressPaymentInput(18, 1200, 0, 0, 100000, true, 0, 0, 0,
            TransportPeriods: new[] { new PaymentRatePeriod(10, 1000), new PaymentRatePeriod(8, 1200) });
        Assert.Throws<ArgumentException>(() => ProgressPaymentCalculator.Calculate(original with { WorkDays = 17 }));
        Assert.Throws<ArgumentException>(() => ProgressPaymentCalculator.Calculate(original with { DailyPrice = 1100 }));
        Assert.Throws<ArgumentException>(() => ProgressPaymentCalculator.Calculate(original with
            { TransportPeriods = new[] { new PaymentRatePeriod(-1, 1000) } }));
    }

    [Fact]
    public void Sections_ShouldKeepFractionalAmountsWithoutWeightedPriceRounding()
    {
        var input = PaymentRatePeriodService.Apply(new ProgressPaymentInput(0, 0, 0, 0, 100000, true, 0, 0, 0),
            new[] { new PaymentRatePeriod(7.5m, 1234.5678m, -12.34m), new PaymentRatePeriod(10.25m, 2345.6789m, 56.78m) }, null);
        Assert.Equal(17.75m, input.WorkDays);
        Assert.Equal(2345.6789m, input.DailyPrice);
        Assert.Equal(44.44m, input.PriceDifference);
        Assert.Equal(7.5m * 1234.5678m + 10.25m * 2345.6789m, ProgressPaymentCalculator.Calculate(input).ServiceAmount);
    }
}
