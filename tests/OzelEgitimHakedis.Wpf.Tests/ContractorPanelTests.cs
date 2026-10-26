using System;
using System.IO;
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
    [Theory]
    [InlineData(false, false)]
    [InlineData(false, true)]
    [InlineData(true, false)]
    [InlineData(true, true)]
    public void ContractorHistoryEdit_LoadsOwnVatStateAndProgramQualifiedKey(bool archived, bool exempt)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var sample = ArchiveProgressEditorContextTests.CreateArchive();
            sample = sample with { ProgressPayment = sample.ProgressPayment with { ContractorTaxOrIdentityNumber = sample.Contractor.TaxOrIdentityNumber } };
            var firm = CreateFirm("Firma", sample.Contractor.TaxOrIdentityNumber, exempt);
            Field<Button>(window, "NavContractors").Tag = "Contractors";
            Collection(window, "_contractors").Add(firm);
            Field<CheckBox>(window, "ContractorVatExemptCheckBox").IsChecked = !exempt;
            if (archived)
            {
                SetField(window, "_editingArchiveSourcePath", "unused-archive.json");
                SetField(window, "_editingArchiveSnapshot", sample with { Contractor = sample.Contractor with { VatExempt = exempt } });
            }
            Invoke(window, "BeginContractorHistoryEdit", sample.ProgressPayment);
            Assert.Equal(exempt, Field<CheckBox>(window, "ContractorVatExemptCheckBox").IsChecked);
            if (!archived)
                Assert.Equal(Invoke(window, "BuildContractorKey", firm), typeof(MainWindow).GetField("_editingContractorKey", System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.NonPublic)!.GetValue(window));
        });
    }

    [Fact]
    public void ContractorCancel_ReturnsToCreateModeWithoutChangingStoredFirm()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var firm = CreateFirm("Asıl firma", "12345678901");
            Collection(window, "_contractors").Add(firm);
            Invoke(window, "EditContractorButton_Click", new Button { Tag = firm }, new RoutedEventArgs());
            Field<TextBox>(window, "ContractorNameTextBox").Text = "Kaydedilmeyen değişiklik";
            Invoke(window, "CancelContractorEditButton_Click", window, new RoutedEventArgs());
            Assert.Equal(Visibility.Collapsed, Field<Button>(window, "CancelContractorEditButton").Visibility);
            Assert.True(Field<Button>(window, "ClearContractorButton").IsEnabled);
            Assert.Equal("Asıl firma", firm.GetType().GetProperty("Name")!.GetValue(firm));
            Assert.Empty(Field<TextBox>(window, "ContractorNameTextBox").Text);
            Assert.False(Field<CheckBox>(window, "ContractorVatExemptCheckBox").IsChecked);
        });
    }

    [Fact]
    public void ContractorSave_MissingEditedRecordMustNotCreateAnotherFirm()
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-firm-missing-");
            try
            {
                var window = CreateHybridUpdateHarness(temp.FullName);
                PrepareContractorSaveSelectors(window);
                Collection(window, "_contractors").Clear();
                SetField(window, "_editingContractorKey", "SpecialEducation|MISSING");
                Field<TextBox>(window, "ContractorNameTextBox").Text = "Yeni kayıt olmamalı";
                Field<TextBox>(window, "ContractorTaxTextBox").Text = "12345678901";
                Invoke(window, "SaveContractorButton_Click", window, new RoutedEventArgs());
                Assert.Empty(Collection(window, "_contractors"));
                Assert.Contains("bulunamadı", Field<TextBlock>(window, "ContractorStatusTextBlock").Text, StringComparison.OrdinalIgnoreCase);
            }
            finally { temp.Delete(true); }
        });
    }

    private static void PrepareContractorSaveSelectors(MainWindow window)
    {
        Field<ComboBox>(window, "ProgressTenderListBox").Items.Clear();
        Field<ListBox>(window, "ProgressRouteComboBox").Items.Clear();
        Field<ComboBox>(window, "ProgressPeriodMonthComboBox").Items.Clear();
        var program = Field<ComboBox>(window, "TransportProgramTypeComboBox");
        program.Items.Clear(); program.Items.Add(new ComboBoxItem { Tag = "SpecialEducation" }); program.SelectedIndex = 0;
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(1366)]
    [InlineData(1920)]
    public void ContractorPanel_InputsAndLongRecordTextFitSupportedWidths(double width)
    {
        RunOnSta(() =>
        {
            var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "ContractorsPanel"));
            layout.Background = Brushes.White;
            ((ComboBox)layout.FindName("ContractorProgramComboBox")).SelectedIndex = 0;
            var longName = "Örnek Öğrenci Taşımacılığı Turizm Seyahat Organizasyon Otomotiv ve Ticaret Limited Şirketi Merkez Şubesi";
            var firm = CreatePanelFirm(longName, "12345678901", TransportationProgramType.SpecialEducation, "TR12 3456 7890 1234 5678 9012 34", true);
            ((TextBox)layout.FindName("ContractorNameTextBox")).Text = longName;
            ((TextBox)layout.FindName("ContractorTaxTextBox")).Text = "12345678901";
            ((TextBox)layout.FindName("ContractorBankTextBox")).Text = "TR12 3456 7890 1234 5678 9012 34";
            var list = (ItemsControl)layout.FindName("ContractorsListBox");
            list.Items.Add(firm);
            ((TextBlock)layout.FindName("ContractorCountTextBlock")).Text = "1 / 1 firma";
            ((Border)layout.FindName("ContractorListEmptyBorder")).Visibility = Visibility.Collapsed;
            ArrangeLayout(layout, width - 253, 850);
            var identity = (TextBox)layout.FindName("ContractorTaxTextBox");
            var bank = (TextBox)layout.FindName("ContractorBankTextBox");
            Assert.True(identity.ActualWidth >= 200);
            Assert.True(bank.ActualWidth >= 400);
            Assert.Equal(identity.ActualHeight, bank.ActualHeight);
            Assert.Equal(identity.ActualHeight, ((TextBox)layout.FindName("ContractorNameTextBox")).ActualHeight);
            var title = Assert.Single(VisualDescendants<TextBlock>(list), text => text.Text == longName);
            Assert.Equal(TextWrapping.Wrap, title.TextWrapping);
            Assert.True(title.ActualHeight > 0);
            if (width == 1024) Assert.True(title.ActualHeight >= 30, "Long company names should wrap at the narrowest supported width.");
            SavePaymentDetailsPreview(layout, "contractor-" + width.ToString("0", CultureInfo.InvariantCulture) + ".png");
        });
    }

    [Fact]
    public void ContractorSearch_FiltersNameIdentityBankAndProgramWithoutChangingDraft()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var first = CreatePanelFirm("İzmir Taşıma", "12345678901", TransportationProgramType.SpecialEducation, "TR12 3456", false);
            var basic = CreatePanelFirm("Ankara Taşıma", "12345678901", TransportationProgramType.BasicOrSecondary, "TR98 7654", true);
            var firms = Collection(window, "_contractors"); firms.Add(first); firms.Add(basic);
            Field<TextBox>(window, "ContractorNameTextBox").Text = "Kaydedilmemiş taslak";
            Invoke(window, "RefreshContractorList");
            Assert.Equal(2, Field<ItemsControl>(window, "ContractorsListBox").Items.Count);
            Field<TextBox>(window, "ContractorSearchTextBox").Text = "izmir"; Invoke(window, "RefreshContractorList");
            Assert.Same(first, Assert.Single(Field<ItemsControl>(window, "ContractorsListBox").Items.Cast<object>()));
            Field<TextBox>(window, "ContractorSearchTextBox").Text = "tr 12 34"; Invoke(window, "RefreshContractorList");
            Assert.Same(first, Assert.Single(Field<ItemsControl>(window, "ContractorsListBox").Items.Cast<object>()));
            Field<TextBox>(window, "ContractorSearchTextBox").Text = "123 456 789 01";
            var program = Field<ComboBox>(window, "TransportProgramTypeComboBox");
            program.Items.Add(new ComboBoxItem { Tag = "BasicOrSecondary" }); program.SelectedIndex = 0;
            Invoke(window, "RefreshContractorList");
            Assert.Same(basic, Assert.Single(Field<ItemsControl>(window, "ContractorsListBox").Items.Cast<object>()));
            Field<TextBox>(window, "ContractorSearchTextBox").Text = "bulunmayan"; Invoke(window, "RefreshContractorList");
            Assert.Empty(Field<ItemsControl>(window, "ContractorsListBox").Items.Cast<object>());
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractorListEmptyBorder").Visibility);
            Assert.Equal("Kaydedilmemiş taslak", Field<TextBox>(window, "ContractorNameTextBox").Text);
            Assert.Equal(2, firms.Count);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void ContractorSave_PersistsNewFirmVatAndAllowsSameIdentityInOtherProgram(bool exempt)
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-firm-new-");
            try
            {
                var window = CreateHybridUpdateHarness(temp.FullName);
                PrepareContractorSaveSelectors(window);
                var firms = Collection(window, "_contractors"); firms.Clear();
                firms.Add(CreatePanelFirm("Diğer program", "12345678901", TransportationProgramType.BasicOrSecondary, "", !exempt));
                Field<TextBox>(window, "ContractorNameTextBox").Text = " Yeni firma ";
                Field<TextBox>(window, "ContractorTaxTextBox").Text = "123 456 789 01";
                Field<TextBox>(window, "ContractorBankTextBox").Text = "  TR12 3456  ";
                Field<CheckBox>(window, "ContractorVatExemptCheckBox").IsChecked = exempt;
                Invoke(window, "SaveContractorButton_Click", window, new RoutedEventArgs());
                var saved = new OehsProgramScopedApplicationDataStore(temp.FullName).Load().Contractors!;
                Assert.Equal(2, saved.Count);
                var added = Assert.Single(saved, firm => firm.ProgramType == TransportationProgramType.SpecialEducation);
                Assert.Equal("Yeni firma", added.Name); Assert.Equal("12345678901", added.TaxOrIdentityNumber);
                Assert.Equal("TR12 3456", added.BankAccountNumber); Assert.Equal(exempt, added.VatExempt);
                Assert.Equal(!exempt, Assert.Single(saved, firm => firm.ProgramType == TransportationProgramType.BasicOrSecondary).VatExempt);
            }
            finally { temp.Delete(true); }
        });
    }

    [Fact]
    public void ContractorSave_DuplicateNormalizedIdentityShowsInlineError()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); PrepareContractorSaveSelectors(window);
            var firm = CreateFirm("Mevcut firma", "12345678901"); Collection(window, "_contractors").Add(firm);
            Field<TextBox>(window, "ContractorNameTextBox").Text = "Farklı unvan";
            Field<TextBox>(window, "ContractorTaxTextBox").Text = "123 456 789 01";
            Invoke(window, "SaveContractorButton_Click", window, new RoutedEventArgs());
            Assert.Same(firm, Assert.Single(Collection(window, "_contractors").Cast<object>()));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractorValidationBanner").Visibility);
            Assert.Contains("Mevcut firma", Field<TextBlock>(window, "ContractorValidationMessageTextBlock").Text, StringComparison.Ordinal);
        });
    }

    [Theory]
    [InlineData("", "12345678901", true)]
    [InlineData("Firma", "---", true)]
    [InlineData("Firma", "12345678901", false)]
    public void ContractorSave_MissingRequiredFieldsShowInlineErrors(string name, string identity, bool selectedProgram)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            if (selectedProgram) PrepareContractorSaveSelectors(window);
            Field<TextBox>(window, "ContractorNameTextBox").Text = name;
            Field<TextBox>(window, "ContractorTaxTextBox").Text = identity;
            Invoke(window, "SaveContractorButton_Click", window, new RoutedEventArgs());
            Assert.Empty(Collection(window, "_contractors"));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractorValidationBanner").Visibility);
        });
    }

    [Theory]
    [InlineData("EditContractorButton_Click")]
    [InlineData("DeleteContractorButton_Click")]
    public void ContractorLiveActions_CannotDiscardActiveArchiveCorrection(string handler)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            SetField(window, "_editingArchiveSourcePath", "unchanged.json"); SetField(window, "_editingArchiveSnapshot", archive);
            var firm = CreateFirm("Firma", "12345678901"); Collection(window, "_contractors").Add(firm);
            Invoke(window, handler, new Button { Tag = firm }, new RoutedEventArgs());
            Assert.Same(archive, Field<ProgressPaymentArchive>(window, "_editingArchiveSnapshot"));
            Assert.Same(firm, Assert.Single(Collection(window, "_contractors").Cast<object>()));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "ContractorValidationBanner").Visibility);
        });
    }

    [Fact]
    public void ContractorDelete_UsedFirmIsKeptWithInlineExplanation()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var firm = CreateFirm("Firma", "12345678901"); Collection(window, "_contractors").Add(firm);
            Collection(window, "_routes").Add(CreateRoute("01", "Hat", "12345678901"));
            Invoke(window, "DeleteContractorButton_Click", new Button { Tag = firm }, new RoutedEventArgs());
            Assert.Same(firm, Assert.Single(Collection(window, "_contractors").Cast<object>()));
            Assert.Contains("silinmedi", Field<TextBlock>(window, "ContractorValidationMessageTextBlock").Text, StringComparison.Ordinal);
        });
    }

    private static object CreatePanelFirm(string name, string identity, TransportationProgramType program, string bank, bool exempt) =>
        Activator.CreateInstance(typeof(MainWindow).GetNestedType("ContractorDraft", System.Reflection.BindingFlags.NonPublic)!,
            name, identity, "", "", "", bank, "", program, exempt)!;
}
