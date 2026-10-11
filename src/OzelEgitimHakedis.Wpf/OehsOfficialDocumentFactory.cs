using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Documents;
using System.Windows.Markup;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

internal sealed record OehsOfficialDocumentContext(
    string WorkName,
    string TenderNumber,
    string RouteNumber,
    string ContractorName,
    string ContractNumber,
    decimal ContractAmount,
    DateOnly? TenderDate,
    DateOnly? ContractDate,
    DateOnly? ContractStartDate,
    DateOnly? ContractEndDate,
    int ContractWorkDays,
    DateOnly? PeriodDate,
    int? ProgressPaymentNumber,
    ProgressPaymentCalculation Calculation,
    IReadOnlyList<ProgressPaymentHistoryData> Payments,
    ProgressPaymentReportTotals ReportTotals,
    decimal ReportNetPayable,
    decimal PreviousDeduction,
    DateOnly GeneratedDate);

internal enum OehsOfficialDocumentKind
{
    HakedisRaporu,
    HizmetIsleriHakedisRaporu,
    KabulTutanagi,
    HakedisIcmali
}

internal sealed record OehsOfficialDocumentOutput(
    OehsOfficialDocumentKind Kind,
    string Title,
    string FileSuffix,
    FixedDocument Document);

/// <summary>
/// Builds fixed A4 pages from one shared data snapshot. Preview, printing and
/// PDF export all use these same page builders so data/layout do not drift apart.
/// </summary>
internal static class OehsOfficialDocumentFactory
{
    private const double PageWidth = 793.7007874d;   // A4 portrait, 96 DIP/inch
    private const double PageHeight = 1122.519685d;
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly Brush Black = Brushes.Black;
    private static readonly Brush White = Brushes.White;
    private static readonly Brush TotalGray = new SolidColorBrush(Color.FromRgb(217, 217, 217));
    private static readonly Brush WarningRed = new SolidColorBrush(Color.FromRgb(255, 0, 0));
    private static readonly Brush LightGray = new SolidColorBrush(Color.FromRgb(242, 242, 242));

    public static FixedDocument BuildCombined(OehsOfficialDocumentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return CreateDocument(
            BuildHakedisRaporu(context),
            BuildHizmetIsleriHakedisRaporu(context),
            BuildKabulTutanagi(context),
            BuildHakedisIcmali(context));
    }

    public static IReadOnlyList<OehsOfficialDocumentOutput> BuildSeparate(OehsOfficialDocumentContext context)
    {
        ArgumentNullException.ThrowIfNull(context);
        return new[]
        {
            new OehsOfficialDocumentOutput(
                OehsOfficialDocumentKind.HakedisRaporu,
                "Hakediş Raporu",
                "01_Hakedis_Raporu",
                CreateDocument(BuildHakedisRaporu(context))),
            new OehsOfficialDocumentOutput(
                OehsOfficialDocumentKind.HizmetIsleriHakedisRaporu,
                "Hizmet İşleri Hakediş Raporu",
                "02_Hizmet_Isleri_Hakedis_Raporu",
                CreateDocument(BuildHizmetIsleriHakedisRaporu(context))),
            new OehsOfficialDocumentOutput(
                OehsOfficialDocumentKind.KabulTutanagi,
                "Hizmet İşleri Kabul Tutanağı",
                "03_Hizmet_Isleri_Kabul_Tutanagi",
                CreateDocument(BuildKabulTutanagi(context))),
            new OehsOfficialDocumentOutput(
                OehsOfficialDocumentKind.HakedisIcmali,
                "Hakediş İcmali",
                "04_Hakedis_Icmali",
                CreateDocument(BuildHakedisIcmali(context)))
        };
    }

    private static FixedDocument CreateDocument(params FixedPage[] pages)
    {
        var document = new FixedDocument();
        document.DocumentPaginator.PageSize = new Size(PageWidth, PageHeight);
        foreach (var page in pages)
        {
            page.Measure(new Size(PageWidth, PageHeight));
            page.Arrange(new Rect(0, 0, PageWidth, PageHeight));
            page.UpdateLayout();

            var pageContent = new PageContent();
            ((IAddChild)pageContent).AddChild(page);
            document.Pages.Add(pageContent);
        }
        return document;
    }

