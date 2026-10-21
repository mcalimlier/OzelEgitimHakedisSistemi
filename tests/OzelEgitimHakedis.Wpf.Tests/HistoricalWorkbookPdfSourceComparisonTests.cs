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

    [Fact]
    public void LatestUploadedWorkbookRows_ShouldRenderEightA4DocumentsAndExposeIcmalDifference()
    {
        RunOnSta(() =>
        {
            var special = BuildLatestSpecialEducationContext();
            var specialOutputs = OehsOfficialDocumentFactory.BuildSeparate(special);
            Assert.Equal(4, specialOutputs.Count);
            var specialReport = PageText(specialOutputs, OehsOfficialDocumentKind.HakedisRaporu);
            Assert.Contains("77.667,27", specialReport, System.StringComparison.Ordinal);
            Assert.Contains("236.014,30", specialReport, System.StringComparison.Ordinal);
            Assert.Contains("158.347,03", specialReport, System.StringComparison.Ordinal);
            Assert.Contains("61.508,94", specialReport, System.StringComparison.Ordinal);
            var specialIcmal = PageText(specialOutputs, OehsOfficialDocumentKind.HakedisIcmali);
            Assert.Contains("₺66.637,27", specialIcmal, System.StringComparison.Ordinal); // K+T base excludes I/R
            Assert.Contains("₺11.030,00", specialIcmal, System.StringComparison.Ordinal); // I/R appears once
            Assert.Contains("₺77.667,27", specialIcmal, System.StringComparison.Ordinal); // row total equals K+T
            Assert.DoesNotContain("₺88.697,27", specialIcmal, System.StringComparison.Ordinal);

            var basic = BuildLatestBasicEducationContext();
            var basicOutputs = OehsOfficialDocumentFactory.BuildSeparate(basic);
            Assert.Equal(4, basicOutputs.Count);
            var basicReport = PageText(basicOutputs, OehsOfficialDocumentKind.HakedisRaporu);
            Assert.Contains("319.991,92", basicReport, System.StringComparison.Ordinal);
            Assert.Contains("1.018.593,25", basicReport, System.StringComparison.Ordinal);
            Assert.Contains("698.601,33", basicReport, System.StringComparison.Ordinal);
            Assert.Contains("332.516,08", basicReport, System.StringComparison.Ordinal);
            var basicIcmal = PageText(basicOutputs, OehsOfficialDocumentKind.HakedisIcmali);
            Assert.Contains("₺312.626,57", basicIcmal, System.StringComparison.Ordinal); // K base excludes I
            Assert.Contains("₺26.840,39", basicIcmal, System.StringComparison.Ordinal); // I plus deductions
            Assert.Contains("₺339.466,96", basicIcmal, System.StringComparison.Ordinal); // I appears once in A+B
            Assert.DoesNotContain("₺346.832,31", basicIcmal, System.StringComparison.Ordinal);

            SaveToTemporaryA4Pdfs(specialOutputs);
            SaveToTemporaryA4Pdfs(basicOutputs);
        });
    }

    private static OehsOfficialDocumentContext BuildLatestSpecialEducationContext()
    {
        var previousInput = new ProgressPaymentInput(
            WorkDays: 1m, DailyPrice: 156943.457m, DiscountAmount: 0m, PriceDifference: 1403.573m,
            ContractAmount: 764629.97m, IsVatExempt: true, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, ProgramType: TransportationProgramType.SpecialEducation);
        var previous = History(
            previousInput, ProgressPaymentCalculator.Calculate(previousInput), 2,
            new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc));

        var input = new ProgressPaymentInput(
            WorkDays: 16m, DailyPrice: 2320m, DiscountAmount: 1020m, PriceDifference: 7530m,
            ContractAmount: 764629.97m, IsVatExempt: false, TaxDebt: 1200m, SocialSecurityDebt: 12560m,
            MinimumWageSupport: 1362.49m, InspectionPenalty: 2500m,
            GuideWorkDays: 23m, GuideDailyPrice: 1362.49m, GuideDiscountAmount: 800m,
            GuidePriceDifference: 3500m, ProgramType: TransportationProgramType.SpecialEducation);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var current = History(
            input, calculation, 3, new DateOnly(2026, 11, 30),
            new DateTime(2026, 11, 30, 12, 0, 0, DateTimeKind.Utc));
        var payments = new[] { previous, current };
        var totals = ProgressPaymentReportService.Calculate(current, payments);
        return Context(
            current, payments, totals,
            ProgressPaymentReportService.CalculateReportNetPayable(current, totals),
            26448.176274448m, TransportationProgramType.SpecialEducation,
            764629.97m, new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 14),
            new DateOnly(2026, 9, 14), new DateOnly(2027, 6, 25), 181,
            new DateOnly(2026, 11, 30), new DateOnly(2026, 10, 19));
    }

    private static OehsOfficialDocumentContext BuildLatestBasicEducationContext()
    {
        var previousInput = new ProgressPaymentInput(
            WorkDays: 1m, DailyPrice: 691235.978m, DiscountAmount: 0m, PriceDifference: 7365.35m,
            ContractAmount: 2643686m, IsVatExempt: true, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, ProgramType: TransportationProgramType.BasicOrSecondary);
        var previous = History(
            previousInput, ProgressPaymentCalculator.Calculate(previousInput), 2,
            new DateOnly(2026, 10, 31), new DateTime(2026, 10, 31, 12, 0, 0, DateTimeKind.Utc));

        var input = new ProgressPaymentInput(
            WorkDays: 16m, DailyPrice: 19539.1608m, DiscountAmount: 0m, PriceDifference: 7365.35m,
            ContractAmount: 2643686m, IsVatExempt: false, TaxDebt: 0m, SocialSecurityDebt: 0m,
            MinimumWageSupport: 0m, InspectionPenalty: 0m,
            ProgramType: TransportationProgramType.BasicOrSecondary);
        var calculation = ProgressPaymentCalculator.Calculate(input);
        var current = History(
            input, calculation, 3, new DateOnly(2026, 11, 30),
            new DateTime(2026, 11, 30, 12, 0, 0, DateTimeKind.Utc));
        var payments = new[] { previous, current };
        var totals = ProgressPaymentReportService.Calculate(current, payments);
        return Context(
            current, payments, totals,
            ProgressPaymentReportService.CalculateReportNetPayable(current, totals),
            26448.176274448m, TransportationProgramType.BasicOrSecondary,
            2643686m, new DateOnly(2026, 8, 26), new DateOnly(2026, 9, 14),
            new DateOnly(2026, 9, 14), new DateOnly(2027, 6, 25), 181,
            new DateOnly(2026, 11, 30), new DateOnly(2026, 10, 19));
    }

    private static void SaveToTemporaryA4Pdfs(IReadOnlyList<OehsOfficialDocumentOutput> outputs)
    {
        var directory = Directory.CreateTempSubdirectory("oehs-latest-source-pdf-").FullName;
        try
        {
            foreach (var output in outputs)
            {
                Assert.Single(output.Document.Pages);
                var page = Assert.IsType<FixedPage>(output.Document.Pages[0].Child);
                Assert.Equal(793.7008d, page.Width, 2);
                Assert.Equal(1122.5197d, page.Height, 2);
                var path = Path.Combine(directory, $"{output.Kind}.pdf");
                PdfVectorWriter.Save(output.Document, path);
                Assert.True(new FileInfo(path).Length > 1000);
                using var pdf = PdfReader.Open(path, PdfDocumentOpenMode.Import);
                Assert.Single(pdf.Pages);
                Assert.Equal(595.2756d, pdf.Pages[0].Width.Point, 1);
                Assert.Equal(841.8898d, pdf.Pages[0].Height.Point, 1);
            }
        }
        finally
        {
            if (Directory.Exists(directory))
                Directory.Delete(directory, recursive: true);
        }
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
        Assert.Contains("146.640,60 TL", report, StringComparison.Ordinal);
        Assert.Contains("1.403,57 TL", report, StringComparison.Ordinal);
        Assert.Contains("148.044,18 TL", report, StringComparison.Ordinal);
        Assert.Contains("54.725,903 TL", report, StringComparison.Ordinal);
        Assert.Contains("93.318,27 TL", report, StringComparison.Ordinal);
        Assert.Contains("968,87 TL", report, StringComparison.Ordinal);
        Assert.Contains("92.349,40 TL", report, StringComparison.Ordinal);
        Assert.Contains("1 Numaralı Kaynak Test Güzergâhı Hattı Taşıma İşi", report, StringComparison.Ordinal);
        Assert.Contains("31.10.2026 TARİHİNE KADAR YAPILAN İŞİN", report, StringComparison.Ordinal);
        Assert.DoesNotContain("30.04.2026 TARİHİNE KADAR YAPILAN İŞİN", report, StringComparison.Ordinal);

        var service = PageText(outputs, OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu);
        Assert.Contains("19.10.2026", service, StringComparison.Ordinal);
        Assert.Contains("Sözleşme Tarihi ve No.su", service, StringComparison.Ordinal);
        Assert.Contains("181 İş Günü", service, StringComparison.Ordinal);
        Assert.Contains("M.Y.H.B.Y. Örnek No: 4", service, StringComparison.Ordinal);
        Assert.Contains("No.su" + Environment.NewLine + ":" + Environment.NewLine + "1", service, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("Örnek Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("İşyeri Teslim Tarihi" + Environment.NewLine + ":" + Environment.NewLine + "14.09.2026", service, StringComparison.Ordinal);

        var acceptance = PageText(outputs, OehsOfficialDocumentKind.KabulTutanagi);
        Assert.Contains("01.10.2026 / 31.10.2026", acceptance, StringComparison.Ordinal);
        Assert.Contains("181 İş Günü", acceptance, StringComparison.Ordinal);
        Assert.Contains("14.09.2026", acceptance, StringComparison.Ordinal);
        Assert.Contains("25.06.2027", acceptance, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", acceptance, StringComparison.Ordinal);

        var icmal = PageText(outputs, OehsOfficialDocumentKind.HakedisIcmali);
        Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmal, StringComparison.Ordinal);
        Assert.Contains("₺53.322,33", icmal, StringComparison.Ordinal); // September base service
        Assert.Contains("₺1.403,57", icmal, StringComparison.Ordinal);  // price difference once
        Assert.Contains("₺54.725,90", icmal, StringComparison.Ordinal); // September A+B equals K+T
        Assert.DoesNotContain("₺56.129,48", icmal, StringComparison.Ordinal);
        Assert.Contains("₺91.914,70", icmal, StringComparison.Ordinal); // current base service
        Assert.Contains("₺93.318,27", icmal, StringComparison.Ordinal); // current A+B equals K+T
        Assert.DoesNotContain("₺94.721,85", icmal, StringComparison.Ordinal);
        Assert.Contains("603,02", icmal, StringComparison.Ordinal);
        Assert.Contains("968,87", icmal, StringComparison.Ordinal);
        Assert.Contains("1.571,89", icmal, StringComparison.Ordinal);
        Assert.Contains("146.472,29", icmal, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", icmal, StringComparison.Ordinal);
    }

    private static void AssertBasicEducationSourceFields(
        IReadOnlyList<OehsOfficialDocumentOutput> outputs)
    {
        var report = PageText(outputs, OehsOfficialDocumentKind.HakedisRaporu);
        Assert.Contains("192.009,97 TL", report, StringComparison.Ordinal);
        Assert.Contains("7.365,35 TL", report, StringComparison.Ordinal);
        Assert.Contains("199.375,32 TL", report, StringComparison.Ordinal);
        Assert.Contains("219.312,85 TL", report, StringComparison.Ordinal);
        Assert.Contains("1.890,08 TL", report, StringComparison.Ordinal);
        Assert.Contains("12.300,77 TL", report, StringComparison.Ordinal);
        Assert.Contains("207.012,08 TL", report, StringComparison.Ordinal);
        Assert.Contains("1 Numaralı Kaynak Test Güzergâhı Hattı Taşıma İşi", report, StringComparison.Ordinal);
        Assert.Contains("30.09.2026 TARİHİNE KADAR YAPILAN İŞİN", report, StringComparison.Ordinal);

        var service = PageText(outputs, OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu);
        Assert.Contains("19.10.2026", service, StringComparison.Ordinal);
        Assert.Contains("Sözleşme Tarihi ve No.su", service, StringComparison.Ordinal);
        Assert.Contains("14.09.2026-25.06.2027 Tarihleri Arasıdır.", service, StringComparison.Ordinal);
        Assert.Contains("M.Y.H.B.Y. Örnek No: 4", service, StringComparison.Ordinal);
        Assert.Contains("No.su" + Environment.NewLine + ":" + Environment.NewLine + "1", service, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("Örnek Öğrenci Taşıma İşi", service, StringComparison.Ordinal);
        Assert.Contains("İşyeri Teslim Tarihi" + Environment.NewLine + ":" + Environment.NewLine + "14.09.2026", service, StringComparison.Ordinal);

        var acceptance = PageText(outputs, OehsOfficialDocumentKind.KabulTutanagi);
        Assert.Contains("14.09.2026 / 30.09.2026", acceptance, StringComparison.Ordinal);
        Assert.Contains("181", acceptance, StringComparison.Ordinal);
        Assert.DoesNotContain("181 İş Günü", acceptance, StringComparison.Ordinal);
        Assert.Contains("25.06.2027", acceptance, StringComparison.Ordinal);
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", acceptance, StringComparison.Ordinal);

        var icmal = PageText(outputs, OehsOfficialDocumentKind.HakedisIcmali);
        Assert.Contains("Hizmet Alımı İşi Eylül 2026", icmal, StringComparison.Ordinal);
        Assert.Contains("₺192.009,97", icmal, StringComparison.Ordinal); // base service excludes I
        Assert.Contains("₺27.031,47", icmal, StringComparison.Ordinal);  // I plus deductions once
        Assert.Contains("₺219.041,44", icmal, StringComparison.Ordinal); // row total without duplicate I
        Assert.Contains("Kaynak Test Güzergâhı Hattı Öğrenci Taşıma İşi", icmal, StringComparison.Ordinal);
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
