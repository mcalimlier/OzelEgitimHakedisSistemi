using System;
using System.IO;
using System.Linq;
using System.Xml.Linq;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class HakedisCreateHybridUiContractTests
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void HakedişCreate_ShouldKeepDesignOneCardsAndLiveExcelHybridControls()
    {
        var document = LoadMainWindowXaml();
        var root = document.Root;
        Assert.NotNull(root);

        var named = root!.Descendants()
            .Where(element => element.Attribute(XamlNamespace + "Name") is not null)
            .ToDictionary(
                element => (string)element.Attribute(XamlNamespace + "Name")!,
                StringComparer.Ordinal);

        Assert.Contains("ProgressContractorComboBox", named.Keys);
        Assert.Contains("ProgressPeriodMonthComboBox", named.Keys);
        Assert.Contains("ProgressWorkDaysCardTextBlock", named.Keys);
        Assert.Contains("ProgressRouteComboBox", named.Keys);
        Assert.Contains("ProgressRouteRelationTextBlock", named.Keys);
        Assert.Contains("ProgressWorkDaysSummaryTextBlock", named.Keys);

        Assert.Equal("ProgressContractorComboBox_SelectionChanged",
            EventName(named["ProgressContractorComboBox"], "SelectionChanged"));
        Assert.Equal("ProgressPeriodMonthComboBox_SelectionChanged",
            EventName(named["ProgressPeriodMonthComboBox"], "SelectionChanged"));
        Assert.Equal("Extended", (string?)named["ProgressRouteComboBox"].Attribute("SelectionMode"));
        Assert.Equal("ProgressRouteComboBox_SelectionChanged",
            EventName(named["ProgressRouteComboBox"], "SelectionChanged"));

        var cardTitles = root.Descendants()
            .Where(element => element.Name.LocalName == "TextBlock")
            .Select(element => (string?)element.Attribute("Text"))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();

        Assert.Contains(cardTitles, text => text!.Contains("İKN VE SÖZLEŞME", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cardTitles, text => text!.Contains("YÜKLENİCİ (FİRMA) BİLGİLERİ", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cardTitles, text => text!.Contains("HAKEDİŞ DÖNEMİ", StringComparison.OrdinalIgnoreCase));
        Assert.Contains(cardTitles, text => text!.Contains("Hat Seçimi ve Veri Girişi", StringComparison.OrdinalIgnoreCase));

        var vatCheckBox = root.Descendants()
            .Single(element => (string?)element.Attribute(XamlNamespace + "Name") == "ContractorVatExemptCheckBox");
        Assert.Equal("ContractorVatExemptCheckBox_Changed", EventName(vatCheckBox, "Checked"));
        Assert.Equal("ContractorVatExemptCheckBox_Changed", EventName(vatCheckBox, "Unchecked"));

        var firmCard = vatCheckBox.Ancestors()
            .FirstOrDefault(ancestor => ancestor.Name.LocalName == "Border"
                && ancestor.Descendants().Any(element =>
                    element.Name.LocalName == "TextBlock"
                    && string.Equals((string?)element.Attribute("Text"), "KDV İSTİSNASI", StringComparison.Ordinal))
                && ancestor.Descendants().Any(element =>
                    element.Name.LocalName == "Button"
                    && string.Equals((string?)element.Attribute("Click"), "SaveContractorButton_Click", StringComparison.Ordinal)));
        Assert.NotNull(firmCard);
        Assert.Contains(firmCard!.Descendants(), element =>
            element.Name.LocalName == "Button"
            && string.Equals((string?)element.Attribute("Click"), "SaveContractorButton_Click", StringComparison.Ordinal));
    }

    private static string EventName(XElement element, string eventAttribute) =>
        (string?)element.Attribute(eventAttribute) ?? string.Empty;

    private static XDocument LoadMainWindowXaml()
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var candidate in candidates)
        {
            var directory = new DirectoryInfo(candidate);
            while (directory is not null)
            {
                var path = Path.Combine(directory.FullName, "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml");
                if (File.Exists(path))
                    return XDocument.Load(path, LoadOptions.PreserveWhitespace);
                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            "MainWindow.xaml was not found from the test working directory or test output directory.");
    }
}