    private static FixedPage NewPage() => new()
    {
        Width = PageWidth,
        Height = PageHeight,
        Background = White,
        UseLayoutRounding = true,
        SnapsToDevicePixels = true
    };

    private static FixedPage BuildHakedisRaporu(OehsOfficialDocumentContext c)
    {
        var page = NewPage();
        const double x = 16;
        const double w = 761;
        var tender = string.IsNullOrWhiteSpace(c.TenderNumber) ? "-" : c.TenderNumber.Trim();
        var routeNo = string.IsNullOrWhiteSpace(c.RouteNumber) ? "-" : c.RouteNumber.Trim();
        var paymentNo = c.ProgressPaymentNumber?.ToString(Turkish) ?? "-";
        var date = FormatDate(c.PeriodDate);

        AddText(page, "HAK EDİŞ RAPORU", x, 16, w, 29, 16, true, TextAlignment.Center);
        AddCell(page,
            $"ı Numaralı {c.WorkName}\n",
            x, 47, 505, 77, 10, true, TextAlignment.Left, White, new Thickness(7, 5, 6, 3));
        AddCell(page, $"Hakediş No : {paymentNo}", x + 505, 47, w - 505, 38, 11, true, TextAlignment.Center);
        AddCell(page, $"Hat No : {routeNo}", x + 505, 85, w - 505, 39, 11, true, TextAlignment.Center);

        AddCell(page, $"{date} TARİHİNE KADAR YAPILAN İŞİN", x, 132, w, 29, 11, true, TextAlignment.Center, LightGray);
        var amountRows = new (string Key, string Label, decimal Value)[]
        {
            ("A", "Sözleşme Fiyatları İle Yapılan İş", c.ReportTotals.CumulativeServiceAmount),
            ("B", "Fiyat Farkı Tutarı", c.ReportTotals.CurrentPriceDifference),
            ("C", "Toplam Tutar  ( A + B )", c.ReportTotals.CumulativeTotal)
        };
        var y = 161d;
        foreach (var row in amountRows)
        {
            var bold = row.Key == "C";
            AddCell(page, row.Key, x, y, 48, 29, 10, bold, TextAlignment.Center);
            AddCell(page, row.Label, x + 48, y, 430, 29, 10, bold, TextAlignment.Left);
            AddCell(page, FormatMoney(row.Value), x + 478, y, w - 478, 29, 10, true, TextAlignment.Right, White, new Thickness(4, 2, 10, 2));
            y += 29;
        }

        y += 24;
        var summaryRows = new (string Key, string Label, decimal Value, Brush Fill)[]
        {
            ("D", "Bir Önceki Hakedişlerin Toplam Tutarı", c.ReportTotals.PreviousTotal, White),
            ("E", "Bu Hakedişin Tutarı ( C - D )", c.ReportTotals.CurrentTotal, WarningRed),
            ("F", "KDV %10 + %20", c.Calculation.Vat, White),
            ("G", "Tahakkuk Tutarı", c.ReportTotals.CurrentTotal + c.Calculation.Vat, TotalGray)
        };
        foreach (var row in summaryRows)
        {
            var bold = row.Key is "G" or "E";
            AddCell(page, row.Key, x, y, 48, 29, 10, bold, TextAlignment.Center, row.Fill);
            AddCell(page, row.Label, x + 48, y, 430, 29, 10, bold, TextAlignment.Left, row.Fill);
            AddCell(page, FormatMoney(row.Value), x + 478, y, w - 478, 29, 10, true, TextAlignment.Right, row.Fill, new Thickness(4, 2, 10, 2));
            y += 29;
        }

        y += 18;
        var deductions = new (string Key, string Label, decimal Value, double Height)[]
        {
            ("a)", "Gelir / Kurumlar Vergisi ( E x % .. )", 0m, 22),
            ("b)", "Damga Vergisi ( E - g x % 9,48 )", c.Calculation.StampTax, 22),
            ("c)", "KDV Tevkifatı ( 5/10 + 9/10 kesilen)", c.Calculation.Withholding, 22),
            ("d)", "Sosyal Sigortalar Kurumu Kesintisi", c.Calculation.Deductions.SocialSecurityDebt, 22),
            ("e)", "İdare Makinesi Kiraları", 0m, 22),
            ("f)", "Gecikme Cezası", 0m, 22),
            ("g)", "Avans Mahsubu", 0m, 22),
            ("h)", "Bu Hakedişle Ödenen Fiyat Farkı Teminat Kesintisi", c.Calculation.AdditionalGuarantee, 32),
            ("ı)", "Vergi Borcu", c.Calculation.Deductions.TaxDebt, 22),
            ("j)", "İcra Borcu", 0m, 22),
            ("k)", "Ceza ve Diğer Kesintiler", c.Calculation.Deductions.InspectionPenalty, 22),
            ("l)", "Asgari Ücret Destek Tutarı Kesintisi", c.Calculation.Deductions.MinimumWageSupport, 22)
        };

        var deductionsHeight = deductions.Sum(d => d.Height) + 2 * 29;
        AddCell(page, "KESİNTİLER\nVE\nMAHSUPLAR", x, y, 66, deductionsHeight, 10, true, TextAlignment.Center, White,
            new Thickness(4, 4, 4, 4));
        foreach (var row in deductions)
        {
            var fill = White;
            AddCell(page, row.Key, x + 66, y, 45, row.Height, 9.5, false, TextAlignment.Center, fill);
            AddCell(page, row.Label, x + 111, y, 430, row.Height, 9.5, false, TextAlignment.Left, fill);
            AddCell(page, FormatMoney(row.Value), x + 541, y, w - 541, row.Height, 9.5, true, TextAlignment.Right, fill, new Thickness(3, 2, 10, 2));
            y += row.Height;
        }

        AddCell(page, "H", x + 66, y, 45, 29, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, "Kesintiler ve Mahsuplar Toplamı", x + 111, y, 430, 29, 10, true, TextAlignment.Left, TotalGray);
        AddCell(page, FormatMoney(c.Calculation.TotalDeductions), x + 541, y, w - 541, 29, 10, true, TextAlignment.Right, TotalGray, new Thickness(3, 2, 10, 2));
        y += 29;
        AddCell(page, "I", x + 66, y, 45, 29, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, "Yükleniciye Ödenecek Tutar ( G - H )", x + 111, y, 430, 29, 10, true, TextAlignment.Left, TotalGray);
        AddCell(page, FormatMoney(c.ReportNetPayable), x + 541, y, w - 541, 29, 10, true, TextAlignment.Right, TotalGray, new Thickness(3, 2, 10, 2));

        var footerY = Math.Max(y + 38, 828);
        AddText(page, "Yüklenici", x + 30, footerY, w - 60, 22, 10, true, TextAlignment.Left);
        AddText(page, c.ContractorName, x + 30, footerY + 23, w - 60, 36, 10, true, TextAlignment.Left);
        AddSignatures(page, footerY + 85, c.GeneratedDate, "M.Y.H.B.Y. Örnek No :4/10");
        return page;
    }

