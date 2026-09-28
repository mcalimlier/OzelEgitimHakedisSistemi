using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Interop;
using System.Windows.Documents;
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
    private readonly ObservableCollection<ReportRow> _reportRows = new();
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
        ReportItemsControl.ItemsSource = _reportRows;
        ProgressRouteComboBox.ItemsSource = _routes;
        ReportRouteComboBox.ItemsSource = _routes;
        LoadPersistedData();
        UpdateRouteSummary();
        UpdateContractorSummary();
        UpdateHistoryView();
        UpdateSummaryView();
        UpdateReportView();
        UpdateAcceptanceView();
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

    private void UpdateAcceptanceView()
    {
        if (_dataStore is null)
            return;

        if (AcceptanceContractTextBlock is null)
            return;

        if (_progressHistory.Count == 0)
        {
            AcceptanceContractTextBlock.Text = "Kayıt yok";
            AcceptanceProgressTextBlock.Text = "Kayıt yok";
            AcceptanceNetTextBlock.Text = FormatMoney(0);
            AcceptanceDetailTextBlock.Text = "Sözleşme ve hakediş kaydı oluşturulduğunda bilgiler burada görünür.";
            return;
        }

        var latest = _progressHistory
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .First();

        AcceptanceContractTextBlock.Text = string.IsNullOrWhiteSpace(ContractorTextBox.Text)
            ? "Sözleşme bilgisi"
            : ContractorTextBox.Text.Trim();

        AcceptanceProgressTextBlock.Text = latest.ProgressPaymentNumber.HasValue
            ? $"Hakediş No {latest.ProgressPaymentNumber.Value}"
            : "Son hakediş";

        AcceptanceNetTextBlock.Text = FormatMoney(latest.Calculation.NetPayable);

        var period = latest.PeriodDate.HasValue
            ? latest.PeriodDate.Value.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture)
            : "-";

        AcceptanceDetailTextBlock.Text =
            $"Dönem: {period} • Brüt: {FormatMoney(latest.Calculation.GrossAmount)} • " +
            $"Net: {FormatMoney(latest.Calculation.NetPayable)} • " +
            $"Toplam kesinti: {FormatMoney(latest.Calculation.TotalDeductions)}";
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
            RequireSelectedRoute();
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

            var selectedRoute = ProgressRouteComboBox.SelectedItem as RouteDraft;
            var routeNumber = selectedRoute?.RouteNumber;
            var routeName = selectedRoute?.Name;
            var periodDate = ProgressPeriodDatePicker.SelectedDate is DateTime selectedPeriodDate
                ? DateOnly.FromDateTime(selectedPeriodDate)
                : (DateOnly?)null;

            _progressHistory.Add(new ProgressPaymentHistoryData(
                Guid.NewGuid(),
                DateTime.Now,
                input,
                result,
                progressNumber,
                periodDate,
                routeNumber,
                routeName));
            SavePersistedData();
            UpdateHistoryView();
            UpdateSummaryView();
            UpdateReportView();

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

    private void UpdateReportView()
    {
        _reportRows.Clear();

        foreach (var item in _progressHistory.OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue).ThenByDescending(x => x.CreatedAt))
        {
            var title = item.ProgressPaymentNumber.HasValue
                ? $"Hakediş No {item.ProgressPaymentNumber.Value}"
                : "Hakediş Hesaplama";

            var period = item.PeriodDate.HasValue
                ? $"Dönem: {item.PeriodDate.Value:dd.MM.yyyy}"
                : $"Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}";

            _reportRows.Add(new ReportRow(
                title,
                period,
                FormatMoney(item.Calculation.GrossAmount),
                FormatMoney(item.Calculation.NetPayable)));
        }

        ReportCountTextBlock.Text = _progressHistory.Count.ToString(CultureInfo.CurrentCulture);
        ReportGrossTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.GrossAmount));
        ReportNetTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.NetPayable));

        var latest = _progressHistory
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latest is null)
        {
            ReportDetailTitleTextBlock.Text = "Henüz kayıt yok";
            ReportDetailGrossTextBlock.Text = FormatMoney(0);
            ReportDetailNetTextBlock.Text = FormatMoney(0);
            ReportDetailMetaTextBlock.Text = "Hakediş ekranından bir hesaplama kaydedildiğinde son kayıt burada gösterilir.";
        }
        else
        {
            var title = latest.ProgressPaymentNumber.HasValue
                ? $"Hakediş No {latest.ProgressPaymentNumber.Value}"
                : "Hakediş Hesaplama";

            var period = latest.PeriodDate.HasValue
                ? $"Dönem: {latest.PeriodDate.Value:dd.MM.yyyy}"
                : $"Kayıt: {latest.CreatedAt:dd.MM.yyyy HH:mm}";

            var vatText = latest.Input.IsVatExempt ? "KDV istisnası uygulanıyor" : "KDV uygulanıyor";
            var deductionText = $"Toplam kesinti: {FormatMoney(latest.Calculation.TotalDeductions)}";

            ReportDetailTitleTextBlock.Text = title;
            ReportDetailGrossTextBlock.Text = FormatMoney(latest.Calculation.GrossAmount);
            ReportDetailNetTextBlock.Text = FormatMoney(latest.Calculation.NetPayable);
            ReportDetailMetaTextBlock.Text = $"{period} • {vatText} • {deductionText}";
        }

        ReportEmptyStateBorder.Visibility = _reportRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private ProgressPaymentHistoryData? GetSelectedRouteLatestPayment()
    {
        var route = ReportRouteComboBox.SelectedItem as RouteDraft;
        var query = _progressHistory.AsEnumerable();

        if (route is not null)
            query = query.Where(x => string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase));

        return query
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    private IReadOnlyList<ProgressPaymentHistoryData> GetSelectedRoutePayments()
    {
        var route = ReportRouteComboBox.SelectedItem as RouteDraft;
        IEnumerable<ProgressPaymentHistoryData> query = _progressHistory;

        if (route is not null)
            query = query.Where(x => string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase));

        return query.OrderBy(x => x.PeriodDate ?? DateOnly.MinValue).ThenBy(x => x.CreatedAt).ToList();
    }

    private void RequireSelectedRoute()
    {
        if (ProgressRouteComboBox.SelectedItem is null && _routes.Count > 0)
            ProgressRouteComboBox.SelectedIndex = 0;
        if (ReportRouteComboBox.SelectedItem is null && _routes.Count > 0)
            ReportRouteComboBox.SelectedIndex = 0;
    }

    private Table CreateDocumentTable(params (string Label, string Value)[] rows)
    {
        var table = new Table { CellSpacing = 0, BorderBrush = System.Windows.Media.Brushes.DarkGray, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 12) };
        table.Columns.Add(new TableColumn { Width = new GridLength(250) });
        table.Columns.Add(new TableColumn { Width = new GridLength(430) });
        var group = new TableRowGroup();
        foreach (var (label, value) in rows)
        {
            var row = new TableRow();
            row.Cells.Add(new TableCell(new Paragraph(new Run(label))) { Padding = new Thickness(7), FontWeight = FontWeights.Bold });
            row.Cells.Add(new TableCell(new Paragraph(new Run(value ?? "-"))) { Padding = new Thickness(7) });
            group.Rows.Add(row);
        }
        table.RowGroups.Add(group);
        return table;
    }

    private FlowDocument CreateBaseDocument(string title, string subtitle)
    {
        var document = new FlowDocument { PagePadding = new Thickness(40), FontFamily = new System.Windows.Media.FontFamily("Segoe UI"), FontSize = 10.5, ColumnWidth = 720 };
        var header = new Paragraph { TextAlignment = TextAlignment.Center, Margin = new Thickness(0, 0, 0, 16) };
        header.Inlines.Add(new Run(title) { FontSize = 18, FontWeight = FontWeights.Bold });
        header.Inlines.Add(new LineBreak());
        header.Inlines.Add(new Run(subtitle) { FontSize = 11, FontWeight = FontWeights.SemiBold });
        document.Blocks.Add(header);
        return document;
    }

    private bool TryShowDocument(FlowDocument document, string title)
    {
        var dialog = new System.Windows.Controls.PrintDialog();
        if (dialog.ShowDialog() != true) return false;
        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        return true;
    }

    private bool EnsureRouteForOutput()
    {
        RequireSelectedRoute();
        if (ReportRouteComboBox.SelectedItem is not RouteDraft route)
        {
            MessageBox.Show("Önce Hatlar ekranından en az bir hat tanımlayın.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }
        return true;
    }

    private void ReportActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string page)
        {
            NavigationButton_Click(button, e);
        }
    }

    private void PrintRoutePaymentReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput()) return;
        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var latest = GetSelectedRouteLatestPayment();
        if (latest is null) { MessageBox.Show("Seçilen hat için henüz hakediş kaydı yok.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information); return; }

        var doc = CreateBaseDocument("HAKEDİŞ RAPORU", $"Hat No: {route.RouteNumber} — {route.Name}");
        doc.Blocks.Add(new Paragraph(new Run("1. HAKEDİŞ BİLGİLERİ")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(CreateDocumentTable(
            ("Hat No", route.RouteNumber), ("Hat Adı", route.Name),
            ("Hakediş No", latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"),
            ("Dönem", latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Kayıt Tarihi", latest.CreatedAt.ToString("dd.MM.yyyy HH:mm", CultureInfo.CurrentCulture))));
        doc.Blocks.Add(new Paragraph(new Run("2. TUTAR VE KESİNTİLER")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(CreateDocumentTable(
            ("Hizmet Tutarı", FormatMoney(latest.Calculation.ServiceAmount)),
            ("Fiyat Farkı / İndirim", FormatMoney(latest.Calculation.PriceDifferenceExcludingVat)),
            ("Toplam Tutar", FormatMoney(latest.Calculation.TotalAmount)),
            ("KDV", FormatMoney(latest.Calculation.Vat)),
            ("Tahakkuk / Brüt", FormatMoney(latest.Calculation.GrossAmount)),
            ("Tevkifat", FormatMoney(latest.Calculation.Withholding)),
            ("Damga Vergisi", FormatMoney(latest.Calculation.StampTax)),
            ("Toplam Kesinti", FormatMoney(latest.Calculation.TotalDeductions)),
            ("Yükleniciye Ödenecek", FormatMoney(latest.Calculation.NetPayable))));
        doc.Blocks.Add(new Paragraph(new Run("Created by Aşkın KESKİNBALTA")) { FontWeight = FontWeights.SemiBold });
        TryShowDocument(doc, "Hakediş Raporu");
    }

    private void PrintServicePaymentReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput()) return;
        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var latest = GetSelectedRouteLatestPayment();
        if (latest is null) { MessageBox.Show("Seçilen hat için henüz hakediş kaydı yok.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information); return; }

        var doc = CreateBaseDocument("HİZMET İŞLERİ HAKEDİŞ RAPORU", "Hizmet işleri rapor formatı");
        doc.Blocks.Add(CreateDocumentTable(
            ("Tarihi", latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("No.su", latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"),
            ("Uygulama Yılı", (latest.PeriodDate?.Year ?? DateTime.Now.Year).ToString(CultureInfo.CurrentCulture)),
            ("Yapılan Hizmetin Adı", route.Name),
            ("Hat No", route.RouteNumber),
            ("Yüklenicinin Adı / Ticari Unvanı", ContractorTextBox.Text),
            ("Sözleşme Bedeli", FormatMoney(latest.Input.ContractAmount)),
            ("Sözleşme Başlangıç", StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme Bitiş", EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-")));
        doc.Blocks.Add(new Paragraph(new Run("SÖZLEŞME / HAKEDİŞ TUTARLARI")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(CreateDocumentTable(
            ("Sözleşme Fiyatları ile Yapılan İş", FormatMoney(latest.Calculation.ServiceAmount)),
            ("Fiyat Farkı Tutarı", FormatMoney(latest.Calculation.PriceDifferenceExcludingVat)),
            ("Toplam Tutar (A+B)", FormatMoney(latest.Calculation.TotalAmount)),
            ("Bu Hakedişin Tutarı", FormatMoney(latest.Calculation.TotalAmount)),
            ("KDV", FormatMoney(latest.Calculation.Vat)),
            ("Tahakkuk Tutarı", FormatMoney(latest.Calculation.GrossAmount))));
        doc.Blocks.Add(new Paragraph(new Run("KESİNTİLER VE MAHSUPLAR")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(CreateDocumentTable(
            ("Gelir / Kurumlar Vergisi", "0,00 ₺"),
            ("Damga Vergisi", FormatMoney(latest.Calculation.StampTax)),
            ("KDV Tevkifatı", FormatMoney(latest.Calculation.Withholding)),
            ("Sosyal Sigortalar Kurumu Kesintisi", FormatMoney(latest.Calculation.Deductions.SocialSecurityDebt)),
            ("İdare Makinesi Kiraları", "0,00 ₺"),
            ("Gecikme Cezası", "0,00 ₺"),
            ("Avans Mahsubu", "0,00 ₺"),
            ("Bu Hakedişle Ödenen Fiyat Farkı Teminat Kesintisi", FormatMoney(latest.Calculation.AdditionalGuarantee)),
            ("Vergi Borcu", FormatMoney(latest.Calculation.Deductions.TaxDebt)),
            ("İcra Borcu", "0,00 ₺"),
            ("Ceza ve Diğer Kesintiler", FormatMoney(latest.Calculation.Deductions.InspectionPenalty)),
            ("Asgari Ücret Destek Tutarı Kesintisi", FormatMoney(latest.Calculation.Deductions.MinimumWageSupport)),
            ("Kesintiler ve Mahsuplar Toplamı", FormatMoney(latest.Calculation.TotalDeductions)),
            ("Yükleniciye Ödenecek Tutar", FormatMoney(latest.Calculation.NetPayable))));
        doc.Blocks.Add(new Paragraph(new Run("Created by Aşkın KESKİNBALTA")) { FontWeight = FontWeights.SemiBold });
        TryShowDocument(doc, "Hizmet İşleri Hakediş Raporu");
    }

    private void PrintAcceptanceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput()) return;
        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var latest = GetSelectedRouteLatestPayment();
        if (latest is null) { MessageBox.Show("Seçilen hat için henüz hakediş kaydı yok.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information); return; }

        var doc = CreateBaseDocument("HİZMET İŞLERİ KABUL TUTANAĞI", "");
        doc.Blocks.Add(CreateDocumentTable(
            ("İşin Adı", route.Name),
            ("Hat No", route.RouteNumber),
            ("Yüklenicinin adı/ticari unvanı", ContractorTextBox.Text),
            ("Sözleşme Tarihi", StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme Bedeli", FormatMoney(latest.Input.ContractAmount)),
            ("Sözleşmeye göre işin süresi", $"{(EndDatePicker.SelectedDate - StartDatePicker.SelectedDate)?.Days ?? 0} Gün"),
            ("Sözleşmeye göre işin bitirilmesi gereken tarih", EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Varsa süre uzatımları", "-"),
            ("İşin bitirildiği tarih", latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-")));
        doc.Blocks.Add(new Paragraph(new Run("İlçe Millî Eğitim Müdürlüğünün sözleşme kapsamında gerçekleştirilen iş için yapılan incelemede, kabulü engel olabilecek eksik, kusur ve arızaların bulunup bulunmadığı ayrıca doğrulanmalıdır."))
        { Margin = new Thickness(0, 12, 0, 35) });
        doc.Blocks.Add(CreateDocumentTable(
            ("Yüklenici", ContractorTextBox.Text),
            ("Düzenleyen", "Aşkın KESKİNBALTA — Memur"),
            ("Kontrol Eden", "Kontrol görevlisi — kurum tarafından belirlenecek"),
            ("Tarih", DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture))));
        doc.Blocks.Add(new Paragraph(new Run("Created by Aşkın KESKİNBALTA")) { FontWeight = FontWeights.SemiBold });
        TryShowDocument(doc, "Hizmet İşleri Kabul Tutanağı");
    }

    private void PrintSummaryReportButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput()) return;
        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var payments = GetSelectedRoutePayments();
        if (payments.Count == 0) { MessageBox.Show("Seçilen hat için henüz hakediş kaydı yok.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information); return; }

        var doc = CreateBaseDocument("HAKEDİŞ İCMALİ", $"Hat No: {route.RouteNumber} — {route.Name}");
        doc.Blocks.Add(new Paragraph(new Run("HAKEDİŞ DAĞILIMI")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        var table = new Table { CellSpacing = 0, BorderBrush = System.Windows.Media.Brushes.DarkGray, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 12) };
        foreach (var width in new[] { 70.0, 260.0, 150.0, 150.0 }) table.Columns.Add(new TableColumn { Width = new GridLength(width) });
        var group = new TableRowGroup();
        var header = new TableRow();
        foreach (var h in new[] { "Sıra No", "İşin Cinsi", "İş Tutarı (a)", "Fiyat Farkı (b)" })
            header.Cells.Add(new TableCell(new Paragraph(new Run(h))) { Padding = new Thickness(6), FontWeight = FontWeights.Bold });
        group.Rows.Add(header);
        var totalA=0m; var totalB=0m;
        for (var idx=0; idx<payments.Count; idx++)
        {
            var p=payments[idx]; totalA+=p.Calculation.ServiceAmount; totalB+=p.Calculation.PriceDifferenceExcludingVat;
            var row=new TableRow();
            foreach (var v in new[] { (idx+1).ToString(CultureInfo.CurrentCulture), route.Name, FormatMoney(p.Calculation.ServiceAmount), FormatMoney(p.Calculation.PriceDifferenceExcludingVat) })
                row.Cells.Add(new TableCell(new Paragraph(new Run(v))) { Padding = new Thickness(6) });
            group.Rows.Add(row);
        }
        var totalRow=new TableRow();
        foreach (var v in new[] {"", "Toplam", FormatMoney(totalA), FormatMoney(totalB)})
            totalRow.Cells.Add(new TableCell(new Paragraph(new Run(v))) { Padding = new Thickness(6), FontWeight = FontWeights.Bold });
        group.Rows.Add(totalRow); table.RowGroups.Add(group); doc.Blocks.Add(table);
        doc.Blocks.Add(new Paragraph(new Run("HAKEDİŞ İÇERİĞİNDEN YAPILAN KESİNTİLER")) { FontSize = 12.5, FontWeight = FontWeights.Bold });
        doc.Blocks.Add(CreateDocumentTable(
            ("Bir Evvelki Hakedişten Yapılan Kesinti", "0,00 ₺"),
            ("Bu Hakedişten Yapılan Kesinti", FormatMoney(payments.Sum(p => p.Calculation.TotalDeductions))),
            ("Toplam Kesinti Tutarı", FormatMoney(payments.Sum(p => p.Calculation.TotalDeductions))),
            ("Toplam Hakediş Tutarı (A - B)", FormatMoney(payments.Sum(p => p.Calculation.NetPayable)))));
        doc.Blocks.Add(new Paragraph(new Run("Created by Aşkın KESKİNBALTA")) { FontWeight = FontWeights.SemiBold });
        TryShowDocument(doc, "Hakediş İcmali");
    }

    private void PrintReportButton_Click(object sender, RoutedEventArgs e)
    {
        PrintRoutePaymentReportButton_Click(sender, e);
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
        public string DisplayText => $"Hat {RouteNumber} — {Name}";
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

    private sealed record ReportRow(
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
