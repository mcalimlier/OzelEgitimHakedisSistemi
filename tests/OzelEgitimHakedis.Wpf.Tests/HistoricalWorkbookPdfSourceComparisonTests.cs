using System.IO;
using System.Runtime.ExceptionServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;
using PdfSharp.Pdf.IO;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

/// <summary>
/// PDF text/field checks built from historical XLSM row values and source-PDF
/// totals. The source files were recalculated/exported with LibreOffice; VBA was
/// not executed. Known source icmal inconsistencies are asserted as discrepancies,
/// not silently treated as accepted financial behavior.
/// </summary>
public sealed class HistoricalWorkbookPdfSourceComparisonTests
{
    [Fact]
    public void HistoricalSourceVectors_ShouldRenderEightA4DocumentsAndExposeBasicIcmalMismatch()
    {
        RunOnSta(() =>
        {
            var special = BuildSpecialEducationSourceContext();
            var specialOutputs = OehsOfficialDocumentFactory.BuildSeparate(special);
            Assert.Equal(4, specialOutputs.Count);
            AssertSpecialEducationSourceFields(specialOutputs);

            var basic = BuildBasicEducationSourceContext();
            var basicOutputs = OehsOfficialDocumentFactory.BuildSeparate(basic);
            Assert.Equal(4, basicOutputs.Count);
            AssertBasicEducationSourceFields(basicOutputs);

            SaveAndAssertA4(specialOutputs, "Historical_Special");
            SaveAndAssertA4(basicOutputs, "Historical_Basic");
        });
    }