    private static FixedPage BuildHizmetIsleriHakedisRaporu(OehsOfficialDocumentContext c)
    {
        var page = NewPage();
        const double x = 16;
        const double w = 761;
        AddText(page, "HİZMET İŞLERİ HAK EDİŞ RAPORU", x, 17, w, 30, 15, true, TextAlignment.Center);

        var info = new (string Label, string Value, double Height)[]
        {
            ("Tarihi", FormatDate(c.PeriodDate), 25),
            ("No.su", c.ProgressPaymentNumber?.ToString(Turkish) ?? "-", 25),
            ("Uygulama Yılı", (c.PeriodDate?.Year ?? c.GeneratedDate.Year).ToString(Turkish), 25),
            ("Yapılan Hizmetin Adı", c.WorkName, 70),
            ("Yapılan Hizmetin Etüt / Proje No.su", "Proje Dışıdır.", 28),
            ("Yüklenicinin Adı / Ticari Unvanı", c.ContractorName, 28),
            ("Sözleşme Bedeli", FormatMoney(c.ContractAmount), 28),
            ("İhale Tarihi", FormatDate(c.TenderDate), 28),
            ("Sözleşme Tarihi ve No.su", $"{FormatDate(c.ContractDate)}{(string.IsNullOrWhiteSpace(c.ContractNumber) ? "" : " / " + c.ContractNumber)}", 28),
            ("Sözleşmenin Sayıştayca Tescil Tarihi ve No'su", "Tescile Dahil Değil", 28),
            ("Tescilin Yükleniciye Tebliğ Tarihi", "Tescile Dahil Değil", 28),
            ("İşyeri Teslim Tarihi", FormatDate(c.ContractStartDate), 28),
            ("Sözleşmeye Göre İşin Süresi", $"{CalculateDuration(c)} İş Günü", 28),
            ("Sözleşmeye Göre İş Bitim Tarihi", FormatDate(c.ContractEndDate), 28),
            ("Verilen Avanslar Toplamı", "Avans verilmeyecektir.", 28),
            ("Mahsubu Yapılan Avansın Toplam Tutarı", "Yoktur.", 28)
        };

        var y = 51d;
        foreach (var row in info)
        {
            AddCell(page, row.Label, x, y, 360, row.Height, 9.5, true, TextAlignment.Left);
            AddCell(page, ":  " + row.Value, x + 360, y, w - 360, row.Height, 9.5, false, TextAlignment.Left);
            y += row.Height;
        }

        var colWidths = new[] { 164d, 202d, 184d, w - 164d - 202d - 184d };
        var headings = new[]
        {
            "Sözleşme Bedeli",
            "Sözleşme Artış\nOnayının Tarihi No.su",
            "Ek Sözleşme\nBedeli",
            "Toplam Sözleşme\nBedeli"
        };
        var tx = x;
        for (var i = 0; i < colWidths.Length; i++)
        {
            AddCell(page, headings[i], tx, y, colWidths[i], 42, 9, true, TextAlignment.Center, White);
            tx += colWidths[i];
        }
        y += 42;
        for (var row = 0; row < 6; row++)
        {
            var values = row == 0
                ? new[] { FormatMoney(c.ContractAmount, false), "........................", "........................", FormatMoney(c.ContractAmount, false) }
                : new[] { "........................", "........................", "........................", "........................" };
            tx = x;
            for (var col = 0; col < colWidths.Length; col++)
            {
                AddCell(page, values[col], tx, y, colWidths[col], 22, 8.5, row == 0 && col is 0 or 3, TextAlignment.Center);
                tx += colWidths[col];
            }
            y += 22;
        }

        AddCell(page, "Süre Uzatım Kararlarının", x, y, w, 24, 9.5, true, TextAlignment.Left, LightGray);
        y += 24;
        var extensionWidths = new[] { 170d, 140d, 225d, w - 170d - 140d - 225d };
        var extensionHeads = new[] { "Tarih", "Sayısı", "Verilen Süre", "İş Bitim Tarihi" };
        tx = x;
        for (var i = 0; i < extensionWidths.Length; i++)
        {
            AddCell(page, extensionHeads[i], tx, y, extensionWidths[i], 27, 9, true, TextAlignment.Center);
            tx += extensionWidths[i];
        }
        y += 27;
        for (var row = 0; row < 4; row++)
        {
            tx = x;
            for (var col = 0; col < extensionWidths.Length; col++)
            {
                AddCell(page, "....................", tx, y, extensionWidths[col], 19, 8.5, false, TextAlignment.Center);
                tx += extensionWidths[col];
            }
            y += 19;
        }

        AddSignatures(page, Math.Max(y + 30, 855), c.GeneratedDate, "M.Y.H.B.Y. Örnek No: 4");
        return page;
    }

