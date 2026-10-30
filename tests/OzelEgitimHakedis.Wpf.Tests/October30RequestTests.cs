using System.IO;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation, "Özel Eğitim Taşıma")]
    [InlineData(TransportationProgramType.BasicOrSecondary, "Temel Eğitim/Ortaöğretim Taşıma")]
    public void TenderSelectors_ShouldShowOnlyIknAndTransportProgram(TransportationProgramType program, string label)
    {
        RunOnSta(() =>
        {
            var contract = ArchiveProgressEditorContextTests.CreateArchive().Contract with
            { ProgramType = program, TenderNumber = "2026/1528595", WorkName = "GİZLENMESİ GEREKEN UZUN GÜZERGAH AÇIKLAMASI" };
            foreach (var target in new[] { ("HomePanel", "HomeTenderListBox"), ("ProgressPanel", "ProgressTenderListBox") })
            {
                var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), target.Item1));
                var combo = (ComboBox)layout.FindName(target.Item2);
                combo.ItemsSource = new[] { contract }; combo.SelectedIndex = 0;
                ArrangeLayout(layout, 1100, 800);
                var texts = VisualDescendants<TextBlock>(combo).Select(b => b.Text).ToArray();
                Assert.Contains("2026/1528595", texts);
                Assert.Contains(label, texts);
                Assert.DoesNotContain(texts, text => text.Contains("GİZLENMESİ", StringComparison.Ordinal));
                SavePaymentDetailsPreview(layout, $"ikn-{program}-{target.Item1}.png");
            }
        });
    }

    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation)]
    [InlineData(TransportationProgramType.BasicOrSecondary)]
    public void PeriodDays_ShouldComeOnlyFromHybridEntry(TransportationProgramType program)
    {
        RunOnSta(() =>
        {
            var window = CreateEditorHarness();
            var route = CreateRoute("01", "Hat", "12345678901", transportDays: 22m, program: program);
            var list = Field<ListBox>(window, "ProgressRouteComboBox");
            list.Items.Add(route); list.SelectedItems.Add(route);
            Invoke(window, "UpdateHybridRouteDraftRows");
            var rows = Field<System.Collections.ObjectModel.ObservableCollection<MainWindow.MultiRoutePaymentDraftRow>>(window, "_multiRoutePaymentRows");
            var row = Assert.Single(rows);
            Assert.Equal(0m, row.WorkDays);
            row.WorkDays = 17.5m; row.AcceptChanges();
            Invoke(window, "ConfigureHybridRow", row, CreateRoute("01", "Hat", "12345678901", transportDays: 8m, program: program));
            Assert.Equal(17.5m, row.WorkDays);
        });
    }

    [Fact]
    public void Routes_ShouldNotExposePeriodDaysEditor()
    {
        var xaml = LoadMainWindowXaml();
        Assert.DoesNotContain(xaml.Descendants(), e => (string?)e.Attribute(XamlNamespace + "Name") == "RouteWorkDaysField");
        Assert.DoesNotContain(xaml.Descendants(), e => (string?)e.Attribute(XamlNamespace + "Name") == "RouteTransportWorkDaysTextBox");
        Assert.Contains(NamedElement(xaml, "SaveReportsPdfsButton").Descendants(),
            element => ((string?)element.Attribute("Text"))?.Contains("TEK PDF", StringComparison.Ordinal) == true);
    }
}

public sealed partial class OfficialDocumentOutputTests
{
    [Theory]
    [InlineData(TransportationProgramType.SpecialEducation)]
    [InlineData(TransportationProgramType.BasicOrSecondary)]
    public void PaymentDocuments_ShouldSaveFourPagesAsOnePdfWithCurrentEvidence(TransportationProgramType program)
    {
        RunOnSta(() =>
        {
            var payment = CreatePayment(new ProgressPaymentInput(18, 100, 0, 0, 100000, true, 0, 0, 0, ProgramType: program))
                with { TenderNumber = "2026/1592223", ContractorName = "Bayram ERDOĞAN" };
            var context = CreateDocumentContext(payment);
            var folder = Directory.CreateTempSubdirectory("oehs-combined-");
            try
            {
                var path = OehsOutputPathService.BuildPaymentDocumentsPdfFilePath(folder.FullName, payment);
                Assert.Equal("2026-1592223-Bayram ERDOĞAN-Ödemeye Ait Evraklar.pdf", Path.GetFileName(path));
                PdfVectorWriter.Save(OehsOfficialDocumentFactory.BuildCombined(context), path);
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
                Assert.Equal(4, pdf.NumberOfPages);
                var titles = new[] { "HAKEDİŞRAPORU", "HİZMETİŞLERİHAKEDİŞRAPORU", "HİZMETİŞLERİKABULTUTANAĞI", "HAKEDİŞİCMALİ" };
                for (var i = 0; i < titles.Length; i++)
                    Assert.Contains(titles[i], WithoutWhitespace(pdf.GetPage(i + 1).Text), StringComparison.Ordinal);
                OutputEvidenceStore.Save(folder.FullName, payment.Id, "combined-v1", new[] { path }, program);
                Assert.True(OutputEvidenceStore.IsCurrent(folder.FullName, payment.Id, "combined-v1", program));
                var saved = (string)typeof(MainWindow).GetMethod("SaveRouteDocumentsArchiveMetadata",
                    System.Reflection.BindingFlags.Static | System.Reflection.BindingFlags.NonPublic)!.Invoke(null, new object[] { payment, new[] { path } })!;
                Assert.Equal(Path.GetDirectoryName(path), saved);
                var evidenceFolder = Environment.GetEnvironmentVariable("OEHS_COMBINED_QA_OUTPUT");
                if (!string.IsNullOrWhiteSpace(evidenceFolder))
                {
                    Directory.CreateDirectory(evidenceFolder);
                    File.Copy(path, Path.Combine(evidenceFolder, program + ".pdf"), true);
                }
            }
            finally { folder.Delete(true); }
        });
    }
}
