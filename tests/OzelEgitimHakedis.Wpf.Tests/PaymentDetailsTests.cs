using System.Globalization;
using System.Reflection;
using System.Xml.Linq;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class PaymentDetailsTests
{
    [Fact]
    public void DetailsService_ExistsAndUsesSavedAmounts()
    {
        var input = new ProgressPaymentInput(8m, 2446.9323m, 0m, 841.87m, 100000m, false, 12m, 34m, 56m,
            GuideWorkDays: 17m, GuideDailyPrice: 1362.49m, GuidePriceDifference: 1321.2946m);
        var rates = ProgressPaymentCalculationConstants.Defaults with { StampTaxRate = 0.011m };
        var saved = ProgressPaymentCalculator.Calculate(input, rates) with { NetPayable = 98765.4321m };
        var payment = new ProgressPaymentHistoryData(Guid.NewGuid(), DateTime.Now, input, saved);
        var rows = PaymentCalculationDetailsService.Build(payment, [payment]);
        Assert.Equal(98765.4321m, rows.Single(r => r.Name == "Net ödenecek (hesaplama)").Value);
        Assert.Equal(saved.StampTax, rows.Single(r => r.Name == "Damga vergisi").Value);
        Assert.Contains("%1,1", rows.Single(r => r.Name == "Damga vergisi").Formula, StringComparison.Ordinal);
        Assert.Equal("2.446,9323", rows.Single(r => r.Name == "Taşıma günlük fiyatı").ValueText);
        Assert.True(rows.Count >= 45);
        var old = payment with { Calculation = saved with { AppliedConstants = null } };
        Assert.Contains("oran kaydı yok", PaymentCalculationDetailsService.Build(old, [old]).Single(r => r.Name == "Damga vergisi").Formula, StringComparison.Ordinal);
    }

    [Theory]
    [InlineData("2.446,9323", 2446.9323)]
    [InlineData("1.403,573", 1403.573)]
    [InlineData("1.321,2946", 1321.2946)]
    public void WorkbookPrecision_IsAcceptedWithoutRounding(string text, double expected)
    {
        var parse = typeof(MainWindow).GetMethod("TryParseRequiredMoney", BindingFlags.Static | BindingFlags.NonPublic)!;
        object?[] args = [text, 0m];
        Assert.True((bool)parse.Invoke(null, args)!);
        Assert.Equal((decimal)expected, args[1]);
    }
}