    private static FixedPage BuildKabulTutanagi(OehsOfficialDocumentContext c)
    {
        var page = NewPage();
        const double x = 16;
        const double w = 761;
        AddText(page, "HİZMET İŞLERİ KABUL TUTANAĞI", x, 17, w, 30, 15, true, TextAlignment.Center);

        var servicePeriod = ProgressPaymentAcceptancePeriodService.BuildServicePeriod(
            c.PeriodDate ?? c.GeneratedDate);
        var endDate = c.ContractEndDate;
        var duration = CalculateDuration(c);
        var rows = new (string Label, string Value, double Height)[]
        {
            ("İşin adı", c.WorkName, 82),
            ("Yüklenicinin adı/ticari unvanı", c.ContractorName, 36),
            ("Sözleşme tarihi", FormatDate(c.ContractDate), 36),
            ("Sözleşme bedeli", FormatMoney(c.ContractAmount), 36),
            ("Sözleşmeye göre işin süresi(takvim günü)", $"{duration} İş Günü", 36),
            ("Sözleşmeye göre işin bitirilmesi gereken tarih", FormatDate(endDate), 36),
            ("Varsa süre uzatımları", "", 36),
            ("Süre uzatımı dahil işin bitirilmesi gereken tarih", FormatLongDate(endDate), 36),
            ("İşin bitirildiği tarih", FormatLongDate(endDate), 36)
        };

        var y = 55d;
        foreach (var row in rows)
        {
            AddCell(page, row.Label, x, y, 347, row.Height, 9.5, true, TextAlignment.Left);
            AddCell(page, row.Value, x + 347, y, w - 347, row.Height, 9.5, false, TextAlignment.Left);
            y += row.Height;
        }

        y += 43;
        var paragraphOne =
            $"İlçe Milli Eğitim Müdürlüğü'nün {c.RouteNumber} Numaralı {c.WorkName} işi yüklenicisi {c.ContractorName} arasında imzalanan sözleşme kapsamında gerçekleştirilen iş için, Memur Aşkın KESKİNBALTA ve İlçe Milli Eğitim Şube Müdürü Halil ÖZENÇ tarafından inceleme yapılmış yüklenici, {c.ContractorName} tarafından {servicePeriod.Start:dd.MM.yyyy} / {servicePeriod.End:dd.MM.yyyy} tarihleri arasında yapılan işlerin,";
        var paragraphTwo =
            "Sözleşme ve eklerine uygun olduğu ve kabule engel olabilecek eksik kusur ve arızaların bulunmadığı görülmüştür.";
        AddText(page, paragraphOne, x + 2, y, w - 4, 83, 9.5, false, TextAlignment.Justify);
        y += 91;
        AddText(page, paragraphTwo, x + 2, y, w - 4, 44, 9.5, false, TextAlignment.Justify);

        AddText(page, "Yüklenici", x + 40, 738, w - 80, 25, 10, true, TextAlignment.Center);
        AddText(page, c.ContractorName, x + 40, 763, w - 80, 34, 10, false, TextAlignment.Center);
        AddSignatures(page, 854, c.GeneratedDate, "");
        return page;
    }

