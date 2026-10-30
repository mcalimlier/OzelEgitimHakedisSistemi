using System;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    [Theory]
    [InlineData(1024)]
    [InlineData(1366)]
    [InlineData(1920)]
    public void TenderPanel_InputsMustRemainReadableAtSupportedWidths(double width)
    {
        RunOnSta(() =>
        {
            var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "ContractPanel"));
            layout.Background = Brushes.White;
            var program = (ComboBox)layout.FindName("TransportProgramTypeComboBox");
            program.SelectedIndex = 1;
            ((TextBox)layout.FindName("TenderNumberTextBox")).Text = "2026/1592223";
            ((DatePicker)layout.FindName("StartDatePicker")).SelectedDate = new DateTime(2026, 9, 1);
            ((DatePicker)layout.FindName("EndDatePicker")).SelectedDate = new DateTime(2027, 6, 30);
            var basic = ArchiveProgressEditorContextTests.CreateArchive().Contract with
            { TenderNumber = "2026/1592223", ProgramType = TransportationProgramType.BasicOrSecondary, WorkName = "Merkez ve çevre köylerden ilkokullara öğrenci taşıma hizmeti" };
            var special = basic with { TenderNumber = "2026/1592224", ProgramType = TransportationProgramType.SpecialEducation };
            var items = (ItemsControl)layout.FindName("ContractsListBox");
            items.ItemsSource = new System.Collections.Generic.List<ContractDraftData> { basic, special };
            ((TextBlock)layout.FindName("TenderListCountTextBlock")).Text = "2 / 2 ihale";
            ((Border)layout.FindName("TenderListEmptyBorder")).Visibility = Visibility.Collapsed;
            ArrangeLayout(layout, width - 253, 730);
            var start = (DatePicker)layout.FindName("StartDatePicker");
            var end = (DatePicker)layout.FindName("EndDatePicker");
            Assert.True(start.ActualWidth >= 140, $"Start date is only {start.ActualWidth}px at {width}px");
            Assert.True(end.ActualWidth >= 140, $"End date is only {end.ActualWidth}px at {width}px");
            Assert.Equal(start.ActualHeight, end.ActualHeight);
            Assert.Equal(start.ActualHeight, ((TextBox)layout.FindName("ContractWorkDaysTextBox")).ActualHeight);
            Assert.Contains(VisualDescendants<TextBlock>(items), t => t.Text == "Temel Eğitim/Ortaöğretim Taşıma");
            SavePaymentDetailsPreview(layout, "tender-" + width.ToString("0", CultureInfo.InvariantCulture) + ".png");
        });
    }

    [Theory]
    [InlineData("ContractAmountTextBox", "bozuk")]
    [InlineData("ContractAmountTextBox", "-1")]
    [InlineData("ContractTransportDailyPriceTextBox", "bozuk")]
    [InlineData("ContractTransportDailyPriceTextBox", "-1")]
    public void TenderSave_InvalidLegacyMoneyMustNotBecomeZero(string field, string value)
    {
        RunOnSta(() =>
        {
            var window = CreateContractSaveHarness("2026-10-01", "2026-10-31");
            Field<TextBox>(window, field).Text = value;
            Field<TextBox>(window, "ContractNumberTextBox").TextChanged += (_, _) => throw new DraftPreparationReachedException();
            Assert.Null(Record.Exception(() => Invoke(window, "SaveContractButton_Click", new Button(), new RoutedEventArgs())));
            Assert.Empty(Collection(window, "_contracts"));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractValidationBanner").Visibility);
        });
    }

    [Theory]
    [InlineData("StartDatePicker")]
    [InlineData("EndDatePicker")]
    [InlineData("TenderDatePicker")]
    public void TenderSave_InvalidDateTextMustNotSavePreviousOrEmptyDate(string field)
    {
        RunOnSta(() =>
        {
            var window = CreateContractSaveHarness("2026-10-01", "2026-10-31");
            var picker = Field<DatePicker>(window, field);
            picker.DateValidationError += (sender, e) => Invoke(window, "TenderDate_InvalidInput", sender!, e);
            picker.SelectedDateChanged += (sender, e) => Invoke(window, "TenderDate_Changed", sender!, e);
            picker.Text = "31.02.2026";
            Field<TextBox>(window, "ContractNumberTextBox").TextChanged += (_, _) => throw new DraftPreparationReachedException();
            Assert.Null(Record.Exception(() => Invoke(window, "SaveContractButton_Click", new Button(), new RoutedEventArgs())));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractValidationBanner").Visibility);
        });
    }

    [Fact]
    public void TenderSearch_FiltersWithoutChangingDraftOrStoredCollections()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var first = ArchiveProgressEditorContextTests.CreateArchive().Contract with { TenderNumber = "2026/101", ContractNumber = "SOZ-01", WorkName = "" };
            var second = first with { TenderNumber = "2026/202", ContractNumber = "B" };
            var basic = first with { TenderNumber = "2026/303", ContractNumber = "C", ProgramType = TransportationProgramType.BasicOrSecondary };
            Collection(window, "_contracts").Add(first); Collection(window, "_contracts").Add(second); Collection(window, "_contracts").Add(basic);
            Collection(window, "_routes").Add(CreateRoute("01", "Merkez İlkokulu", "12345678901", tender: "2026/101"));
            Field<TextBox>(window, "TenderNumberTextBox").Text = "KAYDEDİLMEMİŞ-TASLAK";
            Invoke(window, "RefreshTenderList");
            Assert.Equal(3, Field<ItemsControl>(window, "ContractsListBox").Items.Count);
            Field<TextBox>(window, "TenderSearchTextBox").Text = "2026/202"; Invoke(window, "RefreshTenderList");
            Assert.Same(second, Assert.Single(Field<ItemsControl>(window, "ContractsListBox").Items.Cast<object>()));
            Field<TextBox>(window, "TenderSearchTextBox").Text = "Merkez"; Invoke(window, "RefreshTenderList");
            Assert.Same(first, Assert.Single(Field<ItemsControl>(window, "ContractsListBox").Items.Cast<object>()));
            var programs = Field<ComboBox>(window, "TransportProgramTypeComboBox");
            programs.Items.Add(new ComboBoxItem { Tag = "BasicOrSecondary" }); programs.SelectedIndex = 0;
            Invoke(window, "RefreshTenderList");
            Assert.Empty(Field<ItemsControl>(window, "ContractsListBox").Items.Cast<object>());
            Assert.Equal(Visibility.Visible, Field<Border>(window, "TenderListEmptyBorder").Visibility);
            Field<TextBox>(window, "TenderSearchTextBox").Clear(); Invoke(window, "RefreshTenderList");
            Assert.Same(basic, Assert.Single(Field<ItemsControl>(window, "ContractsListBox").Items.Cast<object>()));
            Assert.Equal("KAYDEDİLMEMİŞ-TASLAK", Field<TextBox>(window, "TenderNumberTextBox").Text);
            Assert.Equal(3, Collection(window, "_contracts").Count);
        });
    }

    [Fact]
    public void TenderSave_NewRecordCanLeaveUnknownLegacyAmountsAndDaysEmpty()
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-tender-save-");
            try
            {
                var window = CreateHybridUpdateHarness(temp.FullName);
                var tender = Field<ComboBox>(window, "ProgressTenderListBox"); tender.Items.Clear(); tender.ItemsSource = Collection(window, "_contracts");
                var routes = Field<ListBox>(window, "ProgressRouteComboBox"); routes.Items.Clear(); routes.ItemsSource = Collection(window, "_routes");
                Field<ComboBox>(window, "ProgressPeriodMonthComboBox").Items.Clear();
                var program = Field<ComboBox>(window, "TransportProgramTypeComboBox"); program.Items.Add(new ComboBoxItem { Tag = "SpecialEducation" }); program.SelectedIndex = 0;
                Field<TextBox>(window, "TenderNumberTextBox").Text = "2026/NEW";
                Field<TextBox>(window, "ContractAmountTextBox").Clear(); Field<TextBox>(window, "ContractTransportDailyPriceTextBox").Clear(); Field<TextBox>(window, "ContractWorkDaysTextBox").Clear();
                Field<DatePicker>(window, "StartDatePicker").SelectedDate = new DateTime(2026, 10, 1);
                Field<DatePicker>(window, "EndDatePicker").SelectedDate = new DateTime(2026, 10, 31);
                Invoke(window, "SaveContractButton_Click", window, new RoutedEventArgs());
                var saved = new OehsProgramScopedApplicationDataStore(temp.FullName).Load().Contracts!;
                var added = Assert.Single(saved.Where(c => c.TenderNumber == "2026/NEW"));
                Assert.Equal(0m, added.ContractAmount); Assert.Equal(0m, added.TransportDailyPrice); Assert.Equal(0, added.ContractWorkDays);
            }
            finally { temp.Delete(true); }
        });
    }

    [Fact]
    public void TenderDate_ClearActionAllowsOmittingAnInvalidOptionalDate()
    {
        RunOnSta(() =>
        {
            var window = CreateContractSaveHarness("2026-10-01", "2026-10-31");
            var picker = Field<DatePicker>(window, "TenderDatePicker");
            picker.DateValidationError += (sender, e) => Invoke(window, "TenderDate_InvalidInput", sender!, e);
            picker.Text = "31.02.2026";
            Invoke(window, "ClearTenderDate_Click", window, new RoutedEventArgs());
            Assert.Null(picker.SelectedDate);
            Field<TextBox>(window, "ContractNumberTextBox").TextChanged += (_, _) => throw new DraftPreparationReachedException();
            Assert.Throws<DraftPreparationReachedException>(() => Invoke(window, "SaveContractButton_Click", new Button(), new RoutedEventArgs()));
        });
    }
}
