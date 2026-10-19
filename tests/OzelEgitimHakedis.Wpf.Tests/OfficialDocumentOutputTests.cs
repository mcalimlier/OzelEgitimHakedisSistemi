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
            Assert.Contains("preparerName", File.ReadAllText(settingsPath), StringComparison.Ordinal);
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

            Assert.Contains(settingsPath, exception.Message, StringComparison.Ordinal);
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
            Assert.Equal(
                new[]
                {
                    OehsOfficialDocumentKind.HakedisRaporu,
                    OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu,
                    OehsOfficialDocumentKind.KabulTutanagi,
                    OehsOfficialDocumentKind.HakedisIcmali
                }.OrderBy(kind => kind),
                outputs.Select(output => output.Kind).OrderBy(kind => kind));

            // Every official output must be a single A4 portrait page; a valid
            // first report must not hide a missing, duplicated, or oversized sibling.
            foreach (var output in outputs)
            {
                Assert.Single(output.Document.Pages);
                var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                Assert.True(page.Width > 0 && page.Height > page.Width,
                    $"{output.Kind} must use a portrait page, got {page.Width} x {page.Height}.");
            }

            // Verify that the test PDFs use the same previous/current period data
            // as the application report services rather than empty hard-coded history.
            var paymentOutput = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisRaporu));
            var paymentPage = Assert.IsType<FixedPage>(paymentOutput.Document.Pages[0].Child);
            var paymentText = GetPageText(paymentPage);

            // Regression guard: the rotated deductions heading must share the
            // center of the full-height merged cell, without changing its font or angle.
            var deductionsCell = Assert.Single(paymentPage.Children.OfType<Border>()
                .Where(border => Math.Abs(border.Width - 165d) < 0.01 && border.Height > 300d));
            var deductionsTitle = Assert.Single(paymentPage.Children.OfType<TextBlock>()
                .Where(block => block.Text == "KESİNTİLER VE MAHSUPLAR"));
            Assert.IsType<RotateTransform>(deductionsTitle.RenderTransform);
            Assert.Equal(-90d, ((RotateTransform)deductionsTitle.RenderTransform).Angle);
            Assert.Equal(VerticalAlignment.Center, deductionsTitle.VerticalAlignment);
            Assert.Equal(TextAlignment.Center, deductionsTitle.TextAlignment);
            Assert.Equal(deductionsCell.Width, deductionsTitle.Height, 2);
            Assert.Equal(deductionsCell.Height, deductionsTitle.Width, 2);
            Assert.Equal(
                FixedPage.GetLeft(deductionsCell) + deductionsCell.Width / 2d,
                FixedPage.GetLeft(deductionsTitle) + deductionsTitle.Width / 2d,
                2);
            Assert.Equal(
                FixedPage.GetTop(deductionsCell) + deductionsCell.Height / 2d,
                FixedPage.GetTop(deductionsTitle) + deductionsTitle.Height / 2d,
                2);

            // Excel D6 = prior + current monthly K+T, less current I+R.
            // D7 contains only the current month's raw I+R.
            // With two 100 TL base periods, 20 TL previous F and 10 TL current F:
            // A=220, B=10, C=230, D=120, E=110, F=0.
            // The workbook's D11 adds the current raw price difference to H2, so G/D13=120.
            Assert.Equal("220,00 TL", GetCellTextAt(paymentPage, 476d, 172d)); // A
            Assert.Equal("10,00 TL", GetCellTextAt(paymentPage, 476d, 201d));  // B
            Assert.Equal("230,00 TL", GetCellTextAt(paymentPage, 476d, 230d)); // C = A + B
            Assert.Equal("120,00 TL", GetCellTextAt(paymentPage, 476d, 293d)); // D
            Assert.Equal("110,00 TL", GetCellTextAt(paymentPage, 476d, 322d)); // E = C - D
            Assert.Equal("0,00 TL", GetCellTextAt(paymentPage, 476d, 351d));   // F
            Assert.Equal("120,00 TL", GetCellTextAt(paymentPage, 476d, 380d)); // G = D11 + D12
            Assert.Equal("5,00 TL", GetCellTextAt(paymentPage, 476d, 768d));   // H
            Assert.Equal("115,00 TL", GetCellTextAt(paymentPage, 476d, 796d)); // I = G - H
            Assert.Equal("Bu Hakedişin Tutarı ( C - D )", GetCellTextAt(paymentPage, 120d, 322d));
            Assert.Contains("Hakediş No : 2", paymentText, StringComparison.Ordinal);
            Assert.Contains("KESİNTİLER VE MAHSUPLAR", paymentText, StringComparison.Ordinal);
            Assert.Contains("KDV (taşıma %10 + rehber %20)", paymentText, StringComparison.Ordinal);
            Assert.Contains("KDV Tevkifatı (taşıma 5/10; rehber 9/10)", paymentText, StringComparison.Ordinal);
            Assert.Contains("Damga Vergisi (oran %0,948)", paymentText, StringComparison.Ordinal);

            // The report must expose the exact payment period and contractor identity,
            // not only the computed monetary cells.
            Assert.Contains("Örnek Yüklenici", paymentText, StringComparison.Ordinal);
            Assert.Contains("31.10.2026", paymentText, StringComparison.Ordinal);

            var serviceReport = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu));
            var serviceReportText = GetPageText(
                Assert.IsType<FixedPage>(serviceReport.Document.Pages[0].Child));
            Assert.Contains("Örnek Yüklenici", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("100.000,00 TL", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("25.08.2026", serviceReportText, StringComparison.Ordinal);
            Assert.DoesNotContain("TEST-SOZ", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("180 İş Günü", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("Tescile Dahil Değil", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("Avans verilmeyecektir.", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("Yoktur.", serviceReportText, StringComparison.Ordinal);
            Assert.Contains("M.Y.H.B.Y. Örnek No: 5", serviceReportText, StringComparison.Ordinal);
            Assert.DoesNotContain("M.Y.H.B.Y. Örnek No: 4", serviceReportText, StringComparison.Ordinal);

            var specialIcmal = Assert.Single(outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisIcmali));
            var specialIcmalText = GetPageText(Assert.IsType<FixedPage>(specialIcmal.Document.Pages[0].Child));
            Assert.Contains("₺140,00", specialIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺120,00", specialIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺20,00", specialIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺10,00", specialIcmalText, StringComparison.Ordinal);

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
            foreach (var output in basicOutputs)
            {
                var outputPage = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                var textBlocks = GetPageText(outputPage);

                // Keep the Basic/Secondary document family isolated but subject
                // to the same cross-document identity and period acceptance gate.
                Assert.Contains("Örnek Güzergâh", textBlocks, StringComparison.Ordinal);
                switch (output.Kind)
                {
                    case OehsOfficialDocumentKind.HakedisRaporu:
                    case OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu:
                        Assert.Contains("31.10.2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Örnek Yüklenici", textBlocks, StringComparison.Ordinal);
                        break;
                    case OehsOfficialDocumentKind.KabulTutanagi:
                        Assert.Contains("01.10.2026 / 31.10.2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Örnek Yüklenici", textBlocks, StringComparison.Ordinal);
                        break;
                    case OehsOfficialDocumentKind.HakedisIcmali:
                        Assert.Contains("Hizmet Alımı İşi Eylül 2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Hizmet Alımı İşi Ekim 2026", textBlocks, StringComparison.Ordinal);
                        break;
                }
            }

            // July is a real period for Basic/Secondary, unlike the current
            // Special Education lookup range where July remains a zero placeholder.
            var julyInput = input with
            {
                ProgramType = TransportationProgramType.BasicOrSecondary
            };
            var julyCalculation = ProgressPaymentCalculator.Calculate(julyInput);
            var julyPayment = currentPayment with
            {
                Id = Guid.NewGuid(),
                CreatedAt = new DateTime(2027, 7, 31, 12, 0, 0, DateTimeKind.Utc),
                Input = julyInput,
                Calculation = julyCalculation,
                ProgressPaymentNumber = 9,
                PeriodDate = new DateOnly(2027, 7, 31)
            };
            var basicJulyTotals = ProgressPaymentReportService.Calculate(
                julyPayment,
                new[] { julyPayment });
            var basicJulyContext = basicContext with
            {
                PeriodDate = new DateOnly(2027, 7, 31),
                ContractEndDate = new DateOnly(2027, 7, 31),
                Payments = new[] { julyPayment },
                ReportTotals = basicJulyTotals,
                ReportNetPayable = ProgressPaymentReportService.CalculateReportNetPayable(
                    julyPayment,
                    basicJulyTotals),
                PreviousDeduction = 0m
            };
            var basicJulyIcmal = Assert.Single(
                OehsOfficialDocumentFactory.BuildSeparate(basicJulyContext)
                    .Where(output => output.Kind == OehsOfficialDocumentKind.HakedisIcmali));
            var basicJulyIcmalText = GetPageText(
                Assert.IsType<FixedPage>(basicJulyIcmal.Document.Pages[0].Child));
            Assert.Contains("Hizmet Alımı İşi Temmuz 2027", basicJulyIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺110,00", basicJulyIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺15,64", basicJulyIcmalText, StringComparison.Ordinal);
            Assert.Contains("₺125,64", basicJulyIcmalText, StringComparison.Ordinal);

            var basicPayment = Assert.Single(
                basicOutputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisRaporu));
            var basicPaymentText = GetPageText(
                Assert.IsType<FixedPage>(basicPayment.Document.Pages[0].Child));
            Assert.Contains("KDV (taşıma %10)", basicPaymentText, StringComparison.Ordinal);
            Assert.DoesNotContain("rehber %0", basicPaymentText, StringComparison.Ordinal);
            Assert.Contains("KDV Tevkifatı (taşıma 5/10)", basicPaymentText, StringComparison.Ordinal);
            Assert.DoesNotContain("rehber 0/10", basicPaymentText, StringComparison.Ordinal);

            var acceptanceOutputForMapping = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.KabulTutanagi));
            var acceptanceMappingPage = Assert.IsType<FixedPage>(
                acceptanceOutputForMapping.Document.Pages[0].Child);
            var acceptanceMappingText = GetPageText(acceptanceMappingPage);
            // The source template retains its official label; duration formatting is program-specific.
            Assert.Contains("Sözleşmeye göre işin süresi (takvim günü)", acceptanceMappingText, StringComparison.Ordinal);
            Assert.Contains("180 İş Günü", acceptanceMappingText, StringComparison.Ordinal);
            Assert.Contains("30.06.2027", acceptanceMappingText, StringComparison.Ordinal);
            Assert.Contains("30 Haziran 2027 Çarşamba", acceptanceMappingText, StringComparison.Ordinal);
            Assert.DoesNotContain("Tescile Dahil Değil", acceptanceMappingText, StringComparison.Ordinal);
            Assert.DoesNotContain("Avans verilmeyecektir.", acceptanceMappingText, StringComparison.Ordinal);
            Assert.DoesNotContain("Yoktur.", acceptanceMappingText, StringComparison.Ordinal);
            var acceptanceNarrative = Assert.Single(acceptanceMappingPage.Children
                .OfType<TextBlock>()
                .Where(block => block.Text.StartsWith(
                    "İlçe Milli Eğitim Müdürlüğü",
                    StringComparison.Ordinal)));
            Assert.Contains("TEST MEMUR TEST DÜZENLEYEN", acceptanceNarrative.Text, StringComparison.Ordinal);
            Assert.Contains("TEST ŞUBE MÜDÜRÜ TEST KONTROL EDEN", acceptanceNarrative.Text, StringComparison.Ordinal);
            Assert.Contains("01.10.2026 / 31.10.2026", acceptanceNarrative.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("Aşkın KESKİNBALTA", acceptanceNarrative.Text, StringComparison.Ordinal);
            Assert.DoesNotContain("Halil ÖZENÇ", acceptanceNarrative.Text, StringComparison.Ordinal);

            var icmalOutput = Assert.Single(
                outputs.Where(output => output.Kind == OehsOfficialDocumentKind.HakedisIcmali));
            var icmalText = GetPageText(Assert.IsType<FixedPage>(icmalOutput.Document.Pages[0].Child));
            Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmalText, StringComparison.Ordinal);
            Assert.Contains("Hizmet Alımı İşi Ekim 2026", icmalText, StringComparison.Ordinal);
            Assert.Contains("₺120,00", icmalText, StringComparison.Ordinal); // September K+T includes I/R
            Assert.Contains("₺110,00", icmalText, StringComparison.Ordinal); // October K+T includes I/R
            Assert.Contains("₺10,00", icmalText, StringComparison.Ordinal);  // raw current-period I+R
            Assert.Contains("₺20,00", icmalText, StringComparison.Ordinal);  // raw prior-period I+R
            Assert.Contains("₺30,00", icmalText, StringComparison.Ordinal);  // cumulative raw I+R
            Assert.Contains("₺230,00", icmalText, StringComparison.Ordinal); // cumulative K+T monthly transfers
            Assert.Contains("₺260,00", icmalText, StringComparison.Ordinal); // summary total after cumulative raw I+R

            // The source Excel Hakediş Raporu contains signature drawings; all four
            // PDF templates therefore use the configured signatories.
            var contractorReportPage = Assert.IsType<FixedPage>(paymentOutput.Document.Pages[0].Child);
            Assert.Contains("Örnek Yüklenici", GetPageText(contractorReportPage), StringComparison.Ordinal);
            Assert.Contains("M.Y.H.B.Y. Örnek No :4/10", GetPageText(contractorReportPage), StringComparison.Ordinal);

            foreach (var output in outputs)
            {
                var outputPage = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                var textBlocks = GetPageText(outputPage);

                // Cross-document consistency guard: all four outputs must refer to
                // the same route/work, even though their financial layouts differ.
                Assert.Contains("Örnek Güzergâh", textBlocks, StringComparison.Ordinal);
                switch (output.Kind)
                {
                    case OehsOfficialDocumentKind.HakedisRaporu:
                    case OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu:
                        Assert.Contains("31.10.2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Örnek Yüklenici", textBlocks, StringComparison.Ordinal);
                        break;
                    case OehsOfficialDocumentKind.KabulTutanagi:
                        Assert.Contains("01.10.2026 / 31.10.2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Örnek Yüklenici", textBlocks, StringComparison.Ordinal);
                        break;
                    case OehsOfficialDocumentKind.HakedisIcmali:
                        Assert.Contains("Hizmet Alımı İşi Eylül 2026", textBlocks, StringComparison.Ordinal);
                        Assert.Contains("Hizmet Alımı İşi Ekim 2026", textBlocks, StringComparison.Ordinal);
                        break;
                }

                Assert.Contains("TEST DÜZENLEYEN", textBlocks, StringComparison.Ordinal);
                Assert.Contains("TEST MEMUR", textBlocks, StringComparison.Ordinal);
                Assert.Contains("TEST KONTROL EDEN", textBlocks, StringComparison.Ordinal);
                Assert.Contains("TEST ŞUBE MÜDÜRÜ", textBlocks, StringComparison.Ordinal);
                Assert.Contains("TEST ONAYLAYAN", textBlocks, StringComparison.Ordinal);
                Assert.Contains("TEST MÜDÜR", textBlocks, StringComparison.Ordinal);
            }

            var reportOuterFrame = Assert.Single(contractorReportPage.Children
                .OfType<Border>()
                .Where(border =>
                    Math.Abs(FixedPage.GetLeft(border) - 67d) < 0.01 &&
                    Math.Abs(FixedPage.GetTop(border) - 56d) < 0.01 &&
                    Math.Abs(border.Width - 659d) < 0.01 &&
                    Math.Abs(border.Height - 1014d) < 0.01));
            Assert.Equal(0d, reportOuterFrame.BorderThickness.Bottom);
            var reportFooter = Assert.Single(contractorReportPage.Children
                .OfType<TextBlock>()
                .Where(block => block.Text == "M.Y.H.B.Y. Örnek No :4/10"));
            Assert.True(
                FixedPage.GetTop(reportFooter) >= FixedPage.GetTop(reportOuterFrame) + reportOuterFrame.Height,
                "The report number footer must sit below the outer frame so the frame border cannot cross its text.");
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
                    SaveAndAssertA4Pdf(output.Document, pdfPath);
                }

                // Export a separate four-document set for Basic/Secondary transport too.
                // Prefix the filenames so both program types can be reviewed side by side.
                foreach (var output in basicOutputs)
                {
                    Assert.Single(output.Document.Pages);
                    var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                    Assert.Equal(793.7008d, page.Width, 2);
                    Assert.Equal(1122.5197d, page.Height, 2);

                    var pdfPath = Path.Combine(
                        temporaryDirectory,
                        "Temel_Ortaogretim_" + output.FileSuffix + ".pdf");
                    SaveAndAssertA4Pdf(output.Document, pdfPath);
                }
            }
            finally
            {
                if (cleanupTemporaryDirectory && Directory.Exists(temporaryDirectory))
                    Directory.Delete(temporaryDirectory, recursive: true);
            }
        });
    }

    [Fact]
    public void LatestSpecialEducationWorkbookRow4_ShouldKeepReportD28DistinctFromCalculationAGInPdf()
    {
        RunOnSta(() =>
        {
            // Source workbook vector: Hesaplama!AG4 = 95,527.1346528,
            // while Hakediş Raporu!D28 = 98,907.13. The PDF report must use
            // the report formula, not silently substitute the calculation-sheet AG value.
            var input = new ProgressPaymentInput(
                WorkDays: 19m,
                DailyPrice: 2608.26m,
                DiscountAmount: 150m,
                PriceDifference: 1850m,
                ContractAmount: 100000m,
                IsVatExempt: false,
                TaxDebt: 0m,
                SocialSecurityDebt: 0m,
                MinimumWageSupport: 0m,
                InspectionPenalty: 0m,
                GuideWorkDays: 30m,
                GuideDailyPrice: 1362.49m,
                GuideDiscountAmount: 450m,
                GuidePriceDifference: 1530m,
                ProgramType: TransportationProgramType.SpecialEducation);
            var calculation = ProgressPaymentCalculator.Calculate(input);
            var payment = new ProgressPaymentHistoryData(
                Id: Guid.NewGuid(),
                CreatedAt: new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc),
                Input: input,
                Calculation: calculation,
                ProgressPaymentNumber: 1,
                PeriodDate: new DateOnly(2026, 10, 31),
                RouteNumber: "1",
                RouteName: "Örnek Güzergâh",
                WorkName: "Örnek Güzergâh",
                ContractorName: "Örnek Yüklenici",
                TenderNumber: "TEST-IHALE",
                ContractNumber: "TEST-SOZ",
                ContractorTaxOrIdentityNumber: "12345678901");
            var payments = new[] { payment };
            var totals = ProgressPaymentReportService.Calculate(payment, payments);
            var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(payment, totals);
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
                ProgressPaymentNumber: 1,
                Calculation: calculation,
                Payments: payments,
                ReportTotals: totals,
                ReportNetPayable: reportNet,
                PreviousDeduction: ProgressPaymentIcmalService.FindPreviousDeduction(payments, payment),
                GeneratedDate: new DateOnly(2026, 10, 31),
                ProgramType: TransportationProgramType.SpecialEducation);

            var report = Assert.Single(OehsOfficialDocumentFactory.BuildSeparate(context)
                .Where(output => output.Kind == OehsOfficialDocumentKind.HakedisRaporu));
            var page = Assert.IsType<FixedPage>(report.Document.Pages[0].Child);

            Assert.Equal(98907.13m, reportNet);
            Assert.Equal("98.907,13 TL", GetCellTextAt(page, 476d, 796d));
            Assert.NotEqual(calculation.NetPayable, reportNet);
        });
    }

    private static void SaveAndAssertA4Pdf(FixedDocument document, string pdfPath)
    {
        PdfVectorWriter.Save(document, pdfPath);

        Assert.True(File.Exists(pdfPath));
        Assert.True(new FileInfo(pdfPath).Length > 1000);

        using var pdf = PdfReader.Open(pdfPath, PdfDocumentOpenMode.Import);
        Assert.Single(pdf.Pages);
        Assert.Equal(595.2756d, pdf.Pages[0].Width.Point, 1);
        Assert.Equal(841.8898d, pdf.Pages[0].Height.Point, 1);
    }

    private static string GetPageText(FixedPage page) =>
        string.Join(
            Environment.NewLine,
            page.Children.Cast<UIElement>().SelectMany(element => element switch
            {
                TextBlock directText => new[] { directText.Text },
                Border { Child: TextBlock borderText } => new[] { borderText.Text },
                _ => Array.Empty<string>()
            }));

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