    private static FixedPage BuildHakedisIcmali(OehsOfficialDocumentContext c)
    {
        var page = NewPage();
        const double x = 16;
        const double w = 761;
        AddText(page, "HAKEDİŞ İCMALİ", x, 16, w, 28, 15, true, TextAlignment.Center);

        AddCell(page, c.WorkName, x, 46, 502, 80, 9, true, TextAlignment.Left, White, new Thickness(6, 5, 6, 4));
        AddCell(page, "Sayfa No : 1", x + 502, 46, w - 502, 27, 9.5, true, TextAlignment.Left);
        AddCell(page, $"Hakediş No : {c.ProgressPaymentNumber?.ToString(Turkish) ?? "-"}", x + 502, 73, w - 502, 27, 9.5, true, TextAlignment.Left);
        AddCell(page, $"Hat No : {c.RouteNumber}", x + 502, 100, w - 502, 26, 9.5, true, TextAlignment.Left);

        var widths = new[] { 66d, 282d, 138d, 138d, w - 66d - 282d - 138d - 138d };
        var headers = new[] { "Sıra No", "İşin Cinsi", "İş Tutarı\n(a)", "Fiyat Farkı Tutarı\n(b)", "Toplam\n(a+b)" };
        var y = 145d;
        var xx = x;
        for (var i = 0; i < widths.Length; i++)
        {
            AddCell(page, headers[i], xx, y, widths[i], 48, 9, true, TextAlignment.Center, LightGray);
            xx += widths[i];
        }
        y += 48;

        var periods = ProgressPaymentIcmalService.BuildPeriods(c.PeriodDate ?? c.GeneratedDate);
        decimal sumService = 0m;
        decimal sumDifference = 0m;
        for (var index = 0; index < periods.Count; index++)
        {
            var month = periods[index];
            var monthPayments = c.Payments
                .Where(p => p.PeriodDate.HasValue &&
                            p.PeriodDate.Value.Year == month.Year &&
                            p.PeriodDate.Value.Month == month.Month)
                .ToArray();
            var service = monthPayments.Sum(ProgressPaymentReportService.CalculateWorkbookServiceAmount);
            var difference = monthPayments.Sum(ProgressPaymentReportService.CalculateWorkbookPriceDifference);
            sumService += service;
            sumDifference += difference;
            var values = new[]
            {
                (index + 1).ToString(Turkish),
                "Hizmet Alımı İşi " + month.ToString("MMMM yyyy", Turkish),
                FormatLira(service),
                FormatLira(difference),
                FormatLira(service + difference)
            };
            xx = x;
            for (var col = 0; col < widths.Length; col++)
            {
                AddCell(page, values[col], xx, y, widths[col], 27, 8.8, col == 0, col == 0 ? TextAlignment.Center : col == 1 ? TextAlignment.Left : TextAlignment.Right,
                    White, new Thickness(4, 2, 6, 2));
                xx += widths[col];
            }
            y += 27;
        }

        xx = x;
        var totals = new[]
        {
            "Toplam", "", FormatLira(sumService), FormatLira(sumDifference), FormatLira(sumService + sumDifference)
        };
        for (var col = 0; col < widths.Length; col++)
        {
            AddCell(page, totals[col], xx, y, widths[col], 28, 9.3, true, col < 2 ? TextAlignment.Center : TextAlignment.Right, TotalGray,
                new Thickness(4, 2, 6, 2));
            xx += widths[col];
        }
        y += 28;
        AddCell(page, "A", x, y, 66, 38, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, "T  O  P  L  A  M", x + 66, y, widths[1] + widths[2] + widths[3], 38, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, FormatLira(sumService + sumDifference), x + 66 + widths[1] + widths[2] + widths[3], y, widths[4], 38, 10, true, TextAlignment.Right, TotalGray, new Thickness(4, 2, 6, 2));
        y += 52;

        AddCell(page, "Hakediş İçeriğinden Yapılan Kesintiler", x, y, w, 28, 10, true, TextAlignment.Center, LightGray);
        y += 28;
        AddCell(page, "a)", x, y, 66, 27, 9.5, true, TextAlignment.Center);
        AddCell(page, "Bir Evvelki Hakedişten Yapılan Kesinti", x + 66, y, w - 66 - 170, 27, 9.5, false, TextAlignment.Left);
        AddCell(page, FormatMoney(c.PreviousDeduction, false), x + w - 170, y, 170, 27, 9.5, false, TextAlignment.Right, White, new Thickness(3, 2, 8, 2));
        y += 27;
        AddCell(page, "b)", x, y, 66, 27, 9.5, true, TextAlignment.Center);
        AddCell(page, "Bu Hakedişten Yapılan Kesinti", x + 66, y, w - 66 - 170, 27, 9.5, false, TextAlignment.Left);
        AddCell(page, FormatMoney(c.Calculation.TotalDeductions, false), x + w - 170, y, 170, 27, 9.5, false, TextAlignment.Right, White, new Thickness(3, 2, 8, 2));
        y += 27;
        AddCell(page, "B", x, y, 66, 30, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, "Toplam Kesinti Tutarı", x + 66, y, w - 66 - 170, 30, 10, true, TextAlignment.Left, TotalGray);
        AddCell(page, FormatMoney(c.PreviousDeduction + c.Calculation.TotalDeductions, false), x + w - 170, y, 170, 30, 10, true, TextAlignment.Right, TotalGray, new Thickness(3, 2, 8, 2));
        y += 30;
        AddCell(page, "C", x, y, 66, 32, 10, true, TextAlignment.Center, TotalGray);
        AddCell(page, "Toplam Hakediş Tutarı : ( A - B )", x + 66, y, w - 66 - 170, 32, 10, true, TextAlignment.Left, TotalGray);
        AddCell(page, FormatMoney(sumService + sumDifference - c.PreviousDeduction - c.Calculation.TotalDeductions, false), x + w - 170, y, 170, 32, 10, true, TextAlignment.Right, TotalGray, new Thickness(3, 2, 8, 2));

        AddSignatures(page, 866, c.GeneratedDate, "M.Y.H.B.Y. Örnek No :4/9");
        return page;
    }

