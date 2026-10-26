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
    [InlineData("program")]
    [InlineData("missing")]
    [InlineData("date")]
    public void RouteSave_RejectsMismatchedProgramMissingEditAndInvalidDate(string scenario)
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-route-validation-");
            try
            {
                var window = PrepareRoutePanelSave(temp.FullName);
                if (scenario == "program")
                {
                    var wrong = CreatePanelFirm("Yanlış program", "98765432109", TransportationProgramType.BasicOrSecondary, "", false);
                    Collection(window, "_contractors").Add(wrong);
                    var selector = Field<ComboBox>(window, "RouteContractorListBox");
                    selector.ItemsSource = Collection(window, "_contractors"); selector.SelectedItem = wrong;
                }
                else if (scenario == "missing") SetField(window, "_editingRoute", CreateRoute("99", "Silinmiş hat", "12345678901"));
                else Field<DatePicker>(window, "RouteContractDatePicker").Text = "31.02.2026";
                Invoke(window, "AddRouteButton_Click", window, new RoutedEventArgs());
                Assert.Single(Collection(window, "_routes"));
                Assert.Equal(Visibility.Visible, Field<Border>(window, "RouteValidationBanner").Visibility);
            }
            finally { temp.Delete(true); }
        });
    }

    [Theory]
    [InlineData("EditRouteButton_Click")]
    [InlineData("RemoveRouteButton_Click")]
    public void RouteListEdit_CannotDiscardActiveArchiveCorrection(string handler)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            SetField(window, "_editingArchiveSourcePath", "unchanged.json"); SetField(window, "_editingArchiveSnapshot", archive);
            var route = CreateRoute("01", "Hat", "12345678901"); Collection(window, "_routes").Add(route);
            Invoke(window, handler, new Button { Tag = route }, new RoutedEventArgs());
            Assert.Same(archive, Field<ProgressPaymentArchive>(window, "_editingArchiveSnapshot"));
            Assert.Equal(Visibility.Visible, Field<Border>(window, "RouteValidationBanner").Visibility);
        });
    }

    [Fact]
    public void RouteCancel_ClearsUnsavedInputsAndKeepsStoredRoute()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var route = CreateRoute("01", "Asıl hat", "12345678901"); Collection(window, "_routes").Add(route);
            Invoke(window, "EditRouteButton_Click", new Button { Tag = route }, new RoutedEventArgs());
            Field<TextBox>(window, "RouteNameTextBox").Text = "Kaydedilmemiş";
            Invoke(window, "CancelRouteEditButton_Click", window, new RoutedEventArgs());
            Assert.Empty(Field<TextBox>(window, "RouteNameTextBox").Text);
            Assert.Same(route, Assert.Single(Collection(window, "_routes").Cast<object>()));
            Assert.Equal(Visibility.Collapsed, Field<Button>(window, "CancelRouteEditButton").Visibility);
        });
    }

    private static MainWindow PrepareRoutePanelSave(string root)
    {
        var window = CreateHybridUpdateHarness(root);
        var progressTender = Field<ComboBox>(window, "ProgressTenderListBox"); progressTender.Items.Clear(); progressTender.ItemsSource = Collection(window, "_contracts");
        var progressRoutes = Field<ListBox>(window, "ProgressRouteComboBox"); progressRoutes.Items.Clear(); progressRoutes.ItemsSource = Collection(window, "_routes");
        Field<ComboBox>(window, "ProgressPeriodMonthComboBox").Items.Clear();
        var tender = Field<ComboBox>(window, "RouteTenderListBox"); tender.ItemsSource = Collection(window, "_contracts"); tender.SelectedIndex = 0;
        var firms = Field<ComboBox>(window, "RouteContractorListBox"); firms.ItemsSource = Collection(window, "_contractors"); firms.SelectedIndex = 0;
        Field<TextBox>(window, "RouteNumberTextBox").Text = "02";
        Field<TextBox>(window, "RouteNameTextBox").Text = "Kuzey İlkokulu";
        Field<TextBox>(window, "RouteDailyPriceTextBox").Text = "2.446,9323";
        Field<TextBox>(window, "RouteContractAmountTextBox").Text = "100.000,00";
        Field<TextBox>(window, "RouteTransportWorkDaysTextBox").Text = "20,5";
        Field<TextBox>(window, "RouteGuideDailyPriceTextBox").Text = "1.362,49";
        var date = Field<DatePicker>(window, "RouteContractDatePicker");
        date.DateValidationError += (sender, e) => Invoke(window, "RouteDate_InvalidInput", sender!, e);
        date.SelectedDateChanged += (sender, e) => Invoke(window, "TenderDate_Changed", sender!, e);
        date.SelectedDate = new DateTime(2026, 1, 1);
        return window;
    }

    [Theory]
    [InlineData(1024)]
    [InlineData(1366)]
    [InlineData(1920)]
    public void RoutePanel_FieldsAndLongRecordFitSupportedWidths(double width)
    {
        RunOnSta(() =>
        {
            var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "LinesPanel")); layout.Background = Brushes.White;
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract;
            var tender = (ComboBox)layout.FindName("RouteTenderListBox"); tender.Items.Add(contract); tender.SelectedIndex = 0;
            var firms = (ComboBox)layout.FindName("RouteContractorListBox"); firms.Items.Add(CreateFirm("Örnek Taşımacılık Limited Şirketi", "12345678901")); firms.SelectedIndex = 0;
            ((TextBox)layout.FindName("RouteNumberTextBox")).Text = "01";
            ((TextBox)layout.FindName("RouteNameTextBox")).Text = "Merkez ve çevre köylerden Atatürk İlkokuluna taşıma hattı";
            ((TextBox)layout.FindName("RouteDailyPriceTextBox")).Text = "2.446,9323";
            ((TextBox)layout.FindName("RouteGuideDailyPriceTextBox")).Text = "1.362,49";
            ((TextBox)layout.FindName("RouteTransportWorkDaysTextBox")).Text = "20,5";
            ((TextBox)layout.FindName("RouteContractAmountTextBox")).Text = "100.000,00";
            ((DatePicker)layout.FindName("RouteContractDatePicker")).SelectedDate = new DateTime(2026, 1, 1);
            var list = (ItemsControl)layout.FindName("RoutesListBox"); list.Items.Add(CreateRoute("01", "Merkez ve çevre köylerden Atatürk İlkokuluna öğrenci taşıma hizmeti", "12345678901", dailyPrice: 2446.9323m, guidePrice: 1362.49m));
            ((TextBlock)layout.FindName("RouteCountTextBlock")).Text = "1 / 1 hat";
            ((Border)layout.FindName("RouteListEmptyBorder")).Visibility = Visibility.Collapsed;
            ArrangeLayout(layout, width - 253, 1000);
            var price = (TextBox)layout.FindName("RouteDailyPriceTextBox"); var date = (DatePicker)layout.FindName("RouteContractDatePicker");
            Assert.True(price.ActualWidth >= 200); Assert.True(date.ActualWidth >= 200);
            Assert.Equal(price.ActualHeight, date.ActualHeight);
            Assert.Contains(VisualDescendants<TextBlock>(list), text => text.Text.Contains("2.446,9323", StringComparison.Ordinal));
            foreach (var button in VisualDescendants<Button>(list))
            {
                var bounds = button.TransformToAncestor(layout).TransformBounds(new Rect(button.RenderSize));
                Assert.InRange(bounds.Right, 1, layout.ActualWidth);
            }
            SavePaymentDetailsPreview(layout, "route-" + width.ToString("0", CultureInfo.InvariantCulture) + ".png");
        });
    }

    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation, "tr-TR")]
    [InlineData(TransportationProgramType.BasicOrSecondary, "tr-TR")]
    [InlineData(TransportationProgramType.SpecialEducation, "en-US")]
    [InlineData(TransportationProgramType.BasicOrSecondary, "en-US")]
    public void RouteSave_PreservesFourDecimalPricesAndSelectedProgram(TransportationProgramType program, string culture)
    {
        RunOnSta(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var temp = Directory.CreateTempSubdirectory("oehs-route-program-");
            try
            {
                var window = PrepareRoutePanelSave(temp.FullName);
                var contracts = Collection(window, "_contracts"); contracts[0] = ((ContractDraftData)contracts[0]!) with { ProgramType = program };
                var firms = Collection(window, "_contractors"); firms[0] = CreatePanelFirm("Firma", "12345678901", program, "", false);
                Field<ComboBox>(window, "RouteTenderListBox").SelectedIndex = 0; Field<ComboBox>(window, "RouteContractorListBox").SelectedIndex = 0;
                Invoke(window, "AddRouteButton_Click", window, new RoutedEventArgs());
                var added = Assert.Single(new OehsProgramScopedApplicationDataStore(temp.FullName).Load().Routes, route => route.RouteNumber == "02");
                Assert.Equal(program, added.ProgramType); Assert.Equal(2446.9323m, added.DailyPrice); Assert.Equal(20.5m, added.TransportWorkDays);
                Assert.Equal(program == TransportationProgramType.SpecialEducation ? 1362.49m : 0m, added.GuideDailyPrice);
                Assert.Equal(100000m, added.ContractAmount); Assert.Equal(new DateOnly(2026, 1, 1), added.ContractDate);
            }
            finally { temp.Delete(true); }
        });
    }

    [Theory]
    [InlineData("tr-TR")]
    [InlineData("en-US")]
    public void RouteSearch_UsesTurkishTextAndKeepsUnselectedFirmRoutesVisible(string culture)
    {
        RunOnSta(() =>
        {
            CultureInfo.CurrentCulture = CultureInfo.GetCultureInfo(culture);
            var window = CreateEditorHarness(); var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract;
            Field<ComboBox>(window, "RouteTenderListBox").Items.Add(contract); Field<ComboBox>(window, "RouteTenderListBox").SelectedItem = contract;
            var first = CreateRoute("01", "İzmir İlkokulu", "12345678901"); var second = CreateRoute("02", "Ankara", "98765432109");
            Collection(window, "_routes").Add(first); Collection(window, "_routes").Add(second);
            Field<TextBox>(window, "RouteNameTextBox").Text = "Kaydedilmemiş taslak";
            Invoke(window, "UpdateRouteListView"); Assert.Equal(2, Field<ItemsControl>(window, "RoutesListBox").Items.Count);
            Field<TextBox>(window, "RouteSearchTextBox").Text = "izmir"; Invoke(window, "UpdateRouteListView");
            Assert.Same(first, Assert.Single(Field<ItemsControl>(window, "RoutesListBox").Items.Cast<object>()));
            var firms = Field<ComboBox>(window, "RouteContractorListBox"); var firm = CreateFirm("İkinci firma", "98765432109"); firms.Items.Add(firm); firms.SelectedItem = firm;
            Invoke(window, "UpdateRouteListView"); Assert.Empty(Field<ItemsControl>(window, "RoutesListBox").Items.Cast<object>());
            Field<TextBox>(window, "RouteSearchTextBox").Clear(); Invoke(window, "UpdateRouteListView");
            Assert.Same(second, Assert.Single(Field<ItemsControl>(window, "RoutesListBox").Items.Cast<object>()));
            Assert.Equal("Kaydedilmemiş taslak", Field<TextBox>(window, "RouteNameTextBox").Text);
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteDelete_UpdatesPersistenceAndDraftsOrRollsBackTogether(bool failPersistence)
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-route-delete-");
            try
            {
                var window = PrepareRoutePanelSave(temp.FullName); var route = Collection(window, "_routes")[0]!;
                var key = Assert.IsType<string>(Invoke(window, "HybridRouteKey", route));
                var cache = Field<RouteDraftCache<MainWindow.MultiRoutePaymentDraftRow>>(window, "_multiRouteDraftCache");
                Assert.True(cache.ContainsKey(key)); SetField(window, "_editingRoute", route); SetField(window, "_dataLoadFailed", failPersistence);
                Invoke(window, "RemoveRouteAfterConfirmation", route);
                Assert.Equal(failPersistence ? 1 : 0, Collection(window, "_routes").Count);
                Assert.Equal(failPersistence, cache.ContainsKey(key));
                if (failPersistence) Assert.Equal(Visibility.Visible, Field<Border>(window, "RouteValidationBanner").Visibility);
                else
                {
                    Assert.Empty(new OehsProgramScopedApplicationDataStore(temp.FullName).Load().Routes);
                    Assert.Empty(Field<TextBox>(window, "RouteNameTextBox").Text);
                }
            }
            finally { temp.Delete(true); }
        });
    }

    [Fact]
    public void RouteHistoryReferences_KeepTheTwoProgramsSeparate()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); var sample = ArchiveProgressEditorContextTests.CreateArchive().ProgressPayment;
            var route = CreateRoute("01", "Hat", "12345678901", TransportationProgramType.BasicOrSecondary);
            sample = sample with { TenderNumber = "IHALE-01", ContractNumber = "SOZ-01", RouteNumber = "01", ContractorTaxOrIdentityNumber = "12345678901" };
            Collection(window, "_progressHistory").Add(sample);
            Assert.Equal(0, Invoke(window, "CountRouteHistoryReferences", route));
            Collection(window, "_progressHistory").Add(sample with { Input = sample.Input with { ProgramType = TransportationProgramType.BasicOrSecondary } });
            Assert.Equal(1, Invoke(window, "CountRouteHistoryReferences", route));
        });
    }

    [Theory]
    [InlineData(false)]
    [InlineData(true)]
    public void RouteSave_RejectsDuplicateOrDifferentOwnerForSameRoute(bool differentOwner)
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-route-owner-");
            try
            {
                var window = PrepareRoutePanelSave(temp.FullName); Field<TextBox>(window, "RouteNumberTextBox").Text = "01";
                if (differentOwner)
                {
                    var firm = CreateFirm("İkinci firma", "98765432109"); Collection(window, "_contractors").Add(firm);
                    Field<ComboBox>(window, "RouteContractorListBox").SelectedItem = firm;
                }
                Invoke(window, "AddRouteButton_Click", window, new RoutedEventArgs());
                Assert.Single(Collection(window, "_routes"));
                Assert.Equal(Visibility.Visible, Field<Border>(window, "RouteValidationBanner").Visibility);
            }
            finally { temp.Delete(true); }
        });
    }

    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation, Visibility.Visible)]
    [InlineData(TransportationProgramType.BasicOrSecondary, Visibility.Collapsed)]
    public void RouteEdit_ShowsGuideFieldsForOwnProgram(TransportationProgramType program, Visibility expected)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract with { ProgramType = program };
            Collection(window, "_contracts").Add(contract); Field<ComboBox>(window, "RouteTenderListBox").Items.Add(contract);
            Collection(window, "_contractors").Add(CreatePanelFirm("Firma", "12345678901", program, "", false));
            var route = CreateRoute("01", "Hat", "12345678901", program, guidePrice: 123.4567m); Collection(window, "_routes").Add(route);
            Invoke(window, "EditRouteButton_Click", new Button { Tag = route }, new RoutedEventArgs());
            Assert.Equal(expected, Field<StackPanel>(window, "RouteGuideFieldPanel").Visibility);
            Assert.Equal(program == TransportationProgramType.SpecialEducation ? 1 : 2, Grid.GetColumnSpan(Field<StackPanel>(window, "RouteWorkDaysField")));
        });
    }

    [Fact]
    public void RouteDate_CorrectingRejectedDateAllowsSave()
    {
        RunOnSta(() =>
        {
            var temp = Directory.CreateTempSubdirectory("oehs-route-date-");
            try
            {
                var window = PrepareRoutePanelSave(temp.FullName); var date = Field<DatePicker>(window, "RouteContractDatePicker");
                date.Text = "31.02.2026"; date.SelectedDate = new DateTime(2026, 2, 28);
                Invoke(window, "AddRouteButton_Click", window, new RoutedEventArgs());
                Assert.Equal(2, Collection(window, "_routes").Count);
                Assert.Equal(new DateOnly(2026, 2, 28), Assert.Single(new OehsProgramScopedApplicationDataStore(temp.FullName).Load().Routes, route => route.RouteNumber == "02").ContractDate);
            }
            finally { temp.Delete(true); }
        });
    }

    [Fact]
    public void RouteTenderSelector_KeepsBothProgramsAccessible()
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness(); var special = ArchiveProgressEditorContextTests.CreateArchive().Contract;
            Collection(window, "_contracts").Add(special); Collection(window, "_contracts").Add(special with { ProgramType = TransportationProgramType.BasicOrSecondary });
            var global = Field<ComboBox>(window, "TransportProgramTypeComboBox"); global.Items.Add(new ComboBoxItem { Tag = "SpecialEducation" }); global.SelectedIndex = 0;
            Invoke(window, "UpdateContractSelectors"); Assert.Equal(2, Field<ComboBox>(window, "RouteTenderListBox").Items.Count);
        });
    }

    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation, Visibility.Visible)]
    [InlineData(TransportationProgramType.BasicOrSecondary, Visibility.Collapsed)]
    public void ArchivedRouteEdit_ShowsGuideFieldsFromArchivedProgram(TransportationProgramType program, Visibility expected)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var archive = ArchiveProgressEditorContextTests.CreateArchive();
            archive = archive with { Contract = archive.Contract with { ProgramType = program } };
            Field<Button>(window, "NavLines").Tag = "Lines";
            SetField(window, "_editingArchiveSourcePath", "unused.json"); SetField(window, "_editingArchiveSnapshot", archive);
            Field<StackPanel>(window, "RouteGuideFieldPanel").Visibility = expected == Visibility.Visible ? Visibility.Collapsed : Visibility.Visible;
            Invoke(window, "BeginRouteHistoryEdit", archive.ProgressPayment);
            Assert.Equal(expected, Field<StackPanel>(window, "RouteGuideFieldPanel").Visibility);
        });
    }
}
