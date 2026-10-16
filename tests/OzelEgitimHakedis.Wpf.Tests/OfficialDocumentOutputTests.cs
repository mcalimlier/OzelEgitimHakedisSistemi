using System.Runtime.ExceptionServices;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Controls;
using System.Windows.Media;
using System.Linq;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using PdfSharp.Pdf.IO;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class OfficialDocumentOutputTests
{
    [Fact]
    public void SignatorySettingsStore_ShouldCreateEditableDefaults_WhenFileIsMissing()
    {
        var temporaryDirectory = Directory.CreateTempSubdirectory("oehs-document-settings-");
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory.FullName, "official-document-settings.json");
            var settings = OehsOfficialDocumentSettingsStore.Load(settingsPath);

            Assert.True(File.Exists(settingsPath));
            Assert.Equal("Aşkın KESKİNBALTA", settings.PreparerName);
            Assert.Equal("Halil ÖZENÇ", settings.ControllerName);
            Assert.Equal("Özgür BAYSAL", settings.ApproverName);
            Assert.Contains("preparerName", File.ReadAllText(settingsPath));
        }
        finally
        {
            temporaryDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void SignatorySettingsStore_ShouldUseCustomValues_AndFallbackForBlankValues()
    {
        var temporaryDirectory = Directory.CreateTempSubdirectory("oehs-document-settings-");
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory.FullName, "official-document-settings.json");
            File.WriteAllText(settingsPath, """
            {
              "preparerName": "  Örnek Düzenleyen  ",
              "preparerRole": "",
              "controllerName": "Örnek Kontrol",
              "controllerRole": "Kontrol Görevlisi",
              "approverName": "Örnek Onaylayan",
              "approverRole": "Kurum Müdürü"
            }
            """);

            var settings = OehsOfficialDocumentSettingsStore.Load(settingsPath);

            Assert.Equal("Örnek Düzenleyen", settings.PreparerName);
            Assert.Equal("Memur", settings.PreparerRole);
            Assert.Equal("Örnek Kontrol", settings.ControllerName);
            Assert.Equal("Kontrol Görevlisi", settings.ControllerRole);
            Assert.Equal("Örnek Onaylayan", settings.ApproverName);
            Assert.Equal("Kurum Müdürü", settings.ApproverRole);
        }
        finally
        {
            temporaryDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void SignatorySettingsStore_ShouldReportInvalidJson()
    {
        var temporaryDirectory = Directory.CreateTempSubdirectory("oehs-document-settings-");
        try
        {
            var settingsPath = Path.Combine(temporaryDirectory.FullName, "official-document-settings.json");
            File.WriteAllText(settingsPath, "{ invalid json");

            var exception = Assert.Throws<InvalidDataException>(
                () => OehsOfficialDocumentSettingsStore.Load(settingsPath));

            Assert.Contains(settingsPath, exception.Message);
        }
        finally
        {
            temporaryDirectory.Delete(recursive: true);
        }
    }

    [Fact]
    public void FourOfficialTemplates_ShouldExportAsSinglePageA4VectorPdfs()
    {
        RunOnSta(() =>
        {
            var previousInput = new ProgressPaymentInput(
                WorkDays: 1,
                DailyPrice: 100m,
                DiscountAmount: 0m,
                PriceDifference: 20m,
                ContractAmount: 100000m,
                IsVatExempt: true,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                InspectionPenalty: 0m);

            var noDeductions = new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m);
            var previousCalculation = new ProgressPaymentCalculation(
                ServiceAmount: 100m,
                PriceDifferenceExcludingVat: 20m,
                TotalAmount: 120m,
                Vat: 0m,
                Withholding: 0m,
                InvoiceAmountAfterWithholding: 120m,
                StampTax: 0m,
                GrossAmount: 120m,
                TotalDeductions: 0m,
                NetPayable: 120m,
                AdditionalGuarantee: 0m,
                Deductions: noDeductions);

            var previousPayment = new ProgressPaymentHistoryData(
                Id: Guid.NewGuid(),
                CreatedAt: new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc),
                Input: previousInput,
                Calculation: previousCalculation,
                ProgressPaymentNumber: 1,
                PeriodDate: new DateOnly(2026, 9, 30),
                RouteNumber: "1",
                RouteName: "Örnek Güzergâh",
                WorkName: "Örnek Güzergâh",
                ContractorName: "Örnek Yüklenici",
                TenderNumber: "TEST-IHALE",
                ContractNumber: "TEST-SOZ",
                TenderDate: new DateOnly(2026, 8, 25),
                ContractDate: new DateOnly(2026, 8, 25),
                ContractStartDate: new DateOnly(2026, 9, 1),
                ContractEndDate: new DateOnly(2027, 6, 30),
                ContractorTaxOrIdentityNumber: "12345678901");

            var input = new ProgressPaymentInput(
                WorkDays: 1,
                DailyPrice: 100m,
                DiscountAmount: 0m,
                PriceDifference: 10m,
                ContractAmount: 100000m,
                IsVatExempt: true,
                TaxDebt: 2m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                InspectionPenalty: 2m);

            var calculation = new ProgressPaymentCalculation(
                ServiceAmount: 100m,
                PriceDifferenceExcludingVat: 10m,
                TotalAmount: 110m,
                Vat: 0m,
                Withholding: 0m,
                InvoiceAmountAfterWithholding: 110m,
                StampTax: 1m,
                GrossAmount: 110m,
                TotalDeductions: 5m,
                NetPayable: 105m,
                AdditionalGuarantee: 0m,
                Deductions: new DeductionBreakdown(0m, 1m, 2m, 2m, 0m, 0m, 0m));

            var currentPayment = new ProgressPaymentHistoryData(
                Id: Guid.NewGuid(),
                CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
                Input: input,
                Calculation: calculation,
                ProgressPaymentNumber: 2,
                PeriodDate: new DateOnly(2026, 10, 31),
                RouteNumber: "1",
                RouteName: "Örnek Güzergâh",
                WorkName: "Örnek Güzergâh",
                ContractorName: "Örnek Yüklenici",
                TenderNumber: "TEST-IHALE",
                ContractNumber: "TEST-SOZ",
                TenderDate: new DateOnly(2026, 8, 25),
                ContractDate: new DateOnly(2026, 8, 25),
                ContractStartDate: new DateOnly(2026, 9, 1),
                ContractEndDate: new DateOnly(2027, 6, 30),
                ContractorTaxOrIdentityNumber: "12345678901");

            var payments = new[] { previousPayment, currentPayment };
            var reportTotals = ProgressPaymentReportService.Calculate(currentPayment, payments);
            var reportNetPayable = ProgressPaymentReportService.CalculateReportNetPayable(
                currentPayment,
                reportTotals);
            var previousDeduction = ProgressPaymentIcmalService.FindPreviousDeduction(
                payments,
                currentPayment);

            var context = new OehsOfficialDocumentContext(
                WorkName: "Örnek Güzergâh",
                TenderNumber: "TEST-IHALE",
                RouteNumber: "1",
                ContractorName: "Örnek Yüklenici",
                ContractNumber: "TEST-SOZ",
                ContractAmount: 100000m,
                TenderDate: new DateOnly(2026, 8, 25),
                ContractDate: new DateOnly(2026, 8, 25),
                ContractStartDate: new DateOnly(2026, 9, 1),
                ContractEndDate: new DateOnly(2027, 6, 30),
                ContractWorkDays: 180,
                PeriodDate: new DateOnly(2026, 10, 31),
                ProgressPaymentNumber: 2,
                Calculation: calculation,
                Payments: ProgressPaymentReportService.OrderByPaymentPeriod(payments),
                ReportTotals: reportTotals,
                ReportNetPayable: reportNetPayable,
                PreviousDeduction: previousDeduction,
                GeneratedDate: new DateOnly(2026, 10, 31),
                PreparerName: "TEST DÜZENLEYEN",
                PreparerRole: "TEST MEMUR",
                ControllerName: "TEST KONTROL EDEN",
                ControllerRole: "TEST ŞUBE MÜDÜRÜ",
                ApproverName: "TEST ONAYLAYAN",
                ApproverRole: "TEST MÜDÜR");
            var outputs = OehsOfficialDocumentFactory.BuildSeparate(context);
            Assert.Equal(4, outputs.Count);

            // Verify that the test PDFs use the same previous/current period data
            // as the application report services rather than empty hard-coded history.
            var paymentOutput = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisRaporu));
            var paymentPage = Assert.IsType<FixedPage>(paymentOutput.Document.Pages[0].Child);
            var paymentText = GetPageText(paymentPage);

            // A is cumulative contract-price work; B is cumulative signed price difference.
            // With two 100 TL base periods, 20 TL previous F and 10 TL current F:
            // A=200, B=30, C=230, D=120, E=110.
            Assert.Equal("200,00 TL", GetCellTextAt(paymentPage, 476d, 172d)); // A
            Assert.Equal("30,00 TL", GetCellTextAt(paymentPage, 476d, 201d));  // B
            Assert.Equal("230,00 TL", GetCellTextAt(paymentPage, 476d, 230d)); // C = A + B
            Assert.Equal("120,00 TL", GetCellTextAt(paymentPage, 476d, 293d)); // D
            Assert.Equal("110,00 TL", GetCellTextAt(paymentPage, 476d, 322d)); // E = C - D
            Assert.Equal("0,00 TL", GetCellTextAt(paymentPage, 476d, 351d));   // F
            Assert.Equal("110,00 TL", GetCellTextAt(paymentPage, 476d, 380d)); // G
            Assert.Equal("5,00 TL", GetCellTextAt(paymentPage, 476d, 768d));   // H
            Assert.Equal("105,00 TL", GetCellTextAt(paymentPage, 476d, 796d)); // I
            Assert.Equal("Bu Hakedişin Tutarı ( C - D )", GetCellTextAt(paymentPage, 120d, 322d));
            Assert.Contains("Hakediş No : 2", paymentText);
            Assert.Contains("KESİNTİLER VE MAHSUPLAR", paymentText);
            Assert.Contains("KDV (taşıma %10 + rehber %20)", paymentText);
            Assert.Contains("KDV Tevkifatı (taşıma 5/10; rehber 9/10)", paymentText);
            Assert.Contains("Damga Vergisi (oran %0,948)", paymentText);

            var serviceReport = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu));
            var serviceReportText = GetPageText(
                Assert.IsType<FixedPage>(serviceReport.Document.Pages[0].Child));
            Assert.Contains("Örnek Yüklenici", serviceReportText);
            Assert.Contains("100.000,00 TL", serviceReportText);
            Assert.Contains("25.08.2026", serviceReportText);
            Assert.DoesNotContain("TEST-SOZ", serviceReportText);
            Assert.Contains("180 İş Günü", serviceReportText);
            Assert.Contains("Tescile Dahil Değil", serviceReportText);
            Assert.Contains("Avans verilmeyecektir.", serviceReportText);
            Assert.Contains("Yoktur.", serviceReportText);
            Assert.Contains("M.Y.H.B.Y. Örnek No: 5", serviceReportText);
            Assert.DoesNotContain("M.Y.H.B.Y. Örnek No: 4", serviceReportText);

            var basicCalculation = ProgressPaymentCalculator.Calculate(
                input with { ProgramType = TransportationProgramType.BasicOrSecondary },
                ProgressPaymentCalculationConstants.Defaults);
            var basicContext = context with
            {
                ProgramType = TransportationProgramType.BasicOrSecondary,
                Calculation = basicCalculation
            };
            var basicOutputs = OehsOfficialDocumentFactory.BuildSeparate(basicContext);
            Assert.Equal(4, basicOutputs.Count);
            var basicPayment = Assert.Single(
                basicOutputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisRaporu));
            var basicPaymentText = GetPageText(
                Assert.IsType<FixedPage>(basicPayment.Document.Pages[0].Child));
            Assert.Contains("KDV (taşıma %10)", basicPaymentText);
            Assert.DoesNotContain("rehber %0", basicPaymentText);
            Assert.Contains("KDV Tevkifatı (taşıma 5/10)", basicPaymentText);
            Assert.DoesNotContain("rehber 0/10", basicPaymentText);

            var acceptanceOutputForMapping = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.KabulTutanagi));
            var acceptanceMappingPage = Assert.IsType<FixedPage>(
                acceptanceOutputForMapping.Document.Pages[0].Child);
            var acceptanceMappingText = GetPageText(acceptanceMappingPage);
            // The displayed value is in workdays, so the label must use the same unit.
            Assert.Contains("Sözleşmeye göre işin süresi (iş günü)", acceptanceMappingText);
            Assert.DoesNotContain("takvim günü", acceptanceMappingText);
            Assert.Contains("180 İş Günü", acceptanceMappingText);
            Assert.Contains("30.06.2027", acceptanceMappingText);
            Assert.Contains("30 Haziran 2027 Çarşamba", acceptanceMappingText);
            Assert.DoesNotContain("Tescile Dahil Değil", acceptanceMappingText);
            Assert.DoesNotContain("Avans verilmeyecektir.", acceptanceMappingText);
            Assert.DoesNotContain("Yoktur.", acceptanceMappingText);
            var acceptanceNarrative = Assert.Single(acceptanceMappingPage.Children
                .OfType<TextBlock>()
                .Where(block => block.Text.StartsWith(
                    "İlçe Milli Eğitim Müdürlüğü",
                    StringComparison.Ordinal)));
            Assert.Contains("TEST MEMUR TEST DÜZENLEYEN", acceptanceNarrative.Text);
            Assert.Contains("TEST ŞUBE MÜDÜRÜ TEST KONTROL EDEN", acceptanceNarrative.Text);
            Assert.Contains("01.10.2026 / 31.10.2026", acceptanceNarrative.Text);
            Assert.DoesNotContain("Aşkın KESKİNBALTA", acceptanceNarrative.Text);
            Assert.DoesNotContain("Halil ÖZENÇ", acceptanceNarrative.Text);

            var icmalOutput = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisIcmali));
            var icmalText = GetPageText(Assert.IsType<FixedPage>(icmalOutput.Document.Pages[0].Child));
            Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmalText);
            Assert.Contains("Hizmet Alımı İşi Ekim 2026", icmalText);
            Assert.Contains("₺100,00", icmalText); // September contract-price work
            Assert.Contains("₺100,00", icmalText); // October contract-price work, excluding I/R
            Assert.Contains("₺10,00", icmalText);  // raw current-period I+R
            Assert.Contains("₺20,00", icmalText);  // raw prior-period I+R
            Assert.Contains("₺30,00", icmalText);  // cumulative raw I+R
            Assert.Contains("₺200,00", icmalText); // cumulative contract-price work
            Assert.Contains("₺230,00", icmalText); // summary total after cumulative raw I+R

            // The source Excel Hakediş Raporu contains signature drawings; all four
            // PDF templates therefore use the configured signatories.
            var contractorReportPage = Assert.IsType<FixedPage>(paymentOutput.Document.Pages[0].Child);
            Assert.Contains("Örnek Yüklenici", GetPageText(contractorReportPage));
            Assert.Contains("M.Y.H.B.Y. Örnek No :4/10", GetPageText(contractorReportPage));

            foreach (var output in outputs)
            {
                var outputPage = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                var textBlocks = GetPageText(outputPage);

                Assert.Contains("TEST DÜZENLEYEN", textBlocks);
                Assert.Contains("TEST MEMUR", textBlocks);
                Assert.Contains("TEST KONTROL EDEN", textBlocks);
                Assert.Contains("TEST ŞUBE MÜDÜRÜ", textBlocks);
                Assert.Contains("TEST ONAYLAYAN", textBlocks);
                Assert.Contains("TEST MÜDÜR", textBlocks);
            }

            var reportOuterFrame = Assert.Single(contractorReportPage.Children
                .OfType<Border>()
                .Where(border =>
                    Math.Abs(FixedPage.GetLeft(border) - 67d) < 0.01 &&
                    Math.Abs(FixedPage.GetTop(border) - 56d) < 0.01 &&
                    Math.Abs(border.Width - 659d) < 0.01 &&
                    Math.Abs(border.Height - 1014d) < 0.01));
            Assert.Equal(0d, reportOuterFrame.BorderThickness.Bottom);
            var reportTitle = Assert.Single(contractorReportPage.Children
                .OfType<TextBlock>()
                .Where(block => block.Text == "HAK EDİŞ RAPORU"));
            Assert.True(
                FixedPage.GetTop(reportTitle) < FixedPage.GetTop(reportOuterFrame),
                "The Hakediş Raporu title must sit above the source workbook's top form rule.");
            
            var eRowCells = contractorReportPage.Children
                .OfType<Border>()
                .Where(border => Math.Abs(FixedPage.GetTop(border) - 322d) < 0.01)
                .ToArray();
            Assert.Equal(3, eRowCells.Length);
            Assert.All(eRowCells, border =>
                Assert.Equal(Colors.White, Assert.IsType<SolidColorBrush>(border.Background).Color));

            // The white acceptance-form frame must be behind the title, not cover it.
            var acceptanceOutput = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.KabulTutanagi));
            var acceptancePage = Assert.IsType<FixedPage>(acceptanceOutput.Document.Pages[0].Child);
            var acceptanceFrame = Assert.Single(acceptancePage.Children
                .OfType<Border>()
                .Where(border =>
                    Math.Abs(FixedPage.GetLeft(border) - 50d) < 0.01 &&
                    Math.Abs(FixedPage.GetTop(border) - 38d) < 0.01));
            var acceptanceTitle = Assert.Single(acceptancePage.Children
                .OfType<TextBlock>()
                .Where(block => block.Text == "HİZMET İŞLERİ KABUL TUTANAĞI"));
            Assert.True(
                acceptancePage.Children.IndexOf(acceptanceTitle) > acceptancePage.Children.IndexOf(acceptanceFrame),
                "The acceptance title must be drawn after the white form frame.");

            var configuredOutputDirectory = Environment.GetEnvironmentVariable("OEHS_PDF_SMOKE_OUTPUT_DIR");
            var cleanupTemporaryDirectory = string.IsNullOrWhiteSpace(configuredOutputDirectory);
            var temporaryDirectory = cleanupTemporaryDirectory
                ? Directory.CreateTempSubdirectory("oehs-pdf-smoke-").FullName
                : Path.GetFullPath(configuredOutputDirectory!);
            Directory.CreateDirectory(temporaryDirectory);

            try
            {
                foreach (var output in outputs)
                {
                    Assert.Single(output.Document.Pages);
                    var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                    Assert.Equal(793.7008d, page.Width, 2);
                    Assert.Equal(1122.5197d, page.Height, 2);

                    var pdfPath = Path.Combine(temporaryDirectory, output.FileSuffix + ".pdf");
                    PdfVectorWriter.Save(output.Document, pdfPath);

                    Assert.True(File.Exists(pdfPath));
                    Assert.True(new FileInfo(pdfPath).Length > 1000);

                    using var pdf = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
                    Assert.Single(pdf.Pages);
                    Assert.Equal(595.2756d, pdf.Pages[0].Width.Point, 1);
                    Assert.Equal(841.8898d, pdf.Pages[0].Height.Point, 1);
                }
            }
            finally
            {
                if (cleanupTemporaryDirectory && Directory.Exists(temporaryDirectory))
                    Directory.Delete(temporaryDirectory, recursive: true);
            }
        });
    }

    private static string[] GetPageText(FixedPage page) =>
        page.Children.Cast<UIElement>().SelectMany(element => element switch
        {
            TextBlock directText => new[] { directText.Text },
            Border { Child: TextBlock borderText } => new[] { borderText.Text },
            _ => Array.Empty<string>()
        }).ToArray();

    private static string? GetCellTextAt(FixedPage page, double left, double top) =>
        page.Children.OfType<Border>()
            .Where(border =>
                Math.Abs(FixedPage.GetLeft(border) - left) < 0.01 &&
                Math.Abs(FixedPage.GetTop(border) - top) < 0.01)
            .Select(border => (border.Child as TextBlock)?.Text)
            .SingleOrDefault();

    private static void RunOnSta(Action action)
    {
        Exception? captured = null;
        var thread = new Thread(() =>
        {
            try
            {
                action();
            }
            catch (Exception ex)
            {
                captured = ex;
            }
        });

        thread.SetApartmentState(ApartmentState.STA);
        thread.Start();
        thread.Join();

        if (captured is not null)
            ExceptionDispatchInfo.Capture(captured).Throw();
    }
}