    private static void AddSignatures(FixedPage page, double y, DateOnly generatedDate, string footer)
    {
        AddText(page, "Düzenleyen", 100, y, 220, 24, 10, false, TextAlignment.Center);
        AddText(page, "Kontrol Eden", 482, y, 220, 24, 10, false, TextAlignment.Center);
        AddText(page, "Aşkın KESKİNBALTA", 100, y + 34, 220, 25, 10, true, TextAlignment.Center);
        AddText(page, "Halil ÖZENÇ", 482, y + 34, 220, 25, 10, true, TextAlignment.Center);
        AddText(page, "Memur", 100, y + 57, 220, 23, 9.5, false, TextAlignment.Center);
        AddText(page, "İlçe Milli Eğitim Şube Müdürü", 452, y + 57, 280, 23, 9.5, false, TextAlignment.Center);

        AddText(page, generatedDate.ToString("dd.MM.yyyy", Turkish), 270, y + 102, 250, 24, 10, true, TextAlignment.Center);
        AddText(page, "Özgür BAYSAL", 270, y + 142, 250, 23, 10, true, TextAlignment.Center);
        AddText(page, "İlçe Milli Eğitim Müdürü", 270, y + 164, 250, 22, 9.5, false, TextAlignment.Center);
        if (!string.IsNullOrWhiteSpace(footer))
            AddText(page, footer, 17, PageHeight - 26, 330, 17, 8, false, TextAlignment.Left);
    }

