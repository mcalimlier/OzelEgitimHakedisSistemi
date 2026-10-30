using System;
using System.IO;
using System.Linq;
using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    private static readonly bool[] RecordArchiveModes = [false, true];
    private static readonly string[] SettingsPrimaryControlNames = ["BackupCreateButton", "BackupRestoreButton", "WindowLayoutModeComboBox"];
    [Theory]
    [InlineData("DailyPriceTextBox")]
    [InlineData("GuideDailyPriceTextBox")]
    [InlineData("GuidePriceDifferenceTextBox")]
    [InlineData("DiscountTextBox")]
    [InlineData("GuideDiscountTextBox")]
    [InlineData("TaxDebtTextBox")]
    [InlineData("SocialSecurityDebtTextBox")]
    [InlineData("MinimumWageSupportTextBox")]
    [InlineData("InspectionPenaltyTextBox")]
    public void BackupRestore_MustDetectUnsavedPaymentValues(string field)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); Field<TextBox>(window, field).Text = "100,25";
            Assert.True(Assert.IsType<bool>(Invoke(window, "HasOpenWorkForRestore")));
        });
    }

    [Fact]
    public void History_EmptySearchMustShowFilteredEmptyState()
    {
        RunOnSta(() =>
        {
            var path = Path.Combine(Path.GetTempPath(), "tehys-records-" + Guid.NewGuid().ToString("N"));
            var window = CreateEditorHarness(); SetField(window, "_archiveStore", new ProgressPaymentArchiveStore(path));
            SetField(window, "_operationsReady", true);
            Collection(window, "_progressHistory").Add(CreateOutputPayment(new DateOnly(2026, 9, 30), 1, 1000m));
            Field<TextBox>(window, "HistorySearchBox").Text = "bulunmayan firma";
            Invoke(window, "UpdateHistoryView");
            Assert.Empty(Collection(window, "_historyRows"));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "HistoryEmptyStateBorder").Visibility);
        });
    }

    [Fact]
    public void History_RefreshPreservesSelectedPayment()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            Collection(window, "_progressHistory").Add(CreateOutputPayment(new DateOnly(2026, 9, 30), 1, 1000m));
            var grid = Field<DataGrid>(window, "HistoryItemsControl"); grid.ItemsSource = Collection(window, "_historyRows");
            Invoke(window, "UpdateHistoryView"); grid.SelectedIndex = 0;
            Invoke(window, "UpdateHistoryView");
            Assert.NotNull(grid.SelectedItem);
            Assert.Equal(0, grid.SelectedIndex);
        });
    }

    [Fact]
    public void Records_ProgramFilterMustBeExplicitAndIndependentFromHome()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var home = Field<ComboBox>(window, "HomeProgramComboBox"); home.Items.Add(new ComboBoxItem { Tag = "SpecialEducation" }); home.SelectedIndex = 0;
            var records = Field<ComboBox>(window, "RecordsProgramComboBox"); records.Items.Add(new ComboBoxItem { Tag = "BasicOrSecondary" }); records.SelectedIndex = 0;
            var filter = Assert.IsType<PaymentFilter>(Invoke(window, "ReadRecordFilter", false));
            Assert.Equal(TransportationProgramType.BasicOrSecondary, filter.Program);
            Assert.Equal(filter.Program, Assert.IsType<PaymentFilter>(Invoke(window, "ReadRecordFilter", true)).Program);
        });
    }

    [Fact]
    public void Records_NavigationMustRemovePlaceholderAndDuplicateEntries()
    {
        var document = LoadMainWindowXaml();
        var nav = document.Descendants().Where(e => e.Name.LocalName == "Button" && ((string?)e.Attribute(XamlNamespace + "Name"))?.StartsWith("Nav", StringComparison.Ordinal) == true).ToArray();
        Assert.DoesNotContain(nav, e => (string?)e.Attribute("Tag") is "Archives" or "Statistics" or "Trace");
        Assert.Single(nav.Where(e => (string?)e.Attribute("Tag") == "History"));
        Assert.DoesNotContain(document.Descendants(), e => (string?)e.Attribute(XamlNamespace + "Name") == "StatisticsPanel");
        Assert.Equal("TabControl", NamedElement(document, "RecordsTabs").Name.LocalName);
        Assert.Equal("DataGrid", NamedElement(document, "ArchiveItemsControl").Name.LocalName);
        var trace = NamedElement(document, "LiveCalculationTraceButton");
        Assert.Contains(NamedElement(document, "ProgressPanel"), trace.Ancestors());
    }

    [Theory]
    [InlineData("History", "NavHistory", "Kayıtlar ve Arşiv")]
    [InlineData("Archives", "NavHistory", "Kayıtlar ve Arşiv")]
    [InlineData("Trace", "NavProgress", "Canlı Hesap Dökümü")]
    [InlineData("Constants", "NavSettings", "Hesaplama Sabitleri")]
    public void Records_NavigationPreservesAliasesAndActiveMenu(string page, string active, string title)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            Field<Button>(window, "NavHistory").Tag = "History"; Field<Button>(window, "NavSettings").Tag = "Settings";
            var tabs = Field<TabControl>(window, "RecordsTabs"); tabs.Items.Add(Field<TabItem>(window, "HistoryRecordsTab")); tabs.Items.Add(Field<TabItem>(window, "ArchiveRecordsTab"));
            Invoke(window, "NavigationButton_Click", new Button { Tag = page }, new RoutedEventArgs());
            Assert.Equal("Active", Field<Button>(window, active).Uid);
            Assert.Equal(title, Field<TextBlock>(window, "HeaderModuleText").Text);
            Assert.Equal(page is "History" or "Archives" ? Visibility.Visible : Visibility.Collapsed, Field<Grid>(window, "RecordsPanel").Visibility);
            if (page is "History" or "Archives") Assert.Equal(page == "Archives" ? 1 : 0, tabs.SelectedIndex);
        });
    }

    [Theory]
    [InlineData("0", false)]
    [InlineData("0,0000", false)]
    [InlineData("-1,25", true)]
    [InlineData("geçersiz", true)]
    public void BackupRestore_ZeroInputsRemainCleanButInvalidOrSignedValuesAreOpenWork(string input, bool dirty)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); Field<TextBox>(window, "GuidePriceDifferenceTextBox").Text = input;
            Assert.Equal(dirty, Assert.IsType<bool>(Invoke(window, "HasOpenWorkForRestore")));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void Records_MissingFolderDoesNotCreateAnEmptyDirectory(bool archive)
    {
        RunOnSta(() =>
        {
            var path = Path.Combine(Path.GetTempPath(), "tehys-missing-" + Guid.NewGuid().ToString("N"));
            var window = CreateEditorHarness(); Invoke(window, "OpenRecordsFolder", path, archive);
            Assert.False(Directory.Exists(path));
            Assert.Contains("klasör", Field<TextBlock>(window, archive ? "ArchiveStatusTextBlock" : "HistoryOperationStatusTextBlock").Text, StringComparison.OrdinalIgnoreCase);
        });
    }

    [Fact]
    public void ArchiveActions_RequireSelectionAndDisableUnreadableSnapshotOperations()
    {
        RunOnSta(() =>
        {
            var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "ArchivePanel"));
            var grid = (DataGrid)layout.FindName("ArchiveItemsControl");
            var valid = new { DisplayTitle = "Hakediş 1", DetailText = "Eylül 2026", NetPayableText = "100,00 ₺", RevisionText = "v01", IsReadable = true };
            var invalid = new { DisplayTitle = "Okunamayan arşiv", DetailText = "Dosyayı kontrol edin", NetPayableText = "—", RevisionText = "GEÇERSİZ", IsReadable = false };
            grid.ItemsSource = new[] { valid, invalid }; ArrangeLayout(layout, 740, 700);
            var load = (Button)layout.FindName("ArchiveLoadButton"); var edit = (Button)layout.FindName("ArchiveEditButton"); var folder = (Button)layout.FindName("ArchiveFolderButton");
            Assert.False(load.IsEnabled); Assert.False(edit.IsEnabled); Assert.False(folder.IsEnabled);
            grid.SelectedItem = valid; layout.UpdateLayout();
            Assert.True(load.IsEnabled); Assert.True(edit.IsEnabled); Assert.True(folder.IsEnabled); Assert.Same(valid, load.Tag);
            grid.SelectedItem = invalid; layout.UpdateLayout();
            Assert.False(load.IsEnabled); Assert.False(edit.IsEnabled); Assert.True(folder.IsEnabled);
            grid.SelectedItem = null; layout.UpdateLayout(); Assert.False(folder.IsEnabled);
        });
    }

    [Theory]
    [InlineData(1024, 600)]
    [InlineData(1024, 768)]
    [InlineData(1366, 600)]
    [InlineData(1366, 768)]
    [InlineData(1920, 1080)]
    public void RecordsAndSettings_KeepTablesActionsAndPreferencesReachable(double width, double height)
    {
        RunOnSta(() =>
        {
            var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "RecordsPanel")); layout.Background = Brushes.White;
            var tabs = (TabControl)layout.FindName("RecordsTabs");
            foreach (var archive in RecordArchiveModes)
            {
                var name = archive ? "Archive" : "History";
                ((Grid)layout.FindName("HistoryPanel")).Visibility = archive ? Visibility.Collapsed : Visibility.Visible;
                ((Grid)layout.FindName("ArchivePanel")).Visibility = archive ? Visibility.Visible : Visibility.Collapsed;
                tabs.SelectedIndex = archive ? 1 : 0;
                var grid = (DataGrid)layout.FindName(name + "ItemsControl");
                grid.ItemsSource = Enumerable.Range(1, 14).Select(i => new { DisplayTitle = "Hakediş " + i + " • İKN 2026/123456 • Merkez ve çevre köyler taşıma hattı", CreatedAtText = "Ekim 2026 • Uzun unvanlı örnek taşımacılık şirketi", DetailText = "Ekim 2026 • Uzun unvanlı örnek taşımacılık şirketi", TotalAmountText = "20.000,00 ₺", NetPayableText = "20.810,40 ₺", RevisionText = "v02", IsReadable = true }).ToArray();
                ((Border)layout.FindName(name + "EmptyStateBorder")).Visibility = Visibility.Collapsed;
                ArrangeLayout(layout, 1667, 982); // Exercise shrinking an already measured, visible grid.
                ArrangeLayout(layout, width - 253, height - 98); grid.SelectedIndex = 0; layout.UpdateLayout();
                Assert.True(grid.ActualHeight >= 160, name + " table must retain several visible rows.");
                Assert.True(grid.Columns[2].ActualWidth >= 119, name + " money column collapsed during resizing.");
                Assert.True(grid.Columns[3].ActualWidth >= (archive ? 99 : 119), name + " final column collapsed during resizing.");
                var actions = (WrapPanel)layout.FindName(name + "ActionsPanel");
                foreach (var button in actions.Children.OfType<Button>())
                {
                    Assert.True(button.IsEnabled);
                    var bounds = button.TransformToAncestor(layout).TransformBounds(new Rect(button.RenderSize));
                    Assert.InRange(bounds.Right, 1, layout.ActualWidth + 1); Assert.InRange(bounds.Bottom, 1, layout.ActualHeight + 1);
                }
                SavePaymentDetailsPreview(layout, "records-" + name.ToLowerInvariant() + "-" + width.ToString("0", CultureInfo.InvariantCulture) + "x" + height.ToString("0", CultureInfo.InvariantCulture) + ".png");
                var advanced = VisualDescendants<Expander>((Grid)layout.FindName(name + "Panel")).Single();
                advanced.IsExpanded = true; ArrangeLayout(layout, width - 253, height - 98);
                Assert.True(grid.ActualHeight >= 100);
                foreach (var button in actions.Children.OfType<Button>())
                    Assert.InRange(button.TransformToAncestor(layout).TransformBounds(new Rect(button.RenderSize)).Bottom, 1, layout.ActualHeight + 1);
                advanced.IsExpanded = false;
            }
            var settings = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "SettingsPanel")); settings.Background = Brushes.White;
            ArrangeLayout(settings, width - 253, height - 98);
            foreach (var name in SettingsPrimaryControlNames)
            {
                var control = (FrameworkElement)settings.FindName(name);
                var bounds = control.TransformToAncestor(settings).TransformBounds(new Rect(control.RenderSize));
                Assert.InRange(bounds.Right, 1, settings.ActualWidth + 1);
            }
            SavePaymentDetailsPreview(settings, "settings-" + width.ToString("0", CultureInfo.InvariantCulture) + "x" + height.ToString("0", CultureInfo.InvariantCulture) + ".png");
        });
    }
}
