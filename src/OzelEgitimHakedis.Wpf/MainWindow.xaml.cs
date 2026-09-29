using System.Collections.ObjectModel;
using System.Globalization;
using System.Runtime.InteropServices;
using System.Printing;
using System.Windows;
using System.Windows.Controls;
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
    private static readonly double[] ThreeColumnWidths = { 70d, 390d, 220d };
    private static readonly double[] ThreeValueWidths = { 55d, 470d, 155d };
    private static readonly double[] FourColumnWidths = { 60d, 310d, 155d, 155d };
    private static readonly double[] FiveColumnWidths = { 45d, 255d, 126d, 126d, 128d };
    private static readonly string[] MainReportHeaders = { "", "Kalem", "Tutar" };
    private static readonly string[] DeductionHeaders = { "", "KESİNTİLER VE MAHSUPLAR", "Tutar" };
    private static readonly string[] ContractHeaders = { "Sözleşme Bedeli", "Sözleşme Artış Onayının Tarihi No.su", "Ek Sözleşme Bedeli", "Toplam Sözleşme Bedeli" };
    private static readonly string[] ExtensionHeaders = { "Tarih", "Sayısı", "Verilen Süre", "İş Bitim Tarihi" };
    private static readonly string[] IcmalHeaders = { "Sıra No", "İşin Cinsi", "İş Tutarı (a)", "Fiyat Farkı Tutarı (b)", "Toplam (a+b)" };



    public MainWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        SourceInitialized += MainWindow_SourceInitialized;
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        TaxDebtTextBox.Text = "0";
        SocialSecurityDebtTextBox.Text = "0";
        MinimumWageSupportTextBox.Text = "0";
        InspectionPenaltyTextBox.Text = "0";
        GuideWorkDaysTextBox.Text = "0";
        GuideDailyPriceTextBox.Text = "0";
        GuideDiscountTextBox.Text = "0";
        GuidePriceDifferenceTextBox.Text = "0";
        UpdateShellText("Ana Ekran");
        RoutesListBox.ItemsSource = _routes;
        ContractorsListBox.ItemsSource = _contractors;
        HistoryItemsControl.ItemsSource = _historyRows;
        SummaryItemsControl.ItemsSource = _summaryRows;
        ReportItemsControl.ItemsSource = _reportRows;
        ProgressRouteComboBox.ItemsSource = _routes;
        ReportRouteComboBox.ItemsSource = _routes;
        LoadPersistedData();

        if (_routes.Count > 0)
        {
            ProgressRouteComboBox.SelectedIndex = 0;
            ReportRouteComboBox.SelectedIndex = 0;
        }

        UpdateRouteSummary();
        UpdateContractorSummary();
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        UpdateProgressRouteDefaults();
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

        var selectedRoute = ReportRouteComboBox.SelectedItem as RouteDraft;
        var latest = _progressHistory
            .Where(x => selectedRoute is null ||
                        string.Equals(x.RouteNumber, selectedRoute.RouteNumber, StringComparison.OrdinalIgnoreCase))
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latest is null)
        {
            AcceptanceContractTextBlock.Text = "Kayıt yok";
            AcceptanceProgressTextBlock.Text = "Kayıt yok";
            AcceptanceNetTextBlock.Text = FormatMoney(0);
            AcceptanceDetailTextBlock.Text = selectedRoute is null
                ? "Hakediş kaydı oluşturulduğunda bilgiler burada görünür."
                : "Seçilen hat için henüz hakediş kaydı yok.";
            return;
        }

        AcceptanceContractTextBlock.Text = string.IsNullOrWhiteSpace(latest.ContractorName)
            ? (string.IsNullOrWhiteSpace(ContractorTextBox.Text) ? "Sözleşme bilgisi" : ContractorTextBox.Text.Trim())
            : latest.ContractorName.Trim();

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
        AcceptancePanel.Visibility = page == "Acceptance" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPanel.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderPanel.Visibility = page is "Home" or "Contract" or "Progress" or "Lines" or "Contractors"
            or "History" or "Summary" or "Reports" or "Acceptance" or "Settings"
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
            "Acceptance" => "Kabul Tutanağı",
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
            NavProgress, NavHistory, NavSummary, NavReports, NavAcceptance, NavSettings
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

        if (_contractors.Any(x => string.Equals(x.TaxOrIdentityNumber, taxOrIdentityNumber, StringComparison.OrdinalIgnoreCase)))
        {
            ContractorStatusTextBlock.Text = "Bu Vergi / T.C. Kimlik No zaten tanımlı.";
            return;
        }

        _contractors.Add(new ContractorDraft(name, taxOrIdentityNumber));
        SavePersistedData();
        UpdateContractorSummary();
        UpdateHomeSummaryView();
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
            UpdateHomeSummaryView();
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
        HomeContractorCountTextBlock.Text = _contractors.Count.ToString(CultureInfo.CurrentCulture);
    }

    private void UpdateHomeSummaryView()
    {
        HomeContractStatusTextBlock.Text =
            string.IsNullOrWhiteSpace(WorkNameTextBox.Text) ? "Bekleniyor" : "KAYITLI";
        HomeRouteCountTextBlock.Text = _routes.Count.ToString(CultureInfo.CurrentCulture);
        HomeNetPayableTextBlock.Text = FormatMoney(_progressHistory.Sum(x => x.Calculation.NetPayable));
    }

    private void UpdateContractSummaryView()
    {
        var hasContract = !string.IsNullOrWhiteSpace(WorkNameTextBox.Text);
        ContractStatusSummaryTextBlock.Text = hasContract ? "KAYITLI" : "TASLAK";
        ContractAmountSummaryTextBlock.Text =
            TryParseDecimal(ContractAmountTextBox.Text, out var amount) ? FormatMoney(amount) : FormatMoney(0);
        ContractStartSummaryTextBlock.Text =
            StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";
        ContractEndSummaryTextBlock.Text =
            EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";

        ContractSummaryTenderTextBlock.Text =
            string.IsNullOrWhiteSpace(TenderNumberTextBox.Text) ? "Bekleniyor" : TenderNumberTextBox.Text.Trim();
        ContractSummaryContractorTextBlock.Text =
            string.IsNullOrWhiteSpace(ContractorTextBox.Text) ? "Bekleniyor" : ContractorTextBox.Text.Trim();
        ContractSummaryVatTextBlock.Text =
            VatExemptCheckBox.IsChecked == true ? "KDV İstisnası" : "KDV Dahil";
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

        if (!TryParseDecimal(RouteDailyPriceTextBox.Text, out var dailyPrice) || dailyPrice < 0 ||
            !int.TryParse(RouteGuideWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var guideWorkDays) ||
            guideWorkDays < 0 ||
            !TryParseDecimal(RouteGuideDailyPriceTextBox.Text, out var guideDailyPrice) || guideDailyPrice < 0)
        {
            RouteStatusTextBlock.Text = "Taşıma ve rehber personel fiyat/gün bilgileri geçerli olmalıdır.";
            return;
        }

        if (_routes.Any(x => string.Equals(x.RouteNumber, routeNumber, StringComparison.OrdinalIgnoreCase)))
        {
            RouteStatusTextBlock.Text = "Bu Hat No zaten tanımlı.";
            return;
        }

        _routes.Add(new RouteDraft(routeNumber, routeName, dailyPrice, guideWorkDays, guideDailyPrice));
        SavePersistedData();
        UpdateRouteSummary();
        UpdateHomeSummaryView();
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
            UpdateHomeSummaryView();
            RouteStatusTextBlock.Text = $"Hat silindi: {route.RouteNumber}.";
        }
    }

    private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
    {
        RouteNumberTextBox.Clear();
        RouteNameTextBox.Clear();
        RouteDailyPriceTextBox.Clear();
        RouteGuideWorkDaysTextBox.Clear();
        RouteGuideDailyPriceTextBox.Clear();
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
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        ContractStatusTextBlock.Text =
            $"Sözleşme taslağı kaydedildi: {WorkNameTextBox.Text.Trim()} — {contractAmount:N2} ₺.";
    }

    private void LoadPersistedData()
    {
        var data = _dataStore.Load();

        _routes.Clear();
        foreach (var route in data.Routes)
            _routes.Add(new RouteDraft(route.RouteNumber, route.Name, route.DailyPrice, route.GuideWorkDays, route.GuideDailyPrice));

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

        UpdateContractSummaryView();
        UpdateHomeSummaryView();
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
            _routes.Select(x => new RouteData(x.RouteNumber, x.Name, x.DailyPrice, x.GuideWorkDays, x.GuideDailyPrice)).ToArray(),
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
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        ContractStatusTextBlock.Text = "Sözleşme taslağı temizlendi.";
    }

    private void CalculateProgressButton_Click(object sender, RoutedEventArgs e)
    {
        if (!int.TryParse(WorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays) ||
            workDays < 0 ||
            !int.TryParse(GuideWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var guideWorkDays) ||
            guideWorkDays < 0)
        {
            CalculationStatusTextBlock.Text = "Taşıma ve rehber iş günleri 0 veya daha büyük tam sayılar olmalıdır.";
            return;
        }

        if (!TryParseDecimal(DailyPriceTextBox.Text, out var dailyPrice) || dailyPrice < 0 ||
            !TryParseDecimal(GuideDailyPriceTextBox.Text, out var guideDailyPrice) || guideDailyPrice < 0 ||
            !TryParseDecimal(DiscountTextBox.Text, out var discount) || discount < 0 ||
            !TryParseDecimal(GuideDiscountTextBox.Text, out var guideDiscount) || guideDiscount < 0 ||
            !TryParseDecimal(PriceDifferenceTextBox.Text, out var priceDifference) ||
            !TryParseDecimal(GuidePriceDifferenceTextBox.Text, out var guidePriceDifference) ||
            !TryParseDecimal(ProgressContractAmountTextBox.Text, out var contractAmount) || contractAmount < 0 ||
            !TryParseDecimal(TaxDebtTextBox.Text, out var taxDebt) || taxDebt < 0 ||
            !TryParseDecimal(SocialSecurityDebtTextBox.Text, out var socialSecurityDebt) || socialSecurityDebt < 0 ||
            !TryParseDecimal(MinimumWageSupportTextBox.Text, out var minimumWageSupport) || minimumWageSupport < 0 ||
            !TryParseDecimal(InspectionPenaltyTextBox.Text, out var inspectionPenalty) || inspectionPenalty < 0)
        {
            ProgressCalculationSummaryTextBlock.Text = "BEKLENİYOR";
            CalculationStatusTextBlock.Text = "Tutar alanlarını 0 veya daha büyük geçerli sayılarla doldurun.";
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
                ProgressVatExemptCheckBox.IsChecked != true,
                taxDebt,
                socialSecurityDebt,
                minimumWageSupport,
                inspectionPenalty,
                guideWorkDays,
                guideDailyPrice,
                guideDiscount,
                guidePriceDifference);

            var result = ProgressPaymentCalculator.Calculate(input);

            ServiceAmountResultTextBlock.Text = FormatMoney(result.ServiceAmount);
            GuideServiceAmountResultTextBlock.Text = FormatMoney(result.GuideServiceAmount);
            CombinedPriceDifferenceResultTextBlock.Text = FormatMoney(result.CombinedPriceDifferenceExcludingVat);
            TotalAmountResultTextBlock.Text = FormatMoney(result.TotalAmount);
            VatResultTextBlock.Text = FormatMoney(result.Vat);
            DeductionsResultTextBlock.Text = FormatMoney(result.TotalDeductions);
            GrossResultTextBlock.Text = FormatMoney(result.GrossAmount);
            NetResultTextBlock.Text = FormatMoney(result.NetPayable);
            NetControlResultTextBlock.Text = FormatMoney(result.NetPayable);
            WithholdingResultTextBlock.Text = FormatMoney(result.Withholding);
            StampTaxResultTextBlock.Text = FormatMoney(result.StampTax);
            TaxDebtResultTextBlock.Text = FormatMoney(result.Deductions.TaxDebt);
            SocialSecurityDebtResultTextBlock.Text = FormatMoney(result.Deductions.SocialSecurityDebt);
            MinimumWageSupportResultTextBlock.Text = FormatMoney(result.Deductions.MinimumWageSupport);
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
                routeName,
                WorkNameTextBox.Text.Trim(),
                ContractorTextBox.Text.Trim(),
                TenderNumberTextBox.Text.Trim(),
                ContractNumberTextBox.Text.Trim(),
                DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
                DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
                DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
                DateOnly.FromDateTime(EndDatePicker.SelectedDate ?? DateTime.Today)));
            SavePersistedData();
            UpdateHistoryView();
            UpdateSummaryView();
            UpdateReportView();
            UpdateHomeSummaryView();
            UpdateAcceptanceView();

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
            x.Calculation.Deductions.TaxDebt
            + x.Calculation.Deductions.SocialSecurityDebt
            + x.Calculation.Deductions.MinimumWageSupport
            + x.Calculation.Deductions.InspectionPenalty
            + x.Calculation.Deductions.AdditionalGuarantee));

        SummaryEmptyStateBorder.Visibility = _summaryRows.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void UpdateReportView()
    {
        _reportRows.Clear();

        var selectedRoute = ReportRouteComboBox.SelectedItem as RouteDraft;
        var routePayments = selectedRoute is null
            ? _progressHistory.ToArray()
            : _progressHistory
                .Where(x => string.Equals(x.RouteNumber, selectedRoute.RouteNumber, StringComparison.OrdinalIgnoreCase))
                .ToArray();

        foreach (var item in routePayments
                     .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
                     .ThenByDescending(x => x.CreatedAt))
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

        ReportCountTextBlock.Text = routePayments.Length.ToString(CultureInfo.CurrentCulture);
        ReportGrossTextBlock.Text = FormatMoney(routePayments.Sum(x => x.Calculation.GrossAmount));
        ReportNetTextBlock.Text = FormatMoney(routePayments.Sum(x => x.Calculation.NetPayable));

        var latest = routePayments
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        if (latest is null)
        {
            ReportDetailTitleTextBlock.Text = "Henüz kayıt yok";
            ReportDetailGrossTextBlock.Text = FormatMoney(0);
            ReportDetailNetTextBlock.Text = FormatMoney(0);
            ReportDetailMetaTextBlock.Text = selectedRoute is null
                ? "Hakediş ekranından bir hesaplama kaydedildiğinde son kayıt burada gösterilir."
                : "Seçilen hat için henüz hakediş kaydı yok.";
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

    private void ProgressRouteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        UpdateProgressRouteDefaults();
    }

    private void UpdateProgressRouteDefaults()
    {
        if (ProgressRouteComboBox.SelectedItem is not RouteDraft route)
            return;

        DailyPriceTextBox.Text = route.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        GuideDailyPriceTextBox.Text = route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);

        if (string.IsNullOrWhiteSpace(WorkDaysTextBox.Text))
            WorkDaysTextBox.Text = "0";

        if (string.IsNullOrWhiteSpace(GuideWorkDaysTextBox.Text))
            GuideWorkDaysTextBox.Text = "0";
    }

    private void ReportRouteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        UpdateReportView();
        UpdateAcceptanceView();
    }

    private ProgressPaymentHistoryData? GetSelectedRouteLatestPayment()
    {
        var route = ReportRouteComboBox.SelectedItem as RouteDraft;
        IEnumerable<ProgressPaymentHistoryData> query = _progressHistory;

        if (route is not null)
            query = query.Where(x => string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase));

        return query
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    private List<ProgressPaymentHistoryData> GetSelectedRoutePayments()
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

    private static TableCell CreateCell(string text, bool bold = false, bool right = false, bool center = false, bool shaded = false)
    {
        return new TableCell(new Paragraph(new Run(text ?? "-")))
        {
            Padding = new Thickness(5),
            FontWeight = bold ? FontWeights.Bold : FontWeights.Normal,
            TextAlignment = right ? TextAlignment.Right : center ? TextAlignment.Center : TextAlignment.Left,
            Background = shaded ? System.Windows.Media.Brushes.Gainsboro : System.Windows.Media.Brushes.White,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(0.5)
        };
    }

    private static Table CreateDocumentTable(params (string Label, string Value)[] rows)
    {
        var table = new Table { CellSpacing = 0, BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 10) };
        table.Columns.Add(new TableColumn { Width = new GridLength(330) });
        table.Columns.Add(new TableColumn { Width = new GridLength(350) });
        var group = new TableRowGroup();
        foreach (var (label, value) in rows)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(label, true, false, false, true));
            row.Cells.Add(CreateCell(value));
            group.Rows.Add(row);
        }
        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateThreeColumnTable(
        (string A, string B, string C)[] rows,
        string[] headers,
        bool includeHeader = true)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };
        table.Columns.Add(new TableColumn { Width = new GridLength(60) });
        table.Columns.Add(new TableColumn { Width = new GridLength(400) });
        table.Columns.Add(new TableColumn { Width = new GridLength(220) });

        var group = new TableRowGroup();

        if (includeHeader)
        {
            var header = new TableRow();
            for (var i = 0; i < headers.Length; i++)
                header.Cells.Add(CreateCell(headers[i], true, false, true, true));
            group.Rows.Add(header);
        }

        foreach (var item in rows)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(item.A, false, false, true));
            row.Cells.Add(CreateCell(item.B));
            row.Cells.Add(CreateCell(item.C, false, true));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateDeductionTable((string A, string B, string C)[] rows)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };
        table.Columns.Add(new TableColumn { Width = new GridLength(52) });
        table.Columns.Add(new TableColumn { Width = new GridLength(42) });
        table.Columns.Add(new TableColumn { Width = new GridLength(406) });
        table.Columns.Add(new TableColumn { Width = new GridLength(180) });

        var group = new TableRowGroup();
        var verticalCell = new TableCell
        {
            RowSpan = rows.Length,
            Padding = new Thickness(0),
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(0.5)
        };

        var verticalText = new TextBlock
        {
            Text = "KESİNTİLER VE MAHSUPLAR",
            FontSize = 9,
            FontWeight = FontWeights.Bold,
            TextAlignment = TextAlignment.Center,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Center,
            LayoutTransform = new System.Windows.Media.RotateTransform(-90),
            Width = 190,
            Height = 34
        };
        verticalCell.Blocks.Add(new BlockUIContainer(verticalText));

        for (var i = 0; i < rows.Length; i++)
        {
            var item = rows[i];
            var row = new TableRow();

            if (i == 0)
                row.Cells.Add(verticalCell);

            var shaded = item.A is "H" or "I";
            row.Cells.Add(CreateCell(item.A, item.A is "H" or "I", false, true, shaded));
            row.Cells.Add(CreateCell(item.B, item.A is "H" or "I", false, false, shaded));
            row.Cells.Add(CreateCell(item.C, item.A is "H" or "I", true, false, shaded));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateReportHeaderTable(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 8)
        };
        table.Columns.Add(new TableColumn { Width = new GridLength(525) });
        table.Columns.Add(new TableColumn { Width = new GridLength(155) });

        var group = new TableRowGroup();
        var left = CreateCell(route.Name, true);
        left.RowSpan = 2;
        left.Padding = new Thickness(7);
        var row1 = new TableRow();
        row1.Cells.Add(left);
        row1.Cells.Add(CreateCell($"Hakediş No : {latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"}", true, false, true));
        group.Rows.Add(row1);

        var row2 = new TableRow();
        row2.Cells.Add(CreateCell($"Hat No : {route.RouteNumber}", true, false, true));
        group.Rows.Add(row2);

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateIcmalHeaderTable(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 8)
        };
        table.Columns.Add(new TableColumn { Width = new GridLength(525) });
        table.Columns.Add(new TableColumn { Width = new GridLength(155) });

        var group = new TableRowGroup();
        var left = CreateCell(route.Name, true);
        left.RowSpan = 3;
        left.Padding = new Thickness(7);

        var row1 = new TableRow();
        row1.Cells.Add(left);
        row1.Cells.Add(CreateCell("Sayfa No : 1", true, false, true));
        group.Rows.Add(row1);

        var row2 = new TableRow();
        row2.Cells.Add(CreateCell($"Hakediş No : {latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"}", true, false, true));
        group.Rows.Add(row2);

        var row3 = new TableRow();
        row3.Cells.Add(CreateCell($"Hat No : {route.RouteNumber}", true, false, true));
        group.Rows.Add(row3);

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateExtensionTable()
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };
        foreach (var width in FourColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        var title = CreateCell("Süre Uzatım Kararlarının", true, false, true, true);
        title.ColumnSpan = 4;
        group.Rows.Add(new TableRow { Cells = { title } });

        var header = new TableRow();
        foreach (var h in ExtensionHeaders)
            header.Cells.Add(CreateCell(h, true, false, true, true));
        group.Rows.Add(header);

        for (var i = 0; i < 4; i++)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell("................", false, false, true));
            row.Cells.Add(CreateCell("................", false, false, true));
            row.Cells.Add(CreateCell("................", false, false, true));
            row.Cells.Add(CreateCell("................", false, false, true));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateThreeValueTable(params (string A, string B, string C)[] rows)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };

        foreach (var width in ThreeValueWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        foreach (var item in rows)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(item.A, true, false, true));
            row.Cells.Add(CreateCell(item.B, true));
            row.Cells.Add(CreateCell(item.C, true, true));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateFourColumnTable(IEnumerable<(string A, string B, string C, string D)> rows, string[] headers)
    {
        var table = new Table { CellSpacing = 0, BorderBrush = System.Windows.Media.Brushes.Black, BorderThickness = new Thickness(1), Margin = new Thickness(0, 0, 0, 10) };
        foreach (var width in FourColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        var header = new TableRow();
        foreach (var h in headers)
            header.Cells.Add(CreateCell(h, true, false, true, true));
        group.Rows.Add(header);

        foreach (var item in rows)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(item.A, false, false, true));
            row.Cells.Add(CreateCell(item.B));
            row.Cells.Add(CreateCell(item.C, false, true));
            row.Cells.Add(CreateCell(item.D, false, true));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateFiveColumnTable(IEnumerable<(string A, string B, string C, string D, string E)> rows, string[] headers)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderBrush = System.Windows.Media.Brushes.Black,
            BorderThickness = new Thickness(1),
            Margin = new Thickness(0, 0, 0, 10)
        };

        foreach (var width in FiveColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

        var group = new TableRowGroup();
        var header = new TableRow();
        foreach (var h in headers)
            header.Cells.Add(CreateCell(h, true, false, true, true));
        group.Rows.Add(header);

        foreach (var item in rows)
        {
            var row = new TableRow();
            row.Cells.Add(CreateCell(item.A, false, false, true));
            row.Cells.Add(CreateCell(item.B));
            row.Cells.Add(CreateCell(item.C, false, true));
            row.Cells.Add(CreateCell(item.D, false, true));
            row.Cells.Add(CreateCell(item.E, false, true));
            group.Rows.Add(row);
        }

        table.RowGroups.Add(group);
        return table;
    }

    private static Table CreateSignatureTable(
        string leftTitle, string leftName, string leftRole,
        string rightTitle, string rightName, string rightRole)
    {
        var table = new Table
        {
            CellSpacing = 0,
            BorderThickness = new Thickness(0),
            Margin = new Thickness(0, 18, 0, 8)
        };

        table.Columns.Add(new TableColumn { Width = new GridLength(340) });
        table.Columns.Add(new TableColumn { Width = new GridLength(340) });

        var group = new TableRowGroup();
        var row = new TableRow();

        row.Cells.Add(CreateSignatureCell(leftTitle, leftName, leftRole));
        row.Cells.Add(CreateSignatureCell(rightTitle, rightName, rightRole));

        group.Rows.Add(row);
        table.RowGroups.Add(group);
        return table;
    }

    private static TableCell CreateSignatureCell(string title, string name, string role)
    {
        var cell = new TableCell
        {
            BorderThickness = new Thickness(0),
            Padding = new Thickness(12, 2, 12, 2)
        };

        cell.Blocks.Add(new Paragraph(new Run(title))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 14)
        });

        cell.Blocks.Add(new Paragraph(new Run(name))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 4)
        });

        cell.Blocks.Add(new Paragraph(new Run(role))
        {
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0)
        });

        return cell;
    }

    private static FlowDocument CreateBaseDocument(string title, string subtitle)
    {
        const double a4Width = 794;
        const double a4Height = 1123;
        const double contentWidth = 680;

        var document = new FlowDocument
        {
            PageWidth = a4Width,
            PageHeight = a4Height,
            PagePadding = new Thickness(40, 32, 40, 32),
            FontFamily = new System.Windows.Media.FontFamily("Arial"),
            FontSize = 9,
            ColumnWidth = contentWidth,
            IsOptimalParagraphEnabled = true
        };

        var header = new Paragraph
        {
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 10)
        };
        header.Inlines.Add(new Run(title) { FontSize = 15, FontWeight = FontWeights.Bold });
        if (!string.IsNullOrWhiteSpace(subtitle))
        {
            header.Inlines.Add(new LineBreak());
            header.Inlines.Add(new Run(subtitle) { FontSize = 9.5, FontWeight = FontWeights.Bold });
        }

        document.Blocks.Add(header);
        return document;
    }

    private static Paragraph SectionHeading(string text)
    {
        return new Paragraph(new Run(text))
        {
            FontSize = 10.5,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 5, 0, 4)
        };
    }

    private static bool TryShowDocument(FlowDocument document, string title)
    {
        var dialog = new System.Windows.Controls.PrintDialog
        {
            PrintTicket = new PrintTicket
            {
                PageMediaSize = new PageMediaSize(PageMediaSizeName.ISOA4),
                PageOrientation = PageOrientation.Portrait
            }
        };

        if (dialog.ShowDialog() != true)
            return false;

        dialog.PrintDocument(((IDocumentPaginatorSource)document).DocumentPaginator, title);
        return true;
    }

    private void PreviewAllRouteDocumentsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput())
            return;

        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var latest = GetSelectedRouteLatestPayment()!;

        var documents = new (FlowDocument Document, string Title)[]
        {
            (BuildHakedişRaporu(route, latest), "Hakediş Raporu"),
            (BuildHizmetIsleriHakedisRaporu(route, latest), "Hizmet İşleri Hakediş Raporu"),
            (BuildKabulTutanağı(route, latest), "Hizmet İşleri Kabul Tutanağı"),
            (BuildHakedişIcmali(route, latest), "Hakediş İcmali")
        };

        var previewWindow = new Window
        {
            Title = "OEHS — Belge Önizleme",
            Owner = this,
            Width = 1180,
            Height = 820,
            MinWidth = 900,
            MinHeight = 650,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = System.Windows.Media.Brushes.White
        };

        var root = new Grid { Margin = new Thickness(10) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBlock = new TextBlock
        {
            Text = "BELGE ÖNİZLEME",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            Margin = new Thickness(4, 4, 4, 8)
        };
        Grid.SetRow(titleBlock, 0);
        root.Children.Add(titleBlock);

        var tabs = new System.Windows.Controls.TabControl();
        foreach (var item in documents)
        {
            var viewer = new System.Windows.Controls.FlowDocumentScrollViewer
            {
                Document = item.Document,
                HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
                VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
                HorizontalContentAlignment = HorizontalAlignment.Center,
                VerticalContentAlignment = VerticalAlignment.Top,
                Margin = new Thickness(4),
                IsToolBarVisible = true
            };
            tabs.Items.Add(new System.Windows.Controls.TabItem
            {
                Header = item.Title,
                Content = viewer
            });
        }
        Grid.SetRow(tabs, 1);
        root.Children.Add(tabs);

        var closeButton = new System.Windows.Controls.Button
        {
            Content = "KAPAT",
            Width = 110,
            Height = 38,
            HorizontalAlignment = HorizontalAlignment.Right,
            Margin = new Thickness(4, 8, 4, 2)
        };
        closeButton.Click += (_, _) => previewWindow.Close();
        Grid.SetRow(closeButton, 2);
        root.Children.Add(closeButton);

        previewWindow.Content = root;
        previewWindow.ShowDialog();
    }

    private bool EnsureRouteForOutput()
    {
        RequireSelectedRoute();

        if (ReportRouteComboBox.SelectedItem is not RouteDraft route)
        {
            MessageBox.Show("Önce Hatlar ekranından en az bir hat tanımlayın.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        if (GetSelectedRouteLatestPayment() is null)
        {
            MessageBox.Show("Seçilen hat için henüz hakediş kaydı yok.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
            return false;
        }

        return true;
    }

    private FlowDocument BuildHakedişRaporu(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var payments = GetSelectedRoutePayments();
        var previousTotal = payments
            .Where(x => x.Id != latest.Id && x.CreatedAt < latest.CreatedAt)
            .Sum(x => x.Calculation.TotalAmount);
        var currentDifference = latest.Calculation.TotalAmount - previousTotal;

        var doc = CreateBaseDocument(
            "HAK EDİŞ RAPORU",
            $"{route.Name} — Hat No : {route.RouteNumber}    Hakediş No : {latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"}");

        doc.Blocks.Add(CreateReportHeaderTable(route, latest));

        doc.Blocks.Add(SectionHeading(
            $"{latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"} TARİHİNE KADAR YAPILAN İŞİN"));

        var mainRows = new List<(string, string, string)>
        {
            ("A", "Sözleşme Fiyatları İle Yapılan İş", FormatMoney(latest.Calculation.CombinedServiceAmount)),
            ("B", "Fiyat Farkı Tutarı", FormatMoney(latest.Calculation.CombinedPriceDifferenceExcludingVat)),
            ("C", "Toplam Tutar  ( A + B )", FormatMoney(latest.Calculation.TotalAmount)),
            ("D", "Bir Önceki Hakedişlerin Toplam Tutarı", FormatMoney(previousTotal)),
            ("E", "Bu Hakedişin Tutarı  ( C - D )", FormatMoney(currentDifference)),
            ("F", "KDV  %10 + %20", FormatMoney(latest.Calculation.Vat)),
            ("G", "Tahakkuk Tutarı", FormatMoney(latest.Calculation.GrossAmount))
        };
        doc.Blocks.Add(CreateThreeColumnTable(mainRows.ToArray(), MainReportHeaders, false));
        var d = latest.Calculation.Deductions;
        var hRows = new List<(string, string, string)>
        {
            ("a)", "Gelir / Kurumlar Vergisi  ( E x % .. )", "0,00 ₺"),
            ("b)", "Damga Vergisi  ( E - g x % 9,48 )", FormatMoney(latest.Calculation.StampTax)),
            ("c)", "KDV Tevkifatı  ( 5/10 + 9/10 kesilen )", FormatMoney(latest.Calculation.Withholding)),
            ("d)", "Sosyal Sigortalar Kurumu Kesintisi", FormatMoney(d.SocialSecurityDebt)),
            ("e)", "İdare Makinesi Kiraları", "0,00 ₺"),
            ("f)", "Gecikme Cezası", "0,00 ₺"),
            ("g)", "Avans Mahsubu", "0,00 ₺"),
            ("h)", "Bu Hakedişle Ödenen Fiyat Farkı Teminat Kesintisi", FormatMoney(latest.Calculation.AdditionalGuarantee)),
            ("ı)", "Vergi Borcu", FormatMoney(d.TaxDebt)),
            ("j)", "İcra Borcu", "0,00 ₺"),
            ("k)", "Ceza ve Diğer Kesintiler", FormatMoney(d.InspectionPenalty)),
            ("l)", "Asgari Ücret Destek Tutarı Kesintisi", FormatMoney(d.MinimumWageSupport)),
            ("H", "Kesintiler ve Mahsuplar Toplamı", FormatMoney(latest.Calculation.TotalDeductions)),
            ("I", "Yükleniciye Ödenecek Tutar  ( G - H )", FormatMoney(latest.Calculation.NetPayable))
        };
        doc.Blocks.Add(CreateDeductionTable(hRows.ToArray()));

        var contractor = string.IsNullOrWhiteSpace(latest.ContractorName)
            ? ContractorTextBox.Text.Trim()
            : latest.ContractorName.Trim();

        doc.Blocks.Add(CreateDocumentTable(
            ("Yüklenici", contractor)));

        doc.Blocks.Add(CreateSignatureTable(
            "Düzenleyen", "Aşkın KESKİNBALTA", "Memur",
            "Kontrol Eden", "Halil ÖZENÇ", "İlçe Milli Eğitim Şube Müdürü"));

        doc.Blocks.Add(new Paragraph(new Run(DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture)))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 10, 0, 10)
        });

        doc.Blocks.Add(new Paragraph(new Run("Özgür BAYSAL"))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.SemiBold,
            Margin = new Thickness(0, 0, 0, 3)
        });
        doc.Blocks.Add(new Paragraph(new Run("İlçe Milli Eğitim Müdürü"))
        {
            TextAlignment = TextAlignment.Center
        });
        doc.Blocks.Add(new Paragraph(new Run("M.Y.H.B.Y. Örnek No : 4/10"))
        {
            FontSize = 8,
            Margin = new Thickness(0, 2, 0, 0)
        });

        return doc;
    }

    private FlowDocument BuildHizmetIsleriHakedisRaporu(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var doc = CreateBaseDocument("HİZMET İŞLERİ HAK EDİŞ RAPORU", "");
        doc.Blocks.Add(CreateDocumentTable(
            ("Tarihi", latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("No.su", latest.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"),
            ("Uygulama Yılı", (latest.PeriodDate?.Year ?? DateTime.Today.Year).ToString(CultureInfo.CurrentCulture)),
            ("Yapılan Hizmetin Adı", route.Name),
            ("Yapılan Hizmetin Etüt / Proje No.su", "Proje Dışıdır."),
            ("Yüklenicinin Adı / Ticari Unvanı", string.IsNullOrWhiteSpace(latest.ContractorName) ? ContractorTextBox.Text : latest.ContractorName),
            ("Sözleşme Bedeli", FormatMoney(latest.Input.ContractAmount)),
            ("İhale Tarihi", latest.TenderDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme Tarihi ve No.su", latest.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşmenin Sayıştayca Tescil Tarihi ve No'su", "Tescile Dahil Değil"),
            ("Tescilin Yükleniciye Tebliğ Tarihi", "Tescile Dahil Değil"),
            ("İşyeri Teslim Tarihi", latest.ContractStartDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşmeye Göre İşin Süresi", $"{Math.Max(0, (latest.ContractEndDate - latest.ContractStartDate)?.Days ?? 0)} Gün"),
            ("Sözleşmeye Göre İş Bitim Tarihi", latest.ContractEndDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Verilen Avanslar Toplamı", "Avans verilmeyecektir."),
            ("Mahsubu Yapılan Avansın Toplam Tutarı", "Yoktur.")
        ));

        doc.Blocks.Add(CreateFourColumnTable(
            new[] { ("", "Sözleşme Bedeli", FormatMoney(latest.Input.ContractAmount), "Toplam Sözleşme Bedeli"),
                    ("", "", "", FormatMoney(latest.Input.ContractAmount)) },
            ContractHeaders));

        doc.Blocks.Add(CreateExtensionTable());

        doc.Blocks.Add(CreateSignatureTable(
            "Düzenleyen", "Aşkın KESKİNBALTA", "Memur",
            "Kontrol Eden", "Halil ÖZENÇ", "İlçe Milli Eğitim Şube Müdürü"));
        doc.Blocks.Add(new Paragraph(new Run("M.Y.H.B.Y. Örnek No: 4"))
        {
            FontSize = 8,
            Margin = new Thickness(0, 8, 0, 0)
        });
        return doc;
    }

    private FlowDocument BuildKabulTutanağı(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var doc = CreateBaseDocument("HİZMET İŞLERİ KABUL TUTANAĞI", "");

        var tr = new CultureInfo("tr-TR");
        var contractStart = StartDatePicker.SelectedDate;
        var contractEnd = EndDatePicker.SelectedDate;
        var duration = Math.Max(0, (contractEnd - contractStart)?.Days ?? 0);

        // Örnekteki kabul dönemi, seçilen hakediş tarihini takip eden takvim ayıdır.
        // Böylece 29.09.2026 tarihli kayıt için metin 01.10.2026 / 31.10.2026 şeklinde oluşur.
        var periodAnchor = latest.PeriodDate ?? DateOnly.FromDateTime(DateTime.Today);
        var serviceMonth = periodAnchor.AddMonths(1);
        var serviceStart = new DateOnly(serviceMonth.Year, serviceMonth.Month, 1);
        var serviceEnd = serviceStart.AddMonths(1).AddDays(-1);

        var contractAmount = latest.Input.ContractAmount;
        var contractor = string.IsNullOrWhiteSpace(latest.ContractorName)
            ? (string.IsNullOrWhiteSpace(ContractorTextBox.Text)
                ? "S.S. Selçikler Min. Kar. Yolcu Taşıma Kooperatifi"
                : ContractorTextBox.Text.Trim())
            : latest.ContractorName.Trim();

        doc.Blocks.Add(CreateDocumentTable(
            ("İşin adı", route.Name),
            ("Yüklenicinin adı/ticari unvanı", contractor),
            ("Sözleşme tarihi", contractStart?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme bedeli", FormatMoney(contractAmount)),
            ("Sözleşmeye göre işin süresi(takvim günü)", $"{duration} İş Günü"),
            ("Sözleşmeye göre işin bitirilmesi gereken tarih", contractEnd?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Varsa süre uzatımları", ""),
            ("Süre uzatımı dahil işin bitirilmesi gereken tarih", contractEnd?.ToString("dd MMMM yyyy dddd", tr) ?? "-"),
            ("İşin bitirildiği tarih", contractEnd?.ToString("dd MMMM yyyy dddd", tr) ?? "-")));

        var paragraphText =
            $"İlçe Milli Eğitim Müdürlüğü'nün {route.RouteNumber} Numaralı {route.Name} işi yüklenicisi {contractor} arasında imzalanan sözleşme kapsamında gerçekleştirilen iş için, " +
            $"Memur Aşkın KESKİNBALTA ve İlçe Milli Eğitim Şube Müdürü Halil ÖZENÇ tarafından inceleme yapılmış yüklenici, " +
            $"{contractor} tarafından {serviceStart:dd.MM.yyyy} / {serviceEnd:dd.MM.yyyy} tarihleri arasında yapılan işlerin, " +
            "Sözleşme ve eklerine uygun olduğu ve kabule engel olabilecek eksik kusur ve arızaların bulunmadığı görülmüştür.";

        doc.Blocks.Add(new Paragraph(new Run(paragraphText))
        {
            TextAlignment = TextAlignment.Justify,
            Margin = new Thickness(0, 20, 0, 52),
            FontSize = 10
        });

        doc.Blocks.Add(new Paragraph(new Run("Yüklenici"))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 3)
        });
        doc.Blocks.Add(new Paragraph(new Run(contractor))
        {
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 0, 0, 58)
        });

        doc.Blocks.Add(CreateSignatureTable(
            "Düzenleyen", "Aşkın KESKİNBALTA", "Memur",
            "Kontrol Eden", "Halil ÖZENÇ", "İlçe Milli Eğitim Şube Müdürü"));
        doc.Blocks.Add(new Paragraph(new Run(DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture)))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        doc.Blocks.Add(new Paragraph(new Run("Özgür BAYSAL"))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        });
        doc.Blocks.Add(new Paragraph(new Run("İlçe Milli Eğitim Müdürü"))
        {
            TextAlignment = TextAlignment.Center
        });

        return doc;
    }

    private FlowDocument BuildHakedişIcmali(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var payments = GetSelectedRoutePayments();
        var latestDate = latest.PeriodDate ?? DateOnly.FromDateTime(DateTime.Today);
        var baseYear = latestDate.Month >= 9 ? latestDate.Year : latestDate.Year - 1;

        var rows = new List<(string, string, string, string, string)>();
        var totalA = 0m;
        var totalB = 0m;

        for (var monthIndex = 0; monthIndex < 11; monthIndex++)
        {
            var month = new DateOnly(baseYear, 9, 1).AddMonths(monthIndex);
            var monthPayment = payments.LastOrDefault(x =>
                x.PeriodDate.HasValue &&
                x.PeriodDate.Value.Year == month.Year &&
                x.PeriodDate.Value.Month == month.Month);

            var a = monthPayment?.Calculation.TotalAmount ?? 0m;
            var b = monthPayment?.Calculation.CombinedPriceDifferenceExcludingVat ?? 0m;
            var monthlyTotal = a + b;
            totalA += a;
            totalB += b;

            var monthName = month.ToString("MMMM yyyy", new CultureInfo("tr-TR"));
            rows.Add((
                (monthIndex + 1).ToString(CultureInfo.CurrentCulture),
                $"Hizmet Alımı İşi {monthName}",
                FormatMoney(a),
                FormatMoney(b),
                FormatMoney(monthlyTotal)));
        }

        var doc = CreateBaseDocument("HAKEDİŞ İCMALİ", "");
        doc.Blocks.Add(CreateIcmalHeaderTable(route, latest));

        doc.Blocks.Add(CreateFiveColumnTable(
            rows,
            IcmalHeaders));

        var total = totalA + totalB;
        doc.Blocks.Add(CreateThreeValueTable(
            ("A", "T O P L A M", FormatMoney(total))));

        var previousDeduction = payments
            .Where(x => x.Id != latest.Id && x.CreatedAt < latest.CreatedAt)
            .Sum(x => x.Calculation.TotalDeductions);
        var currentDeduction = latest.Calculation.TotalDeductions;
        var totalDeduction = previousDeduction + currentDeduction;
        var net = total - totalDeduction;

        doc.Blocks.Add(CreateThreeValueTable(
            ("a)", "Bir Evvelki Hakedişten Yapılan Kesinti", FormatMoney(previousDeduction)),
            ("b)", "Bu Hakedişten Yapılan Kesinti", FormatMoney(currentDeduction)),
            ("B", "Toplam Kesinti Tutarı", FormatMoney(totalDeduction)),
            ("C", "Toplam Hakediş Tutarı : ( A - B )", FormatMoney(net))));

        doc.Blocks.Add(CreateSignatureTable(
            "Düzenleyen", "Aşkın KESKİNBALTA", "Memur",
            "Kontrol Eden", "Halil ÖZENÇ", "İlçe Milli Eğitim Şube Müdürü"));
        doc.Blocks.Add(new Paragraph(new Run(DateTime.Today.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture)))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 8)
        });
        doc.Blocks.Add(new Paragraph(new Run("Özgür BAYSAL"))
        {
            TextAlignment = TextAlignment.Center,
            FontWeight = FontWeights.Bold,
            Margin = new Thickness(0, 0, 0, 2)
        });
        doc.Blocks.Add(new Paragraph(new Run("İlçe Milli Eğitim Müdürü"))
        {
            TextAlignment = TextAlignment.Center
        });
        doc.Blocks.Add(new Paragraph(new Run("M.Y.H.B.Y. Örnek No :4/9"))
        {
            FontSize = 8,
            Margin = new Thickness(0, 12, 0, 0)
        });

        return doc;
    }

    private void PrintAllRouteDocumentsButton_Click(object sender, RoutedEventArgs e)
    {
        if (!EnsureRouteForOutput())
            return;

        var route = (RouteDraft)ReportRouteComboBox.SelectedItem;
        var latest = GetSelectedRouteLatestPayment()!;

        var documents = new (FlowDocument Document, string Title)[]
        {
            (BuildHakedişRaporu(route, latest), "Hakediş Raporu"),
            (BuildHizmetIsleriHakedisRaporu(route, latest), "Hizmet İşleri Hakediş Raporu"),
            (BuildKabulTutanağı(route, latest), "Hizmet İşleri Kabul Tutanağı"),
            (BuildHakedişIcmali(route, latest), "Hakediş İcmali")
        };

        foreach (var item in documents)
        {
            if (!TryShowDocument(item.Document, item.Title))
                break;
        }
    }

    private void ReportActionButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is System.Windows.Controls.Button button && button.Tag is string)
            NavigationButton_Click(button, e);
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

    private sealed record RouteDraft(
        string RouteNumber,
        string Name,
        decimal DailyPrice,
        int GuideWorkDays = 0,
        decimal GuideDailyPrice = 0m)
    {
        public string DailyPriceText => $"Taşıma: {DailyPrice:N2} ₺";
        public string GuideDailyPriceText => $"Rehber: {GuideDailyPrice:N2} ₺";
        public string ServiceSummaryText =>
            $"Hizmet hattı • Rehber {GuideWorkDays} yıllık gün";
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
