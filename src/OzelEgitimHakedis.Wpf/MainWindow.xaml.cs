using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow : Window
{
    private const double DesignWidth = 1522.5;
    private const double DesignHeight = 1012.1;

    private const int WmGetMinMaxInfo = 0x0024;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private HwndSource? _windowSource;
    private readonly ObservableCollection<RouteDraft> _routes = new();
    private readonly ObservableCollection<ContractorDraft> _contractors = new();
    private readonly ObservableCollection<ProgressPaymentHistoryData> _progressHistory = new();
    private readonly ObservableCollection<HistoryRow> _historyRows = new();
    private readonly ObservableCollection<SummaryRow> _summaryRows = new();
    private readonly OehsApplicationDataStore _dataStore = new();

    public MainWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        SourceInitialized += MainWindow_SourceInitialized;
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        UpdateShellText("Ana Ekran");
        RoutesListBox.ItemsSource = _routes;
        ContractorsListBox.ItemsSource = _contractors;
        HistoryItemsControl.ItemsSource = _historyRows;
        SummaryItemsControl.ItemsSource = _summaryRows;
        LoadPersistedData();
        UpdateRouteSummary();
        UpdateContractorSummary();
        UpdateHistoryView();
        UpdateSummaryView();
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
        var maxHeight = Math.Min(DesignHeight, workArea.Height * 0.965);
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

        var page = button.Tag?.ToString() ?? "Home";

        HomePanel.Visibility = page == "Home" ? Visibility.Visible : Visibility.Collapsed;
        ContractPanel.Visibility = page == "Contract" ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = page == "Progress" ? Visibility.Visible : Visibility.Collapsed;
        LinesPanel.Visibility = page == "Lines" ? Visibility.Visible : Visibility.Collapsed;
        ContractorsPanel.Visibility = page == "Contractors" ? Visibility.Visible : Visibility.Collapsed;
        HistoryPanel.Visibility = page == "History" ? Visibility.Visible : Visibility.Collapsed;
        SummaryPanel.Visibility = page == "Summary" ? Visibility.Visible : Visibility.Collapsed;
        ReportsPanel.Visibility = page == "Reports" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPanel.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderPanel.Visibility = page is "Home" or "Contract" or "Progress" or "Lines" or "Contractors"
            or "History" or "Summary" or "Reports" or "Settings"
            ? Visibility.Collapsed
            : Visibility.Visible;

        var module = page switch
        {
            "Contract" => "İhale / Sözleşme",
            "Progress" => "Hakediş Oluştur",
            "Lines" => "Hatlar",
            "Contractors" => "Yükleniciler",
            "History" => "Hakediş Geçmişi",
            "Summary" => "İcmal",
            "Reports" => "Raporlar",
            "Settings" => "Ayarlar",
            _ => "Ana Ekran"
        };

        UpdateShellText(module);

        if (PlaceholderPanel.Visibility == Visibility.Visible)
            PlaceholderTitleTextBlock.Text = module;

        UpdateActiveNavigation(page);
    }

    private void UpdateShellText(string module)
    {
        HeaderModuleText.Text = module;
    }

    private void UpdateActiveNavigation(string page)
    {
        foreach (var button in new[]
        {
            NavHome, NavContract, NavLines, NavContractors,
            NavProgress, NavHistory, NavSummary, NavReports, NavSettings
        })
        {
            button.Uid = button.Tag?.ToString() == page ? "Active" : string.Empty;
        }
    }

    private void AddContractorButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ContractorNameTextBox.Text.Trim();
        var taxOrIdentityNumber = ContractorTaxNumberTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(name))
        {
            ContractorStatusTextBlock.Text = "Yüklenici Adı / Unvanı alanı boş bırakılamaz.";
            return;
        }

        if (string.IsNullOrWhiteSpace(taxOrIdentityNumber))
        {
            ContractorStatusTextBlock.Text = "Vergi / T.C. Kimlik No alanı boş bırakılamaz.";
            return;
        }

        if (_contractors.Any(x => string.Equals(x.TaxOrIdentityNumber, taxOrIdentityNumber, StringComparison.CurrentCultureIgnoreCase)))
        {
            ContractorStatusTextBlock.Text = "Bu Vergi / T.C. Kimlik No zaten tanımlı.";
            return;
        }

        _contractors.Add(new ContractorDraft(name, taxOrIdentityNumber));
        SavePersistedData();
        UpdateContractorSummary();
        ClearContractorButton_Click(sender, e);
        ContractorStatusTextBlock.Text = $"Yüklenici eklendi: {name}.";
    }

    private void RemoveContractorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: ContractorDraft contractor })
        {
            _contractors.Remove(contractor);
            SavePersistedData();
            UpdateContractorSummary();
            ContractorStatusTextBlock.Text = $"Yüklenici silindi: {contractor.Name}.";
        }
    }

    private void ClearContractorButton_Click(object sender, RoutedEventArgs e)
    {
        ContractorNameTextBox.Clear();
        ContractorTaxNumberTextBox.Clear();
    }

    private void UpdateContractorSummary()
    {
        ContractorCountTextBlock.Text = _contractors.Count.ToString(CultureInfo.CurrentCulture);
    }

    private void AddRouteButton_Click(object sender, RoutedEventArgs e)
    {
        var routeNumber = RouteNumberTextBox.Text.Trim();
        var routeName = RouteNameTextBox.Text.Trim();

        if (string.IsNullOrWhiteSpace(routeNumber))
        {
            RouteStatusTextBlock.Text = "Hat No alanı boş bırakılamaz.";
            return;
        }

        if (string.IsNullOrWhiteSpace(routeName))
        {
            RouteStatusTextBlock.Text = "Hat Adı alanı boş bırakılamaz.";
            return;
        }

        if (!TryParseDecimal(RouteDailyPriceTextBox.Text, out var dailyPrice) || dailyPrice < 0)
        {
            RouteStatusTextBlock.Text = "Günlük Birim Fiyat geçerli bir tutar olmalıdır.";
            return;
        }

        if (_routes.Any(x => string.Equals(x.RouteNumber, routeNumber, StringComparison.CurrentCultureIgnoreCase)))
        {
            RouteStatusTextBlock.Text = "Bu Hat No zaten tanımlı.";
            return;
        }

        _routes.Add(new RouteDraft(routeNumber, routeName, dailyPrice));
        SavePersistedData();
        UpdateRouteSummary();
        ClearRouteButton_Click(sender, e);
        RouteStatusTextBlock.Text = $"Hat eklendi: {routeNumber} — {routeName}.";
    }

    private void RemoveRouteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button { Tag: RouteDraft route })
        {
            _routes.Remove(route);
            SavePersistedData();
            UpdateRouteSummary();
            RouteStatusTextBlock.Text = $"Hat silindi: {route.RouteNumber}.";
        }
    }

    private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
    {
        RouteNumberTextBox.Clear();
        RouteNameTextBox.Clear();
        RouteDailyPriceTextBox.Clear();
    }

    private void UpdateRouteSummary()
    {
        RouteCountTextBlock.Text = _routes.Count.ToString(CultureInfo.CurrentCulture);
        RouteTotalDailyPriceTextBlock.Text = FormatMoney(_routes.Sum(x => x.DailyPrice));
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

        SavePersistedData();
        ContractStatusTextBlock.Text =
            $"Sözleşme taslağı kaydedildi: {WorkNameTextBox.Text.Trim()} — {contractAmount:N2} ₺.";
    }

    private void LoadPersistedData()
    {
        var data = _dataStore.Load();

        _routes.Clear();
        foreach (var route in data.Routes)
            _routes.Add(new RouteDraft(route.RouteNumber, route.Name, route.DailyPrice));

        _contractors.Clear();
        foreach (var contractor in data.Contractors)
            _contractors.Add(new ContractorDraft(contractor.Name, contractor.TaxOrIdentityNumber));

        _progressHistory.Clear();
        foreach (var history in data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
            _progressHistory.Add(history);

        if (data.Contract is not null)
        {
            TenderNumberTextBox.Text = data.Contract.TenderNumber;
            ContractNumberTextBox.Text = data.Contract.ContractNumber;
            WorkNameTextBox.Text = data.Contract.WorkName;
            ContractorTextBox.Text = data.Contract.ContractorName;
            ContractAmountTextBox.Text = data.Contract.ContractAmount.ToString("N2", CultureInfo.CurrentCulture);
            StartDatePicker.SelectedDate = data.Contract.StartDate.ToDateTime(TimeOnly.MinValue);
            EndDatePicker.SelectedDate = data.Contract.EndDate.ToDateTime(TimeOnly.MinValue);
            VatExemptCheckBox.IsChecked = data.Contract.VatExempt;
            ContractStatusTextBlock.Text = "Kaydedilmiş sözleşme taslağı yüklendi.";
        }
    }

    private void SavePersistedData()
    {
        var contract = string.IsNullOrWhiteSpace(WorkNameTextBox.Text)
            ? null
            : new ContractDraftData(
                TenderNumberTextBox.Text.Trim(),
                ContractNumberTextBox.Text.Trim(),
                WorkNameTextBox.Text.Trim(),
                ContractorTextBox.Text.Trim(),
                TryParseDecimal(ContractAmountTextBox.Text, out var amount) ? amount : 0m,
                DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
                DateOnly.FromDateTime(EndDatePicker.SelectedDate ?? DateTime.Today),
                VatExemptCheckBox.IsChecked == true);

        var data = new OehsApplicationData(
            contract,
            _routes.Select(x => new RouteData(x.RouteNumber, x.Name, x.DailyPrice)).ToArray(),
            _contractors.Select(x => new Contractor(x.Name, x.TaxOrIdentityNumber)).ToArray(),
            _progressHistory.ToArray());

        _dataStore.Save(data);
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
        SavePersistedData();
        ContractStatusTextBlock.Text = "Sözleşme taslağı temizlendi.";
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
            ProgressCalculationSummaryTextBlock.Text = "BEKLENİYOR";
            CalculationStatusTextBlock.Text = "Tutar alanlarını geçerli sayılarla doldurun.";
            return;
        }

        try
        {
            var input = new ProgressPaymentInput(
                workDays,
                dailyPrice,
                discount,
                priceDifference,
                contractAmount,
                ProgressVatExemptCheckBox.IsChecked == true,
                0m,
                0m,
                0m,
                InspectionPenaltyCheckBox.IsChecked == true);

            var result = ProgressPaymentCalculator.Calculate(input);

            ServiceAmountResultTextBlock.Text = FormatMoney(result.ServiceAmount);
            TotalAmountResultTextBlock.Text = FormatMoney(result.TotalAmount);
            VatResultTextBlock.Text = FormatMoney(result.Vat);
            DeductionsResultTextBlock.Text = FormatMoney(result.TotalDeductions);
            GrossResultTextBlock.Text = FormatMoney(result.GrossAmount);
            NetResultTextBlock.Text = FormatMoney(result.NetPayable);
            NetControlResultTextBlock.Text = FormatMoney(result.NetPayable);
            WithholdingResultTextBlock.Text = FormatMoney(result.Withholding);
            StampTaxResultTextBlock.Text = FormatMoney(result.StampTax);
            InspectionPenaltyResultTextBlock.Text = FormatMoney(result.Deductions.InspectionPenalty);
            AdditionalGuaranteeResultTextBlock.Text = FormatMoney(result.AdditionalGuarantee);
            ProgressWorkDaysSummaryTextBlock.Text = workDays.ToString(CultureInfo.CurrentCulture);
            ProgressNetSummaryTextBlock.Text = FormatMoney(result.NetPayable);
            ProgressCalculationSummaryTextBlock.Text = "TAMAM";

            int? progressNumber = null;
            if (int.TryParse(ProgressNumberTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedProgressNumber) &&
                parsedProgressNumber > 0)
            {
                progressNumber = parsedProgressNumber;
            }

            var periodDate = ProgressPeriodDatePicker.SelectedDate is DateTime selectedPeriodDate
                ? DateOnly.FromDateTime(selectedPeriodDate)
                : (DateOnly?)null;

            _progressHistory.Add(new ProgressPaymentHistoryData(
                Guid.NewGuid(),
                DateTime.Now,
                input,
                result,
                progressNumber,
                periodDate));
            SavePersistedData();
            UpdateHistoryView();
            UpdateSummaryView();

            CalculationStatusTextBlock.Text = "Hesaplama başarıyla tamamlandı ve geçmişe kaydedildi.";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ProgressCalculationSummaryTextBlock.Text = "KONTROL";
            CalculationStatusTextBlock.Text = ex.Message;
        }
    }

    private void UpdateHistoryView()
    {
        _historyRows.Clear();

        for (var index = 0; index < _progressHistory.Count; index++)
        {
            var item = _progressHistory[index];
            var displayNumber = item.ProgressPaymentNumber.HasValue
                ? $"Hakediş No {item.ProgressPaymentNumber.Value}"
                : $"Hesaplama {index + 1}";
            var periodText = item.PeriodDate.HasValue
                ? $"Dönem: {item.PeriodDate.Value:dd.MM.yyyy} • Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}"
                : $"Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}";

            _historyRows.Add(new HistoryRow(
                displayNumber,
                periodText,
                FormatMoney(item.Calculation.TotalAmount),
                FormatMoney(item.Calculation.NetPayable)));
        }

        HistoryCountTextBlock.Text = _progressHistory.Count.ToString(CultureInfo.CurrentCulture);
        HistoryNetTotalTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.NetPayable));
        HistoryStatusTextBlock.Text = _progressHistory.Count == 0 ? "HAZIR" : "GÜNCEL";
        HistoryEmptyStateBorder.Visibility = _progressHistory.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void UpdateSummaryView()
    {
        _summaryRows.Clear();

        var ordered = _progressHistory
            .OrderBy(x => x.PeriodDate ?? DateOnly.MaxValue)
            .ThenBy(x => x.ProgressPaymentNumber ?? int.MaxValue)
            .ThenBy(x => x.CreatedAt)
            .ToArray();

        foreach (var item in ordered)
        {
            var title = item.ProgressPaymentNumber.HasValue
                ? $"Hakediş No {item.ProgressPaymentNumber.Value}"
                : "Hakediş Hesaplama";

            var period = item.PeriodDate.HasValue
                ? $"Dönem: {item.PeriodDate.Value:dd.MM.yyyy}"
                : $"Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}";

            _summaryRows.Add(new SummaryRow(
                title,
                period,
                FormatMoney(item.Calculation.GrossAmount),
                FormatMoney(item.Calculation.NetPayable)));
        }

        var latest = _progressHistory
            .Where(x => x.PeriodDate.HasValue)
            .OrderByDescending(x => x.PeriodDate)
            .FirstOrDefault();

        SummaryLatestPeriodTextBlock.Text = latest?.PeriodDate?.ToString("MM.yyyy", CultureInfo.CurrentCulture) ?? "—";
        SummaryCountTextBlock.Text = $"{_progressHistory.Count.ToString(CultureInfo.CurrentCulture)} hakediş";
        SummaryGrossTotalTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.GrossAmount));
        SummaryNetTotalTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.NetPayable));
        SummaryWithholdingTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.Deductions.Withholding));
        SummaryStampTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.Deductions.StampTax));
        SummaryOtherDeductionsTextBlock.Text = FormatMoney(_progressHistory.Sum(x =>
            x.Calculation.TotalDeductions
            - x.Calculation.Deductions.Withholding
            - x.Calculation.Deductions.StampTax));

        SummaryEmptyStateBorder.Visibility = _summaryRows.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
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

    private sealed record RouteDraft(string RouteNumber, string Name, decimal DailyPrice)
    {
        public string DailyPriceText => $"{DailyPrice:N2} ₺";
    }

    private sealed record ContractorDraft(string Name, string TaxOrIdentityNumber);

    private sealed record HistoryRow(
        string DisplayTitle,
        string CreatedAtText,
        string TotalAmountText,
        string NetPayableText);

    private sealed record SummaryRow(
        string Title,
        string PeriodText,
        string GrossText,
        string NetText);

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
