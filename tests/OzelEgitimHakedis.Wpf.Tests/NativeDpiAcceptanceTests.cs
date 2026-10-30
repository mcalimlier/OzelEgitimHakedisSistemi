using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Wpf;
using Xunit;

namespace OzelEgitimHakedis.Wpf.Tests;

public sealed partial class HakedisCreateHybridUiContractTests
{
    private static readonly System.Text.Json.JsonSerializerOptions NativeDpiEvidenceJsonOptions = new JsonSerializerOptions { WriteIndented = true };
    private static readonly string[] NativeDpiSelectionInputNames = new[] { "ProgressTenderListBox", "ProgressContractorComboBox", "ProgressPeriodMonthComboBox" };

    [DllImport("user32.dll")]
    private static extern uint GetDpiForWindow(IntPtr hwnd);

    [DllImport("user32.dll")]
    private static extern IntPtr SetThreadDpiAwarenessContext(IntPtr context);

    [Fact]
    public void PaymentPanel_ShouldAlignOnNativeWindowAtActualMonitorDpi()
    {
        RunOnSta(() =>
        {
            var previousContext = SetThreadDpiAwarenessContext(new IntPtr(-4));
            Assert.NotEqual(IntPtr.Zero, previousContext);
            Window? window = null;
            try
            {
                var layout = LoadLayoutFragment(NamedElement(LoadMainWindowXaml(), "ProgressPanel"));
                ((Grid)layout.FindName("SinglePaymentFieldsGrid")).Visibility = Visibility.Collapsed;
                ((Border)layout.FindName("MultiRouteEntryBorder")).Visibility = Visibility.Visible;
                var grid = (DataGrid)layout.FindName("MultiRoutePaymentGrid");
                var rows = Enumerable.Range(1, 6).Select(i =>
                {
                    var row = new MainWindow.MultiRoutePaymentDraftRow { RouteNumber = i.ToString(System.Globalization.CultureInfo.InvariantCulture),
                        RouteName = "QA Hat " + i, WorkDays = 16 + i, DailyPrice = 2320m, PriceDifference = i * 125.25m };
                    row.Configure(TransportationProgramType.SpecialEducation, 100000m, false, ProgressPaymentCalculationConstants.Defaults);
                    return row;
                }).ToArray();
                grid.ItemsSource = rows;
                window = new Window { Title = "OEHS isolated native DPI acceptance", Content = layout,
                    Width = 1200, Height = 760, ShowInTaskbar = false };
                window.Show();
                window.UpdateLayout();
                var nativeDpi = GetDpiForWindow(new WindowInteropHelper(window).Handle);
                Assert.InRange(nativeDpi, 96u, 768u);
                var actualDpi = VisualTreeHelper.GetDpi(layout);
                Assert.InRange(Math.Abs(actualDpi.PixelsPerInchX - nativeDpi), 0d, 0.01d);
                var expectedDpi = Environment.GetEnvironmentVariable("OEHS_EXPECTED_NATIVE_DPI");
                if (expectedDpi is not null) Assert.Equal(uint.Parse(expectedDpi, System.Globalization.CultureInfo.InvariantCulture), nativeDpi);
                var controls = NativeDpiSelectionInputNames
                    .Select(name => (FrameworkElement)layout.FindName(name)).ToArray();
                var bounds = controls.Select(c => c.TransformToAncestor(layout).TransformBounds(new Rect(c.RenderSize))).ToArray();
                Assert.All(bounds, b => Assert.InRange(Math.Abs(b.Top - bounds[0].Top), 0d, 1d));
                Assert.All(bounds, b => Assert.InRange(Math.Abs(b.Height - bounds[0].Height), 0d, 1d));
                var output = Environment.GetEnvironmentVariable("OEHS_NATIVE_DPI_OUTPUT");
                if (output is not null)
                {
                    Directory.CreateDirectory(output);
                    var evidence = new { NativeDpi = nativeDpi, WpfDpi = actualDpi.PixelsPerInchX,
                        Awareness = "PerMonitorV2", Width = layout.ActualWidth, Height = layout.ActualHeight,
                        HybridRowCount = rows.Length,
                        InputBounds = bounds.Select(b => new { b.X, b.Y, b.Width, b.Height }),
                        Method = "Shown native HWND; actual OS monitor DPI; no simulated scale or WM_DPICHANGED injection" };
                    File.WriteAllText(Path.Combine(output, $"native-dpi-{nativeDpi}.json"), JsonSerializer.Serialize(evidence, NativeDpiEvidenceJsonOptions));
                    var bmp = new RenderTargetBitmap((int)Math.Ceiling(layout.ActualWidth * actualDpi.DpiScaleX),
                        (int)Math.Ceiling(layout.ActualHeight * actualDpi.DpiScaleY), nativeDpi, nativeDpi, PixelFormats.Pbgra32);
                    bmp.Render(layout);
                    var encoder = new PngBitmapEncoder(); encoder.Frames.Add(BitmapFrame.Create(bmp));
                    using var file = File.Create(Path.Combine(output, $"native-dpi-{nativeDpi}.png")); encoder.Save(file);
                    grid.BringIntoView(); grid.ScrollIntoView(rows[^1]); window.UpdateLayout();
                    var lastRow = Assert.IsType<DataGridRow>(grid.ItemContainerGenerator.ContainerFromIndex(5));
                    Assert.True(lastRow.ActualHeight > 0);
                    var gridBmp = new RenderTargetBitmap((int)Math.Ceiling(layout.ActualWidth * actualDpi.DpiScaleX),
                        (int)Math.Ceiling(layout.ActualHeight * actualDpi.DpiScaleY), nativeDpi, nativeDpi, PixelFormats.Pbgra32);
                    gridBmp.Render(layout);
                    var gridEncoder = new PngBitmapEncoder(); gridEncoder.Frames.Add(BitmapFrame.Create(gridBmp));
                    using var gridFile = File.Create(Path.Combine(output, $"native-dpi-{nativeDpi}-six-routes.png")); gridEncoder.Save(gridFile);
                }
            }
            finally
            {
                window?.Close();
                SetThreadDpiAwarenessContext(previousContext);
            }
        });
    }
}
