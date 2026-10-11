using System.Runtime.ExceptionServices;
using System.Threading;
using System.IO;
using System.Windows;
using System.Windows.Documents;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using PdfSharp.Pdf.IO;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed class OfficialDocumentOutputTests
{
    [Fact]
    public void FourOfficialTemplates_ShouldExportAsSinglePageA4VectorPdfs()
    {
        RunOnSta(() =>
        {
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
                Deductions: new DeductionBreakdown(0m, 1m, 0m, 0m, 0m, 0m, 0m));

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
                Payments: Array.Empty<ProgressPaymentHistoryData>(),
                ReportTotals: new ProgressPaymentReportTotals(
                    CumulativeServiceAmount: 200m,
                    CurrentPriceDifference: 10m,
                    CumulativeTotal: 210m,
                    PreviousTotal: 100m,
                    CurrentTotal: 120m),
                ReportNetPayable: 115m,
                PreviousDeduction: 0m,
                GeneratedDate: new DateOnly(2026, 10, 31));

            var outputs = OehsOfficialDocumentFactory.BuildSeparate(context);
            Assert.Equal(4, outputs.Count);

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
