using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow : Window
{
    private const double DesignWidth = 1450.0;
    private const double DesignHeight = 945.0;

    private const int WmGetMinMaxInfo = 0x0024;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private HwndSource? _windowSource;

    public MainWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        SourceInitialized += MainWindow_SourceInitialized;
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        UpdateShellText("Ana Ekran");
    }

    private void ConfigureWindow()
    {
        SizeToContent = SizeToContent.Manual;
        WindowStartupLocation = WindowStartupLocation.Manual;
        WindowStyle = WindowStyle.None;
        ResizeMode = ResizeMode.CanResizeWithGrip;
        ShowInTaskbar = true;
        Background = System.Windows.Media.Brushes.White;
        UseLayoutRounding = true;
        SnapsToDevicePixels = true;
        ConfigureWindowForCurrentScreen();
        UpdateMaximizeRestoreButton();
    }

    private void ConfigureWindowForCurrentScreen()
    {
        var workArea = SystemParameters.WorkArea;
        var maxWidth = Math.Min(DesignWidth, workArea.Width * 0.92);
        var maxHeight = Math.Min(DesignHeight, workArea.Height * 0.90);
        var scale = Math.Min(maxWidth / DesignWidth, maxHeight / DesignHeight);

        Width = Math.Round(DesignWidth * scale);
        Height = Math.Round(DesignHeight * scale);
        Left = workArea.Left + ((workArea.Width - Width) / 2.0);
        Top = workArea.Top + ((workArea.Height - Height) / 2.0);
    }

    private void MainWindow_SourceInitialized(object? sender, EventArgs e)
    {
        _windowSource = HwndSource.FromHwnd(new WindowInteropHelper(this).Handle);
        _windowSource?.AddHook(WindowMessageHook);
    }

    private IntPtr WindowMessageHook(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg != WmGetMinMaxInfo || lParam == IntPtr.Zero)
            return IntPtr.Zero;

        var minMax = Marshal.PtrToStructure<MinMaxInfo>(lParam);
        var monitor = MonitorFromWindow(hwnd, MonitorDefaultToNearest);

        if (monitor != IntPtr.Zero)
        {
            var monitorInfo = new MonitorInfo
            {
                cbSize = Marshal.SizeOf<MonitorInfo>()
            };

            if (GetMonitorInfo(monitor, ref monitorInfo))
            {
                var work = monitorInfo.rcWork;
                var bounds = monitorInfo.rcMonitor;
                minMax.ptMaxPosition.x = work.left - bounds.left;
                minMax.ptMaxPosition.y = work.top - bounds.top;
                minMax.ptMaxSize.x = work.right - work.left;
                minMax.ptMaxSize.y = work.bottom - work.top;
            }
        }

        Marshal.StructureToPtr(minMax, lParam, false);
        handled = true;
        return IntPtr.Zero;
    }

    private void UpdateMaximizeRestoreButton()
    {
        if (MaximizeRestoreButton is null || MaximizeRestoreIcon is null)
            return;

        var maximized = WindowState == WindowState.Maximized;
        MaximizeRestoreButton.ToolTip = maximized ? "Geri yükle" : "Tam ekran";
        MaximizeRestoreIcon.Text = maximized ? "\uE923" : "\uE922";
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
            return;

        var page = button.Name switch
        {
            "NavHome" => "Home",
            "NavContract" => "Contract",
            "NavLines" => "Lines",
            "NavContractors" => "Contractors",
            "NavProgress" => "Progress",
            "NavHistory" => "History",
            "NavSummary" => "Summary",
            "NavReports" => "Reports",
            "NavSettings" => "Settings",
            _ => button.Tag?.ToString() ?? "Home"
        };

        HomePanel.Visibility = page == "Home" ? Visibility.Visible : Visibility.Collapsed;
        ContractPanel.Visibility = page == "Contract" ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = page == "Progress" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderPanel.Visibility = page is "Home" or "Contract" or "Progress"
            ? Visibility.Collapsed
            : Visibility.Visible;

        UpdateShellText(page switch
        {
            "Contract" => "İhale / Sözleşme",
            "Progress" => "Hakediş Oluştur",
            _ when PlaceholderPanel.Visibility == Visibility.Visible => button.Content?.ToString() ?? "Modül",
            _ => "Ana Ekran"
        });

        if (PlaceholderPanel.Visibility == Visibility.Visible)
            PlaceholderTitleTextBlock.Text = button.Content?.ToString() ?? "Modül";

        UpdateActiveNavigation(button);
    }

    private void UpdateShellText(string module)
    {
        HeaderModuleText.Text = module;
    }

    private void UpdateActiveNavigation(System.Windows.Controls.Button activeButton)
    {
        foreach (var button in new[]
        {
            NavHome, NavContract, NavLines, NavContractors,
            NavProgress, NavHistory, NavSummary, NavReports, NavSettings
        })
        {
            button.Tag = button == activeButton ? "Active" : null;
        }
    }

    private void SaveContractButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text))
        {
            ContractStatusTextBlock.Text = "İşin Adı alanı boş bırakılamaz.";
            return;
        }

        if (!TryParseDecimal(ContractAmountTextBox.Text, out var contractAmount) || contractAmount < 0)
        {
            ContractStatusTextBlock.Text = "Sözleşme Bedeli geçerli bir tutar olmalıdır.";
            return;
        }

        ContractStatusTextBlock.Text =
            $"Taslak hazır: {WorkNameTextBox.Text.Trim()} — {contractAmount:N2} ₺. Kalıcı kayıt katmanı bir sonraki adımda eklenecek.";
    }

    private void ClearContractButton_Click(object sender, RoutedEventArgs e)
    {
        TenderNumberTextBox.Clear();
        ContractNumberTextBox.Clear();
        WorkNameTextBox.Clear();
        ContractorTextBox.Clear();
        ContractAmountTextBox.Clear();
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        VatExemptCheckBox.IsChecked = false;
        ContractStatusTextBlock.Text = string.Empty;
    }

    private void CalculateProgressButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(WorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays) ||
            workDays < 0)
        {
            CalculationStatusTextBlock.Text = "İş Günü 0 veya daha büyük bir tam sayı olmalıdır.";
            return;
        }

        if (!TryParseDecimal(DailyPriceTextBox.Text, out var dailyPrice) || dailyPrice < 0 ||
            !TryParseDecimal(DiscountTextBox.Text, out var discount) ||
            !TryParseDecimal(PriceDifferenceTextBox.Text, out var priceDifference) ||
            !TryParseDecimal(ProgressContractAmountTextBox.Text, out var contractAmount) || contractAmount < 0)
        {
            CalculationStatusTextBlock.Text = "Tutar alanlarını geçerli sayılarla doldurun.";
            return;
        }

        try
        {
            var result = ProgressPaymentCalculator.Calculate(
                new ProgressPaymentInput(
                    workDays,
                    dailyPrice,
                    discount,
                    priceDifference,
                    contractAmount,
                    ProgressVatExemptCheckBox.IsChecked == true,
                    0m,
                    0m,
                    0m,
                    InspectionPenaltyCheckBox.IsChecked == true));

            ServiceAmountResultTextBlock.Text = FormatMoney(result.ServiceAmount);
            TotalAmountResultTextBlock.Text = FormatMoney(result.TotalAmount);
            VatResultTextBlock.Text = FormatMoney(result.Vat);
            DeductionsResultTextBlock.Text = FormatMoney(result.TotalDeductions);
            GrossResultTextBlock.Text = FormatMoney(result.GrossAmount);
            NetResultTextBlock.Text = FormatMoney(result.NetPayable);
            CalculationStatusTextBlock.Text = "Hesaplama başarıyla tamamlandı.";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            CalculationStatusTextBlock.Text = ex.Message;
        }
    }

    private void Minimize_Click(object sender, RoutedEventArgs e) => WindowState = WindowState.Minimized;

    private void MaximizeRestore_Click(object sender, RoutedEventArgs e)
    {
        WindowState = WindowState == WindowState.Maximized ? WindowState.Normal : WindowState.Maximized;
        UpdateMaximizeRestoreButton();
    }

    private void Close_Click(object sender, RoutedEventArgs e) => Close();

    private void Header_MouseLeftButtonDown(object sender, System.Windows.Input.MouseButtonEventArgs e)
    {
        if (e.ClickCount == 2)
        {
            MaximizeRestore_Click(sender, e);
            return;
        }

        if (e.LeftButton == System.Windows.Input.MouseButtonState.Pressed)
            DragMove();
    }

    private void Window_StateChanged(object? sender, EventArgs e) => UpdateMaximizeRestoreButton();

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static string FormatMoney(decimal value) => $"{value:N2} ₺";

    [DllImport("user32.dll")]
    private static extern IntPtr MonitorFromWindow(IntPtr hwnd, uint dwFlags);

    [DllImport("user32.dll", CharSet = CharSet.Auto)]
    [return: MarshalAs(UnmanagedType.Bool)]
    private static extern bool GetMonitorInfo(IntPtr hMonitor, ref MonitorInfo lpmi);

    [StructLayout(LayoutKind.Sequential)]
    private struct NativePoint
    {
        public int x;
        public int y;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct MinMaxInfo
    {
        public NativePoint ptReserved;
        public NativePoint ptMaxSize;
        public NativePoint ptMaxPosition;
        public NativePoint ptMinTrackSize;
        public NativePoint ptMaxTrackSize;
    }

    [StructLayout(LayoutKind.Sequential, CharSet = CharSet.Auto)]
    private struct MonitorInfo
    {
        public int cbSize;
        public NativeRect rcMonitor;
        public NativeRect rcWork;
        public uint dwFlags;
    }

    [StructLayout(LayoutKind.Sequential)]
    private struct NativeRect
    {
        public int left;
        public int top;
        public int right;
        public int bottom;
    }
}
