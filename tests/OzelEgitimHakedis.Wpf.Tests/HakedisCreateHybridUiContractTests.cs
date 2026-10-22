using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class HakedisCreateHybridUiContractTests
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    [Fact]
    public void HakedisCreate_ShouldKeepDesignOneCardsAndLiveExcelHybridControls()
    {
        var document = LoadMainWindowXaml();
        var root = document.Root;
        Assert.NotNull(root);

        var named = root!.Descendants()
            .Where(element => element.Attribute(XamlNamespace + "Name") is not null)
            .ToDictionary(
                element => (string)element.Attribute(XamlNamespace + "Name")!,
                StringComparer.Ordinal);

        Assert.Contains("ProgressTenderListBox", named.Keys);
        Assert.Equal("ProgressTenderListBox_SelectionChanged",
            EventName(named["ProgressTenderListBox"], "SelectionChanged"));
        Assert.Contains("ProgressNewTenderShortcutButton", named.Keys);
        Assert.Equal("Contract", (string?)named["ProgressNewTenderShortcutButton"].Attribute("Tag"));
        Assert.Equal("NavigationButton_Click",
            EventName(named["ProgressNewTenderShortcutButton"], "Click"));
        Assert.Contains("ProgressContractorComboBox", named.Keys);
        Assert.Contains("ProgressContractorDetailShortcutButton", named.Keys);
        Assert.Equal("Contractors", (string?)named["ProgressContractorDetailShortcutButton"].Attribute("Tag"));
        Assert.Equal("NavigationButton_Click",
            EventName(named["ProgressContractorDetailShortcutButton"], "Click"));
        Assert.Contains("ProgressPeriodMonthComboBox", named.Keys);
        Assert.Contains("ProgressWorkDaysCardTextBlock", named.Keys);
        Assert.Contains("ManualAdditionalGuaranteeTextBox", named.Keys);
        Assert.Equal("True", (string?)named["ManualAdditionalGuaranteeTextBox"].Attribute("IsReadOnly"));
        Assert.Equal("0,00 ₺", (string?)named["ManualAdditionalGuaranteeTextBox"].Attribute("Text"));
        Assert.Contains("Hesaplanan", (string?)named["ManualAdditionalGuaranteeLabel"].Attribute("Text"), StringComparison.Ordinal);
        Assert.Contains("I × %6",
            (string?)named["GuaranteeRuleHintTextBlock"].Attribute("Text"), StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox", named.Keys);
        Assert.Contains("ProgressRouteTableHeaderGrid", named.Keys);
        var routeTableHeaders = named["ProgressRouteTableHeaderGrid"].Descendants()
            .Where(element => element.Name.LocalName == "TextBlock")
            .Select(element => (string?)element.Attribute("Text"))
            .Where(text => !string.IsNullOrWhiteSpace(text))
            .ToArray();
        Assert.Contains("Hat", routeTableHeaders);
        Assert.Contains("Güzergâh", routeTableHeaders);
        Assert.Contains("Söz. Gün", routeTableHeaders);
        Assert.Contains("Günlük Fiyat", routeTableHeaders);
        Assert.Contains("Durum", routeTableHeaders);
        Assert.Contains("ProgressRouteRelationTextBlock", named.Keys);
        Assert.Contains("ProgressWorkDaysSummaryTextBlock", named.Keys);
        Assert.Contains("MultiRoutePaymentGrid", named.Keys);
        Assert.Contains("CalculateProgressButton", named.Keys);
        Assert.Equal("CalculateProgressButton_Click",
            EventName(named["CalculateProgressButton"], "Click"));
        Assert.Equal("False", (string?)named["MultiRoutePaymentGrid"].Attribute("CanUserAddRows"));
        Assert.Equal("False", (string?)named["MultiRoutePaymentGrid"].Attribute("CanUserDeleteRows"));
        Assert.Equal("CellOrRowHeader", (string?)named["MultiRoutePaymentGrid"].Attribute("SelectionUnit"));
        Assert.Equal("IncludeHeader", (string?)named["MultiRoutePaymentGrid"].Attribute("ClipboardCopyMode"));
        Assert.Equal("1", (string?)named["MultiRoutePaymentGrid"].Attribute("FrozenColumnCount"));
        Assert.Equal("False", (string?)named["MultiRoutePaymentGrid"].Attribute("AutoGenerateColumns"));
        Assert.Contains(named["MultiRoutePaymentGrid"].Descendants(),
            element => element.Name.LocalName == "DataGridTextColumn"
                && string.Equals((string?)element.Attribute("Header"), "Taşıma Günlük Fiyat", StringComparison.Ordinal)
                && string.Equals((string?)element.Attribute("IsReadOnly"), "True", StringComparison.Ordinal));
        Assert.Contains(named["MultiRoutePaymentGrid"].Descendants(),
            element => element.Name.LocalName == "DataGridTextColumn"
                && string.Equals((string?)element.Attribute("Header"), "Taşıma Fiyat Farkı", StringComparison.Ordinal)
                && ((string?)element.Attribute("Binding"))?.Contains("Mode=TwoWay", StringComparison.Ordinal) == true);

        var columns = named["MultiRoutePaymentGrid"].Descendants()
            .Where(element => element.Name.LocalName == "DataGridTextColumn")
            .ToDictionary(element => (string?)element.Attribute("Header") ?? string.Empty,
                StringComparer.Ordinal);
        foreach (var editableHeader in new[]
        {
            "Taşıma Gün", "Taşıma İskonto", "Taşıma Fiyat Farkı",
            "Rehber Gün", "Rehber İskonto", "Rehber Fiyat Farkı",
            "Vergi Borcu", "SGK Borcu", "Denetim Cezası"
        })
        {
            Assert.Contains(editableHeader, columns.Keys);
            Assert.Contains("Mode=TwoWay",
                (string?)columns[editableHeader].Attribute("Binding") ?? string.Empty);
        }

        foreach (var readOnlyHeader in new[] { "Hat", "Taşıma Günlük Fiyat", "Rehber Günlük Fiyat" })
        {
            Assert.Contains(readOnlyHeader, columns.Keys);
            Assert.Equal("True", (string?)columns[readOnlyHeader].Attribute("IsReadOnly"));
        }

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

    [Fact]
    public void ApplicationManifest_ShouldDeclarePerMonitorV2DpiAwareness()
    {
        var manifest = XDocument.Load(LoadProjectFile("src", "OzelEgitimHakedis.Wpf", "app.manifest"));
        XNamespace windowsSettings = "http://schemas.microsoft.com/SMI/2016/WindowsSettings";
        XNamespace legacyWindowsSettings = "http://schemas.microsoft.com/SMI/2005/WindowsSettings";

        Assert.Equal("PerMonitorV2",
            manifest.Descendants(windowsSettings + "dpiAwareness").Single().Value);
        Assert.Equal("true",
            manifest.Descendants(legacyWindowsSettings + "dpiAware").Single().Value);
    }

    [Fact]
    public void ArchiveProgressCorrection_ShouldLoadOnlySnapshotSelectorContext()
    {
        var source = File.ReadAllText(LoadProjectFile(
            "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));

        var historyEdit = MethodBody(source, "BeginProgressHistoryEdit");
        var contextLoader = MethodBody(source, "LoadArchiveProgressEditorContext");
        var editorMode = MethodBody(source, "SetProgressEditorMode");
        var multiRouteRows = MethodBody(source, "UpdateMultiRouteDraftRows");

        Assert.Contains("LoadArchiveProgressEditorContext(ArchiveProgressEditorContext.Create(archive));", historyEdit, StringComparison.Ordinal);
        Assert.Contains("ProgressTenderListBox.ItemsSource = new[] { context.Contract };", contextLoader, StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox.ItemsSource = new[] { snapshotRoute };", contextLoader, StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox.SelectedItems.Clear();", contextLoader, StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox.SelectedItem = snapshotRoute;", contextLoader, StringComparison.Ordinal);
        Assert.Contains("_multiRouteDraftCache.Clear();", contextLoader, StringComparison.Ordinal);
        Assert.Contains("UpdateProgressPeriodOptions(context.Contract, preserveSelection: false);", contextLoader, StringComparison.Ordinal);
        Assert.Contains("UpdateMultiRouteDraftRows();", contextLoader, StringComparison.Ordinal);
        Assert.Contains("ProgressTenderListBox.IsEnabled = !(editing && archiveCorrection);", editorMode, StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox.IsEnabled = !(editing && archiveCorrection);", editorMode, StringComparison.Ordinal);
        Assert.Contains("MultiRouteEntryBorder.Visibility = Visibility.Collapsed;", multiRouteRows, StringComparison.Ordinal);
        Assert.Contains("SetGlobalMonetaryFieldsVisible(true);", multiRouteRows, StringComparison.Ordinal);
    }

    [Fact]
    public void ArchiveProgressCorrection_ShouldRestoreLiveSelectorsWhenCorrectionStateClears()
    {
        var source = File.ReadAllText(LoadProjectFile(
            "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));

        var clearArchiveState = MethodBody(source, "ClearArchiveCorrectionState");
        var selectorRestore = MethodBody(source, "RestoreLiveProgressEditorSelectorContext");
        var saveArchiveCorrection = MethodBody(source, "SaveArchiveCorrectionSnapshot");
        var cancelProgressEdit = MethodBody(source, "CancelProgressEditButton_Click");

        Assert.Contains("RestoreLiveProgressEditorSelectorContext();", clearArchiveState, StringComparison.Ordinal);
        Assert.Contains("ProgressTenderListBox.ItemsSource = _contracts;", selectorRestore, StringComparison.Ordinal);
        Assert.Contains("ProgressTenderListBox.SelectedItem = null;", selectorRestore, StringComparison.Ordinal);
        Assert.Contains("ProgressRouteComboBox.SelectedItems.Clear();", selectorRestore, StringComparison.Ordinal);
        Assert.Contains("_multiRouteDraftCache.Clear();", selectorRestore, StringComparison.Ordinal);
        Assert.Contains("ClearArchiveCorrectionState();", saveArchiveCorrection, StringComparison.Ordinal);
        Assert.Contains("ClearArchiveCorrectionState();", cancelProgressEdit, StringComparison.Ordinal);
    }

    private static string EventName(XElement element, string eventAttribute) =>
        (string?)element.Attribute(eventAttribute) ?? string.Empty;

    private static XDocument LoadMainWindowXaml() =>
        XDocument.Load(LoadProjectFile("src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml"),
            LoadOptions.PreserveWhitespace);

    private static string MethodBody(string source, string methodName)
    {
        var methodMatch = Regex.Match(
            source,
            $@"private\s+(?:static\s+)?[\w<>?,]+\s+{Regex.Escape(methodName)}\(");
        Assert.True(methodMatch.Success, $"Expected MainWindow to define {methodName}.");
        var methodStart = methodMatch.Index;

        var bodyStart = source.IndexOf('{', methodStart);
        Assert.True(bodyStart >= 0, $"Expected {methodName} to have a body.");

        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}' && --depth == 0)
                return source[(bodyStart + 1)..index];
        }

        throw new InvalidOperationException($"Expected {methodName} to have a closing brace.");
    }

    private static string LoadProjectFile(params string[] segments)
    {
        var candidates = new[] { Directory.GetCurrentDirectory(), AppContext.BaseDirectory };
        foreach (var candidate in candidates)
        {
            var directory = new DirectoryInfo(candidate);
            while (directory is not null)
            {
                var path = segments.Aggregate(directory.FullName, Path.Combine);
                if (File.Exists(path))
                    return path;
                directory = directory.Parent;
            }
        }

        throw new FileNotFoundException(
            $"Project file '{Path.Combine(segments)}' was not found from the test working directory or test output directory.");
    }
}