    private static int CalculateDuration(OehsOfficialDocumentContext c)
    {
        if (c.ContractWorkDays > 0)
            return c.ContractWorkDays;
        if (!c.ContractStartDate.HasValue || !c.ContractEndDate.HasValue)
            return 0;
        return Math.Max(0, c.ContractEndDate.Value.DayNumber - c.ContractStartDate.Value.DayNumber);
    }

    private static string FormatDate(DateOnly? date) =>
        date?.ToString("dd.MM.yyyy", Turkish) ?? "-";

    private static string FormatLongDate(DateOnly? date) =>
        date?.ToDateTime(TimeOnly.MinValue).ToString("dd MMMM yyyy dddd", Turkish) ?? "-";

    private static string FormatMoney(decimal value, bool withCurrency = true)
    {
        var formatted = value.ToString("#,##0.00", Turkish);
        return withCurrency ? formatted + " TL" : formatted;
    }

    private static string FormatLira(decimal value) =>
        "₺" + value.ToString("#,##0.00", Turkish);

    private static void AddText(
        FixedPage page,
        string text,
        double x,
        double y,
        double width,
        double height,
        double fontSize,
        bool bold,
        TextAlignment alignment,
        Brush? foreground = null)
    {
        var block = MakeText(text, fontSize, bold, alignment, foreground, new Thickness(0));
        block.Width = width;
        block.Height = height;
        FixedPage.SetLeft(block, x);
        FixedPage.SetTop(block, y);
        page.Children.Add(block);
    }

    private static void AddCell(
        FixedPage page,
        string text,
        double x,
        double y,
        double width,
        double height,
        double fontSize,
        bool bold,
        TextAlignment alignment,
        Brush? background = null,
        Thickness? padding = null)
    {
        var block = MakeText(text, fontSize, bold, alignment, Black, new Thickness(0));
        var border = new Border
        {
            Width = width,
            Height = height,
            Background = background ?? White,
            BorderBrush = Black,
            BorderThickness = new Thickness(0.65),
            Padding = padding ?? new Thickness(5, 2, 5, 2),
            Child = block,
            SnapsToDevicePixels = true
        };
        FixedPage.SetLeft(border, x);
        FixedPage.SetTop(border, y);
        page.Children.Add(border);
    }

    private static TextBlock MakeText(
        string text,
        double fontSize,
        bool bold,
        TextAlignment alignment,
        Brush? foreground,
        Thickness margin) => new()
    {
        Text = text ?? string.Empty,
        FontFamily = new FontFamily("Arial"),
        FontSize = fontSize,
        FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
        Foreground = foreground ?? Black,
        TextAlignment = alignment,
        TextWrapping = TextWrapping.Wrap,
        VerticalAlignment = VerticalAlignment.Center,
        HorizontalAlignment = HorizontalAlignment.Stretch,
        Margin = margin,
        UseLayoutRounding = true,
        SnapsToDevicePixels = true
    };
}
