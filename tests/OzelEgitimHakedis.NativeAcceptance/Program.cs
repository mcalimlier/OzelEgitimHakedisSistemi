using System.Collections;
using System.Globalization;
using System.IO;
using System.Reflection;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Threading;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Wpf;

// Runs the actual application window, real editable grid cells and Calculate/Save button.
// Its own output directory is always marked as an isolated test package BEFORE paths resolve.
internal static class PayrollNativeAcceptance
{
    private static readonly CultureInfo Turkish = CultureInfo.GetCultureInfo("tr-TR");
    private static readonly BindingFlags Private = BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public;
    private static string output = "";
    private static T Control<T>(MainWindow window, string name) => (T)window.FindName(name);
    private static object Call(MainWindow window, string name, params object[] args) => typeof(MainWindow).GetMethod(name, Private)!.Invoke(window, args)!;
    private static T Property<T>(object owner, string name) => (T)owner.GetType().GetProperty(name, Private)!.GetValue(owner)!;
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); }
    private static void Pump()
    {
        var frame = new DispatcherFrame();
        Dispatcher.CurrentDispatcher.BeginInvoke(DispatcherPriority.Background, new Action(() => frame.Continue = false));
        Dispatcher.PushFrame(frame);
    }

    [STAThread]
    private static int Main(string[] args)
    {
        if (args.Length != 2) { Console.Error.WriteLine("Usage: NativeAcceptance <fixture.json> <evidence-directory>"); return 2; }
        CultureInfo.CurrentCulture = Turkish;
        CultureInfo.CurrentUICulture = Turkish;
        output = Path.GetFullPath(args[1]); Directory.CreateDirectory(output);
        File.WriteAllText(Path.Combine(AppContext.BaseDirectory, OehsRuntimePaths.TestPackageMarkerFileName), "Isolated payroll acceptance only");
        Check(OehsRuntimePaths.IsIsolatedTestPackage, "Isolation marker missing");
        Check(Path.GetFullPath(OehsRuntimePaths.DataRoot).StartsWith(Path.GetFullPath(AppContext.BaseDirectory), StringComparison.OrdinalIgnoreCase), "Unsafe data root");
        using var fixture = JsonDocument.Parse(File.ReadAllText(args[0]));
        var samples = fixture.RootElement.GetProperty("Samples").EnumerateArray().Select(s => s.Clone()).ToArray();
        var contracts = new List<ContractDraftData>(); var firms = new List<Contractor>(); var routes = new List<RouteData>();
        for (var i = 0; i < samples.Length; i++)
        {
            var input = samples[i].GetProperty("Input").Deserialize<ProgressPaymentInput>()!;
            var tender = $"QA-2026-{i + 1:000}"; var identity = (90000000000L + i).ToString(CultureInfo.InvariantCulture);
            var firm = new Contractor($"Bordro deneme firması {i + 1}", identity, ProgramType: input.ProgramType, VatExempt: input.IsVatExempt);
            var contract = new ContractDraftData(tender, tender, "Bordro doğrulama taşıma hizmeti", firm.Name, input.ContractAmount,
                new DateOnly(2026, 10, 1), new DateOnly(2026, 10, 31), input.IsVatExempt,
                new DateOnly(2026, 9, 1), new DateOnly(2026, 9, 15), identity, ContractWorkDays: 23, ProgramType: input.ProgramType);
            contracts.Add(contract); firms.Add(firm);
            routes.Add(new("01", $"{samples[i].GetProperty("Sheet").GetString()} — kaynak satır {samples[i].GetProperty("Row")}",
                input.DailyPrice, input.GuideDailyPrice, input.WorkDays, tender, identity, tender, input.ProgramType,
                input.ContractAmount, contract.ContractDate));
        }
        OehsFreshStartService.EnsureFreshStart();
        var store = new OehsProgramScopedApplicationDataStore();
        store.Save(new(null, routes, firms, [], contracts));
        var app = new App(); app.InitializeComponent();
        var exitCode = 0;
        app.Startup += (_, _) => app.Dispatcher.BeginInvoke(DispatcherPriority.ApplicationIdle, new Action(() =>
        {
            var window = (MainWindow)app.MainWindow;
            window.Title = "TEHYS — EXCEL KABUL DENEMESİ";
            try { Run(window, samples, contracts, store); }
            catch (Exception error) { exitCode = 1; File.WriteAllText(Path.Combine(output, "error.txt"), error.ToString()); Console.Error.WriteLine(error); }
            finally { app.Shutdown(exitCode); }
        }));
        app.Run(); return exitCode;
    }

    private static void Run(MainWindow window, JsonElement[] samples, List<ContractDraftData> contracts, OehsProgramScopedApplicationDataStore store)
    {
        var evidence = new List<object>(); var pdfCount = 0; var roundingDifferences = 0;
        Control<Button>(window, "NavProgress").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); Pump();
        for (var i = 0; i < samples.Length; i++)
        {
            var sample = samples[i]; var input = sample.GetProperty("Input").Deserialize<ProgressPaymentInput>()!;
            File.WriteAllText(Path.Combine(output, "current-step.txt"), $"{i + 1}/{samples.Length}: selectors");
            var tenders = Control<ComboBox>(window, "ProgressTenderListBox");
            tenders.SelectedItem = tenders.Items.OfType<ContractDraftData>().Single(c => c.TenderNumber == contracts[i].TenderNumber); Pump();
            var firmBox = Control<ComboBox>(window, "ProgressContractorComboBox"); firmBox.SelectedIndex = 0; Pump();
            var routeBox = Control<ListBox>(window, "ProgressRouteComboBox");
            routeBox.SelectedItems.Clear(); routeBox.SelectedItems.Add(routeBox.Items[0]); Pump();
            Control<ComboBox>(window, "ProgressPeriodMonthComboBox").SelectedIndex = 0; Pump();
            var grid = Control<DataGrid>(window, "MultiRoutePaymentGrid");
            var row = grid.Items.OfType<MainWindow.MultiRoutePaymentDraftRow>().Single();
            var properties = new[] { "WorkDays", "DailyPrice", "DiscountAmount", "PriceDifference", "GuideWorkDays", "GuideDailyPrice", "GuideDiscountAmount", "GuidePriceDifference", "TaxDebt", "SocialSecurityDebt", "MinimumWageSupport", "InspectionPenalty" };
            foreach (var property in properties)
            {
                var column = grid.Columns.OfType<DataGridBoundColumn>().Single(c => (c.Binding as Binding)?.Path.Path == property);
                var value = (decimal)typeof(ProgressPaymentInput).GetProperty(property)!.GetValue(input)!;
                if (column.Visibility != Visibility.Visible) { Check(value == 0m, $"Nonzero hidden field: {property}"); continue; }
                File.WriteAllText(Path.Combine(output, "current-step.txt"), $"{i + 1}: enter {property}");
                grid.ScrollIntoView(row, column); window.UpdateLayout(); Pump();
                grid.SelectedItem = row; grid.CurrentCell = new DataGridCellInfo(row, column);
                var content = column.GetCellContent(row); Check(content is not null, $"Cell not realized: {property}");
                ((DataGridCell)content!.Parent).Focus(); Check(grid.BeginEdit(), $"Cannot edit {property}"); window.UpdateLayout();
                var editor = (TextBox)column.GetCellContent(row);
                editor.Text = value.ToString("0.####################", Turkish);
                editor.GetBindingExpression(TextBox.TextProperty)!.UpdateSource();
                Check(grid.CommitEdit(DataGridEditingUnit.Cell, true) && grid.CommitEdit(DataGridEditingUnit.Row, true), $"Cell rejected: {property}"); Pump();
            }
            Check(row.CreateInput() == input, $"Entered input changed: {i + 1}");
            File.WriteAllText(Path.Combine(output, "current-step.txt"), $"{i + 1}: calculate/save");
            Control<Button>(window, "CalculateProgressButton").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); Pump();
            var saved = store.Load().ProgressPayments!;
            var payment = saved.Single(p => p.TenderNumber == contracts[i].TenderNumber);
            Check(payment.Input == input, $"Saved inputs differ: {i + 1}");
            var expectedCells = sample.GetProperty("Expected").EnumerateArray().ToArray();
            foreach (var cell in expectedCells)
            {
                if (cell.GetProperty("ReferenceMissing").GetBoolean() || cell.GetProperty("SourceInvoiceVariant").GetBoolean()) continue;
                var actual = (decimal)typeof(ProgressPaymentCalculation).GetProperty(cell.GetProperty("Property").GetString()!)!.GetValue(payment.Calculation)!;
                Check(Math.Abs(actual - cell.GetProperty("Value").GetDecimal()) < 0.0000001m, $"Saved calculation mismatch at sample {i + 1}/{cell.GetProperty("Cell")}");
            }
            var context = Call(window, "BuildOfficialDocumentContextForPayment", payment);
            var reportNet = Property<decimal>(context, "ReportNetPayable");
            var dir = Path.Combine(output, $"case-{i + 1:00}"); Directory.CreateDirectory(dir);
            var factory = typeof(MainWindow).Assembly.GetType("OzelEgitimHakedis.Wpf.OehsOfficialDocumentFactory")!;
            var documents = (IEnumerable)factory.GetMethod("BuildSeparate", BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static)!.Invoke(null, [context])!;
            foreach (var document in documents)
            {
                var fixedDocument = Property<FixedDocument>(document, "Document");
                var path = Path.Combine(dir, Property<string>(document, "FileSuffix") + ".pdf"); PdfVectorWriter.Save(fixedDocument, path); pdfCount++;
                using var pdf = UglyToad.PdfPig.PdfDocument.Open(path);
                Check(pdf.NumberOfPages == 1, $"Unexpected page count: {path}");
                var page = pdf.GetPage(1);
                foreach (var letter in page.Letters)
                    Check(letter.BoundingBox.Left >= -0.5 && letter.BoundingBox.Right <= page.Width + 0.5 && letter.BoundingBox.Bottom >= -0.5 && letter.BoundingBox.Top <= page.Height + 0.5, $"Clipped PDF text: {path}");
                // Check every drawn text block survived actual PDF encoding/extraction, not only the WPF document.
                string Compact(string text) => string.Concat(text.Where(c => !char.IsWhiteSpace(c)));
                var text = Compact(page.Text);
                var fixedPage = (FixedPage)fixedDocument.Pages[0].Child;
                foreach (var block in fixedPage.Children.Cast<UIElement>().Select(c => c is TextBlock t ? t : c is Border { Child: TextBlock b } ? b : null).OfType<TextBlock>())
                    Check(text.Contains(Compact(block.Text), StringComparison.Ordinal), $"PDF lost text: {path}: {block.Text}");
                if (Property<object>(document, "Kind").ToString() == "HakedisRaporu")
                {
                    decimal Expected(string property) => expectedCells.Single(c => c.GetProperty("Property").GetString() == property).GetProperty("Value").GetDecimal();
                    decimal ReadMoney(double left, double top)
                    {
                        var block = fixedPage.Children.OfType<Border>().Single(b => Math.Abs(FixedPage.GetLeft(b) - left) < .01 && Math.Abs(FixedPage.GetTop(b) - top) < .01);
                        var sx = page.Width / fixedPage.Width; var sy = page.Height / fixedPage.Height;
                        var text = string.Concat(page.Letters.Where(l =>
                        {
                            var x = (l.BoundingBox.Left + l.BoundingBox.Right) / 2;
                            var y = (l.BoundingBox.Bottom + l.BoundingBox.Top) / 2;
                            return x >= left * sx && x <= (left + block.Width) * sx && y >= page.Height - (top + block.Height) * sy && y <= page.Height - top * sy;
                        }).Select(l => l.Value));
                        return decimal.Parse(text.Replace("TL", "", StringComparison.Ordinal).Replace("₺", "", StringComparison.Ordinal).Trim(), NumberStyles.Number, Turkish);
                    }
                    void Compare(double left, double top, decimal expected)
                    {
                        var value = ReadMoney(left, top);
                        Check(value == decimal.Round(expected, 2, MidpointRounding.AwayFromZero), $"Actual PDF cell differs from Excel: {path}/{left},{top}: {value} vs {expected}");
                    }
                    var special = input.ProgramType == TransportationProgramType.SpecialEducation;
                    var vat = special ? Expected("TransportVat") + Expected("GuideVat") : Expected("Vat");
                    var withheld = special ? Expected("TransportWithholding") + Expected("GuideWithholding") : Expected("Withholding");
                    Compare(476, 322, Expected("TotalAmount")); Compare(476, 351, vat); Compare(476, 380, Expected("GrossAmount"));
                    Compare(479, 441, Expected("StampTax")); Compare(479, 470, withheld); Compare(479, 615, Expected("AdditionalGuarantee"));
                    Compare(476, 768, Expected("GrossAmount") - Expected("NetPayable"));
                    Compare(476, 796, reportNet);
                    Check(Math.Abs(ReadMoney(476, 796) - decimal.Round(Expected("NetPayable"), 2, MidpointRounding.AwayFromZero)) <= .01m, "PDF net differs by more than accepted 1 kurus");
                }
            }
            var excelNet = decimal.Round(expectedCells.Single(c => c.GetProperty("Property").GetString() == "NetPayable").GetProperty("Value").GetDecimal(), 2, MidpointRounding.AwayFromZero);
            var difference = reportNet - excelNet; if (difference != 0m) roundingDifferences++;
            Check(Math.Abs(difference) <= 0.01m, $"Meaningful PDF net difference at sample {i + 1}: {difference}");
            evidence.Add(new { Case = i + 1, File = sample.GetProperty("File").GetString(), Sheet = sample.GetProperty("Sheet").GetString(), Row = sample.GetProperty("Row").GetInt32(), payment.Id, Input = payment.Input, Calculation = payment.Calculation, ExcelNet = excelNet, CalculationNet = decimal.Round(payment.Calculation.NetPayable, 2, MidpointRounding.AwayFromZero), PdfNet = reportNet, Difference = difference });
            File.WriteAllText(Path.Combine(output, "results.json"), JsonSerializer.Serialize(evidence, new JsonSerializerOptions { WriteIndented = true }));
            Console.WriteLine($"{i + 1}/42 saved; 4 PDFs; Excel={excelNet}, PDF={reportNet}, difference={difference}");
        }
        Check(store.Load().ProgressPayments!.Count == 42, "Expected 42 real saved payments");
        Control<Button>(window, "NavHistory").RaiseEvent(new RoutedEventArgs(ButtonBase.ClickEvent)); Pump();
        Capture(window, "history.png");
        File.WriteAllText(Path.Combine(output, "summary.json"), JsonSerializer.Serialize(new { Rows = samples.Length, SavedPayments = 42, Pdfs = pdfCount, RoundingDifferences = roundingDifferences, DataRoot = OehsRuntimePaths.DataRoot }, new JsonSerializerOptions { WriteIndented = true }));
    }

    private static void Capture(Window window, string name)
    {
        var content = (FrameworkElement)window.Content;
        var bitmap = new RenderTargetBitmap((int)content.ActualWidth, (int)content.ActualHeight, 96, 96, PixelFormats.Pbgra32); bitmap.Render(content);
        var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bitmap));
        using var file = File.Create(Path.Combine(output, name)); encoder.Save(file);
    }
}
