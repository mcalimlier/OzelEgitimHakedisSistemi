using System.Globalization;
using System.Reflection;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class SinglePaymentMoneyInputTests
{
    [Theory]
    [InlineData("en-US", "12.536,48", 12536.48)]
    [InlineData("en-US", "12,34", 12.34)]
    [InlineData("tr-TR", "12.536,48", 12536.48)]
    public void FormMoneyInput_UsesTurkishSeparatorsRegardlessOfMachineCulture(string cultureName, string input, double expected)
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            var method = typeof(MainWindow).GetMethod("TryParseDecimalOrZero", BindingFlags.NonPublic | BindingFlags.Static)!;
            object?[] args = [input, 0m];
            Assert.True((bool)method.Invoke(null, args)!);
            Assert.Equal((decimal)expected, (decimal)args[1]!);
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Theory]
    [InlineData("en-US")]
    [InlineData("tr-TR")]
    public void RestoredFormMoney_ShowsHistoricalPrecisionButRequiresCorrectionBeforeSaving(string cultureName)
    {
        var old = CultureInfo.CurrentCulture;
        try
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(cultureName);
            var format = typeof(MainWindow).GetMethod("FormatCalculationInput", BindingFlags.NonPublic | BindingFlags.Static)!;
            var parse = typeof(MainWindow).GetMethod("TryParseDecimalOrZero", BindingFlags.NonPublic | BindingFlags.Static)!;
            var text = (string)format.Invoke(null, [19539.1608m])!;
            object?[] args = [text, 0m];
            Assert.False((bool)parse.Invoke(null, args)!);
            Assert.Contains(",", text);
        }
        finally { CultureInfo.CurrentCulture = old; }
    }

    [Theory]
    [InlineData("12,345")]
    [InlineData("12,000")]
    [InlineData("12.536,480")]
    public void FormMoneyInput_RejectsMoreThanTwoFractionalDigits(string input)
    {
        var method = typeof(MainWindow).GetMethod("TryParseDecimalOrZero", BindingFlags.NonPublic | BindingFlags.Static)!;
        object?[] args = [input, 0m];
        Assert.False((bool)method.Invoke(null, args)!);
    }
}
