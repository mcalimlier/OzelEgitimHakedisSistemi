using System;
using System.IO;
using System.Linq;
using System.Text.RegularExpressions;
using System.Xml.Linq;
using System.Collections;
using System.Globalization;
using System.Reflection;
using System.Runtime.CompilerServices;
using System.Runtime.ExceptionServices;
using System.Threading;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class HakedisCreateHybridUiContractTests
{
    private static readonly XNamespace XamlNamespace =
        "http://schemas.microsoft.com/winfx/2006/xaml";

    [Theory]
    [InlineData(TransportationProgramType.BasicOrSecondary, "Hesaplama Bilgileri", "Hizmet İşleri Hakediş Raporu", Visibility.Collapsed)]
    [InlineData(TransportationProgramType.SpecialEducation, "Hesaplama", "Hizmetişleri Hakediş Raporu", Visibility.Visible)]
    public void PaymentLabels_ShouldFollowSelectedContractProgram(
        TransportationProgramType program, string calculationSheet, string serviceReport, Visibility guideVisibility)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract with { ProgramType = program };
            Invoke(window, "UpdateProgressProgramTypeFields", contract);

            Assert.Equal("Taşıma Fiyat Farkı KDV Hariç", Field<TextBlock>(window, "PriceDifferenceLabel").Text);
            Assert.Equal(calculationSheet, Field<TextBlock>(window, "CalculationSheetNameTextBlock").Text);
            Assert.Equal(serviceReport, Field<TextBlock>(window, "ServiceReportSheetNameTextBlock").Text);
            Assert.Equal("Ele Geçen", Field<TextBlock>(window, "NetPayableLabel").Text);
            Assert.Equal("Ele Geçen", Field<TextBlock>(window, "NetControlLabel").Text);
            Assert.Equal(guideVisibility, Field<TextBlock>(window, "GuidePriceDifferenceLabel").Visibility);
            if (program == TransportationProgramType.SpecialEducation)
                Assert.Equal("Rehber Personel Fiyat Farkı KDV Hariç", Field<TextBlock>(window, "GuidePriceDifferenceLabel").Text);
        });
    }

    [Fact]
    public void ReportSheetLabel_ShouldFollowReportRouteEvenWhenPaymentProgramDiffers()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract;
            Invoke(window, "UpdateProgressProgramTypeFields", contract);
            var route = CreateRoute("01", "Temel hat", "12345678901", TransportationProgramType.BasicOrSecondary);
            var selector = Field<ComboBox>(window, "ReportRouteComboBox");
            selector.Items.Add(route);
            selector.SelectedItem = route;
            Invoke(window, "UpdateReportView");

            Assert.Equal("Hizmet İşleri Hakediş Raporu", Field<TextBlock>(window, "ServiceReportSheetNameTextBlock").Text);
            Assert.Equal("Hesaplama", Field<TextBlock>(window, "CalculationSheetNameTextBlock").Text);
        });
    }

    [Fact]
    public void ContractDisplay_ShouldNotUseAnotherArchivesRouteName()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            SetField(window, "_editingArchiveSnapshot", archive);
            var otherContract = archive.Contract with { TenderNumber = "OTHER", WorkName = "Other legacy title" };

            Assert.Equal("Hat 01 Hattı Öğrenci Taşıma İşi", Invoke(window, "GetContractWorkName", archive.Contract));
            Assert.Equal("Other legacy title", Invoke(window, "GetContractWorkName", otherContract));
        });
    }

    [Theory]
    [InlineData("  Merkez  ", "Eski iş adı", "Merkez Hattı Öğrenci Taşıma İşi")]
    [InlineData("", "Eski iş adı", "Eski iş adı")]
    public void ContractDisplay_ShouldDeriveWorkNameWithoutChangingStoredLegacyValue(
        string routeName, string legacyWorkName, string expected)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract with { WorkName = legacyWorkName };
            Collection(window, "_contracts").Add(contract);
            if (routeName.Length > 0)
                Collection(window, "_routes").Add(CreateRoute("01", routeName, "12345678901"));
            Invoke(window, "ApplyContractToForm", contract);

            Assert.Equal(expected, Field<TextBox>(window, "WorkNameTextBox").Text);
            Assert.Equal(legacyWorkName, contract.WorkName);
        });
    }

    [Theory]
    [InlineData("")]
    [InlineData("Stale stored title")]
    public void ContractListsAndSummary_ShouldProjectAndRefreshRouteTitlesWithoutChangingStoredContracts(string storedTitle)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract with { WorkName = storedTitle };
            Collection(window, "_contracts").Add(contract);
            var displayedTitle = new TextBlock();
            var converter = Field<ContractWorkNameConverter>(window, "_contractWorkNameConverter");
            var titleBinding = new MultiBinding { Converter = converter, Mode = BindingMode.OneWay };
            titleBinding.Bindings.Add(new Binding { Source = contract });
            titleBinding.Bindings.Add(new Binding(nameof(ContractWorkNameConverter.Titles)) { Source = converter });
            BindingOperations.SetBinding(displayedTitle, TextBlock.TextProperty, titleBinding);
            var routes = Collection(window, "_routes");
            routes.Add(CreateRoute("01", "Merkez", "12345678901"));
            Invoke(window, "UpdateContractSelectors");

            Assert.Contains("Merkez Hattı Öğrenci Taşıma İşi", Field<TextBlock>(window, "HomeSelectedTenderSummaryTextBlock").Text, StringComparison.Ordinal);
            var titles = converter.Titles;
            Assert.Equal("Merkez Hattı Öğrenci Taşıma İşi", titles[contract]);
            Assert.Equal("Merkez Hattı Öğrenci Taşıma İşi", displayedTitle.Text);
            Assert.Same(contract, Field<ComboBox>(window, "HomeTenderListBox").SelectedItem);
            Assert.Same(contract, Assert.Single(Field<ItemsControl>(window, "ContractsListBox").Items.Cast<object>()));

            routes[0] = CreateRoute("01", "Yeni Merkez", "12345678901");
            Invoke(window, "UpdateRouteListView");
            titles = converter.Titles;
            Assert.Equal("Yeni Merkez Hattı Öğrenci Taşıma İşi", titles[contract]);
            Assert.Equal("Yeni Merkez Hattı Öğrenci Taşıma İşi", displayedTitle.Text);
            Assert.Contains("Yeni Merkez Hattı Öğrenci Taşıma İşi", Field<TextBlock>(window, "HomeSelectedTenderSummaryTextBlock").Text, StringComparison.Ordinal);

            routes.Clear();
            Invoke(window, "UpdateRouteListView");
            titles = converter.Titles;
            Assert.Equal(storedTitle, titles[contract]);
            Assert.Equal(storedTitle, displayedTitle.Text);
            Assert.Equal(storedTitle, contract.WorkName);
            Assert.Same(contract, Assert.Single(Collection(window, "_contracts").Cast<object>()));
        });
    }

    [Theory]
    [InlineData("HomeTenderListBox")]
    [InlineData("ContractsListBox")]
    public void ContractListWorkTitle_ShouldBindToRouteDerivedPresentation(string listName)
    {
        var list = LoadMainWindowXaml().Descendants()
            .Single(element => (string?)element.Attribute(XamlNamespace + "Name") == listName);
        var title = list.Descendants().Last(element => element.Name.LocalName == "TextBlock");
        var binding = Assert.Single(title.Descendants().Where(element => element.Name.LocalName == "MultiBinding"));
        Assert.Equal("{StaticResource ContractWorkNameConverter}", (string?)binding.Attribute("Converter"));
        // ComboBox popup items cannot rely on finding their Window visually.
        Assert.Contains(binding.Elements(), element => (string?)element.Attribute("Path") == "Titles"
            && (string?)element.Attribute("Source") == "{StaticResource ContractWorkNameConverter}");
    }

    [Theory]
    [InlineData("Kayıtlı hat", "Canlı hat", "Eski iş", "Kayıtlı hat Hattı Öğrenci Taşıma İşi")]
    [InlineData("", "Canlı hat", "Eski iş", "Canlı hat Hattı Öğrenci Taşıma İşi")]
    [InlineData("", "", "Eski iş", "Eski iş")]
    public void OfficialOutput_ShouldDeriveWorkNameFromPaymentRouteThenLiveRouteWithLegacyFallback(
        string savedRoute, string liveRoute, string legacyWorkName, string expected)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            var payment = archive.ProgressPayment with
            {
                TenderNumber = "IHALE-01", ContractNumber = "SOZ-01", ContractorTaxOrIdentityNumber = "12345678901",
                RouteName = savedRoute, WorkName = legacyWorkName
            };
            Collection(window, "_contracts").Add(archive.Contract);
            Collection(window, "_routes").Add(CreateRoute("01", liveRoute, "12345678901"));
            var context = Assert.IsType<OehsOfficialDocumentContext>(Invoke(window, "BuildOfficialDocumentContextForPayment", payment));
            Assert.Equal(expected, context.WorkName);
            Assert.Equal(legacyWorkName, payment.WorkName);
        });
    }

    [Fact]
    public void PaymentNetSummary_ShouldUseOfficialNetLabel()
    {
        var summaryValue = LoadMainWindowXaml().Descendants()
            .Single(element => (string?)element.Attribute(XamlNamespace + "Name") == "ProgressNetSummaryTextBlock");
        Assert.Equal("Ele Geçen", (string?)summaryValue.Parent!.Elements().First().Attribute("Text"));
    }

    [Fact]
    public void ContractWorkName_ShouldBeReadOnlyAndNotRequiredByAnySaveOrPreviewPath()
    {
        var document = LoadMainWindowXaml();
        var workName = document.Descendants().Single(element => (string?)element.Attribute(XamlNamespace + "Name") == "WorkNameTextBox");
        Assert.Equal("True", (string?)workName.Attribute("IsReadOnly"));
        Assert.Contains("Hattın Adı", (string?)workName.Attribute("ToolTip"), StringComparison.Ordinal);

        var source = File.ReadAllText(LoadProjectFile("src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));
        foreach (var method in new[] { "SaveContractButton_Click", "SaveArchivedContractCorrection", "TryCreateHistoryCorrectionPreview" })
            Assert.DoesNotContain("string.IsNullOrWhiteSpace(WorkNameTextBox.Text)", MethodBody(source, method), StringComparison.Ordinal);
    }

    [Theory]
    [InlineData(true, null, false, 0)]
    [InlineData(false, null, true, 2000)]
    [InlineData(true, false, false, 0)]
    [InlineData(false, true, true, 2000)]
    public void ArchiveCalculation_ShouldKeepPaymentVatWithMissingOrConflictingLiveFirm(
        bool archivedVatExempt, bool? liveVatExempt, bool contractVatExempt, decimal expectedVat)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            archive = archive with
            {
                Contract = archive.Contract with { VatExempt = contractVatExempt },
                Contractor = archive.Contractor with { VatExempt = !archivedVatExempt },
                Route = archive.Route with { ContractorTaxOrIdentityNumber = "12345678901" },
                ProgressPayment = archive.ProgressPayment with
                {
                    Input = archive.ProgressPayment.Input with { IsVatExempt = archivedVatExempt }
                }
            };
            if (liveVatExempt.HasValue)
                Collection(window, "_contractors").Add(CreateFirm("Live firm", "12345678901", liveVatExempt.Value));
            SetField(window, "_editingArchiveSourcePath", "unused-archive.json");
            SetField(window, "_editingArchiveSnapshot", archive);
            Invoke(window, "BeginProgressHistoryEdit", archive.ProgressPayment);

            // Unchanged inputs calculate but never open the save-confirmation dialog.
            Assert.False((bool)Invoke(window, "ArchiveCorrectionInputsDiffer")!);
            Invoke(window, "CalculateProgressButton_Click", window, new RoutedEventArgs());

            Assert.Equal(expectedVat.ToString("N2", CultureInfo.CurrentCulture) + " ₺",
                Field<TextBlock>(window, "VatResultTextBlock").Text);
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ArchiveCalculationTrace_ShouldUsePaymentVatInsteadOfSnapshotFirm(bool archivedVatExempt)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            archive = archive with
            {
                Contractor = archive.Contractor with { VatExempt = !archivedVatExempt },
                ProgressPayment = archive.ProgressPayment with
                {
                    Input = archive.ProgressPayment.Input with { IsVatExempt = archivedVatExempt }
                }
            };
            SetField(window, "_editingArchiveSourcePath", "unused-archive.json");
            SetField(window, "_editingArchiveSnapshot", archive);
            Invoke(window, "BeginProgressHistoryEdit", archive.ProgressPayment);

            Assert.Equal(archivedVatExempt, Invoke(window, "IsVatExemptForCurrentFirm", archive.Contract));
        });
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void HistoryCorrection_ShouldRestoreSavedFirmAndRouteForMultiFirmContract(bool resetDraft)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            var savedFirm = CreateFirm("Same name", "12345678901");
            var otherFirm = CreateFirm("Same name", "98765432109");
            var savedRoute = CreateRoute("01", "Saved route", "12345678901");
            var otherRoute = CreateRoute("01", "Other firm's route", "98765432109");
            var payment = archive.ProgressPayment with
            {
                TenderNumber = "IHALE-01", ContractNumber = "SOZ-01",
                ContractorTaxOrIdentityNumber = "123 456 789 01"
            };
            Collection(window, "_contracts").Add(archive.Contract);
            Collection(window, "_contractors").Add(savedFirm);
            Collection(window, "_contractors").Add(otherFirm);
            Collection(window, "_routes").Add(savedRoute);
            Collection(window, "_routes").Add(otherRoute);
            Collection(window, "_progressHistory").Add(payment);
            Field<ComboBox>(window, "ProgressTenderListBox").ItemsSource = Collection(window, "_contracts");
            Field<ComboBox>(window, "ProgressContractorComboBox").ItemsSource = Collection(window, "_contractors");
            Field<ComboBox>(window, "ProgressContractorComboBox").SelectedItem = otherFirm;

            if (resetDraft)
            {
                Invoke(window, "ResetNewProgressDraftAfterCalculation", payment);
                Assert.Null(Field<ComboBox>(window, "ProgressContractorComboBox").SelectedItem);
            }

            Invoke(window, "BeginProgressHistoryEdit", payment);

            Assert.Same(savedFirm, Field<ComboBox>(window, "ProgressContractorComboBox").SelectedItem);
            var routes = Field<ListBox>(window, "ProgressRouteComboBox");
            Assert.Same(savedRoute, Assert.Single(routes.SelectedItems.Cast<object>()));
            Assert.Same(savedRoute, Assert.Single(routes.Items.Cast<object>()));
            Assert.Equal(new DateOnly(2026, 10, 31),
                Assert.IsType<ProgressPaymentMonthOption>(Field<ComboBox>(window, "ProgressPeriodMonthComboBox").SelectedItem).PeriodDate);
        });
    }

    private static MainWindow CreateEditorHarness()
    {
        // Exercise real handlers and selectors without running the constructor's
        // fresh-start migration, persistence loading, or visible window setup.
        var window = (MainWindow)RuntimeHelpers.GetUninitializedObject(typeof(MainWindow));
        foreach (var field in typeof(MainWindow).GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public))
        {
            if (typeof(FrameworkElement).IsAssignableFrom(field.FieldType)
                || field.FieldType == typeof(ContractWorkNameConverter)
                || field.FieldType.IsGenericType &&
                (field.FieldType.GetGenericTypeDefinition() == typeof(System.Collections.ObjectModel.ObservableCollection<>)
                 || field.FieldType.GetGenericTypeDefinition() == typeof(RouteDraftCache<>)))
                field.SetValue(window, Activator.CreateInstance(field.FieldType));
        }
        SetField(window, "_calculationConstants", ProgressPaymentCalculationConstants.Defaults);
        Field<ListBox>(window, "ProgressRouteComboBox").SelectionMode = SelectionMode.Extended;
        Field<Button>(window, "NavProgress").Tag = "Progress";
        return window;
    }

    private static object CreateFirm(string name, string identity, bool vatExempt = false) =>
        Activator.CreateInstance(typeof(MainWindow).GetNestedType("ContractorDraft", BindingFlags.NonPublic)!,
            name, identity, "", "", "", "", "", TransportationProgramType.SpecialEducation, vatExempt)!;

    private static object CreateRoute(string number, string name, string identity,
        TransportationProgramType program = TransportationProgramType.SpecialEducation) =>
        Activator.CreateInstance(typeof(MainWindow).GetNestedType("RouteDraft", BindingFlags.NonPublic)!,
            number, name, 1_000m, 0m, 0m, 20m, "IHALE-01", identity, "Same name", "SOZ-01",
            program)!;

    private static T Field<T>(MainWindow window, string name) =>
        (T)typeof(MainWindow).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)!.GetValue(window)!;

    private static IList Collection(MainWindow window, string name) => Field<IList>(window, name);

    private static void SetField(MainWindow window, string name, object value) =>
        typeof(MainWindow).GetField(name, BindingFlags.Instance | BindingFlags.NonPublic)!.SetValue(window, value);

    private static object? Invoke(MainWindow window, string name, params object[] arguments)
    {
        try
        {
            return typeof(MainWindow).GetMethod(name, BindingFlags.Instance | BindingFlags.NonPublic)!.Invoke(window, arguments);
        }
        catch (TargetInvocationException error) when (error.InnerException is not null)
        {
            ExceptionDispatchInfo.Capture(error.InnerException).Throw();
            throw;
        }
    }

    private static void RunOnSta(Action test)
    {
        Exception? error = null;
        var thread = new Thread(() =>
        {
            try { test(); }
            catch (Exception exception) { error = exception; }
        }) { IsBackground = true };
        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        Assert.True(thread.Join(TimeSpan.FromSeconds(30)), "Editor handler did not finish within 30 seconds.");
        if (error is not null)
            ExceptionDispatchInfo.Capture(error).Throw();
    }

    [Fact]
    public void ProgressPanel_ShouldPlaceContextRouteSelectionAndActionsInWorkflowOrder()
    {
        var document = LoadMainWindowXaml();
        var progressPanel = document.Root!.Descendants()
            .Single(element => (string?)element.Attribute(XamlNamespace + "Name") == "ProgressPanel");
        var panelElements = progressPanel.Descendants().ToArray();

        var contextIndex = IndexOfNamedElement(panelElements, "ProgressContextPanel");
        var routeSelectionIndex = IndexOfNamedElement(panelElements, "ProgressRouteSelectionPanel");
        var actionIndex = IndexOfNamedElement(panelElements, "ProgressActionPanel");
        var inputIndex = IndexOfNamedElement(panelElements, "WorkDaysTextBox");
        var resultIndex = IndexOfNamedElement(panelElements, "NetResultTextBlock");

        Assert.True(contextIndex < routeSelectionIndex,
            "The progress context strip must appear before route selection.");
        Assert.True(routeSelectionIndex < inputIndex,
            "Route selection must appear before payment inputs.");
        Assert.True(inputIndex < resultIndex && resultIndex < actionIndex,
            "The action bar must appear after the input and result cards.");

        var actionPanel = panelElements[actionIndex];
        foreach (var buttonName in new[]
        {
            "CalculateProgressButton", "CancelProgressEditButton", "EditCurrentProgressButton",
            "ArchiveCurrentProgressButton", "CorrectCurrentProgressButton"
        })
        {
            var button = panelElements[IndexOfNamedElement(panelElements, buttonName)];
            Assert.Equal("Button", button.Name.LocalName);
            Assert.True(button.Ancestors().Contains(actionPanel),
                $"{buttonName} must be inside ProgressActionPanel.");
        }

        var archiveCorrectionState = panelElements
            .Single(element => (string?)element.Attribute(XamlNamespace + "Name") == "ProgressArchiveCorrectionStateTextBlock");
        Assert.Contains("ARŞİV DÜZELTME", (string?)archiveCorrectionState.Attribute("Text"), StringComparison.Ordinal);
        Assert.Equal("#F4C247", (string?)archiveCorrectionState.Attribute("Foreground"));
        Assert.Contains("KİLİTLİ", (string?)archiveCorrectionState.Attribute("Text"), StringComparison.Ordinal);
        Assert.Null(archiveCorrectionState.Attribute("Visibility"));
        var archiveStateTrigger = Assert.Single(archiveCorrectionState.Descendants().Where(
            element => element.Name.LocalName == "DataTrigger"
                && string.Equals((string?)element.Attribute("Value"), "False", StringComparison.Ordinal)
                && ((string?)element.Attribute("Binding"))?.Contains("ProgressTenderListBox", StringComparison.Ordinal) == true
                && ((string?)element.Attribute("Binding"))?.Contains("Path=IsEnabled", StringComparison.Ordinal) == true));
        Assert.Contains(archiveStateTrigger.Elements(), element => element.Name.LocalName == "Setter"
            && string.Equals((string?)element.Attribute("Property"), "Visibility", StringComparison.Ordinal)
            && string.Equals((string?)element.Attribute("Value"), "Visible", StringComparison.Ordinal));
    }

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
        Assert.Contains("Hattın Adı", routeTableHeaders);
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
                && string.Equals((string?)element.Attribute("Header"), "Taşıma Fiyat Farkı KDV Hariç", StringComparison.Ordinal)
                && ((string?)element.Attribute("Binding"))?.Contains("Mode=TwoWay", StringComparison.Ordinal) == true);

        var columns = named["MultiRoutePaymentGrid"].Descendants()
            .Where(element => element.Name.LocalName == "DataGridTextColumn")
            .ToDictionary(element => (string?)element.Attribute("Header") ?? string.Empty,
                StringComparer.Ordinal);
        foreach (var editableHeader in new[]
        {
            "Taşıma Gün", "Taşıma İskonto", "Taşıma Fiyat Farkı KDV Hariç",
            "Rehber Gün", "Rehber İskonto", "Rehber Personel Fiyat Farkı KDV Hariç",
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

    [Fact]
    public void NewProgressCalculation_ShouldClearDraftAndRetainCompletedRecordActions()
    {
        var source = File.ReadAllText(LoadProjectFile(
            "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));
        var reset = MethodBody(source, "ResetNewProgressDraftAfterCalculation");
        var clear = MethodBody(source, "ClearProgressEntryFields");

        Assert.Contains("ClearProgressEntryFields();", reset, StringComparison.Ordinal);
        Assert.Contains("_lastCalculatedHistoryId = lastCalculated.Id;", reset, StringComparison.Ordinal);
        Assert.True(reset.IndexOf("ClearProgressEntryFields();", StringComparison.Ordinal) <
                    reset.IndexOf("_lastCalculatedHistoryId = lastCalculated.Id;", StringComparison.Ordinal));
        Assert.True(reset.IndexOf("_lastCalculatedHistoryId = lastCalculated.Id;", StringComparison.Ordinal) <
                    reset.IndexOf("SetProgressEditorMode(false);", StringComparison.Ordinal));

        foreach (var clearedState in new[]
        {
            "ProgressTenderListBox.SelectedItem = null;",
            "ProgressContractorComboBox.SelectedItem = null;",
            "ProgressContractorComboBox.ItemsSource = Array.Empty<ContractorDraft>();",
            "ProgressPeriodMonthComboBox.SelectedItem = null;",
            "ProgressPeriodMonthComboBox.ItemsSource = Array.Empty<ProgressPaymentMonthOption>();",
            "ProgressRouteComboBox.SelectedItems.Clear();",
            "_multiRoutePaymentRows.Clear();", "_multiRouteDraftCache.Clear();",
            "ProgressNumberTextBox.Clear();", "textBox.Clear();"
        })
            Assert.Contains(clearedState, clear, StringComparison.Ordinal);

        Assert.True(clear.IndexOf("_suppressSelectionEvents = true;", StringComparison.Ordinal) <
                    clear.IndexOf("ProgressPeriodMonthComboBox.SelectedItem = null;", StringComparison.Ordinal));
        Assert.DoesNotContain("_lastCalculatedHistoryId = null;", clear, StringComparison.Ordinal);
        Assert.Contains("GetLastCalculatedProgress() is not null", MethodBody(source, "SetProgressEditorMode"), StringComparison.Ordinal);
        Assert.Contains("_progressHistory.FirstOrDefault(x => x.Id == id)", MethodBody(source, "GetLastCalculatedProgress"), StringComparison.Ordinal);
        Assert.Contains("GetLastCalculatedProgress()", MethodBody(source, "ArchiveCurrentProgressButton_Click"), StringComparison.Ordinal);
        Assert.Contains("GetLastCalculatedProgress()", MethodBody(source, "CorrectCurrentProgressButton_Click"), StringComparison.Ordinal);
    }

    [Fact]
    public void NewSingleRouteCalculation_ShouldResetOnlyAfterPersistenceAndOutsideCorrections()
    {
        var source = File.ReadAllText(LoadProjectFile(
            "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));
        var calculate = MethodBody(source, "CalculateProgressButton_Click");
        var resetIndex = calculate.IndexOf("ResetNewProgressDraftAfterCalculation(historyItem);", StringComparison.Ordinal);

        Assert.True(resetIndex >= 0, "A successful new single-route payment must reset its draft.");
        var persistenceFailure = BlockAfter(calculate, "if (!SavePersistedData())");
        Assert.Contains("return;", persistenceFailure, StringComparison.Ordinal);
        Assert.DoesNotContain("ResetNewProgressDraftAfterCalculation", persistenceFailure, StringComparison.Ordinal);
        Assert.True(resetIndex > calculate.IndexOf(persistenceFailure, StringComparison.Ordinal) + persistenceFailure.Length);
        Assert.Contains("ResetNewProgressDraftAfterCalculation(historyItem);",
            BlockAfter(calculate, "if (!editingHistoryId.HasValue)"), StringComparison.Ordinal);

        var archiveSave = calculate.IndexOf("SaveArchiveCorrectionSnapshot(", StringComparison.Ordinal);
        Assert.True(archiveSave >= 0 && archiveSave < resetIndex);
        Assert.True(calculate.IndexOf("return;", archiveSave, StringComparison.Ordinal) < resetIndex);
        Assert.DoesNotContain("ResetNewProgressDraftAfterCalculation", MethodBody(source, "SaveArchiveCorrectionSnapshot"), StringComparison.Ordinal);
    }

    [Fact]
    public void NewMultiRouteCalculation_ShouldResetOnlyAfterPersistenceAndKeepActionsVisible()
    {
        var source = File.ReadAllText(LoadProjectFile(
            "src", "OzelEgitimHakedis.Wpf", "MainWindow.xaml.cs"));
        var save = MethodBody(source, "SaveMultiRouteProgressPayment");
        var resetIndex = save.IndexOf("ResetNewProgressDraftAfterCalculation(batch[^1]);", StringComparison.Ordinal);

        Assert.True(resetIndex >= 0, "A successful new multi-route batch must reset its draft.");
        var persistenceFailure = BlockAfter(save, "if (!SavePersistedData())");
        Assert.Contains("return;", persistenceFailure, StringComparison.Ordinal);
        Assert.DoesNotContain("ResetNewProgressDraftAfterCalculation", persistenceFailure, StringComparison.Ordinal);
        Assert.True(resetIndex > save.IndexOf(persistenceFailure, StringComparison.Ordinal) + persistenceFailure.Length);
        Assert.DoesNotContain("CorrectCurrentProgressButton.Visibility = Visibility.Collapsed;", save[resetIndex..], StringComparison.Ordinal);
        Assert.DoesNotContain("ArchiveCurrentProgressButton.Visibility = Visibility.Collapsed;", save[resetIndex..], StringComparison.Ordinal);
        Assert.Equal(2, Regex.Matches(source, @"ResetNewProgressDraftAfterCalculation\((?:historyItem|batch\[\^1\])\);").Count);
    }

    private static string BlockAfter(string source, string marker)
    {
        var markerIndex = source.IndexOf(marker, StringComparison.Ordinal);
        Assert.True(markerIndex >= 0, $"Expected source to contain {marker}.");
        var bodyStart = source.IndexOf('{', markerIndex);
        var depth = 0;
        for (var index = bodyStart; index < source.Length; index++)
        {
            if (source[index] == '{')
                depth++;
            else if (source[index] == '}' && --depth == 0)
                return source[(bodyStart + 1)..index];
        }

        throw new InvalidOperationException($"Expected {marker} to have a closing brace.");
    }

    private static string EventName(XElement element, string eventAttribute) =>
        (string?)element.Attribute(eventAttribute) ?? string.Empty;

    private static int IndexOfNamedElement(XElement[] elements, string name)
    {
        var index = Array.FindIndex(elements,
            element => (string?)element.Attribute(XamlNamespace + "Name") == name);
        Assert.True(index >= 0, $"Expected ProgressPanel to contain '{name}'.");
        return index;
    }

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