    private static OehsOfficialDocumentContext BuildSpecialEducationSourceContext()
    {
        var previousInput = new ProgressPaymentInput(
            WorkDays: 1m,
            DailyPrice: 53322.33m,
            DiscountAmount: 0m,
            PriceDifference: 1403.573m,
            ContractAmount: 764629.97m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var previousCalculation = new ProgressPaymentCalculation(
            ServiceAmount: 53322.33m,
            PriceDifferenceExcludingVat: 1403.573m,
            TotalAmount: 54725.903m,
            Vat: 0m,
            Withholding: 0m,
            InvoiceAmountAfterWithholding: 54725.903m,
            StampTax: 518.80156044m,
            GrossAmount: 54725.903m,
            TotalDeductions: 603.01594044m,
            NetPayable: 54122.88705956m,
            AdditionalGuarantee: 84.21438m,
            Deductions: new DeductionBreakdown(0m, 518.80156044m, 0m, 0m, 0m, 84.21438m, 0m));
        var previous = History(
            previousInput, previousCalculation, 1, new DateOnly(2026, 9, 30),
            new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc));

        var currentInput = new ProgressPaymentInput(
            WorkDays: 22m,
            DailyPrice: 2320m,
            DiscountAmount: 0m,
            PriceDifference: 1403.573m,
            ContractAmount: 764629.97m,
            IsVatExempt: true,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            GuideWorkDays: 30m,
            GuideDailyPrice: 1362.49m,
            GuideDiscountAmount: 0m,
            GuidePriceDifference: 0m,
            ProgramType: TransportationProgramType.SpecialEducation);
        var currentCalculation = ProgressPaymentCalculator.Calculate(currentInput);
        var current = History(
            currentInput, currentCalculation, 2, new DateOnly(2026, 10, 31),
            new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc));
        var payments = new[] { previous, current };
        var totals = ProgressPaymentReportService.Calculate(current, payments);
        var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);
        var previousDeduction = ProgressPaymentIcmalService.FindPreviousDeduction(payments, current);

        return Context(
            current, payments, totals, reportNet, previousDeduction,
            TransportationProgramType.SpecialEducation,
            764629.97m, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 14),
            new DateOnly(2026, 9, 14), new DateOnly(2027, 6, 25), 181,
            new DateOnly(2026, 10, 31), new DateOnly(2026, 10, 19));
    }

    private static OehsOfficialDocumentContext BuildBasicEducationSourceContext()
    {
        var input = new ProgressPaymentInput(
            WorkDays: 9.82693033060048m,
            DailyPrice: 19539.1608m,
            DiscountAmount: 0m,
            PriceDifference: 7365.349999999999m,
            ContractAmount: 2643686m,
            IsVatExempt: false,
            TaxDebt: 0m,
            SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m,
            InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var current = History(
            input, calculation, 1, new DateOnly(2026, 9, 30),
            new DateTime(2026, 9, 30, 12, 0, 0, DateTimeKind.Utc));
        var payments = new[] { current };
        var totals = ProgressPaymentReportService.Calculate(current, payments);
        var reportNet = ProgressPaymentReportService.CalculateReportNetPayable(current, totals);

        return Context(
            current, payments, totals, reportNet, 0m,
            TransportationProgramType.BasicOrSecondary,
            2643686m, new DateOnly(2026, 8, 26), new DateOnly(2026, 9, 14),
            new DateOnly(2026, 9, 14), new DateOnly(2027, 6, 25), 181,
            new DateOnly(2026, 9, 30), new DateOnly(2026, 10, 19));
    }

    private static OehsOfficialDocumentContext Context(
        ProgressPaymentHistoryData current,
        IReadOnlyList<ProgressPaymentHistoryData> payments,
        ProgressPaymentReportTotals totals,
        decimal reportNet,
        decimal previousDeduction,
        TransportationProgramType program,
        decimal contractAmount,
        DateOnly tenderDate,
        DateOnly contractDate,
        DateOnly contractStart,
        DateOnly contractEnd,
        int contractWorkDays,
        DateOnly period,
        DateOnly generatedDate) =>
        new(
            WorkName: current.WorkName ?? string.Empty,
            TenderNumber: "TEST-IHALE",
            RouteNumber: "1",
            ContractorName: "Örnek Yüklenici",
            ContractNumber: "TEST-SOZ",
            ContractAmount: contractAmount,
            TenderDate: tenderDate,
            ContractDate: contractDate,
            ContractStartDate: contractStart,
            ContractEndDate: contractEnd,
            ContractWorkDays: contractWorkDays,
            PeriodDate: period,
            ProgressPaymentNumber: current.ProgressPaymentNumber,
            Calculation: current.Calculation,
            Payments: ProgressPaymentReportService.OrderByPaymentPeriod(payments),
            ReportTotals: totals,
            ReportNetPayable: reportNet,
            PreviousDeduction: previousDeduction,
            GeneratedDate: generatedDate,
            PreparerName: "TEST DÜZENLEYEN",
            PreparerRole: "TEST MEMUR",
            ControllerName: "TEST KONTROL EDEN",
            ControllerRole: "TEST ŞUBE MÜDÜRÜ",
            ApproverName: "TEST ONAYLAYAN",
            ApproverRole: "TEST MÜDÜR",
            ProgramType: program,
            RouteName: current.RouteName ?? string.Empty);

    private static ProgressPaymentHistoryData History(
        ProgressPaymentInput input,
        ProgressPaymentCalculation calculation,
        int number,
        DateOnly period,
        DateTime createdAt) =>
        new(
            Guid.NewGuid(),
            createdAt,
            input,
            calculation,
            number,
            period,
            "1",
            "Kaynak Test Güzergâhı",
            "Örnek Öğrenci Taşıma İşi",
            "Örnek Yüklenici",
            "TEST-IHALE",
            "TEST-SOZ",
            TenderDate: new DateOnly(2026, 9, 1),
            ContractDate: new DateOnly(2026, 9, 14),
            ContractStartDate: new DateOnly(2026, 9, 14),
            ContractEndDate: new DateOnly(2027, 6, 25),
            ContractorTaxOrIdentityNumber: "12345678901");

    private static void AssertSpecialEducationSourceFields(
        IReadOnlyList<OehsOfficialDocumentOutput> outputs)
    {
        var report = PageText(outputs, OehsOfficialDocumentKind.HakedisRaporu);
        Assert.Contains("146.640,60 TL", report);
        Assert.Contains("1.403,57 TL", report);
        Assert.Contains("148.044,18 TL", report);
        Assert.Contains("54.725,903 TL", report);
        Assert.Contains("93.318,27 TL", report);
        Assert.Contains("968,87 TL", report);
        Assert.Contains("92.349,40 TL", report);

        var service = PageText(outputs, OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu);
        Assert.Contains("19.10.2026", service);
        Assert.Contains("Sözleşme Tarihi ve No.su", service);
        Assert.Contains("181 İş Günü", service);
        Assert.Contains("M.Y.H.B.Y. Örnek No: 4", service);
        Assert.Contains("No.su" + Environment.NewLine + ":" + Environment.NewLine + "1", service, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("Örnek Öğrenci Taşıma İşi", service, StringComparison.Ordinal);

        var acceptance = PageText(outputs, OehsOfficialDocumentKind.KabulTutanagi);
        Assert.Contains("01.10.2026 / 31.10.2026", acceptance);
        Assert.Contains("181 İş Günü", acceptance);
        Assert.Contains("14.09.2026", acceptance);
        Assert.Contains("25.06.2027", acceptance);

        var icmal = PageText(outputs, OehsOfficialDocumentKind.HakedisIcmali);
        Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmal);
        Assert.Contains("₺54.725,90", icmal);
        Assert.Contains("₺1.403,57", icmal);
        Assert.Contains("₺56.129,48", icmal);
        Assert.Contains("₺93.318,27", icmal);
        Assert.Contains("₺94.721,85", icmal);
        Assert.Contains("603,02", icmal);
        Assert.Contains("968,87", icmal);
        Assert.Contains("1.571,89", icmal);
        Assert.Contains("149.279,43", icmal);
    }

    private static void AssertBasicEducationSourceFields(
        IReadOnlyList<OehsOfficialDocumentOutput> outputs)
    {
        var report = PageText(outputs, OehsOfficialDocumentKind.HakedisRaporu);
        Assert.Contains("192.009,97 TL", report);
        Assert.Contains("7.365,35 TL", report);
        Assert.Contains("199.375,32 TL", report);
        Assert.Contains("219.312,85 TL", report);
        Assert.Contains("1.890,08 TL", report);
        Assert.Contains("12.300,77 TL", report);
        Assert.Contains("207.012,08 TL", report);

        var service = PageText(outputs, OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu);
        Assert.Contains("19.10.2026", service);
        Assert.Contains("Sözleşme Tarihi ve No.su", service);
        Assert.Contains("14.09.2026-25.06.2027 Tarihleri Arasıdır.", service);
        Assert.Contains("M.Y.H.B.Y. Örnek No: 4", service);
        Assert.Contains("No.su" + Environment.NewLine + ":" + Environment.NewLine + "1", service, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("Örnek Öğrenci Taşıma İşi", service, StringComparison.Ordinal);

        var acceptance = PageText(outputs, OehsOfficialDocumentKind.KabulTutanagi);
        Assert.Contains("14.09.2026 / 30.09.2026", acceptance);
        Assert.Contains("181", acceptance);
        Assert.DoesNotContain("181 İş Günü", acceptance);
        Assert.Contains("25.06.2027", acceptance);

        var icmal = PageText(outputs, OehsOfficialDocumentKind.HakedisIcmali);
        Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmal);
        Assert.Contains("₺199.375,32", icmal);
        // The historical Excel PDF prints 19.670,30 and 219.045,63. OEHS currently
        // recalculates the deduction column as 12.300,77 instead of the stale AB4
        // snapshot value 12.304,95, so the generated icmal differs by 4,19 TL.
        Assert.Contains("₺19.666,12", icmal);
        Assert.Contains("₺219.041,44", icmal);
        Assert.DoesNotContain("₺19.670,30", icmal, StringComparison.Ordinal);
        Assert.DoesNotContain("₺219.045,63", icmal, StringComparison.Ordinal);
    }

    private static string PageText(
        IReadOnlyList<OehsOfficialDocumentOutput> outputs,
        OehsOfficialDocumentKind kind)
    {
        var output = Assert.Single(outputs.Where(x => x.Kind == kind));
        var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
        return GetPageText(page);
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

    private static void SaveAndAssertA4(
        IReadOnlyList<OehsOfficialDocumentOutput> outputs,
        string prefix)
    {
        var configuredDirectory = Environment.GetEnvironmentVariable("OEHS_PDF_SMOKE_OUTPUT_DIR");
        var cleanupDirectory = string.IsNullOrWhiteSpace(configuredDirectory);
        var directory = cleanupDirectory
            ? Directory.CreateTempSubdirectory("oehs-historical-pdf-").FullName
            : Path.GetFullPath(configuredDirectory!);
        Directory.CreateDirectory(directory);

        try
        {
            foreach (var output in outputs)
            {
                Assert.Single(output.Document.Pages);
                var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                Assert.Equal(793.7008d, page.Width, 2);
                Assert.Equal(1122.5197d, page.Height, 2);

                var name = output.Kind switch
                {
                    OehsOfficialDocumentKind.HakedisRaporu => "Hakedis_Raporu",
                    OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu => "Hizmet_Isleri_Hakedis_Raporu",
                    OehsOfficialDocumentKind.KabulTutanagi => "Hizmet_Isleri_Kabul_Tutanagi",
                    OehsOfficialDocumentKind.HakedisIcmali => "Hakedis_Icmali",
                    _ => output.Kind.ToString()
                };
                var path = Path.Combine(directory, $"{prefix}_{name}.pdf");
                PdfVectorWriter.Save(output.Document, path);

                Assert.True(File.Exists(path));
                Assert.True(new FileInfo(path).Length > 1000);
                using var pdf = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                Assert.Single(pdf.Pages);
                Assert.Equal(595.2756d, pdf.Pages[0].Width.Point, 1);
                Assert.Equal(841.8898d, pdf.Pages[0].Height.Point, 1);
            }
        }
        finally
        {
            if (cleanupDirectory && Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
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
