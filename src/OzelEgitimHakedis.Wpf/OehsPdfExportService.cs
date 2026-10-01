using System.IO;
using System.Windows;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using PdfSharp.Drawing;
using PdfSharp.Pdf;

namespace OzelEgitimHakedis.Wpf;

internal static class OehsPdfExportService
{
    private const double Dpi = 200d;
    private const double DipsPerInch = 96d;
    private const double PdfA4WidthPoints = 595.2755905511812d;
    private const double PdfA4HeightPoints = 841.8897637795276d;

    public static IReadOnlyList<BitmapSource> RenderDocumentPages(FlowDocument document)
    {
        ArgumentNullException.ThrowIfNull(document);

        var paginator = ((IDocumentPaginatorSource)document).DocumentPaginator;
        paginator.PageSize = new Size(
            PdfA4WidthPoints / 72d * DipsPerInch,
            PdfA4HeightPoints / 72d * DipsPerInch);
        paginator.ComputePageCount();

        var pages = new List<BitmapSource>(paginator.PageCount);
        for (var index = 0; index < paginator.PageCount; index++)
        {
            var documentPage = paginator.GetPage(index);
            var visual = new DrawingVisual();

            using (var drawingContext = visual.RenderOpen())
            {
                drawingContext.DrawRectangle(
                    Brushes.White,
                    null,
                    new Rect(new Point(0, 0), documentPage.Size));

                if (documentPage.Visual is Visual pageVisual)
                {
                    var brush = new VisualBrush(pageVisual)
                    {
                        Stretch = Stretch.None,
                        AlignmentX = AlignmentX.Left,
                        AlignmentY = AlignmentY.Top
                    };
                    drawingContext.DrawRectangle(
                        brush,
                        null,
                        new Rect(new Point(0, 0), documentPage.Size));
                }
            }

            var pixelWidth = Math.Max(1, (int)Math.Ceiling(documentPage.Size.Width * Dpi / DipsPerInch));
            var pixelHeight = Math.Max(1, (int)Math.Ceiling(documentPage.Size.Height * Dpi / DipsPerInch));

            var bitmap = new RenderTargetBitmap(
                pixelWidth,
                pixelHeight,
                Dpi,
                Dpi,
                PixelFormats.Pbgra32);

            bitmap.Render(visual);
            bitmap.Freeze();
            pages.Add(bitmap);
        }

        return pages;
    }

    public static void ExportFlowDocumentPages(
        FlowDocument document,
        string targetPath)
    {
        ArgumentNullException.ThrowIfNull(document);
        ArgumentException.ThrowIfNullOrWhiteSpace(targetPath);

        var pages = RenderDocumentPages(document);
        var targetFolder = Path.GetDirectoryName(targetPath);
        if (!string.IsNullOrWhiteSpace(targetFolder))
            Directory.CreateDirectory(targetFolder);

        var pdf = new PdfDocument();
        pdf.Info.Title = Path.GetFileNameWithoutExtension(targetPath);
        pdf.Info.Creator = "OEHS — Özel Eğitim Hakediş Sistemi";
        pdf.Info.Subject = "OEHS dört belge hakediş çıktısı";

        try
        {
            foreach (var bitmap in pages)
            {
                using var pngStream = new MemoryStream();
                var encoder = new PngBitmapEncoder();
                encoder.Frames.Add(BitmapFrame.Create(bitmap));
                encoder.Save(pngStream);

                pngStream.Position = 0;

                var page = pdf.AddPage();
                page.Width = XUnit.FromPoint(PdfA4WidthPoints);
                page.Height = XUnit.FromPoint(PdfA4HeightPoints);
                page.Orientation = PdfSharp.PageOrientation.Portrait;

                using var image = XImage.FromStream(pngStream);
                using var graphics = XGraphics.FromPdfPage(page);

                graphics.DrawImage(
                    image,
                    0,
                    0,
                    PdfA4WidthPoints,
                    PdfA4HeightPoints);
            }

            pdf.Save(targetPath);
        }
        finally
        {
            pdf.Dispose();
        }
    }
}
