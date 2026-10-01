using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Interop;
using System.Windows.Documents;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow : Window
{
    // Pencere, ekran çalışma alanını doğrudan kullanır; eski Viewbox tasarım oranı
    // yatay alanı daraltıp metinleri sıkıştırıyordu.

    private const int WmGetMinMaxInfo = 0x0024;
    private const uint MonitorDefaultToNearest = 0x00000002;
    private HwndSource? _windowSource;
    private readonly ObservableCollection<ContractDraftData> _contracts = new();
    private readonly ObservableCollection<RouteDraft> _routes = new();
    private readonly ObservableCollection<ContractorDraft> _contractors = new();
    private readonly ObservableCollection<ProgressPaymentHistoryData> _progressHistory = new();
    private readonly ObservableCollection<HistoryRow> _historyRows = new();
    private readonly ObservableCollection<SummaryRow> _summaryRows = new();
    private readonly ObservableCollection<ReportRow> _reportRows = new();
    private Guid? _editingHistoryId;
    private string? _editingContractTender;
    private string? _editingContractorKey;
    private RouteDraft? _editingRoute;
    private bool _suppressSelectionEvents;
    private readonly OehsApplicationDataStore _dataStore = new();
    private readonly ProgressPaymentArchiveStore _archiveStore = new();
    private readonly ObservableCollection<ArchiveRow> _archiveRows = new();
    private Guid? _lastCalculatedHistoryId;
    private string? _editingArchiveSourcePath;
    private ProgressPaymentArchive? _editingArchiveSnapshot;
    private ArchiveCorrectionSection _archiveCorrectionSection;
    private bool _dataLoadFailed;
    private const double DocumentContentWidth = PdfLayoutService.SafeContentWidth;
    private static readonly double[] ThreeColumnWidths = { 58d, 438d, 194d };
    private static readonly double[] ThreeValueWidths = { 45d, 360d, 135d };
    private static readonly double[] FourColumnWidths = { 55d, 308d, 163d, 164d };
    private static readonly double[] FiveColumnWidths = { 40d, 278d, 124d, 124d, 124d };
    private static readonly string[] MainReportHeaders = { "", "Kalem", "Tutar" };
    private static readonly string[] DeductionHeaders = { "", "KESİNTİLER VE MAHSUPLAR", "Tutar" };
    private static readonly string[] ContractHeaders = { "Sözleşme Bedeli", "Sözleşme Artış Onayının Tarihi No.su", "Ek Sözleşme Bedeli", "Toplam Sözleşme Bedeli" };
    private static readonly string[] ExtensionHeaders = { "Tarih", "Sayısı", "Verilen Süre", "İş Bitim Tarihi" };
    private static readonly string[] IcmalHeaders = { "Sıra No", "İşin Cinsi", "İş Tutarı (a)", "Fiyat Farkı Tutarı (b)", "Toplam (a+b)" };
    private static readonly string[] OfficialDocumentNames =
        ProgressPaymentReportService.RequiredDocumentNames.ToArray();



    public MainWindow()
    {
        InitializeComponent();
        ResetEditorUiToCreateMode();
        ConfigureWindow();
        ConfigureZeroValueInputs();
        ConfigureContractValidationInputs();
        Loaded += MainWindow_Loaded;
        SourceInitialized += MainWindow_SourceInitialized;
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        TaxDebtTextBox.Text = "0";
        SocialSecurityDebtTextBox.Text = "0";
        MinimumWageSupportTextBox.Text = "0";
        InspectionPenaltyTextBox.Text = "0";
        DiscountTextBox.Text = "0";
        GuideWorkDaysTextBox.Text = "0";
        GuideDailyPriceTextBox.Text = "0";
        GuideDiscountTextBox.Text = "0";
        GuidePriceDifferenceTextBox.Text = "0";
        UpdateShellText("Ana Ekran");
        RoutesListBox.ItemsSource = _routes;
        ContractorsListBox.ItemsSource = _contractors;
        HomeTenderListBox.ItemsSource = _contracts;
        RouteTenderListBox.ItemsSource = _contracts;
        ProgressTenderListBox.ItemsSource = _contracts;
        HistoryItemsControl.ItemsSource = _historyRows;
        ArchiveItemsControl.ItemsSource = _archiveRows;
        SummaryItemsControl.ItemsSource = _summaryRows;
        ReportItemsControl.ItemsSource = _reportRows;
        ProgressRouteComboBox.ItemsSource = _routes;
        ReportRouteComboBox.ItemsSource = _routes;
        OutputTenderListBox.ItemsSource = _contracts;
        LoadPersistedData();

        UpdateContractSelectors();
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
        UpdateArchiveView();
    }

    private void MainWindow_Loaded(object sender, RoutedEventArgs e)
    {
        Dispatcher.BeginInvoke(
            System.Windows.Threading.DispatcherPriority.Loaded,
            new Action(() => NormalizeButtonWidths(this)));
    }

    private static void NormalizeButtonWidths(DependencyObject root)
    {
        if (root is Button button &&
            button.Visibility == Visibility.Visible &&
            !double.IsNaN(button.Width) &&
            button.Width > 0)
        {
            var originalWidth = button.Width;
            button.Width = double.NaN;
            button.Measure(new Size(double.PositiveInfinity, double.PositiveInfinity));

            var desiredWidth = Math.Ceiling(button.DesiredSize.Width + 32);
            button.Width = Math.Max(originalWidth, desiredWidth);
        }

        var childCount = System.Windows.Media.VisualTreeHelper.GetChildrenCount(root);
        for (var i = 0; i < childCount; i++)
            NormalizeButtonWidths(System.Windows.Media.VisualTreeHelper.GetChild(root, i));
    }

    private enum ArchiveCorrectionSection
    {
        None,
        Contract,
        Contractor,
        Route,
        ProgressPayment
    }

    private bool IsArchiveCorrection =>
        _archiveCorrectionSection != ArchiveCorrectionSection.None &&
        !string.IsNullOrWhiteSpace(_editingArchiveSourcePath) &&
        _editingArchiveSnapshot is not null;

    private bool IsArchiveCorrectionFor(ArchiveCorrectionSection section) =>
        IsArchiveCorrection && _archiveCorrectionSection == section;

    private void ResetEditorUiToCreateMode()
    {
        SaveContractButton.Content = "KAYDET";
        CancelContractEditButton.Visibility = Visibility.Collapsed;
        ClearContractButton.IsEnabled = true;
        ContractEditorTitleTextBlock.Text = "Sözleşme Bilgileri";
        ContractEditorSubtitleTextBlock.Text = "Temel ihale ve sözleşme alanları";

        SaveContractorButton.Content = "KAYDET";
        CancelContractorEditButton.Visibility = Visibility.Collapsed;
        ClearContractorButton.IsEnabled = true;
        ContractorEditorTitleTextBlock.Text = "FİRMA BİLGİLERİ";

        AddRouteButton.Content = "HAT EKLE";
        CancelRouteEditButton.Visibility = Visibility.Collapsed;
        ClearRouteButton.IsEnabled = true;
        RouteEditorTitleTextBlock.Text = "YENİ HAT";
        RouteEditorSubtitleTextBlock.Text = "Hat bilgilerini girin";
        RouteSelectionCard.Visibility = Visibility.Visible;
        RouteNumberTextBox.IsReadOnly = false;

        CalculateProgressButton.Content = "HAKEDİŞİ HESAPLA";
        CancelProgressEditButton.Visibility = Visibility.Collapsed;
        EditCurrentProgressButton.IsEnabled = true;
        ProgressEditorTitleTextBlock.Text = "DÖNEM HESAP BİLGİLERİ";
    }

    private void SetContractEditorMode(bool editing)
    {
        SaveContractButton.Content = editing ? "DÜZENLE" : "KAYDET";
        CancelContractEditButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
        ClearContractButton.IsEnabled = !editing;
        ContractEditorTitleTextBlock.Text = editing ? "İHALE / SÖZLEŞME DÜZENLE" : "Sözleşme Bilgileri";
        ContractEditorSubtitleTextBlock.Text = editing
            ? "Mevcut kaydı düzenliyorsunuz. Vazgeçerseniz DÜZENLEME İPTAL düğmesini kullanın."
            : "Temel ihale ve sözleşme alanları";
    }

    private void SetContractorEditorMode(bool editing)
    {
        SaveContractorButton.Content = editing ? "DÜZENLE" : "KAYDET";
        CancelContractorEditButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
        ClearContractorButton.IsEnabled = !editing;
        ContractorEditorTitleTextBlock.Text = editing ? "YÜKLENİCİ FİRMA DÜZENLE" : "FİRMA BİLGİLERİ";
    }

    private void SetRouteEditorMode(bool editing, bool archiveCorrection = false)
    {
        AddRouteButton.Content = editing ? "DÜZENLE" : "HAT EKLE";
        CancelRouteEditButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
        ClearRouteButton.IsEnabled = !editing;
        RouteEditorTitleTextBlock.Text = editing
            ? (archiveCorrection ? "ARŞİVDEKİ HAT DÜZENLE" : "HAT DÜZENLE")
            : "YENİ HAT";
        RouteEditorSubtitleTextBlock.Text = editing
            ? (archiveCorrection
                ? "Bu alan yalnızca seçtiğiniz arşiv kaydını günceller; yeni hat oluşturulmaz."
                : "Mevcut hattı düzenliyorsunuz. Vazgeçerseniz DÜZENLEME İPTAL düğmesini kullanın.")
            : "Hat bilgilerini girin";
        RouteSelectionCard.Visibility = archiveCorrection ? Visibility.Collapsed : Visibility.Visible;
        RouteNumberTextBox.IsReadOnly = false;
    }

    private void SetProgressEditorMode(bool editing, bool archiveCorrection = false)
    {
        CalculateProgressButton.Content = editing ? "DÜZENLE" : "HAKEDİŞİ HESAPLA";
        CancelProgressEditButton.Visibility = editing ? Visibility.Visible : Visibility.Collapsed;
        EditCurrentProgressButton.IsEnabled = !editing;
        ProgressEditorTitleTextBlock.Text = editing
            ? (archiveCorrection ? "ARŞİV HAKEDİŞ DÜZENLE" : "HAKEDİŞ DÜZENLE")
            : "DÖNEM HESAP BİLGİLERİ";
    }

    private void ClearArchiveCorrectionState()
    {
        _editingArchiveSourcePath = null;
        _editingArchiveSnapshot = null;
        _archiveCorrectionSection = ArchiveCorrectionSection.None;
        ResetEditorUiToCreateMode();
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

        // Çalışma alanının neredeyse tamamını kullan: yatayda daha fazla alan,
        // dikeyde görev çubuğunun altında kalmadan mümkün olan maksimum yükseklik.
        Width = Math.Round(workArea.Width * 0.98);
        Height = Math.Round(workArea.Height * 0.985);

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
                        (string.Equals(x.TenderNumber, selectedRoute.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(x.RouteNumber, selectedRoute.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                         string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(selectedRoute.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal)))
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
            ? (string.IsNullOrWhiteSpace(CurrentContractorName) ? "Sözleşme bilgisi" : CurrentContractorName.Trim())
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

        if (IsArchiveCorrection)
        {
            var correctionPage = _archiveCorrectionSection switch
            {
                ArchiveCorrectionSection.Contract => "Contract",
                ArchiveCorrectionSection.Contractor => "Contractors",
                ArchiveCorrectionSection.Route => "Lines",
                ArchiveCorrectionSection.ProgressPayment => "Progress",
                _ => string.Empty
            };

            if (page == "Archives")
            {
                ClearArchiveCorrectionState();
            }
            else if (!string.Equals(page, correctionPage, StringComparison.Ordinal))
            {
                MessageBox.Show(
                    "Arşiv düzeltmesi devam ediyor. Önce DÜZENLEME İPTAL veya düzenleme kaydetme işlemini tamamlayın.",
                    "OEHS — Düzenleme Devam Ediyor",
                    MessageBoxButton.OK,
                    MessageBoxImage.Information);
                return;
            }
        }

        HomePanel.Visibility = page == "Home" ? Visibility.Visible : Visibility.Collapsed;
        ContractPanel.Visibility = page == "Contract" ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = page == "Progress" ? Visibility.Visible : Visibility.Collapsed;
        LinesPanel.Visibility = page == "Lines" ? Visibility.Visible : Visibility.Collapsed;
        ContractorsPanel.Visibility = page == "Contractors" ? Visibility.Visible : Visibility.Collapsed;
        HistoryPanel.Visibility = page == "History" ? Visibility.Visible : Visibility.Collapsed;
        ArchivePanel.Visibility = page == "Archives" ? Visibility.Visible : Visibility.Collapsed;
        SummaryPanel.Visibility = page == "Summary" ? Visibility.Visible : Visibility.Collapsed;
        ReportsPanel.Visibility = page == "Reports" ? Visibility.Visible : Visibility.Collapsed;
        AcceptancePanel.Visibility = page == "Acceptance" ? Visibility.Visible : Visibility.Collapsed;
        SettingsPanel.Visibility = page == "Settings" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderPanel.Visibility = page is "Home" or "Contract" or "Progress" or "Lines" or "Contractors"
            or "History" or "Archives" or "Summary" or "Reports" or "Acceptance" or "Settings"
            ? Visibility.Collapsed
            : Visibility.Visible;

        var module = page switch
        {
            "Contract" => "İhale / Sözleşme",
            "Progress" => "Hakediş Oluştur",
            "Lines" => "Hatlar",
            "Contractors" => "Yüklenici Firmalar",
            "History" => "Hakediş Geçmişi",
            "Archives" => "Arşivler",
            "Summary" => "İcmal",
            "Reports" => "Çıktılar",
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
            NavHome, NavContract, NavContractors, NavLines,
            NavProgress, NavReports, NavHistory, NavArchives, NavAcceptance, NavSettings
        })
        {
            button.Uid = button.Tag?.ToString() == page ? "Active" : string.Empty;
        }
    }

    private ContractorDraft? FindContractor(string taxOrIdentityNumber)
    {
        if (string.IsNullOrWhiteSpace(taxOrIdentityNumber))
            return null;

        var identity = OehsIdentityService.NormalizeIdentity(taxOrIdentityNumber);
        return _contractors.FirstOrDefault(x =>
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                identity,
                StringComparison.Ordinal));
    }

    private ContractDraftData? GetSelectedContract()
    {
        return HomeTenderListBox.SelectedItem as ContractDraftData
            ?? RouteTenderListBox.SelectedItem as ContractDraftData
            ?? ProgressTenderListBox.SelectedItem as ContractDraftData
            ?? _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private void ApplyContractToForm(ContractDraftData contract)
    {
        _suppressSelectionEvents = true;
        try
        {
            TenderNumberTextBox.Text = contract.TenderNumber;
            ContractNumberTextBox.Text = contract.ContractNumber;
            WorkNameTextBox.Text = contract.WorkName;
            ContractAmountTextBox.Text = contract.ContractAmount.ToString("N2", CultureInfo.CurrentCulture);
            ContractWorkDaysTextBox.Text = contract.ContractWorkDays.ToString(CultureInfo.CurrentCulture);
            ContractTransportDailyPriceTextBox.Text = contract.TransportDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            StartDatePicker.SelectedDate = contract.StartDate.ToDateTime(TimeOnly.MinValue);
            EndDatePicker.SelectedDate = contract.EndDate.ToDateTime(TimeOnly.MinValue);
            TenderDatePicker.SelectedDate = contract.TenderDate?.ToDateTime(TimeOnly.MinValue);
            ContractDatePicker.SelectedDate = contract.ContractDate?.ToDateTime(TimeOnly.MinValue);
            VatExemptCheckBox.IsChecked = contract.VatExempt;

        }
        finally
        {
            _suppressSelectionEvents = false;
        }

        UpdateContractSummaryView();
    }

    private void ClearContractFormOnly()
    {
        TenderNumberTextBox.Clear();
        ContractNumberTextBox.Clear();
        WorkNameTextBox.Clear();
        ContractWorkDaysTextBox.Clear();
        ContractTransportDailyPriceTextBox.Clear();
        ContractAmountTextBox.Clear();
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
        TenderDatePicker.SelectedDate = DateTime.Today;
        ContractDatePicker.SelectedDate = DateTime.Today;
        VatExemptCheckBox.IsChecked = false;
    }

    private void UpdateContractSelectors()
    {
        var selectedTender = TenderNumberTextBox.Text.Trim();
        HomeTenderListBox.ItemsSource = _contracts.ToArray();
        RouteTenderListBox.ItemsSource = _contracts.ToArray();
        ProgressTenderListBox.ItemsSource = _contracts.ToArray();
        OutputTenderListBox.ItemsSource = _contracts.ToArray();
        ContractsListBox.ItemsSource = _contracts.ToArray();

        var contract = _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, selectedTender, StringComparison.OrdinalIgnoreCase))
                       ?? _contracts.FirstOrDefault();

        if (contract is not null)
        {
            _suppressSelectionEvents = true;
            try
            {
                HomeTenderListBox.SelectedItem = contract;
                RouteTenderListBox.SelectedItem = contract;
                ProgressTenderListBox.SelectedItem = contract;
                OutputTenderListBox.SelectedItem = contract;
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            UpdateRouteContractorSelector(contract);
            UpdateProgressContractorAndRoutes(contract);
            UpdateOutputRoutesForContract(contract);
            UpdateHomeTenderSummary(contract);
        }
        else
        {
            RouteContractorListBox.ItemsSource = Array.Empty<ContractorDraft>();
            ProgressRouteComboBox.ItemsSource = Array.Empty<RouteDraft>();
        }
    }

    private void UpdateRouteContractorSelector(ContractDraftData? contract)
    {
        _suppressSelectionEvents = true;
        try
        {
            RouteContractorListBox.ItemsSource = _contractors.ToArray();
            RouteContractorListBox.SelectedIndex = -1;
        }
        finally
        {
            _suppressSelectionEvents = false;
        }

        UpdateRouteListView();
        RouteSelectedTenderTextBlock.Text = contract is null ? "İhale: seçilmedi" : $"İhale: {contract.TenderNumber}";
        RouteSelectedContractorTextBlock.Text = "Yüklenici: firma seçilmemiş";
    }

    private void UpdateRouteListView()
    {
        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        var contractor = RouteContractorListBox.SelectedItem as ContractorDraft;

        RoutesListBox.ItemsSource = _routes.Where(x =>
            contract is not null &&
            string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            contractor is not null &&
            string.Equals(
                    OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                    OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber),
                    StringComparison.Ordinal))
            .ToArray();
    }

    private void UpdateProgressContractorAndRoutes(ContractDraftData? contract)
    {
        if (contract is null)
        {
            ProgressContractorTextBlock.Text = "Seçilmedi";
            ProgressRouteComboBox.ItemsSource = Array.Empty<RouteDraft>();
            ProgressRouteRelationTextBlock.Text = "Hat seçiniz.";
            return;
        }

        var routes = _routes.Where(x =>
            string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var previous = ProgressRouteComboBox.SelectedItem as RouteDraft;
        ProgressRouteComboBox.ItemsSource = routes;

        if (previous is not null)
            ProgressRouteComboBox.SelectedItem = routes.FirstOrDefault(x =>
                x.RouteNumber == previous.RouteNumber &&
                string.Equals(
                    OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                    OehsIdentityService.NormalizeIdentity(previous.ContractorTaxOrIdentityNumber),
                    StringComparison.Ordinal));
        if (ProgressRouteComboBox.SelectedItem is null && routes.Length > 0)
            ProgressRouteComboBox.SelectedIndex = 0;

        UpdateProgressRouteDefaults();
    }

    private void UpdateHomeTenderSummary(ContractDraftData? contract)
    {
        if (contract is null)
        {
            HomeSelectedTenderSummaryTextBlock.Text = "İhale seçiniz.";
            return;
        }

        HomeSelectedTenderSummaryTextBlock.Text =
            $"{contract.TenderNumber} • {contract.WorkName} • " +
            $"Sözleşme {FormatMoney(contract.ContractAmount)} • " +
            $"{contract.StartDate:dd.MM.yyyy}–{contract.EndDate:dd.MM.yyyy} • " +
            $"{(_routes.Count(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase)))} hat";
    }

    private void ShowAlreadyRegistered(string message)
    {
        MessageBox.Show(message, "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private static string BuildArchivePdfFileName(ProgressPaymentHistoryData payment)
    {
        if (payment.ProgressPaymentNumber.HasValue && payment.PeriodDate.HasValue)
        {
            return OehsFileNameService.BuildProgressPdfFileName(
                payment.TenderNumber ?? string.Empty,
                payment.RouteNumber ?? string.Empty,
                payment.ProgressPaymentNumber.Value,
                payment.PeriodDate.Value);
        }

        return "OEHS_Hakediş.pdf";
    }

    private void SaveArchiveCorrectionSnapshot(ProgressPaymentArchive updatedArchive, string sectionName)
    {
        try
        {
            var finalPath = _archiveStore.Replace(_editingArchiveSourcePath!, updatedArchive);
            ArchiveStatusTextBlock.Text = $"ARŞİV v{updatedArchive.Revision:00} GÜNCELLENDİ";
            CalculationStatusTextBlock.Text =
                $"{sectionName} düzeltmesi mevcut arşiv v{updatedArchive.Revision:00} üzerinde kaydedildi.";

            ClearArchiveCorrectionState();
            UpdateArchiveView();
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));

            MessageBox.Show(
                $"{sectionName} başarıyla güncellendi.\n\n" +
                $"Mevcut arşiv v{updatedArchive.Revision:00} aynı revision numarasıyla güncellendi.\n" +
                "Yeni arşiv/revision oluşturulmadı.\n\n" +
                $"Dosya: {Path.GetFileName(finalPath)}",
                "OEHS — Arşiv Güncellendi",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ProgressPaymentArchiveException)
        {
            ArchiveStatusTextBlock.Text = "ARŞİV HATASI";
            CalculationStatusTextBlock.Text = ex.Message;
            MessageBox.Show(
                $"Arşiv güncellenemedi.\n\n{ex.Message}",
                "OEHS — Arşiv Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void SaveArchivedContractorCorrection()
    {
        var archive = _editingArchiveSnapshot!;
        var name = ContractorNameTextBox.Text.Trim();
        var tax = OehsIdentityService.NormalizeIdentity(ContractorTaxTextBox.Text);

        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(tax))
        {
            ContractorStatusTextBlock.Text = "Firma adı ve TC / Vergi No zorunludur.";
            return;
        }

        var updatedContractor = new Contractor(
            name,
            tax,
            ContractorAddressTextBox.Text.Trim(),
            ContractorPhoneTextBox.Text.Trim(),
            ContractorEmailTextBox.Text.Trim(),
            ContractorBankTextBox.Text.Trim(),
            ContractorContactTextBox.Text.Trim());

        var updatedContract = archive.Contract with
        {
            ContractorName = name,
            ContractorTaxOrIdentityNumber = tax,
            BankAccountNumber = updatedContractor.BankAccountNumber
        };

        var payment = archive.ProgressPayment with
        {
            ContractorName = name,
            ContractorTaxOrIdentityNumber = tax,
            BankAccountNumber = updatedContractor.BankAccountNumber
        };

        var route = archive.Route with { ContractorTaxOrIdentityNumber = tax };

        SaveArchiveCorrectionSnapshot(
            archive with
            {
                Contract = updatedContract,
                Contractor = updatedContractor,
                Route = route,
                ProgressPayment = payment,
                PdfFileName = BuildArchivePdfFileName(payment)
            },
            "Yüklenici firma");
    }

    private void SaveArchivedContractCorrection()
    {
        var archive = _editingArchiveSnapshot!;
        ClearContractValidationVisuals();

        var errors = new List<Control>();
        if (string.IsNullOrWhiteSpace(TenderNumberTextBox.Text)) errors.Add(TenderNumberTextBox);
        if (string.IsNullOrWhiteSpace(ContractNumberTextBox.Text)) errors.Add(ContractNumberTextBox);
        if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text)) errors.Add(WorkNameTextBox);
        if (!TryParseDecimal(ContractAmountTextBox.Text, out var contractAmount) || contractAmount < 0) errors.Add(ContractAmountTextBox);
        if (!int.TryParse(ContractWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var contractWorkDays) || contractWorkDays < 0) errors.Add(ContractWorkDaysTextBox);
        if (!TryParseDecimal(ContractTransportDailyPriceTextBox.Text, out var transportDailyPrice) || transportDailyPrice < 0) errors.Add(ContractTransportDailyPriceTextBox);

        var startDate = StartDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(StartDatePicker.SelectedDate.Value)
            : archive.Contract.StartDate;
        var endDate = EndDatePicker.SelectedDate.HasValue
            ? DateOnly.FromDateTime(EndDatePicker.SelectedDate.Value)
            : archive.Contract.EndDate;

        if (endDate < startDate)
        {
            errors.Add(StartDatePicker);
            errors.Add(EndDatePicker);
        }

        if (archive.ProgressPayment.PeriodDate is DateOnly paymentPeriod &&
            (paymentPeriod < startDate || paymentPeriod > endDate))
        {
            errors.Add(StartDatePicker);
            errors.Add(EndDatePicker);
        }

        if (errors.Count > 0)
        {
            foreach (var control in errors) MarkInvalid(control);
            ContractStatusTextBlock.Text = "Arşiv düzeltmesi kaydedilmedi. Kırmızı alanları kontrol edin.";
            return;
        }

        var updatedContract = archive.Contract with
        {
            TenderNumber = TenderNumberTextBox.Text.Trim(),
            ContractNumber = ContractNumberTextBox.Text.Trim(),
            WorkName = WorkNameTextBox.Text.Trim(),
            ContractAmount = contractAmount,
            StartDate = startDate,
            EndDate = endDate,
            VatExempt = VatExemptCheckBox.IsChecked == true,
            TenderDate = TenderDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(TenderDatePicker.SelectedDate.Value) : archive.Contract.TenderDate,
            ContractDate = ContractDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(ContractDatePicker.SelectedDate.Value) : archive.Contract.ContractDate,
            ContractWorkDays = contractWorkDays,
            TransportDailyPrice = transportDailyPrice
        };

        var input = archive.ProgressPayment.Input with
        {
            ContractAmount = updatedContract.ContractAmount,
            IsVatExempt = updatedContract.VatExempt
        };

        var payment = archive.ProgressPayment with
        {
            Input = input,
            Calculation = ProgressPaymentCalculator.Calculate(input),
            WorkName = updatedContract.WorkName,
            TenderNumber = updatedContract.TenderNumber,
            ContractNumber = updatedContract.ContractNumber,
            TenderDate = updatedContract.TenderDate,
            ContractDate = updatedContract.ContractDate,
            ContractStartDate = updatedContract.StartDate,
            ContractEndDate = updatedContract.EndDate
        };

        var route = archive.Route with
        {
            TenderNumber = updatedContract.TenderNumber,
            DailyPrice = updatedContract.TransportDailyPrice
        };

        SaveArchiveCorrectionSnapshot(
            archive with
            {
                Contract = updatedContract,
                Route = route,
                ProgressPayment = payment,
                PdfFileName = BuildArchivePdfFileName(payment)
            },
            "İhale bilgileri");
    }

    private void SaveArchivedRouteCorrection()
    {
        var archive = _editingArchiveSnapshot!;
        ClearRouteValidationVisuals();

        var routeNumber = RouteNumberTextBox.Text.Trim();
        var routeName = RouteNameTextBox.Text.Trim();

        var errors = new List<Control>();
        if (string.IsNullOrWhiteSpace(routeNumber)) errors.Add(RouteNumberTextBox);
        if (string.IsNullOrWhiteSpace(routeName)) errors.Add(RouteNameTextBox);
        if (!int.TryParse(RouteTransportWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var transportWorkDays) || transportWorkDays < 0) errors.Add(RouteTransportWorkDaysTextBox);
        if (!int.TryParse(RouteGuideWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var guideWorkDays) || guideWorkDays < 0) errors.Add(RouteGuideWorkDaysTextBox);
        if (!TryParseDecimal(RouteGuideDailyPriceTextBox.Text, out var guideDailyPrice) || guideDailyPrice < 0) errors.Add(RouteGuideDailyPriceTextBox);

        if (errors.Count > 0)
        {
            foreach (var control in errors) MarkInvalid(control);
            RouteStatusTextBlock.Text = "Arşiv düzeltmesi kaydedilmedi. Kırmızı alanları kontrol edin.";
            return;
        }

        var updatedRoute = archive.Route with
        {
            RouteNumber = routeNumber,
            Name = routeName,
            DailyPrice = archive.Contract.TransportDailyPrice,
            TransportWorkDays = transportWorkDays,
            GuideWorkDays = guideWorkDays,
            GuideDailyPrice = guideDailyPrice
        };

        var input = archive.ProgressPayment.Input with
        {
            WorkDays = transportWorkDays,
            DailyPrice = archive.Contract.TransportDailyPrice,
            GuideWorkDays = guideWorkDays,
            GuideDailyPrice = guideDailyPrice
        };

        var payment = archive.ProgressPayment with
        {
            Input = input,
            Calculation = ProgressPaymentCalculator.Calculate(input),
            RouteNumber = routeNumber,
            RouteName = routeName
        };

        SaveArchiveCorrectionSnapshot(
            archive with
            {
                Route = updatedRoute,
                ProgressPayment = payment,
                PdfFileName = BuildArchivePdfFileName(payment)
            },
            "Hat bilgileri");
    }

    private void SaveContractorButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsArchiveCorrectionFor(ArchiveCorrectionSection.Contractor))
        {
            SaveArchivedContractorCorrection();
            return;
        }

        var name = ContractorNameTextBox.Text.Trim();
        var tax = OehsIdentityService.NormalizeIdentity(ContractorTaxTextBox.Text);
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(tax))
        {
            ContractorStatusTextBlock.Text = "Firma adı ve TC / Vergi No zorunludur.";
            return;
        }

        var normalizedEditingKey = OehsIdentityService.NormalizeIdentity(_editingContractorKey ?? string.Empty);
        var duplicate = _contractors.FirstOrDefault(x =>
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                tax,
                StringComparison.Ordinal) &&
            !string.Equals(
                OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                normalizedEditingKey,
                StringComparison.Ordinal));

        if (duplicate is not null)
        {
            ShowAlreadyRegistered($"'{name}' zaten kayıtlı. Mükerrer firma kaydı oluşturulmadı.");
            return;
        }

        var updated = new ContractorDraft(
            name,
            tax,
            ContractorAddressTextBox.Text.Trim(),
            ContractorPhoneTextBox.Text.Trim(),
            ContractorEmailTextBox.Text.Trim(),
            ContractorBankTextBox.Text.Trim(),
            ContractorContactTextBox.Text.Trim());

        if (_editingContractorKey is null)
        {
            _contractors.Add(updated);
        }
        else
        {
            var editingIdentity = OehsIdentityService.NormalizeIdentity(_editingContractorKey ?? string.Empty);
            var existing = _contractors.FirstOrDefault(x =>
                string.Equals(
                    OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                    editingIdentity,
                    StringComparison.Ordinal));

            if (existing is null)
                _contractors.Add(updated);
            else
            {
                var index = _contractors.IndexOf(existing);
                try
                {
                    UpdateReferencesForContractorEdit(existing, updated);
                    _contractors[index] = updated;
                }
                catch (ProgressPaymentPropagationException ex)
                {
                    ContractorStatusTextBlock.Text = $"Firma kaydedilmedi: {ex.Message}";
                    return;
                }
            }
        }

        _editingContractorKey = null;
        UpdateContractorSummary();
        SavePersistedData();
        UpdateContractSelectors();
        UpdateHomeSummaryView();
        ClearContractorButton_Click(sender, e);
        var contractorArchiveCorrection = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath);
        ContractorStatusTextBlock.Text = contractorArchiveCorrection
            ? $"Firma kaydedildi: {name}. Canlı hakediş ilişkileri güncellendi; önceki arşiv revision'ı korunuyor. İlgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturun."
            : $"Firma kaydedildi: {name}.";

        if (contractorArchiveCorrection)
        {
            MessageBox.Show(
                $"'{name}' başarıyla güncellendi.\n\nÖnceki arşiv revision'ı değiştirilmedi ve korunuyor. İlgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturabilirsiniz.",
                "OEHS — Arşiv Düzeltme Bilgisi",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void EditContractorButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        if (sender is not Button { Tag: ContractorDraft contractor })
            return;

        _editingContractorKey = contractor.TaxOrIdentityNumber;
        SetContractorEditorMode(true);
        ContractorNameTextBox.Text = contractor.Name;
        ContractorTaxTextBox.Text = contractor.TaxOrIdentityNumber;
        ContractorAddressTextBox.Text = contractor.Address;
        ContractorPhoneTextBox.Text = contractor.Phone;
        ContractorEmailTextBox.Text = contractor.Email;
        ContractorBankTextBox.Text = contractor.BankAccountNumber;
        ContractorContactTextBox.Text = contractor.ContactPerson;
        ContractorStatusTextBlock.Text = $"Düzenleme modu: {contractor.Name}. DÜZENLE ile kaydedin.";
    }

    private void CancelContractorEditButton_Click(object sender, RoutedEventArgs e)
    {
        var archiveCorrection = IsArchiveCorrectionFor(ArchiveCorrectionSection.Contractor);
        var original = !archiveCorrection && !string.IsNullOrWhiteSpace(_editingContractorKey)
            ? FindContractor(_editingContractorKey)
            : null;

        ClearArchiveCorrectionState();
        _editingContractorKey = null;

        if (archiveCorrection)
        {
            ContractorNameTextBox.Clear();
            ContractorTaxTextBox.Clear();
            ContractorAddressTextBox.Clear();
            ContractorPhoneTextBox.Clear();
            ContractorEmailTextBox.Clear();
            ContractorBankTextBox.Clear();
            ContractorContactTextBox.Clear();
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));
            return;
        }

        if (original is not null)
        {
            ContractorNameTextBox.Text = original.Name;
            ContractorTaxTextBox.Text = original.TaxOrIdentityNumber;
            ContractorAddressTextBox.Text = original.Address;
            ContractorPhoneTextBox.Text = original.Phone;
            ContractorEmailTextBox.Text = original.Email;
            ContractorBankTextBox.Text = original.BankAccountNumber;
            ContractorContactTextBox.Text = original.ContactPerson;
        }

        ContractorStatusTextBlock.Text = "Firma düzenlemesi iptal edildi; kayıt değiştirilmedi.";
    }

    private void DeleteContractorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ContractorDraft contractor })
            return;

        var normalizedContractorIdentity = OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber);
        var usedByContract = _contracts.Any(x =>
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                normalizedContractorIdentity,
                StringComparison.Ordinal));
        var usedByRoute = _routes.Any(x =>
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                normalizedContractorIdentity,
                StringComparison.Ordinal));
        var usedByHistory = _progressHistory.Any(x =>
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                normalizedContractorIdentity,
                StringComparison.Ordinal));

        if (usedByContract || usedByRoute || usedByHistory)
        {
            ShowAlreadyRegistered($"'{contractor.Name}' kayıtlı hat/hakediş kayıtlarında kullanılıyor. Önce ilişkili kayıtları düzeltin; firma silinmedi.");
            return;
        }

        var result = MessageBox.Show(
            $"'{contractor.Name}' firması silinsin mi? Bu işlem geri alınamaz.",
            "Yüklenici Firmayı Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _contractors.Remove(contractor);
        SavePersistedData();
        UpdateContractorSummary();
        UpdateContractSelectors();
        UpdateHomeSummaryView();
        ContractorStatusTextBlock.Text = $"Firma silindi: {contractor.Name}.";
    }

    private void ClearContractorButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        _editingContractorKey = null;
        ContractorNameTextBox.Clear();
        ContractorTaxTextBox.Clear();
        ContractorAddressTextBox.Clear();
        ContractorPhoneTextBox.Clear();
        ContractorEmailTextBox.Clear();
        ContractorBankTextBox.Clear();
        ContractorContactTextBox.Clear();
        ContractorStatusTextBlock.Text = "Yeni firma kaydı için alanlar temizlendi.";
    }

    private void UpdateContractorSummary()
    {
        ContractorCountTextBlock.Text = _contractors.Count.ToString(CultureInfo.CurrentCulture);
    }

    private void UpdateReferencesForContractorEdit(ContractorDraft oldContractor, ContractorDraft updated)
    {
        var oldCore = new Contractor(
            oldContractor.Name,
            oldContractor.TaxOrIdentityNumber,
            oldContractor.Address,
            oldContractor.Phone,
            oldContractor.Email,
            oldContractor.BankAccountNumber,
            oldContractor.ContactPerson);

        var newCore = new Contractor(
            updated.Name,
            updated.TaxOrIdentityNumber,
            updated.Address,
            updated.Phone,
            updated.Email,
            updated.BankAccountNumber,
            updated.ContactPerson);

        // Önce tüm türetilmiş hakedişler doğrulanır; hata halinde hiçbir kaynak
        // koleksiyonu değiştirilmez.
        var propagated = ProgressPaymentPropagationService.RecalculateForContractorChange(
            _progressHistory.ToArray(),
            oldCore,
            newCore);

        var normalizedOld = OehsIdentityService.NormalizeIdentity(oldContractor.TaxOrIdentityNumber);
        var routeUpdates = _routes
            .Select((route, index) => (route, index))
            .Where(x => string.Equals(
                OehsIdentityService.NormalizeIdentity(x.route.ContractorTaxOrIdentityNumber),
                normalizedOld,
                StringComparison.Ordinal))
            .Select(x => (x.index, route: x.route with
            {
                ContractorTaxOrIdentityNumber = newCore.TaxOrIdentityNumber,
                ContractorName = newCore.Name
            }))
            .ToArray();

        foreach (var item in routeUpdates)
            _routes[item.index] = item.route;

        for (var index = 0; index < _contracts.Count; index++)
        {
            var contract = _contracts[index];
            if (string.Equals(
                    OehsIdentityService.NormalizeIdentity(contract.ContractorTaxOrIdentityNumber),
                    normalizedOld,
                    StringComparison.Ordinal))
            {
                _contracts[index] = contract with
                {
                    ContractorName = newCore.Name,
                    ContractorTaxOrIdentityNumber = newCore.TaxOrIdentityNumber,
                    BankAccountNumber = newCore.BankAccountNumber
                };
            }
        }

        _progressHistory.Clear();
        foreach (var item in propagated)
            _progressHistory.Add(item);
    }

    private void HomeTenderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        if (HomeTenderListBox.SelectedItem is ContractDraftData contract)
        {
            ApplyContractToForm(contract);
            UpdateHomeTenderSummary(contract);
            var latest = _progressHistory
                .Where(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase))
                .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
                .ThenByDescending(x => x.CreatedAt)
                .FirstOrDefault();

            HomeWorkNameTextBlock.Text = contract.WorkName;
            HomeContractorTextBlock.Text = "Hat üzerinden seçilir";
            HomeProgressTextBlock.Text = latest?.ProgressPaymentNumber.HasValue == true
                ? $"Hakediş No {latest.ProgressPaymentNumber.Value}"
                : latest is null ? "Henüz hakediş yok" : "Son hesaplama";
            HomePeriodTextBlock.Text = latest?.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";
            HomeNetPayableTextBlock.Text = FormatMoney(_progressHistory
                .Where(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase))
                .Sum(x => x.Calculation.NetPayable));
            UpdateHomeSummaryView();
        }
    }

    private void UpdateHomeSummaryView()
    {
        var contract = HomeTenderListBox.SelectedItem as ContractDraftData
                       ?? _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))
                       ?? _contracts.FirstOrDefault();

        UpdateHomeTenderSummary(contract);

        var tender = contract?.TenderNumber;
        var history = string.IsNullOrWhiteSpace(tender)
            ? _progressHistory.ToList()
            : _progressHistory.Where(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase)).ToList();

        HomeContractStatusTextBlock.Text = contract is null ? "İhale seçilmedi" : "KAYITLI";
        HomeRouteCountTextBlock.Text = contract is null
            ? "0"
            : _routes.Count(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.CurrentCulture);

        HomeWorkNameTextBlock.Text = contract?.WorkName ?? "—";
        HomeContractorTextBlock.Text = contract is null ? "—" : "Hat üzerinden seçilir";
        HomeNetPayableTextBlock.Text = FormatMoney(history.Sum(x => x.Calculation.NetPayable));

        var latest = history
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();

        HomeProgressTextBlock.Text = latest?.ProgressPaymentNumber.HasValue == true
            ? $"Hakediş No {latest.ProgressPaymentNumber.Value}"
            : latest is null ? "Henüz hakediş yok" : "Son hesaplama";
        HomePeriodTextBlock.Text = latest?.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";
        HomeGrossTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.GrossAmount);
        HomeDeductionsTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.TotalDeductions);
        HomeLatestNetTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.NetPayable);
        HomeVatWithholdingTextBlock.Text = latest is null
            ? "0,00 / 0,00 ₺"
            : $"{FormatMoney(latest.Calculation.Vat)} / {FormatMoney(latest.Calculation.Withholding)}";

        HomeTransportServiceTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.ServiceAmount);
        HomeTransportPriceDifferenceTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.PriceDifferenceExcludingVat);
        HomeTransportNetTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.TransportNetAmount);
        HomeGuideServiceTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.GuideServiceAmount);
        HomeGuideNetTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.GuideNetAmount);
        HomeTotalAmountTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.TotalAmount);
        HomeVatTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.Vat);
        HomeWithholdingTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.Withholding);
        HomeInvoiceTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.InvoiceAmountAfterWithholding);
        HomeGuaranteeTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.AdditionalGuarantee);
        HomeStampTaxTextBlock.Text = latest is null ? FormatMoney(0) : FormatMoney(latest.Calculation.StampTax);
    }

    private void UpdateContractSummaryView()
    {
        var contract = _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
        var hasContract = contract is not null || !string.IsNullOrWhiteSpace(WorkNameTextBox.Text);
        ContractStatusSummaryTextBlock.Text = hasContract ? "KAYITLI" : "TASLAK";
        ContractAmountSummaryTextBlock.Text =
            TryParseDecimal(ContractAmountTextBox.Text, out var amount) ? FormatMoney(amount) : FormatMoney(0);
        ContractStartSummaryTextBlock.Text =
            StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";
        ContractEndSummaryTextBlock.Text =
            EndDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "—";

        ContractSummaryTenderTextBlock.Text = string.IsNullOrWhiteSpace(TenderNumberTextBox.Text)
            ? "Bekleniyor" : TenderNumberTextBox.Text.Trim();

        ContractSummaryContractTextBlock.Text = string.IsNullOrWhiteSpace(ContractNumberTextBox.Text)
            ? "Bekleniyor"
            : ContractNumberTextBox.Text.Trim() +
              (ContractDatePicker.SelectedDate.HasValue ? $" • {ContractDatePicker.SelectedDate.Value:dd.MM.yyyy}" : "");

        ContractSummaryAmountTextBlock.Text = TryParseDecimal(ContractAmountTextBox.Text, out var summaryAmount)
            ? FormatMoney(summaryAmount) : FormatMoney(0);

        var workDaysText = int.TryParse(ContractWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays)
            ? workDays.ToString(CultureInfo.CurrentCulture) : "0";
        var transportPriceText = TryParseDecimal(ContractTransportDailyPriceTextBox.Text, out var transportPrice)
            ? FormatMoney(transportPrice) : FormatMoney(0);

        ContractSummaryWorkDaysTextBlock.Text = $"{workDaysText} gün • {transportPriceText}/gün";
        ContractSummaryVatTextBlock.Text =
            $"{(VatExemptCheckBox.IsChecked == true ? "KDV İstisnası" : "KDV Uygulanır")} • {_routes.Count(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))} hat";
    }

    private void HomeTenderListBox_RefreshSelection()
    {
        if (HomeTenderListBox.SelectedItem is null && _contracts.Count > 0)
            HomeTenderListBox.SelectedIndex = 0;
    }

    private void RouteTenderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        _suppressSelectionEvents = true;
        try
        {
            UpdateRouteContractorSelector(contract);
        }
        finally
        {
            _suppressSelectionEvents = false;
        }
    }

    private void RouteContractorListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        UpdateRouteListView();
        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        var contractor = RouteContractorListBox.SelectedItem as ContractorDraft;
        RouteSelectedTenderTextBlock.Text = contract is null ? "İhale: seçilmedi" : $"İhale: {contract.TenderNumber}";
        RouteSelectedContractorTextBlock.Text = contractor is null ? "Yüklenici: seçilmedi" : $"Yüklenici: {contractor.Name}";
    }

    private void ProgressTenderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        if (ProgressTenderListBox.SelectedItem is ContractDraftData contract)
        {
            ApplyContractToForm(contract);
            UpdateProgressContractorAndRoutes(contract);
        }
    }

    private void AddRouteButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsArchiveCorrectionFor(ArchiveCorrectionSection.Route))
        {
            SaveArchivedRouteCorrection();
            return;
        }

        ClearRouteValidationVisuals();

        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        var contractor = RouteContractorListBox.SelectedItem as ContractorDraft;

        var routeNumber = RouteNumberTextBox.Text.Trim();
        var routeName = RouteNameTextBox.Text.Trim();
        var errors = new List<string>();

        if (contract is null)
            errors.Add("İhale / Dosya No seçimi");
        if (contractor is null)
            errors.Add("Yüklenici firma seçimi");

        if (string.IsNullOrWhiteSpace(routeNumber))
        {
            errors.Add("Hat No");
            MarkInvalid(RouteNumberTextBox);
        }
        if (string.IsNullOrWhiteSpace(routeName))
        {
            errors.Add("Hat Adı");
            MarkInvalid(RouteNameTextBox);
        }
        if (!int.TryParse(RouteTransportWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var transportWorkDays) ||
            transportWorkDays < 0)
        {
            errors.Add("Taşıma İş Günü");
            MarkInvalid(RouteTransportWorkDaysTextBox);
        }
        if (!int.TryParse(RouteGuideWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var guideWorkDays) ||
            guideWorkDays < 0)
        {
            errors.Add("Rehber Yıllık İş Günü");
            MarkInvalid(RouteGuideWorkDaysTextBox);
        }
        if (!TryParseDecimal(RouteGuideDailyPriceTextBox.Text, out var guideDailyPrice) || guideDailyPrice < 0)
        {
            errors.Add("Rehber Personel Günlük Fiyat");
            MarkInvalid(RouteGuideDailyPriceTextBox);
        }

        if (errors.Count > 0)
        {
            ShowRouteValidation("Lütfen kırmızı işaretli alanları tamamlayın: " + string.Join(", ", errors) + ".");
            return;
        }

        var proposed = new RouteDraft(
            routeNumber,
            routeName,
            contract!.TransportDailyPrice,
            guideWorkDays,
            guideDailyPrice,
            transportWorkDays,
            contract.TenderNumber,
            OehsIdentityService.NormalizeIdentity(contractor!.TaxOrIdentityNumber),
            contractor.Name);

        var duplicate = _routes.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, proposed.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                OehsIdentityService.NormalizeIdentity(proposed.ContractorTaxOrIdentityNumber),
                StringComparison.Ordinal) &&
            string.Equals(x.RouteNumber, proposed.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
            !ReferenceEquals(x, _editingRoute));

        if (duplicate is not null)
        {
            ShowAlreadyRegistered($"Bu hat zaten kayıtlı: {proposed.TenderNumber} / {proposed.RouteNumber}.");
            return;
        }

        if (_editingRoute is null)
        {
            _routes.Add(proposed);
        }
        else
        {
            var old = _editingRoute;
            var index = _routes.IndexOf(old);
            if (index >= 0)
            {
                try
                {
                    var propagated = BuildRoutePropagation(old, proposed);
                    _routes[index] = proposed;
                    _progressHistory.Clear();
                    foreach (var item in propagated)
                        _progressHistory.Add(item);
                }
                catch (ProgressPaymentPropagationException ex)
                {
                    RouteStatusTextBlock.Text = $"Hat kaydedilmedi: {ex.Message}";
                    return;
                }
            }
            _editingRoute = null;
        }

        SavePersistedData();
        UpdateRouteSummary();
        UpdateRouteListView();
        UpdateProgressContractorAndRoutes(contract);
        UpdateOutputRoutesForContract(contract);
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        ClearRouteButton_Click(sender, e);
        var routeArchiveCorrection = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath);
        RouteStatusTextBlock.Text = routeArchiveCorrection
            ? $"Hat kaydedildi: {routeNumber} — {routeName}. İlişkili canlı hakedişler güncellendi; önceki arşiv revision'ı korunuyor. İlgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturun."
            : $"Hat kaydedildi: {routeNumber} — {routeName}.";

        if (routeArchiveCorrection)
        {
            MessageBox.Show(
                $"Arşivdeki Hat {routeNumber} — {routeName} başarıyla güncellendi.\n\nYeni bir hat kaydı oluşturulmadı. Aynı hat ilişkisi korunuyor.\nÖnceki arşiv revision'ı değiştirilmedi; ilgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturabilirsiniz.",
                "OEHS — Arşiv Hat Düzeltme",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private IReadOnlyList<ProgressPaymentHistoryData> BuildRoutePropagation(RouteDraft oldRoute, RouteDraft updatedRoute)
    {
        var oldData = new RouteData(
            oldRoute.RouteNumber,
            oldRoute.Name,
            oldRoute.DailyPrice,
            oldRoute.GuideWorkDays,
            oldRoute.GuideDailyPrice,
            oldRoute.TransportWorkDays,
            oldRoute.TenderNumber,
            oldRoute.ContractorTaxOrIdentityNumber);

        var newData = new RouteData(
            updatedRoute.RouteNumber,
            updatedRoute.Name,
            updatedRoute.DailyPrice,
            updatedRoute.GuideWorkDays,
            updatedRoute.GuideDailyPrice,
            updatedRoute.TransportWorkDays,
            updatedRoute.TenderNumber,
            updatedRoute.ContractorTaxOrIdentityNumber);

        var contractor = FindContractor(updatedRoute.ContractorTaxOrIdentityNumber);
        var contractorSnapshot = contractor is null
            ? new Contractor(updatedRoute.ContractorName, updatedRoute.ContractorTaxOrIdentityNumber)
            : new Contractor(
                contractor.Name,
                contractor.TaxOrIdentityNumber,
                contractor.Address,
                contractor.Phone,
                contractor.Email,
                contractor.BankAccountNumber,
                contractor.ContactPerson);

        return ProgressPaymentPropagationService.RecalculateForRouteChange(
            _progressHistory.ToArray(),
            oldData,
            newData,
            contractorSnapshot);
    }

    private void ShowRouteValidation(string message)
    {
        RouteValidationMessageTextBlock.Text = message;
        RouteValidationBanner.Visibility = Visibility.Visible;
        RouteStatusTextBlock.Text = "Hat kaydedilmedi. Kırmızı işaretli alanları kontrol edin.";
        RouteStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
    }

    private void ClearRouteValidationVisuals()
    {
        foreach (var control in new Control[]
        {
            RouteNumberTextBox,
            RouteNameTextBox,
            RouteTransportWorkDaysTextBox,
            RouteGuideWorkDaysTextBox,
            RouteGuideDailyPriceTextBox
        })
            ClearInvalid(control);

        RouteValidationBanner.Visibility = Visibility.Collapsed;
        RouteValidationMessageTextBlock.Text = string.Empty;
    }

    private void RemoveRouteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { Tag: RouteDraft route })
            return;

        var historyCount = _progressHistory.Count(x =>
            string.Equals(x.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal));

        if (historyCount > 0)
        {
            ShowAlreadyRegistered($"Bu hat için {historyCount} hakediş kaydı bulunduğu için hat silinmedi.");
            return;
        }

        var result = MessageBox.Show(
            $"'{route.RouteNumber} — {route.Name}' hattı silinsin mi? Bu işlem geri alınamaz.",
            "Hat Kaydını Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _routes.Remove(route);
        SavePersistedData();
        UpdateRouteSummary();
        UpdateRouteListView();
        UpdateHomeSummaryView();
        RouteStatusTextBlock.Text = $"Hat silindi: {route.RouteNumber}.";
    }

    private void EditRouteButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        if (sender is not System.Windows.Controls.Button { Tag: RouteDraft route })
            return;

        _editingRoute = route;
        SetRouteEditorMode(true);

        var contract = _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase));
        var contractor = FindContractor(route.ContractorTaxOrIdentityNumber);

        _suppressSelectionEvents = true;
        try
        {
            RouteTenderListBox.SelectedItem = contract;
            RouteContractorListBox.ItemsSource = contractor is null ? Array.Empty<ContractorDraft>() : new[] { contractor };
            RouteContractorListBox.SelectedItem = contractor;
        }
        finally
        {
            _suppressSelectionEvents = false;
        }

        RouteNumberTextBox.Text = route.RouteNumber;
        RouteNameTextBox.Text = route.Name;
        RouteTransportWorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
        RouteGuideWorkDaysTextBox.Text = route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
        RouteGuideDailyPriceTextBox.Text = route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        RouteStatusTextBlock.Text = $"Düzenleme modu: {route.TenderNumber} / {route.RouteNumber}. DÜZENLE ile kaydedin.";
    }

    private void CancelRouteEditButton_Click(object sender, RoutedEventArgs e)
    {
        var archiveCorrection = IsArchiveCorrectionFor(ArchiveCorrectionSection.Route);
        var original = !archiveCorrection ? _editingRoute : null;

        ClearArchiveCorrectionState();

        if (archiveCorrection)
        {
            _editingRoute = null;
            RouteNumberTextBox.Clear();
            RouteNameTextBox.Clear();
            RouteTransportWorkDaysTextBox.Text = "0";
            RouteGuideWorkDaysTextBox.Text = "0";
            RouteGuideDailyPriceTextBox.Text = "0";
            ClearRouteValidationVisuals();
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));
            return;
        }

        _editingRoute = null;
        if (original is not null)
        {
            RouteTenderListBox.SelectedItem = _contracts.FirstOrDefault(x =>
                string.Equals(x.TenderNumber, original.TenderNumber, StringComparison.OrdinalIgnoreCase));
            var contractor = FindContractor(original.ContractorTaxOrIdentityNumber);
            RouteContractorListBox.ItemsSource = contractor is null ? Array.Empty<ContractorDraft>() : new[] { contractor };
            RouteContractorListBox.SelectedItem = contractor;
            RouteNumberTextBox.Text = original.RouteNumber;
            RouteNameTextBox.Text = original.Name;
            RouteTransportWorkDaysTextBox.Text = original.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
            RouteGuideWorkDaysTextBox.Text = original.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
            RouteGuideDailyPriceTextBox.Text = original.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        }
        else
        {
            RouteNumberTextBox.Clear();
            RouteNameTextBox.Clear();
            RouteTransportWorkDaysTextBox.Text = "0";
            RouteGuideWorkDaysTextBox.Text = "0";
            RouteGuideDailyPriceTextBox.Text = "0";
        }

        SetRouteEditorMode(false);
        ClearRouteValidationVisuals();
        RouteStatusTextBlock.Text = "Hat düzenlemesi iptal edildi; kayıt değiştirilmedi.";
    }

    private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        _editingRoute = null;
        RouteNumberTextBox.IsReadOnly = false;
        RouteNumberTextBox.Clear();
        RouteNameTextBox.Clear();
        RouteTransportWorkDaysTextBox.Text = "0";
        RouteGuideWorkDaysTextBox.Text = "0";
        RouteGuideDailyPriceTextBox.Text = "0";
        ClearRouteValidationVisuals();
    }

    private void UpdateRouteSummary()
    {
        RouteCountTextBlock.Text = _routes.Count.ToString(CultureInfo.CurrentCulture);
    }

    private void EditContractButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        if (sender is not Button { Tag: ContractDraftData contract })
            return;

        _editingContractTender = contract.TenderNumber;
        ApplyContractToForm(contract);
        SetContractEditorMode(true);
        NavigationButton_Click(NavContract, new RoutedEventArgs(Button.ClickEvent, NavContract));
        ContractStatusTextBlock.Text = $"Düzenleme modu: {contract.TenderNumber}. Değiştirip DÜZENLE'e basın.";
    }

    private void CancelContractEditButton_Click(object sender, RoutedEventArgs e)
    {
        var archiveCorrection = IsArchiveCorrectionFor(ArchiveCorrectionSection.Contract);
        var original = !archiveCorrection && !string.IsNullOrWhiteSpace(_editingContractTender)
            ? _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, _editingContractTender, StringComparison.OrdinalIgnoreCase))
            : null;

        ClearArchiveCorrectionState();
        _editingContractTender = null;

        if (archiveCorrection)
        {
            ClearContractFormOnly();
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));
            return;
        }

        if (original is not null) ApplyContractToForm(original);
        else ClearContractFormOnly();
        ContractStatusTextBlock.Text = "Sözleşme düzenlemesi iptal edildi; kayıt değiştirilmedi.";
    }

    private void DeleteContractButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ContractDraftData contract })
            return;

        var routeCount = _routes.Count(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase));
        var historyCount = _progressHistory.Count(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase));

        if (routeCount > 0 || historyCount > 0)
        {
            MessageBox.Show(
                $"'{contract.TenderNumber}' ihalesi {routeCount} hat ve {historyCount} hakediş kaydıyla ilişkili. Önce ilişkili kayıtları düzeltin; ihale silinmedi.",
                "OEHS",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }

        var result = MessageBox.Show(
            $"'{contract.TenderNumber}' kaydı silinsin mi?",
            "İhale Kaydını Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);
        if (result != MessageBoxResult.Yes)
            return;

        _contracts.Remove(contract);
        SavePersistedData();
        UpdateContractSelectors();
        UpdateHomeSummaryView();
        UpdateContractSummaryView();
    }

    private void SaveContractButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsArchiveCorrectionFor(ArchiveCorrectionSection.Contract))
        {
            SaveArchivedContractCorrection();
            return;
        }

        ClearContractValidationVisuals();

        var errors = new List<string>();
        if (string.IsNullOrWhiteSpace(TenderNumberTextBox.Text))
        {
            errors.Add("İhale / Dosya No");
            MarkInvalid(TenderNumberTextBox);
        }
        if (string.IsNullOrWhiteSpace(ContractNumberTextBox.Text))
        {
            errors.Add("Sözleşme No");
            MarkInvalid(ContractNumberTextBox);
        }
        if (string.IsNullOrWhiteSpace(WorkNameTextBox.Text))
        {
            errors.Add("İşin Adı");
            MarkInvalid(WorkNameTextBox);
        }
        if (!TryParseDecimal(ContractAmountTextBox.Text, out var contractAmount) || contractAmount < 0)
        {
            errors.Add("Sözleşme Bedeli");
            MarkInvalid(ContractAmountTextBox);
        }
        if (!int.TryParse(ContractWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var contractWorkDays) || contractWorkDays < 0)
        {
            errors.Add("Sözleşme İş Günü");
            MarkInvalid(ContractWorkDaysTextBox);
        }
        if (!TryParseDecimal(ContractTransportDailyPriceTextBox.Text, out var transportDailyPrice) || transportDailyPrice < 0)
        {
            errors.Add("Taşıma Günlük Fiyatı");
            MarkInvalid(ContractTransportDailyPriceTextBox);
        }
        if (StartDatePicker.SelectedDate.HasValue && EndDatePicker.SelectedDate.HasValue &&
            EndDatePicker.SelectedDate.Value.Date < StartDatePicker.SelectedDate.Value.Date)
        {
            errors.Add("Başlangıç / Bitiş Tarihi");
            MarkInvalid(StartDatePicker);
            MarkInvalid(EndDatePicker);
        }

        if (errors.Count > 0)
        {
            ShowContractValidation("Lütfen şu alanları kontrol edin: " + string.Join(", ", errors) + ".");
            return;
        }

        var sourceTenderForEdit = _editingContractTender ?? TenderNumberTextBox.Text.Trim();
        var existingContractForLegacy = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, sourceTenderForEdit, StringComparison.OrdinalIgnoreCase));
        var proposedContract = new ContractDraftData(
            TenderNumberTextBox.Text.Trim(),
            ContractNumberTextBox.Text.Trim(),
            WorkNameTextBox.Text.Trim(),
            existingContractForLegacy?.ContractorName ?? string.Empty,
            contractAmount,
            DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
            DateOnly.FromDateTime(EndDatePicker.SelectedDate ?? DateTime.Today),
            VatExemptCheckBox.IsChecked == true,
            TenderDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(TenderDatePicker.SelectedDate.Value) : null,
            ContractDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(ContractDatePicker.SelectedDate.Value) : null,
            existingContractForLegacy?.ContractorTaxOrIdentityNumber ?? string.Empty,
            existingContractForLegacy?.BankAccountNumber ?? string.Empty,
            contractWorkDays,
            transportDailyPrice);

        var existingByTender = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, proposedContract.TenderNumber, StringComparison.OrdinalIgnoreCase));

        if (_editingContractTender is null)
        {
            if (existingByTender is not null)
            {
                ShowAlreadyRegistered($"İhale / Dosya No {proposedContract.TenderNumber} zaten kayıtlı. Mükerrer kayıt oluşturulmadı.");
                return;
            }

            _contracts.Add(proposedContract);
        }
        else
        {
            if (existingByTender is not null &&
                !string.Equals(existingByTender.TenderNumber, _editingContractTender, StringComparison.OrdinalIgnoreCase))
            {
                ShowAlreadyRegistered($"İhale / Dosya No {proposedContract.TenderNumber} zaten kayıtlı. Mükerrer kayıt oluşturulmadı.");
                return;
            }

            var oldTender = _editingContractTender;
            var oldContract = _contracts.FirstOrDefault(x =>
                string.Equals(x.TenderNumber, oldTender, StringComparison.OrdinalIgnoreCase));

            if (oldContract is null)
            {
                _contracts.Add(proposedContract);
            }
            else
            {
                var index = _contracts.IndexOf(oldContract);

                try
                {
                    var proposedHistories = ProgressPaymentPropagationService.RecalculateForContractChange(
                        _progressHistory.ToArray(),
                        _routes.Select(x => new RouteData(
                            x.RouteNumber,
                            x.Name,
                            x.DailyPrice,
                            x.GuideWorkDays,
                            x.GuideDailyPrice,
                            x.TransportWorkDays,
                            x.TenderNumber,
                            x.ContractorTaxOrIdentityNumber)).ToArray(),
                        oldContract,
                        proposedContract);

                    var routeData = _routes.Select(x => new RouteData(
                        x.RouteNumber,
                        x.Name,
                        x.DailyPrice,
                        x.GuideWorkDays,
                        x.GuideDailyPrice,
                        x.TransportWorkDays,
                        x.TenderNumber,
                        x.ContractorTaxOrIdentityNumber)).ToArray();

                    var propagatedRoutes = TenderRoutePropagationService.UpdateTenderNumber(
                        routeData,
                        oldTender,
                        proposedContract.TenderNumber);

                    var affectedRouteIndices = _routes
                        .Select((route, routeIndex) => (route, routeIndex))
                        .Where(x => string.Equals(x.route.TenderNumber, oldTender, StringComparison.OrdinalIgnoreCase))
                        .ToArray();

                    _contracts[index] = proposedContract;

                    for (var routeIndex = 0; routeIndex < affectedRouteIndices.Length; routeIndex++)
                    {
                        var updatedRoute = propagatedRoutes[affectedRouteIndices[routeIndex].routeIndex];
                        var oldRoute = affectedRouteIndices[routeIndex].route;
                        _routes[affectedRouteIndices[routeIndex].routeIndex] = oldRoute with
                        {
                            TenderNumber = updatedRoute.TenderNumber
                        };
                    }

                    _progressHistory.Clear();
                    foreach (var item in proposedHistories)
                        _progressHistory.Add(item);
                }
                catch (ProgressPaymentPropagationException ex)
                {
                    ContractStatusTextBlock.Text = $"İhale kaydedilmedi: {ex.Message}";
                    return;
                }
            }
        }

        _editingContractTender = null;
        ApplyContractToForm(proposedContract);
        SavePersistedData();
        UpdateContractSelectors();
        UpdateRouteListView();
        UpdateRouteSummary();
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        UpdateAcceptanceView();

        ContractStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 104, 216));
        var contractArchiveCorrection = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath);
        ContractStatusTextBlock.Text = contractArchiveCorrection
            ? $"İhale kaydedildi: {proposedContract.TenderNumber}. İlişkili canlı hakedişler güncellendi; önceki arşiv revision'ı korunuyor. İlgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturun."
            : $"İhale kaydedildi: {proposedContract.TenderNumber}.";

        if (contractArchiveCorrection)
        {
            MessageBox.Show(
                $"İhale/Dosya {proposedContract.TenderNumber} başarıyla güncellendi.\n\nÖnceki arşiv revision'ı değiştirilmedi ve korunuyor. İlgili hakedişi yeniden hesaplayıp ARŞİVLE ile yeni revision oluşturabilirsiniz.",
                "OEHS — Arşiv Düzeltme Bilgisi",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
    }

    private void LoadPersistedData()
    {
        try
        {
            var data = _dataStore.Load();

            _contracts.Clear();
            var persistedContracts = data.Contracts ?? Array.Empty<ContractDraftData>();
            if (persistedContracts.Count > 0)
            {
                foreach (var contract in persistedContracts)
                    _contracts.Add(contract);
            }
            else if (data.Contract is not null)
            {
                _contracts.Add(data.Contract);
            }

            _contractors.Clear();
            foreach (var contractor in data.Contractors ?? Array.Empty<Contractor>())
            {
                _contractors.Add(new ContractorDraft(
                    contractor.Name,
                    OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber),
                    contractor.Address,
                    contractor.Phone,
                    contractor.Email,
                    contractor.BankAccountNumber,
                    contractor.ContactPerson));
            }

            _routes.Clear();
            foreach (var route in data.Routes ?? Array.Empty<RouteData>())
            {
                var tender = route.TenderNumber;
                if (string.IsNullOrWhiteSpace(tender))
                    tender = _contracts.FirstOrDefault()?.TenderNumber ?? string.Empty;

                var contractorTax = OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber);
                if (string.IsNullOrWhiteSpace(contractorTax))
                {
                    contractorTax = _contracts
                        .FirstOrDefault(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase))
                        ?.ContractorTaxOrIdentityNumber ?? string.Empty;
                }

                var contractorName = FindContractor(contractorTax)?.Name
                                     ?? _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase))?.ContractorName
                                     ?? string.Empty;

                _routes.Add(new RouteDraft(
                    route.RouteNumber,
                    route.Name,
                    route.DailyPrice,
                    route.GuideWorkDays,
                    route.GuideDailyPrice,
                    route.TransportWorkDays,
                    tender,
                    contractorTax,
                    contractorName));
            }

            _progressHistory.Clear();
            foreach (var history in data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
            {
                var contract = _contracts.FirstOrDefault(x =>
                    string.Equals(x.TenderNumber, history.TenderNumber, StringComparison.OrdinalIgnoreCase));
                var contractorTax = OehsIdentityService.NormalizeIdentity(
                    history.ContractorTaxOrIdentityNumber ?? contract?.ContractorTaxOrIdentityNumber ?? string.Empty);
                var contractor = FindContractor(contractorTax);

                _progressHistory.Add(history with
                {
                    ContractorTaxOrIdentityNumber = string.IsNullOrWhiteSpace(contractorTax) ? history.ContractorTaxOrIdentityNumber : contractorTax,
                    ContractorName = contractor?.Name ?? history.ContractorName ?? contract?.ContractorName,
                    BankAccountNumber = contractor?.BankAccountNumber ?? history.BankAccountNumber ?? contract?.BankAccountNumber
                });
            }

            if (_contracts.Count > 0)
                ApplyContractToForm(_contracts[0]);
            else
                ClearContractFormOnly();

            _dataLoadFailed = false;
            UpdateContractSelectors();
            UpdateRouteListView();
            UpdateContractSummaryView();
            UpdateHomeSummaryView();
        }
        catch (OehsApplicationDataStoreException ex)
        {
            _dataLoadFailed = true;
            _contracts.Clear();
            _contractors.Clear();
            _routes.Clear();
            _progressHistory.Clear();

            MessageBox.Show(
                $"{ex.Message}\n\nOEHS güvenlik nedeniyle mevcut bozuk/veri dosyasının üzerine yazmayacak.",
                "OEHS — Veri Güvenliği",
                MessageBoxButton.OK,
                MessageBoxImage.Error);

            UpdateContractSelectors();
            UpdateRouteListView();
            UpdateContractSummaryView();
            UpdateHomeSummaryView();
        }
    }

    private void SavePersistedData()
    {
        if (_dataLoadFailed)
        {
            MessageBox.Show(
                "OEHS veri dosyası güvenli olarak yüklenemedi. Veri kaybını önlemek için kaydetme durduruldu.",
                "OEHS — Kaydetme Durduruldu",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
            return;
        }

        var activeContract = GetSelectedContract() ?? _contracts.FirstOrDefault();

        var data = new OehsApplicationData(
            activeContract,
            _routes.Select(x => new RouteData(
                x.RouteNumber, x.Name, x.DailyPrice, x.GuideWorkDays, x.GuideDailyPrice,
                x.TransportWorkDays, x.TenderNumber, x.ContractorTaxOrIdentityNumber)).ToArray(),
            _contractors.Select(x => new Contractor(
                x.Name, x.TaxOrIdentityNumber, x.Address, x.Phone, x.Email,
                x.BankAccountNumber, x.ContactPerson)).ToArray(),
            _progressHistory.ToArray(),
            _contracts.ToArray());

        try
        {
            _dataStore.Save(data);
        }
        catch (OehsApplicationDataStoreException ex)
        {
            MessageBox.Show(
                $"{ex.Message}\n\nMevcut veri dosyası korunmuştur. OEHS değişikliklerinizi kaydetmedi.",
                "OEHS — Veri Kaydetme Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }

    private void ClearContractButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        _editingContractTender = null;
        ClearContractFormOnly();
        UpdateContractSummaryView();
        ContractStatusTextBlock.Text = "Yeni ihale/dosya kaydı için alanlar temizlendi.";
    }

    private static string OehsDesktopRoot => OehsOutputPathService.DesktopRoot;

    private static string SafeFileName(string value)
    {
        foreach (var invalid in Path.GetInvalidFileNameChars())
            value = value.Replace(invalid, '_');

        return string.IsNullOrWhiteSpace(value) ? "OEHS" : value;
    }

    private static readonly JsonSerializerOptions JsonWriteOptions = new() { WriteIndented = true };

    private static string BuildArchiveKey(ProgressPaymentHistoryData item)
    {
        var tender = SafeFileName(string.IsNullOrWhiteSpace(item.TenderNumber) ? "Ihale" : item.TenderNumber);
        var route = SafeFileName(string.IsNullOrWhiteSpace(item.RouteNumber) ? "Hat" : item.RouteNumber);
        var progress = item.ProgressPaymentNumber?.ToString(CultureInfo.InvariantCulture) ?? "Yeni";
        var period = item.PeriodDate?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture) ?? "Donemsiz";
        return $"{tender}_{route}_Hakedis-{progress}_{period}_{item.Id:N}";
    }

    private static string SaveRouteDocumentsArchiveMetadata(ProgressPaymentHistoryData latest)
    {
        var tender = latest.TenderNumber ?? string.Empty;
        var route = latest.RouteNumber ?? string.Empty;
        var documentFolder = OehsOutputPathService.BuildPdfDirectory(tender, route);
        Directory.CreateDirectory(documentFolder);

        var fileName = latest.ProgressPaymentNumber.HasValue && latest.PeriodDate.HasValue
            ? OehsFileNameService.BuildProgressPdfFileName(
                tender,
                route,
                latest.ProgressPaymentNumber.Value,
                latest.PeriodDate.Value)
            : "OEHS_Hakediş.pdf";

        var metadataSuffix = latest.ProgressPaymentNumber.HasValue && latest.PeriodDate.HasValue
            ? $"_{latest.ProgressPaymentNumber.Value:00}_{latest.PeriodDate.Value:yyyy-MM-dd}"
            : "_Yeni";

        var metadataPath = Path.Combine(documentFolder, $"OEHS_PDF_Bilgi{metadataSuffix}.json");
        var pdfTargetPath = Path.Combine(documentFolder, fileName);
        var metadata = JsonSerializer.Serialize(new
        {
            latest.Id,
            latest.ProgressPaymentNumber,
            latest.PeriodDate,
            latest.RouteNumber,
            latest.RouteName,
            latest.WorkName,
            latest.ContractorName,
            latest.BankAccountNumber,
            TransportDailyPrice = latest.Input.DailyPrice,
            PdfFileName = fileName,
            PdfTargetPath = pdfTargetPath,
            PdfGenerator = "OEHS PDF Export",
            Documents = OfficialDocumentNames,
            SavedAt = DateTime.Now
        }, JsonWriteOptions);

        File.WriteAllText(metadataPath, metadata);
        return documentFolder;
    }

    private void ShowPdfSaveInfoDialog(string targetFolder, string fileName, string targetPath)
    {
        var dialog = new Window
        {
            Title = "OEHS — PDF Kayıt Bilgisi",
            Owner = this,
            Width = 680,
            Height = 390,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock
        {
            Text = "PDF BAŞARIYLA KAYDEDİLDİ",
            FontSize = 20,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray
        });
        panel.Children.Add(new TextBlock
        {
            Text = "PDF başarıyla oluşturuldu. Bu işlemde yazıcı kullanılmadı; dosya doğrudan OEHS kayıt klasörüne kaydedildi.",
            FontSize = 13.5,
            Foreground = System.Windows.Media.Brushes.DimGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 8, 0, 16)
        });
        panel.Children.Add(new TextBlock
        {
            Text = "KAYIT KLASÖRÜ",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.SteelBlue
        });
        panel.Children.Add(new TextBlock
        {
            Text = targetFolder,
            FontSize = 13.5,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 12)
        });
        panel.Children.Add(new TextBlock
        {
            Text = "ÖNERİLEN DOSYA ADI",
            FontSize = 12,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.SteelBlue
        });
        panel.Children.Add(new TextBlock
        {
            Text = fileName,
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 4, 0, 18)
        });

        panel.Children.Add(new TextBlock
        {
            Text = $"Oluşturulan dosya: {targetPath}",
            FontSize = 11.5,
            Foreground = System.Windows.Media.Brushes.DimGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 18)
        });

        var buttons = new StackPanel { Orientation = Orientation.Horizontal, HorizontalAlignment = HorizontalAlignment.Right };
        var folderButton = new Button
        {
            Content = "KLASÖRÜ AÇ",
            Width = 150,
            Height = 42,
            Style = (Style)FindResource("SecondaryButton"),
            Margin = new Thickness(0, 0, 10, 0)
        };
        folderButton.Click += (_, _) =>
        {
            Directory.CreateDirectory(targetFolder);
            Process.Start(new ProcessStartInfo { FileName = targetFolder, UseShellExecute = true });
            dialog.DialogResult = true;
            dialog.Close();
        };

        var okButton = new Button
        {
            Content = "TAMAM",
            Width = 120,
            Height = 42,
            Style = (Style)FindResource("ModernButton")
        };
        okButton.Click += (_, _) =>
        {
            dialog.DialogResult = true;
            dialog.Close();
        };

        buttons.Children.Add(folderButton);
        buttons.Children.Add(okButton);
        panel.Children.Add(buttons);

        dialog.Content = panel;
        dialog.ShowDialog();
    }

    private static int ExtractArchiveRevision(string path)
    {
        var match = System.Text.RegularExpressions.Regex.Match(
            Path.GetFileNameWithoutExtension(path),
            @"_v(?<revision>\d+)$",
            System.Text.RegularExpressions.RegexOptions.CultureInvariant | System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        return match.Success &&
               int.TryParse(
                   match.Groups["revision"].Value,
                   System.Globalization.NumberStyles.None,
                   System.Globalization.CultureInfo.InvariantCulture,
                   out var revision)
            ? revision
            : 1;
    }

    private void RecalculateHistoryItem(int index, ProgressPaymentInput input, ProgressPaymentHistoryData metadataSource)
    {
        var result = ProgressPaymentCalculator.Calculate(input);
        _progressHistory[index] = metadataSource with
        {
            Input = input,
            Calculation = result
        };
    }

    private void RecalculateHistoriesForTenderChange(ContractDraftData oldContract, ContractDraftData updatedContract)
    {
        var propagated = ProgressPaymentPropagationService.RecalculateForContractChange(
            _progressHistory.ToArray(),
            _routes.Select(x => new RouteData(
                x.RouteNumber,
                x.Name,
                x.DailyPrice,
                x.GuideWorkDays,
                x.GuideDailyPrice,
                x.TransportWorkDays,
                x.TenderNumber,
                x.ContractorTaxOrIdentityNumber)).ToArray(),
            oldContract,
            updatedContract);

        _progressHistory.Clear();
        foreach (var item in propagated)
            _progressHistory.Add(item);
    }

    private void UpdateHistoryForRouteEdit(RouteDraft oldRoute, RouteDraft updatedRoute)
    {
        var propagated = BuildRoutePropagation(oldRoute, updatedRoute);
        _progressHistory.Clear();

        foreach (var item in propagated)
            _progressHistory.Add(item);
    }

    private void CalculateProgressButton_Click(object sender, RoutedEventArgs e)
    {
        var wasArchiveCorrection = IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment);

        ClearProgressValidationVisuals();
        ProgressValidationBanner.Visibility = Visibility.Collapsed;
        ProgressValidationMessageTextBlock.Text = string.Empty;

        var invalidFields = new List<(Control Control, string Name)>();

        if (!int.TryParse(WorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays) || workDays < 0)
            invalidFields.Add((WorkDaysTextBox, "Taşıma İş Günü"));

        if (!int.TryParse(GuideWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var guideWorkDays) || guideWorkDays < 0)
            invalidFields.Add((GuideWorkDaysTextBox, "Rehber İş Günü"));

        if (!TryParseDecimal(DailyPriceTextBox.Text, out var dailyPrice) || dailyPrice < 0)
            invalidFields.Add((DailyPriceTextBox, "Taşıma Günlük Fiyat"));
        if (!TryParseDecimal(GuideDailyPriceTextBox.Text, out var guideDailyPrice) || guideDailyPrice < 0)
            invalidFields.Add((GuideDailyPriceTextBox, "Rehber Günlük Fiyat"));
        if (!TryParseDecimal(DiscountTextBox.Text, out var discount) || discount < 0)
            invalidFields.Add((DiscountTextBox, "Taşıma İskonto"));
        if (!TryParseDecimal(GuideDiscountTextBox.Text, out var guideDiscount) || guideDiscount < 0)
            invalidFields.Add((GuideDiscountTextBox, "Rehber İskonto"));
        if (!TryParseDecimal(PriceDifferenceTextBox.Text, out var priceDifference))
            invalidFields.Add((PriceDifferenceTextBox, "Taşıma Fiyat Farkı"));
        if (!TryParseDecimal(GuidePriceDifferenceTextBox.Text, out var guidePriceDifference))
            invalidFields.Add((GuidePriceDifferenceTextBox, "Rehber Fiyat Farkı"));
        if (!TryParseDecimal(TaxDebtTextBox.Text, out var taxDebt) || taxDebt < 0)
            invalidFields.Add((TaxDebtTextBox, "Vergi Borcu"));
        if (!TryParseDecimal(SocialSecurityDebtTextBox.Text, out var socialSecurityDebt) || socialSecurityDebt < 0)
            invalidFields.Add((SocialSecurityDebtTextBox, "SGK Borcu"));
        if (!TryParseDecimal(MinimumWageSupportTextBox.Text, out var minimumWageSupport) || minimumWageSupport < 0)
            invalidFields.Add((MinimumWageSupportTextBox, "Asgari Ücret Desteği"));
        if (!TryParseDecimal(InspectionPenaltyTextBox.Text, out var inspectionPenalty) || inspectionPenalty < 0)
            invalidFields.Add((InspectionPenaltyTextBox, "Denetim Cezası"));

        if (invalidFields.Count > 0)
        {
            foreach (var (control, _) in invalidFields)
                MarkProgressInvalid(control);

            var message = "Lütfen kırmızı işaretli alanları tamamlayın/düzeltin: " +
                          string.Join(", ", invalidFields.Select(x => x.Name)) + ".";
            ProgressValidationMessageTextBlock.Text = message;
            ProgressValidationBanner.Visibility = Visibility.Visible;
            ProgressCalculationSummaryTextBlock.Text = "KONTROL GEREKLİ";
            CalculationStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
            CalculationStatusTextBlock.FontWeight = FontWeights.Bold;
            CalculationStatusTextBlock.Text = message;
            return;
        }

        var selectedContract = IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment)
            ? _editingArchiveSnapshot!.Contract
            : ProgressTenderListBox.SelectedItem as ContractDraftData;
        if (selectedContract is null)
        {
            ShowAlreadyRegistered("Önce İhale / Dosya No seçin.");
            return;
        }

        try
        {
            RouteDraft? selectedRoute;
            if (IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment))
            {
                var archivedRoute = _editingArchiveSnapshot!.Route;
                selectedRoute = new RouteDraft(
                    archivedRoute.RouteNumber,
                    archivedRoute.Name,
                    archivedRoute.DailyPrice,
                    archivedRoute.GuideWorkDays,
                    archivedRoute.GuideDailyPrice,
                    archivedRoute.TransportWorkDays,
                    archivedRoute.TenderNumber,
                    archivedRoute.ContractorTaxOrIdentityNumber,
                    _editingArchiveSnapshot.Contractor.Name);
            }
            else
            {
                RequireSelectedRoute();
                selectedRoute = ProgressRouteComboBox.SelectedItem as RouteDraft;
            }

            if (selectedRoute is null)
            {
                ShowAlreadyRegistered($"'{selectedContract.TenderNumber}' ihalesine bağlı bir hat seçin.");
                return;
            }

            var input = new ProgressPaymentInput(
                workDays,
                dailyPrice,
                discount,
                priceDifference,
                selectedContract.ContractAmount,
                selectedContract.VatExempt,
                taxDebt,
                socialSecurityDebt,
                minimumWageSupport,
                inspectionPenalty,
                guideWorkDays,
                guideDailyPrice,
                guideDiscount,
                guidePriceDifference);

            int? progressNumber = null;
            if (int.TryParse(
                    ProgressNumberTextBox.Text,
                    NumberStyles.Integer,
                    CultureInfo.CurrentCulture,
                    out var parsedProgressNumber) &&
                parsedProgressNumber > 0)
            {
                progressNumber = parsedProgressNumber;
            }

            var periodDate = ProgressPeriodDatePicker.SelectedDate is DateTime selectedPeriodDate
                ? DateOnly.FromDateTime(selectedPeriodDate)
                : (DateOnly?)null;

            if (IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment))
            {
                if (!progressNumber.HasValue || progressNumber <= 0)
                    invalidFields.Add((ProgressNumberTextBox, "Hakediş No"));

                if (!periodDate.HasValue)
                {
                    invalidFields.Add((ProgressPeriodDatePicker, "Dönem Tarihi"));
                }
                else if (periodDate.Value < _editingArchiveSnapshot!.Contract.StartDate ||
                         periodDate.Value > _editingArchiveSnapshot.Contract.EndDate)
                {
                    invalidFields.Add((ProgressPeriodDatePicker, "Dönem Tarihi"));
                }
            }

            if (invalidFields.Count > 0)
            {
                foreach (var (control, _) in invalidFields)
                    MarkProgressInvalid(control);

                var correctionValidationMessage =
                    "Lütfen kırmızı işaretli alanları tamamlayın/düzeltin: " +
                    string.Join(", ", invalidFields.Select(x => x.Name)) + ".";

                ProgressValidationMessageTextBlock.Text = correctionValidationMessage;
                ProgressValidationBanner.Visibility = Visibility.Visible;
                ProgressCalculationSummaryTextBlock.Text = "KONTROL GEREKLİ";
                CalculationStatusTextBlock.Foreground =
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
                CalculationStatusTextBlock.FontWeight = FontWeights.Bold;
                CalculationStatusTextBlock.Text = correctionValidationMessage;
                return;
            }

            var normalizedContractorIdentity = OehsIdentityService.NormalizeIdentity(
                selectedRoute.ContractorTaxOrIdentityNumber);

            string contractorName;
            string contractorBankAccount;

            if (IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment))
            {
                var archivedContractor = _editingArchiveSnapshot!.Contractor;
                contractorName = archivedContractor.Name;
                contractorBankAccount = archivedContractor.BankAccountNumber;
            }
            else
            {
                var liveContractor = FindContractor(normalizedContractorIdentity);
                contractorName = liveContractor?.Name ?? selectedRoute.ContractorName;
                contractorBankAccount = liveContractor?.BankAccountNumber ?? string.Empty;
            }

            // Ön kontrol, hesaplama motorundan önce çalışır.
            var historyItem = new ProgressPaymentHistoryData(
                _editingHistoryId ?? Guid.NewGuid(),
                _editingHistoryId.HasValue
                    ? _progressHistory.First(x => x.Id == _editingHistoryId.Value).CreatedAt
                    : DateTime.Now,
                input,
                new ProgressPaymentCalculation(
                    0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m, 0m,
                    new DeductionBreakdown(0m, 0m, 0m, 0m, 0m, 0m, 0m)),
                progressNumber,
                periodDate,
                selectedRoute.RouteNumber,
                selectedRoute.Name,
                selectedContract.WorkName,
                contractorName,
                selectedRoute.TenderNumber,
                selectedContract.ContractNumber,
                selectedContract.TenderDate,
                selectedContract.ContractDate,
                selectedContract.StartDate,
                selectedContract.EndDate,
                contractorBankAccount,
                normalizedContractorIdentity);

            var dataForValidation = new OehsApplicationData(
                selectedContract,
                _routes.Select(x => new RouteData(
                    x.RouteNumber,
                    x.Name,
                    x.DailyPrice,
                    x.GuideWorkDays,
                    x.GuideDailyPrice,
                    x.TransportWorkDays,
                    x.TenderNumber,
                    x.ContractorTaxOrIdentityNumber)).ToArray(),
                _contractors.Select(x => new Contractor(
                    x.Name,
                    OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                    x.Address,
                    x.Phone,
                    x.Email,
                    x.BankAccountNumber,
                    x.ContactPerson)).ToArray(),
                _progressHistory.ToArray(),
                _contracts.ToArray());

            var validationErrors = IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment)
                ? Array.Empty<string>()
                : ProgressPaymentValidationService.ValidateCreation(
                    dataForValidation,
                    selectedContract,
                    new RouteData(
                        selectedRoute.RouteNumber,
                        selectedRoute.Name,
                        selectedRoute.DailyPrice,
                        selectedRoute.GuideWorkDays,
                        selectedRoute.GuideDailyPrice,
                        selectedRoute.TransportWorkDays,
                        selectedRoute.TenderNumber,
                        normalizedContractorIdentity),
                    historyItem,
                    _editingHistoryId.HasValue);

            if (validationErrors.Count > 0)
            {
                ProgressCalculationSummaryTextBlock.Text = "KONTROL GEREKLİ";
                ProgressValidationMessageTextBlock.Text = string.Join(Environment.NewLine, validationErrors);
                ProgressValidationBanner.Visibility = Visibility.Visible;
                CalculationStatusTextBlock.Foreground =
                    new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
                CalculationStatusTextBlock.FontWeight = FontWeights.Bold;
                CalculationStatusTextBlock.Text = string.Join(" ", validationErrors);
                ArchiveCurrentProgressButton.Visibility = Visibility.Collapsed;
                CorrectCurrentProgressButton.Visibility = Visibility.Collapsed;
                return;
            }

            ProgressValidationBanner.Visibility = Visibility.Collapsed;
            ProgressValidationMessageTextBlock.Text =
                $"Ön kontrol tamam: İhale/Dosya {selectedContract.TenderNumber} • Hat {selectedRoute.RouteNumber} • " +
                $"Yüklenici {contractorName} • Hakediş {progressNumber} • Dönem {periodDate:dd.MM.yyyy}.";
            CalculationStatusTextBlock.Foreground =
                new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 104, 216));
            CalculationStatusTextBlock.FontWeight = FontWeights.SemiBold;
            CalculationStatusTextBlock.Text = ProgressValidationMessageTextBlock.Text;

            var result = ProgressPaymentCalculator.Calculate(input);

            historyItem = historyItem with { Calculation = result };

            ServiceAmountResultTextBlock.Text = FormatMoney(result.ServiceAmount);
            GuideServiceAmountResultTextBlock.Text = FormatMoney(result.GuideServiceAmount);
            TransportPriceDifferenceResultTextBlock.Text = FormatMoney(result.PriceDifferenceExcludingVat);
            GuidePriceDifferenceResultTextBlock.Text = FormatMoney(result.GuidePriceDifferenceExcludingVat);
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

            if (IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment))
            {
                var archive = _editingArchiveSnapshot!;
                var updatedPayment = archive.ProgressPayment with
                {
                    Input = historyItem.Input,
                    Calculation = historyItem.Calculation,
                    ProgressPaymentNumber = historyItem.ProgressPaymentNumber,
                    PeriodDate = historyItem.PeriodDate,
                    RouteNumber = historyItem.RouteNumber,
                    RouteName = historyItem.RouteName
                };

                SaveArchiveCorrectionSnapshot(
                    archive with
                    {
                        ProgressPayment = updatedPayment,
                        PdfFileName = BuildArchivePdfFileName(updatedPayment)
                    },
                    "Hakediş girdileri");

                ProgressCalculationSummaryTextBlock.Text = "ARŞİV GÜNCELLENDİ";
                ArchiveCurrentProgressButton.Visibility = Visibility.Collapsed;
                CorrectCurrentProgressButton.Visibility = Visibility.Collapsed;
                return;
            }

            if (_editingHistoryId.HasValue)
            {
                var editIndex = _progressHistory.IndexOf(
                    _progressHistory.First(x => x.Id == _editingHistoryId.Value));
                _progressHistory[editIndex] = historyItem;
                _editingHistoryId = null;
            }
            else
            {
                _progressHistory.Add(historyItem);
            }

            SetProgressEditorMode(false);
            SavePersistedData();
            _lastCalculatedHistoryId = historyItem.Id;
            UpdateHistoryView();
            UpdateSummaryView();
            UpdateReportView();
            UpdateHomeSummaryView();
            UpdateAcceptanceView();

            CalculationStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 104, 216));
            CalculationStatusTextBlock.FontWeight = FontWeights.SemiBold;
            CalculationStatusTextBlock.Text = wasArchiveCorrection
                ? "Düzeltme hesaplandı. Önceki arşiv revision'ı korunuyor; yeni revision için ARŞİVLE düğmesine basın."
                : "Hesaplama tamamlandı. Arşivlemek veya düzeltmek için aşağıdaki işlemleri kullanın.";
            ArchiveCurrentProgressButton.Visibility = Visibility.Visible;
            CorrectCurrentProgressButton.Visibility = Visibility.Visible;
            UpdateArchiveView();
        }
        catch (ArgumentOutOfRangeException ex)
        {
            ProgressCalculationSummaryTextBlock.Text = "KONTROL";
            CalculationStatusTextBlock.Text = ex.Message;
        }
    }

    private ProgressPaymentHistoryData? GetLastCalculatedProgress()
    {
        if (_lastCalculatedHistoryId is Guid id)
            return _progressHistory.FirstOrDefault(x => x.Id == id);

        return GetSelectedProgressLatestPayment();
    }

    private void ArchiveCurrentProgressButton_Click(object sender, RoutedEventArgs e)
    {
        if (IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment) && _editingArchiveSnapshot is not null && !string.IsNullOrWhiteSpace(_editingArchiveSourcePath))
        {
            SaveArchiveCorrectionSnapshot(_editingArchiveSnapshot, "Arşiv");
            return;
        }

        var item = GetLastCalculatedProgress();
        if (item is null)
        {
            MessageBox.Show("Arşivlenecek tamamlanmış bir hakediş bulunamadı. Önce HAKEDİŞİ HESAPLA düğmesine basın.", "OEHS",
                MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        var contract = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase));
        var route = _routes.FirstOrDefault(x =>
            string.Equals(
                OehsIdentityService.BuildRouteKey(x.TenderNumber, x.RouteNumber, x.ContractorTaxOrIdentityNumber),
                OehsIdentityService.BuildRouteKey(item.TenderNumber ?? string.Empty, item.RouteNumber ?? string.Empty, item.ContractorTaxOrIdentityNumber ?? string.Empty),
                StringComparison.Ordinal));

        var contractor = route is null ? FindContractor(item.ContractorTaxOrIdentityNumber ?? string.Empty) : FindContractor(route.ContractorTaxOrIdentityNumber);
        if (contract is null || route is null || contractor is null)
        {
            MessageBox.Show("Arşiv için ihale, hat ve yüklenici snapshot'ı çözümlenemedi. Kaydı kontrol edin.", "OEHS",
                MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        var routeData = new RouteData(
            route.RouteNumber,
            route.Name,
            route.DailyPrice,
            route.GuideWorkDays,
            route.GuideDailyPrice,
            route.TransportWorkDays,
            route.TenderNumber,
            route.ContractorTaxOrIdentityNumber);

        var prior = _progressHistory
            .Where(x => string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber ?? string.Empty,
                    x.RouteNumber ?? string.Empty,
                    x.ContractorTaxOrIdentityNumber ?? string.Empty),
                OehsIdentityService.BuildRouteKey(route.TenderNumber, route.RouteNumber, route.ContractorTaxOrIdentityNumber),
                StringComparison.Ordinal))
            .Where(x => !x.Id.Equals(item.Id))
            .Where(x => x.PeriodDate.HasValue && (!item.PeriodDate.HasValue || x.PeriodDate.Value < item.PeriodDate.Value))
            .OrderBy(x => x.PeriodDate)
            .ThenBy(x => x.CreatedAt)
            .ToArray();

        var archive = new ProgressPaymentArchive(
            "1.0",
            1,
            DateTime.UtcNow,
            "ActiveRecord",
            contract,
            new Contractor(
                contractor.Name,
                OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber),
                contractor.Address,
                contractor.Phone,
                contractor.Email,
                contractor.BankAccountNumber,
                contractor.ContactPerson),
            routeData,
            item,
            prior,
            OehsFileNameService.BuildProgressPdfFileName(
                item.TenderNumber ?? string.Empty,
                route.RouteNumber,
                item.ProgressPaymentNumber ?? 1,
                item.PeriodDate ?? DateOnly.FromDateTime(DateTime.Today)),
            new[]
            {
                "Hakediş Raporu",
                "Hizmet İşleri Hakediş Raporu",
                "Hizmet İşleri Kabul Tutanağı",
                "Hakediş İcmali"
            });

        try
        {
            var path = _archiveStore.Save(archive);
            var revision = ExtractArchiveRevision(path);
            var wasCorrection = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath);

            ArchiveStatusTextBlock.Text = "ARŞİV OLUŞTURULDU";
            CalculationStatusTextBlock.Text =
                wasCorrection
                    ? $"Düzeltme başarıyla arşive eklendi: v{revision:00}."
                    : $"Hakediş arşivlendi: v{revision:00}.";

            UpdateArchiveView();
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));

            MessageBox.Show(
                wasCorrection
                    ? $"Hakediş No {item.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"} başarıyla düzeltilerek arşive v{revision:00} olarak eklendi.\n\nÖnceki revision korunmuştur.\n{Path.GetFileName(path)}"
                    : $"Hakediş No {item.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"} arşive v{revision:00} olarak eklendi.\n\n{Path.GetFileName(path)}",
                "OEHS — Arşiv Bilgisi",
                MessageBoxButton.OK,
                MessageBoxImage.Information);

            _editingArchiveSourcePath = null;
        }
        catch (Exception ex) when (ex is IOException or UnauthorizedAccessException or ProgressPaymentArchiveException)
        {
            ArchiveStatusTextBlock.Text = "ARŞİV HATASI";
            CalculationStatusTextBlock.Text = ex.Message;
        }
    }

    private void CorrectCurrentProgressButton_Click(object sender, RoutedEventArgs e)
    {
        var item = GetLastCalculatedProgress();
        if (item is null)
        {
            MessageBox.Show("Düzeltilecek hakediş bulunamadı.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        EditProgressWithTarget(item);
    }

    private void EditProgressWithTarget(ProgressPaymentHistoryData item)
    {
        var target = AskHistoryEditTarget(item);
        if (!target.HasValue)
        {
            ClearArchiveCorrectionState();
            return;
        }

        var selection = ProgressCorrectionTargetService.Select(target.Value, item);
        switch (selection.Target)
        {
            case ProgressCorrectionTarget.Tender:
                BeginContractHistoryEdit(item);
                break;
            case ProgressCorrectionTarget.Contractor:
                BeginContractorHistoryEdit(item);
                break;
            case ProgressCorrectionTarget.Route:
                BeginRouteHistoryEdit(item);
                break;
            case ProgressCorrectionTarget.ProgressPayment:
                BeginProgressHistoryEdit(item);
                break;
            case ProgressCorrectionTarget.Cancel:
                ClearArchiveCorrectionState();
                break;
        }
    }

    private void UpdateArchiveView()
    {
        _archiveRows.Clear();

        foreach (var row in ArchiveDisplayService.BuildRows(_archiveStore))
        {
            _archiveRows.Add(new ArchiveRow(
                row.FullPath,
                row.DisplayTitle,
                row.DetailText,
                row.NetPayableText,
                row.RevisionText));
        }

        ArchiveCountTextBlock.Text = _archiveRows.Count.ToString(CultureInfo.CurrentCulture);
        ArchiveLatestRevisionTextBlock.Text =
            _archiveRows
                .Select(row => row.RevisionText)
                .Where(text => text.StartsWith("v", StringComparison.OrdinalIgnoreCase))
                .LastOrDefault() ?? "-";
        ArchiveStatusTextBlock.Text = _archiveRows.Count == 0 ? "HAZIR" : "GÜNCEL";
        ArchiveEmptyStateBorder.Visibility = _archiveRows.Count == 0
            ? Visibility.Visible
            : Visibility.Collapsed;
    }

    private void LoadArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ArchiveRow row })
            return;

        if (sender is Button loadButton &&
            string.Equals(loadButton.Content?.ToString(), "ÇAĞIR", StringComparison.OrdinalIgnoreCase))
        {
            ClearArchiveCorrectionState();
        }

        ProgressPaymentArchive archive;
        try
        {
            archive = ProgressPaymentArchiveStore.Load(row.FullPath);
        }
        catch (ProgressPaymentArchiveException ex)
        {
            MessageBox.Show(ex.Message, "OEHS", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        try
        {
            var currentData = new OehsApplicationData(
                _contracts.FirstOrDefault(),
                _routes.Select(x => new RouteData(
                    x.RouteNumber,
                    x.Name,
                    x.DailyPrice,
                    x.GuideWorkDays,
                    x.GuideDailyPrice,
                    x.TransportWorkDays,
                    x.TenderNumber,
                    x.ContractorTaxOrIdentityNumber)).ToArray(),
                _contractors.Select(x => new Contractor(
                    x.Name,
                    OehsIdentityService.NormalizeIdentity(x.TaxOrIdentityNumber),
                    x.Address,
                    x.Phone,
                    x.Email,
                    x.BankAccountNumber,
                    x.ContactPerson)).ToArray(),
                _progressHistory.ToArray(),
                _contracts.ToArray());

            var restored = ProgressPaymentArchiveRestoreService.Restore(currentData, archive);

            _contracts.Clear();
            foreach (var contract in restored.Data.Contracts ?? Array.Empty<ContractDraftData>())
                _contracts.Add(contract);

            _contractors.Clear();
            foreach (var contractor in restored.Data.Contractors ?? Array.Empty<Contractor>())
                _contractors.Add(new ContractorDraft(
                    contractor.Name,
                    OehsIdentityService.NormalizeIdentity(contractor.TaxOrIdentityNumber),
                    contractor.Address,
                    contractor.Phone,
                    contractor.Email,
                    contractor.BankAccountNumber,
                    contractor.ContactPerson));

            _routes.Clear();
            foreach (var route in restored.Data.Routes ?? Array.Empty<RouteData>())
            {
                var contractor = FindContractor(route.ContractorTaxOrIdentityNumber);
                _routes.Add(new RouteDraft(
                    route.RouteNumber,
                    route.Name,
                    route.DailyPrice,
                    route.GuideWorkDays,
                    route.GuideDailyPrice,
                    route.TransportWorkDays,
                    route.TenderNumber,
                    OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber),
                    contractor?.Name ?? archive.Contractor.Name));
            }

            _progressHistory.Clear();
            foreach (var item in restored.Data.ProgressPayments ?? Array.Empty<ProgressPaymentHistoryData>())
                _progressHistory.Add(item);

            SavePersistedData();

            var liveContract = _contracts.FirstOrDefault(x =>
                string.Equals(x.TenderNumber, archive.Contract.TenderNumber, StringComparison.OrdinalIgnoreCase))
                ?? archive.Contract;

            var liveRoute = _routes.FirstOrDefault(x =>
                string.Equals(
                    OehsIdentityService.BuildRouteKey(
                        x.TenderNumber,
                        x.RouteNumber,
                        x.ContractorTaxOrIdentityNumber),
                    OehsIdentityService.BuildRouteKey(
                        archive.Route.TenderNumber,
                        archive.Route.RouteNumber,
                        archive.Route.ContractorTaxOrIdentityNumber),
                    StringComparison.Ordinal))
                ?? new RouteDraft(
                    archive.Route.RouteNumber,
                    archive.Route.Name,
                    archive.Route.DailyPrice,
                    archive.Route.GuideWorkDays,
                    archive.Route.GuideDailyPrice,
                    archive.Route.TransportWorkDays,
                    archive.Route.TenderNumber,
                    OehsIdentityService.NormalizeIdentity(archive.Route.ContractorTaxOrIdentityNumber),
                    archive.Contractor.Name);

            var existing = _progressHistory.FirstOrDefault(x => x.Id == archive.ProgressPayment.Id);
            _editingHistoryId = existing?.Id;
            _lastCalculatedHistoryId = archive.ProgressPayment.Id;

            _suppressSelectionEvents = true;
            try
            {
                ProgressTenderListBox.SelectedItem = liveContract;
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            UpdateProgressContractorAndRoutes(liveContract);
            ProgressRouteComboBox.SelectedItem = liveRoute;

            var input = archive.ProgressPayment.Input;
            // Arşiv bir snapshot olduğundan hesabın otomatik alanları canlı kaydın
            // güncel değerlerinden değil, arşivlenen hat snapshot'ından yüklenir.
            WorkDaysTextBox.Text = archive.Route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
            DailyPriceTextBox.Text = archive.Route.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            GuideWorkDaysTextBox.Text = archive.Route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
            GuideDailyPriceTextBox.Text = archive.Route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            DiscountTextBox.Text = input.DiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            GuideDiscountTextBox.Text = input.GuideDiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            PriceDifferenceTextBox.Text = input.PriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            GuidePriceDifferenceTextBox.Text = input.GuidePriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            TaxDebtTextBox.Text = input.TaxDebt.ToString("N2", CultureInfo.CurrentCulture);
            SocialSecurityDebtTextBox.Text = input.SocialSecurityDebt.ToString("N2", CultureInfo.CurrentCulture);
            MinimumWageSupportTextBox.Text = input.MinimumWageSupport.ToString("N2", CultureInfo.CurrentCulture);
            InspectionPenaltyTextBox.Text = input.InspectionPenalty.ToString("N2", CultureInfo.CurrentCulture);
            ProgressNumberTextBox.Text = archive.ProgressPayment.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            ProgressPeriodDatePicker.SelectedDate = archive.ProgressPayment.PeriodDate?.ToDateTime(TimeOnly.MinValue);

            UpdateContractSelectors();
            UpdateHistoryView();
            UpdateArchiveView();
            UpdateHomeSummaryView();
            UpdateAcceptanceView();
            NavigationButton_Click(NavProgress, new RoutedEventArgs(Button.ClickEvent, NavProgress));

            CalculationStatusTextBlock.Text =
                restored.AddedAnything
                    ? $"Arşiv çağrıldı ve aktif veriyle birleştirildi. {restored.Message} Kaydetmeden önce ön kontrol yapılacaktır."
                    : $"Arşiv çağrıldı. {restored.Message} Kaydetmeden önce ön kontrol yapılacaktır.";
        }
        catch (Exception ex) when (ex is ProgressPaymentArchiveException or IOException or UnauthorizedAccessException)
        {
            MessageBox.Show($"Arşiv geri yüklenemedi.\n\n{ex.Message}", "OEHS", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void EditArchiveButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { Tag: ArchiveRow row })
            BeginCorrectionFromArchive(row);
    }

    private void BeginCorrectionFromArchive(ArchiveRow row)
    {
        try
        {
            var archive = ProgressPaymentArchiveStore.Load(row.FullPath);
            _editingArchiveSourcePath = row.FullPath;
            _editingArchiveSnapshot = archive;
            _archiveCorrectionSection = ArchiveCorrectionSection.None;

            EditProgressWithTarget(archive.ProgressPayment);
        }
        catch (ProgressPaymentArchiveException ex)
        {
            ClearArchiveCorrectionState();
            MessageBox.Show(ex.Message, "OEHS", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    private void OpenArchiveFolderButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ArchiveRow row })
            return;

        var folder = Path.GetDirectoryName(row.FullPath) ?? OehsOutputPathService.PdfRoot;

        Directory.CreateDirectory(folder);
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    private void UpdateHistoryView()
    {
        _historyRows.Clear();

        var orderedHistory = ProgressPaymentReportService.OrderByPaymentPeriod(_progressHistory);

        for (var index = 0; index < orderedHistory.Count; index++)
        {
            var item = orderedHistory[index];
            var displayNumber = item.ProgressPaymentNumber.HasValue
                ? $"Hakediş No {item.ProgressPaymentNumber.Value} • İhale/Dosya: {item.TenderNumber ?? "-"} • Hat: {item.RouteNumber ?? "-"}"
                : $"Hesaplama {index + 1} • İhale/Dosya: {item.TenderNumber ?? "-"} • Hat: {item.RouteNumber ?? "-"}";
            var periodText = item.PeriodDate.HasValue
                ? $"Dönem: {item.PeriodDate.Value:dd.MM.yyyy} • Yüklenici: {item.ContractorName ?? "-"} • Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}"
                : $"Yüklenici: {item.ContractorName ?? "-"} • Kayıt: {item.CreatedAt:dd.MM.yyyy HH:mm}";

            _historyRows.Add(new HistoryRow(
                item.Id,
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

    private ProgressPaymentHistoryData? GetHistoryItemFromRow(object? tag)
    {
        return tag is HistoryRow row
            ? _progressHistory.FirstOrDefault(x => x.Id == row.Id)
            : null;
    }

    private static string GetHistoryArchiveFolder(ProgressPaymentHistoryData item) =>
        OehsOutputPathService.BuildPdfDirectory(
            item.TenderNumber ?? string.Empty,
            item.RouteNumber ?? string.Empty);

    private void OpenHistoryFolderButton_Click(object sender, RoutedEventArgs e)
    {
        var item = GetHistoryItemFromRow((sender as Button)?.Tag);
        if (item is null)
            return;

        var folder = GetHistoryArchiveFolder(item);
        Directory.CreateDirectory(folder);
        Process.Start(new ProcessStartInfo
        {
            FileName = folder,
            UseShellExecute = true
        });
    }

    private void EditCurrentProgressButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        var latest = GetSelectedProgressLatestPayment();
        if (latest is null)
        {
            MessageBox.Show("Seçilen ihale/hat için düzenlenecek kayıt bulunamadı.", "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
            return;
        }

        BeginProgressHistoryEdit(latest);
    }

    private void EditHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        ClearArchiveCorrectionState();
        if (sender is Button { Tag: HistoryRow row })
        {
            var item = _progressHistory.FirstOrDefault(x => x.Id == row.Id);
            if (item is not null)
                EditProgressWithTarget(item);
        }
    }

    private ProgressCorrectionTarget? AskHistoryEditTarget(ProgressPaymentHistoryData item)
    {
        ProgressCorrectionTarget? selected = null;
        var dialog = new Window
        {
            Title = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath) ? "Arşiv Kaydı Düzelt" : "Hakediş Düzelt",
            Owner = this,
            Width = 500,
            Height = 430,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock
        {
            Text = !string.IsNullOrWhiteSpace(_editingArchiveSourcePath)
                ? "Arşiv kaydında hangi bölümü düzeltmek istiyorsunuz?"
                : "Hangi bölümde düzeltme yapmak istiyorsunuz?",
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            Margin = new Thickness(0, 0, 0, 6)
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"İhale/Dosya: {item.TenderNumber ?? "-"} • Hat: {item.RouteNumber ?? "-"} • Hakediş: {item.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"}",
            FontSize = 12.5,
            Foreground = System.Windows.Media.Brushes.DimGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 16)
        });

        foreach (var option in new[]
        {
            ("İHALE BİLGİLERİ", ProgressCorrectionTarget.Tender),
            ("YÜKLENİCİ FİRMA", ProgressCorrectionTarget.Contractor),
            ("HAT BİLGİLERİ", ProgressCorrectionTarget.Route),
            ("HAKEDİŞ GİRDİLERİ", ProgressCorrectionTarget.ProgressPayment),
            ("VAZGEÇ", ProgressCorrectionTarget.Cancel)
        })
        {
            var button = new Button
            {
                Content = option.Item1,
                Width = 420,
                Height = 42,
                Margin = new Thickness(0, 4, 0, 4),
                Style = (Style)FindResource(option.Item2 == ProgressCorrectionTarget.Cancel ? "SecondaryButton" : "ModernButton")
            };
            button.Click += (_, _) =>
            {
                selected = option.Item2;
                dialog.DialogResult = true;
                dialog.Close();
            };
            panel.Children.Add(button);
        }

        dialog.Content = panel;
        dialog.ShowDialog();
        return selected;
    }

    private void BeginContractHistoryEdit(ProgressPaymentHistoryData item)
    {
        if (IsArchiveCorrection)
        {
            _archiveCorrectionSection = ArchiveCorrectionSection.Contract;
            var archivedContract = _editingArchiveSnapshot!.Contract;
            _editingContractTender = null;
            ApplyContractToForm(archivedContract);
            SetContractEditorMode(true);
            NavigationButton_Click(NavContract, new RoutedEventArgs(Button.ClickEvent, NavContract));
            ContractStatusTextBlock.Text =
                "ARŞİV DÜZELTME MODU: İhale bilgilerini güncelleyin. DÜZENLE mevcut arşiv revision'ını aynı kayıt üzerinde güncelleyecektir.";
            return;
        }

        var contract = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase));
        if (contract is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu ihale kaydı bulunamadı.");
            return;
        }

        _editingContractTender = contract.TenderNumber;
        ApplyContractToForm(contract);
        SetContractEditorMode(true);
        NavigationButton_Click(NavContract, new RoutedEventArgs(Button.ClickEvent, NavContract));
        ContractStatusTextBlock.Text = "Hakedişten gelen düzenleme: yalnızca İhale Bilgileri alanlarını düzeltin ve DÜZENLE'e basın.";
    }

    private void BeginContractorHistoryEdit(ProgressPaymentHistoryData item)
    {
        if (IsArchiveCorrection)
        {
            _archiveCorrectionSection = ArchiveCorrectionSection.Contractor;
            var archivedContractor = _editingArchiveSnapshot!.Contractor;
            _editingContractorKey = null;
            ContractorNameTextBox.Text = archivedContractor.Name;
            ContractorTaxTextBox.Text = archivedContractor.TaxOrIdentityNumber;
            ContractorAddressTextBox.Text = archivedContractor.Address;
            ContractorPhoneTextBox.Text = archivedContractor.Phone;
            ContractorEmailTextBox.Text = archivedContractor.Email;
            ContractorBankTextBox.Text = archivedContractor.BankAccountNumber;
            ContractorContactTextBox.Text = archivedContractor.ContactPerson;
            SetContractorEditorMode(true);
            NavigationButton_Click(NavContractors, new RoutedEventArgs(Button.ClickEvent, NavContractors));
            ContractorStatusTextBlock.Text =
                "ARŞİV DÜZELTME MODU: firma bilgilerini güncelleyin. DÜZENLE mevcut arşiv revision'ını aynı kayıt üzerinde güncelleyecektir.";
            return;
        }

        var contractor = FindContractor(item.ContractorTaxOrIdentityNumber ?? string.Empty);

        if (contractor is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu yüklenici firma bulunamadı.");
            return;
        }

        _editingContractorKey = contractor.TaxOrIdentityNumber;
        SetContractorEditorMode(true);
        ContractorNameTextBox.Text = contractor.Name;
        ContractorTaxTextBox.Text = contractor.TaxOrIdentityNumber;
        ContractorAddressTextBox.Text = contractor.Address;
        ContractorPhoneTextBox.Text = contractor.Phone;
        ContractorEmailTextBox.Text = contractor.Email;
        ContractorBankTextBox.Text = contractor.BankAccountNumber;
        ContractorContactTextBox.Text = contractor.ContactPerson;
        NavigationButton_Click(NavContractors, new RoutedEventArgs(Button.ClickEvent, NavContractors));
        ContractorStatusTextBlock.Text = "Hakedişten gelen düzenleme: Firma bilgilerini değiştirin ve DÜZENLE'e basın.";
    }

    private void BeginRouteHistoryEdit(ProgressPaymentHistoryData item)
    {
        if (IsArchiveCorrection)
        {
            _archiveCorrectionSection = ArchiveCorrectionSection.Route;
            var archive = _editingArchiveSnapshot!;
            RouteTenderListBox.SelectedItem = null;
            RouteContractorListBox.ItemsSource = Array.Empty<ContractorDraft>();
            RouteContractorListBox.SelectedItem = null;

            RouteNumberTextBox.Text = archive.Route.RouteNumber;
            RouteNameTextBox.Text = archive.Route.Name;
            RouteTransportWorkDaysTextBox.Text = archive.Route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
            RouteGuideWorkDaysTextBox.Text = archive.Route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
            RouteGuideDailyPriceTextBox.Text = archive.Route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            RouteNumberTextBox.IsReadOnly = false;
            RouteSelectedTenderTextBlock.Text = $"Arşiv ihale: {archive.Route.TenderNumber}";
            RouteSelectedContractorTextBlock.Text = $"Arşiv yüklenici: {archive.Contractor.Name}";

            _editingRoute = null;
            SetRouteEditorMode(true, archiveCorrection: true);
            NavigationButton_Click(NavLines, new RoutedEventArgs(Button.ClickEvent, NavLines));
            RouteStatusTextBlock.Text =
                "ARŞİV DÜZELTME MODU: Hat No dahil ilgili alanları değiştirebilirsiniz. DÜZENLE seçili arşiv dosyasını aynı revision numarasıyla günceller; yeni hat oluşturmaz.";
            return;
        }

        var historyRouteKey = OehsIdentityService.BuildRouteKey(
            item.TenderNumber ?? string.Empty,
            item.RouteNumber ?? string.Empty,
            item.ContractorTaxOrIdentityNumber ?? string.Empty);

        var route = _routes.FirstOrDefault(x =>
            string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber,
                    x.RouteNumber,
                    x.ContractorTaxOrIdentityNumber),
                historyRouteKey,
                StringComparison.Ordinal));

        if (route is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu hat bulunamadı.");
            return;
        }

        _editingRoute = route;

        var contract = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase));
        var contractor = FindContractor(route.ContractorTaxOrIdentityNumber);

        _suppressSelectionEvents = true;
        try
        {
            RouteTenderListBox.SelectedItem = contract;
            RouteContractorListBox.ItemsSource = contractor is null
                ? Array.Empty<ContractorDraft>()
                : new[] { contractor };
            RouteContractorListBox.SelectedItem = contractor;
        }
        finally
        {
            _suppressSelectionEvents = false;
        }

        RouteNumberTextBox.Text = route.RouteNumber;
        RouteNumberTextBox.IsReadOnly = false;
        RouteNameTextBox.Text = route.Name;
        RouteTransportWorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
        RouteGuideWorkDaysTextBox.Text = route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
        RouteGuideDailyPriceTextBox.Text = route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        SetRouteEditorMode(true);
        NavigationButton_Click(NavLines, new RoutedEventArgs(Button.ClickEvent, NavLines));
        RouteStatusTextBlock.Text = "Hakedişten gelen düzenleme: Hat bilgilerini değiştirin ve DÜZENLE'e basın.";
    }

    private void BeginProgressHistoryEdit(ProgressPaymentHistoryData item)
    {
        if (IsArchiveCorrection)
        {
            _archiveCorrectionSection = ArchiveCorrectionSection.ProgressPayment;
            var payment = _editingArchiveSnapshot!.ProgressPayment;
            _editingHistoryId = null;
            WorkDaysTextBox.Text = payment.Input.WorkDays.ToString(CultureInfo.CurrentCulture);
            DailyPriceTextBox.Text = payment.Input.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            GuideWorkDaysTextBox.Text = payment.Input.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
            GuideDailyPriceTextBox.Text = payment.Input.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            DiscountTextBox.Text = payment.Input.DiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            GuideDiscountTextBox.Text = payment.Input.GuideDiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            PriceDifferenceTextBox.Text = payment.Input.PriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            GuidePriceDifferenceTextBox.Text = payment.Input.GuidePriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            TaxDebtTextBox.Text = payment.Input.TaxDebt.ToString("N2", CultureInfo.CurrentCulture);
            SocialSecurityDebtTextBox.Text = payment.Input.SocialSecurityDebt.ToString("N2", CultureInfo.CurrentCulture);
            MinimumWageSupportTextBox.Text = payment.Input.MinimumWageSupport.ToString("N2", CultureInfo.CurrentCulture);
            InspectionPenaltyTextBox.Text = payment.Input.InspectionPenalty.ToString("N2", CultureInfo.CurrentCulture);
            ProgressNumberTextBox.Text = payment.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            ProgressPeriodDatePicker.SelectedDate = payment.PeriodDate?.ToDateTime(TimeOnly.MinValue);

            SetProgressEditorMode(true, archiveCorrection: true);
            NavigationButton_Click(NavProgress, new RoutedEventArgs(Button.ClickEvent, NavProgress));
            CalculationStatusTextBlock.Text =
                "ARŞİV DÜZELTME MODU: Hakediş girdilerini güncelleyin. DÜZENLE mevcut arşiv revision'ını aynı kayıt üzerinde güncelleyecektir.";
            return;
        }

        var contract = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase));
        if (contract is not null)
        {
            _suppressSelectionEvents = true;
            ProgressTenderListBox.SelectedItem = contract;
            _suppressSelectionEvents = false;
            UpdateProgressContractorAndRoutes(contract);
        }

        _editingHistoryId = item.Id;
        var historyRouteKey = OehsIdentityService.BuildRouteKey(
            item.TenderNumber ?? string.Empty,
            item.RouteNumber ?? string.Empty,
            item.ContractorTaxOrIdentityNumber ?? string.Empty);

        ProgressRouteComboBox.SelectedItem = _routes.FirstOrDefault(x =>
            string.Equals(
                OehsIdentityService.BuildRouteKey(
                    x.TenderNumber,
                    x.RouteNumber,
                    x.ContractorTaxOrIdentityNumber),
                historyRouteKey,
                StringComparison.Ordinal));

        WorkDaysTextBox.Text = item.Input.WorkDays.ToString(CultureInfo.CurrentCulture);
        DailyPriceTextBox.Text = item.Input.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        GuideWorkDaysTextBox.Text = item.Input.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
        GuideDailyPriceTextBox.Text = item.Input.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        DiscountTextBox.Text = item.Input.DiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
        GuideDiscountTextBox.Text = item.Input.GuideDiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
        PriceDifferenceTextBox.Text = item.Input.PriceDifference.ToString("N2", CultureInfo.CurrentCulture);
        GuidePriceDifferenceTextBox.Text = item.Input.GuidePriceDifference.ToString("N2", CultureInfo.CurrentCulture);
        TaxDebtTextBox.Text = item.Input.TaxDebt.ToString("N2", CultureInfo.CurrentCulture);
        SocialSecurityDebtTextBox.Text = item.Input.SocialSecurityDebt.ToString("N2", CultureInfo.CurrentCulture);
        MinimumWageSupportTextBox.Text = item.Input.MinimumWageSupport.ToString("N2", CultureInfo.CurrentCulture);
        InspectionPenaltyTextBox.Text = item.Input.InspectionPenalty.ToString("N2", CultureInfo.CurrentCulture);
        ProgressNumberTextBox.Text = item.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
        ProgressPeriodDatePicker.SelectedDate = item.PeriodDate?.ToDateTime(TimeOnly.MinValue);

        SetProgressEditorMode(true);
        NavigationButton_Click(NavProgress, new RoutedEventArgs(Button.ClickEvent, NavProgress));
        CalculationStatusTextBlock.Text = "Düzenleme modu: hakediş girdilerini değiştirin ve DÜZENLE'e basın.";
    }

    private void CancelProgressEditButton_Click(object sender, RoutedEventArgs e)
    {
        var archiveCorrection = IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment);
        var original = !archiveCorrection && _editingHistoryId is Guid id
            ? _progressHistory.FirstOrDefault(x => x.Id == id)
            : null;

        ClearArchiveCorrectionState();
        _editingHistoryId = null;

        if (archiveCorrection)
        {
            ProgressValidationBanner.Visibility = Visibility.Collapsed;
            NavigationButton_Click(NavArchives, new RoutedEventArgs(Button.ClickEvent, NavArchives));
            return;
        }

        if (original is not null)
        {
            WorkDaysTextBox.Text = original.Input.WorkDays.ToString(CultureInfo.CurrentCulture);
            DailyPriceTextBox.Text = original.Input.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            GuideWorkDaysTextBox.Text = original.Input.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
            GuideDailyPriceTextBox.Text = original.Input.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);
            DiscountTextBox.Text = original.Input.DiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            GuideDiscountTextBox.Text = original.Input.GuideDiscountAmount.ToString("N2", CultureInfo.CurrentCulture);
            PriceDifferenceTextBox.Text = original.Input.PriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            GuidePriceDifferenceTextBox.Text = original.Input.GuidePriceDifference.ToString("N2", CultureInfo.CurrentCulture);
            TaxDebtTextBox.Text = original.Input.TaxDebt.ToString("N2", CultureInfo.CurrentCulture);
            SocialSecurityDebtTextBox.Text = original.Input.SocialSecurityDebt.ToString("N2", CultureInfo.CurrentCulture);
            MinimumWageSupportTextBox.Text = original.Input.MinimumWageSupport.ToString("N2", CultureInfo.CurrentCulture);
            InspectionPenaltyTextBox.Text = original.Input.InspectionPenalty.ToString("N2", CultureInfo.CurrentCulture);
            ProgressNumberTextBox.Text = original.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? string.Empty;
            ProgressPeriodDatePicker.SelectedDate = original.PeriodDate?.ToDateTime(TimeOnly.MinValue);
        }

        SetProgressEditorMode(false);
        CalculationStatusTextBlock.Text = "Hakediş düzenlemesi iptal edildi; kayıt değiştirilmedi.";
    }

    private void DeleteHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: HistoryRow row })
            return;

        var item = _progressHistory.FirstOrDefault(x => x.Id == row.Id);
        if (item is null)
            return;

        var title = item.ProgressPaymentNumber.HasValue
            ? $"Hakediş No {item.ProgressPaymentNumber.Value}"
            : "Hakediş Hesaplama";

        var result = MessageBox.Show(
            $"{title} kaydı silinsin mi? Bu işlem geri alınamaz.",
            "Hakediş Kaydını Sil",
            MessageBoxButton.YesNo,
            MessageBoxImage.Warning);

        if (result != MessageBoxResult.Yes)
            return;

        _progressHistory.Remove(item);
        if (_editingHistoryId == item.Id)
            _editingHistoryId = null;

        SavePersistedData();
        UpdateHistoryView();
        UpdateSummaryView();
        UpdateReportView();
        UpdateHomeSummaryView();
        UpdateAcceptanceView();
        CalculationStatusTextBlock.Text = $"{title} kaydı silindi.";
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
                .Where(x =>
                    string.Equals(x.TenderNumber, selectedRoute.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.RouteNumber, selectedRoute.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(selectedRoute.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal))
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

        if (selectedRoute is null)
        {
            ReportPdfTargetTextBlock.Text = "PDF hedefi: önce bir hat seçin.";
        }
        else
        {
            var targetFolder = OehsOutputPathService.BuildPdfDirectory(
                selectedRoute.TenderNumber,
                selectedRoute.RouteNumber);
            ReportPdfTargetTextBlock.Text = $"PDF hedefi: {targetFolder}";
        }

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
        {
            ProgressContractorTextBlock.Text = "Seçilmedi";
            ProgressRouteRelationTextBlock.Text = "Hat seçiniz.";
            return;
        }

        ProgressContractorTextBlock.Text = string.IsNullOrWhiteSpace(route.ContractorName)
            ? "Firma seçilmemiş"
            : route.ContractorName;
        ProgressRouteRelationTextBlock.Text = $"İhale: {route.TenderNumber} • Yüklenici: {route.ContractorName} • Hat: {route.RouteNumber} — {route.Name}";

        RouteTransportWorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
        DailyPriceTextBox.Text = route.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        GuideDailyPriceTextBox.Text = route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);

        WorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);

        // Rehber iş günü de seçilen hattın varsayılanı olmalı.
        // Aksi halde bir hattan diğerine geçerken önceki hattın rehber günü kalabilir.
        GuideWorkDaysTextBox.Text = route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);

        if (!_editingHistoryId.HasValue)
        {
            var suggested = ProgressPaymentValidationService.SuggestNextProgressPaymentNumber(
                _progressHistory,
                route.TenderNumber,
                route.RouteNumber,
                route.ContractorTaxOrIdentityNumber);

            ProgressNumberTextBox.Text = suggested.ToString(CultureInfo.CurrentCulture);
        }
    }

    private void OutputTenderListBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        UpdateOutputRoutesForContract(OutputTenderListBox.SelectedItem as ContractDraftData);
        UpdateReportView();
        UpdateAcceptanceView();
    }

    private void UpdateOutputRoutesForContract(ContractDraftData? contract)
    {
        var routes = contract is null
            ? Array.Empty<RouteDraft>()
            : _routes.Where(x =>
                string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase))
              .ToArray();

        var previous = ReportRouteComboBox.SelectedItem as RouteDraft;
        ReportRouteComboBox.ItemsSource = routes;
        if (previous is not null)
            ReportRouteComboBox.SelectedItem = routes.FirstOrDefault(x =>
                x.RouteNumber == previous.RouteNumber &&
                string.Equals(x.ContractorTaxOrIdentityNumber, previous.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase));
        if (ReportRouteComboBox.SelectedItem is null && routes.Length > 0)
            ReportRouteComboBox.SelectedIndex = 0;
    }

    private void ReportRouteComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized)
            return;

        UpdateReportView();
        UpdateAcceptanceView();
    }

    private string CurrentContractorName =>
        (ReportRouteComboBox.SelectedItem as RouteDraft)?.ContractorName
        ?? (ProgressRouteComboBox.SelectedItem as RouteDraft)?.ContractorName
        ?? string.Empty;

    private ProgressPaymentHistoryData? GetSelectedProgressLatestPayment()
    {
        var contract = ProgressTenderListBox.SelectedItem as ContractDraftData;
        var route = ProgressRouteComboBox.SelectedItem as RouteDraft;
        if (contract is null || route is null)
            return null;

        return _progressHistory
            .Where(x =>
                string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal))
            .OrderByDescending(x => x.PeriodDate ?? DateOnly.MinValue)
            .ThenByDescending(x => x.CreatedAt)
            .FirstOrDefault();
    }

    private ProgressPaymentHistoryData? GetSelectedRouteLatestPayment()
    {
        var route = ReportRouteComboBox.SelectedItem as RouteDraft;
        IEnumerable<ProgressPaymentHistoryData> query = _progressHistory;

        if (route is not null)
            query = query.Where(x =>
                string.Equals(x.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal));

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
            query = query.Where(x =>
                string.Equals(x.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(
                        OehsIdentityService.NormalizeIdentity(x.ContractorTaxOrIdentityNumber),
                        OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber),
                        StringComparison.Ordinal));

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
        AddFittedColumns(table, 345d, 345d);
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
        AddFittedColumns(table, ThreeColumnWidths);

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
        AddFittedColumns(table, 56d, 36d, 426d, 172d);

        var group = new TableRowGroup();
        const int deductionItemCount = 12;

        var verticalCell = new TableCell
        {
            RowSpan = deductionItemCount,
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

            if (shaded)
            {
                row.Cells.Add(CreateCell(item.A, true, false, true, true));

                var summaryDescriptionCell = CreateCell(item.B, true, false, false, true);
                summaryDescriptionCell.ColumnSpan = 2;
                row.Cells.Add(summaryDescriptionCell);

                row.Cells.Add(CreateCell(item.C, true, true, false, true));
            }
            else
            {
                row.Cells.Add(CreateCell(item.A, false, false, true, false));
                row.Cells.Add(CreateCell(item.B, false, false, false, false));
                row.Cells.Add(CreateCell(item.C, false, true, false, false));
            }

            if (shaded)
            {
                foreach (var cell in row.Cells)
                    cell.Background = System.Windows.Media.Brushes.Gainsboro;
            }

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
        AddFittedColumns(table, 480d, 210d);

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
        AddFittedColumns(table, 500d, 218.11d);

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
        AddFittedColumns(table, FourColumnWidths);

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

        AddFittedColumns(table, ThreeValueWidths);

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
        AddFittedColumns(table, FourColumnWidths);

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

        AddFittedColumns(table, FiveColumnWidths);

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

        AddFittedColumns(table, 359.055d, 359.055d);

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

    private static Paragraph CreateDocumentExampleFooter(string text)
    {
        return new Paragraph(new Run(text))
        {
            FontSize = 7.5,
            TextAlignment = TextAlignment.Center,
            Margin = new Thickness(0, 6, 0, 0)
        };
    }

    private static FlowDocument CreateBaseDocument(string title, string subtitle)
    {
        const double a4Width = PdfLayoutService.A4Width;
        const double a4Height = PdfLayoutService.A4Height;
        const double horizontalMargin = (a4Width - DocumentContentWidth) / 2d;
        const double verticalMargin = 40d;
        const double contentWidth = DocumentContentWidth;
        // A4 sabit: 210 x 297 mm. Yazıcı sürücüsü için yatayda güvenli içerik alanı,
        // dikeyde gereksiz sıkıştırmayı azaltan makul kenar boşlukları.

        var document = new FlowDocument
        {
            PageWidth = a4Width,
            PageHeight = a4Height,
            PagePadding = new Thickness(horizontalMargin, verticalMargin, horizontalMargin, verticalMargin),
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

    private static void AddFittedColumns(Table table, params double[] widths)
    {
        foreach (var width in PdfLayoutService.FitColumns(widths))
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });
    }


    private static bool IsBeforePayment(ProgressPaymentHistoryData item, ProgressPaymentHistoryData current)
    {
        if (current.PeriodDate.HasValue && item.PeriodDate.HasValue)
            return item.PeriodDate.Value < current.PeriodDate.Value;

        return item.CreatedAt < current.CreatedAt;
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

        var combined = BuildCombinedRouteDocuments(documents);
        var pages = OehsPdfExportService.RenderDocumentPages(combined);

        var workArea = SystemParameters.WorkArea;
        var previewWidth = Math.Min(1180d, Math.Max(760d, workArea.Width - 40d));
        var previewHeight = Math.Min(900d, Math.Max(620d, workArea.Height - 60d));

        var previewWindow = new Window
        {
            Title = "OEHS — Belge Önizleme",
            Owner = this,
            Width = previewWidth,
            Height = previewHeight,
            MinWidth = Math.Min(900d, previewWidth),
            MinHeight = Math.Min(620d, previewHeight),
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            Background = System.Windows.Media.Brushes.White
        };

        var root = new Grid { Margin = new Thickness(8) };
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });
        root.RowDefinitions.Add(new RowDefinition { Height = new GridLength(1, GridUnitType.Star) });
        root.RowDefinitions.Add(new RowDefinition { Height = GridLength.Auto });

        var titleBlock = new TextBlock
        {
            Text = "BELGE ÖNİZLEME — A4",
            FontSize = 18,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            Margin = new Thickness(4, 4, 4, 6)
        };
        Grid.SetRow(titleBlock, 0);
        root.Children.Add(titleBlock);

        var image = new Image
        {
            Source = pages[0],
            Stretch = System.Windows.Media.Stretch.None,
            HorizontalAlignment = HorizontalAlignment.Center,
            VerticalAlignment = VerticalAlignment.Top,
            SnapsToDevicePixels = true,
            UseLayoutRounding = true
        };

        var zoom = 1.1;
        var pageIndex = 0;

        var scroll = new ScrollViewer
        {
            HorizontalScrollBarVisibility = ScrollBarVisibility.Auto,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Top,
            Background = System.Windows.Media.Brushes.White,
            Content = image
        };

        Button MakeButton(string text, double width)
        {
            return new Button
            {
                Content = text,
                Width = width,
                Height = 38,
                Margin = new Thickness(3, 0, 3, 0),
                Style = (Style)FindResource("SecondaryButton")
            };
        }

        var previous = MakeButton("◀", 48);
        var zoomOut = MakeButton("−", 48);
        var reset = MakeButton("100%", 72);
        var zoomIn = MakeButton("+", 48);
        var next = MakeButton("▶", 48);
        var close = MakeButton("KAPAT", 105);
        var pageInfo = new TextBlock
        {
            Text = $"1 / {pages.Count} • %110",
            FontSize = 14,
            FontWeight = FontWeights.SemiBold,
            VerticalAlignment = VerticalAlignment.Center,
            Margin = new Thickness(12, 0, 12, 0)
        };

        void RefreshPreview()
        {
            image.Source = pages[pageIndex];
            image.LayoutTransform = new System.Windows.Media.ScaleTransform(zoom, zoom);
            pageInfo.Text = $"{pageIndex + 1} / {pages.Count} • %{Math.Round(zoom * 100):0}";
            scroll.ScrollToHome();
        }

        previous.Click += (_, _) =>
        {
            if (pageIndex > 0)
            {
                pageIndex--;
                RefreshPreview();
            }
        };
        next.Click += (_, _) =>
        {
            if (pageIndex < pages.Count - 1)
            {
                pageIndex++;
                RefreshPreview();
            }
        };
        zoomOut.Click += (_, _) =>
        {
            zoom = Math.Max(0.8, zoom - 0.1);
            RefreshPreview();
        };
        reset.Click += (_, _) =>
        {
            zoom = 1.0;
            RefreshPreview();
        };
        zoomIn.Click += (_, _) =>
        {
            zoom = Math.Min(1.6, zoom + 0.1);
            RefreshPreview();
        };
        close.Click += (_, _) => previewWindow.Close();

        var controls = new StackPanel
        {
            Orientation = Orientation.Horizontal,
            HorizontalAlignment = HorizontalAlignment.Center,
            Margin = new Thickness(4, 8, 4, 2)
        };
        controls.Children.Add(previous);
        controls.Children.Add(zoomOut);
        controls.Children.Add(reset);
        controls.Children.Add(zoomIn);
        controls.Children.Add(pageInfo);
        controls.Children.Add(next);
        controls.Children.Add(close);

        Grid.SetRow(scroll, 1);
        Grid.SetRow(controls, 2);
        root.Children.Add(scroll);
        root.Children.Add(controls);

        previewWindow.Content = root;
        RefreshPreview();
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
        var reportTotals = ProgressPaymentReportService.Calculate(latest, payments);

        var cumulativeServiceAmount = reportTotals.CumulativeServiceAmount;
        var currentPriceDifference = reportTotals.CurrentPriceDifference;
        var cumulativeTotal = reportTotals.CumulativeTotal;
        var previousTotal = reportTotals.PreviousTotal;
        var currentTotal = reportTotals.CurrentTotal;

        var doc = CreateBaseDocument("HAK EDİŞ RAPORU", "");

        doc.Blocks.Add(CreateReportHeaderTable(route, latest));

        doc.Blocks.Add(SectionHeading(
            $"{latest.PeriodDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"} TARİHİNE KADAR YAPILAN İŞİN"));

        var mainRows = new List<(string, string, string)>
        {
            ("A", "Sözleşme Fiyatları İle Yapılan İş", FormatMoney(cumulativeServiceAmount)),
            ("B", "Fiyat Farkı Tutarı", FormatMoney(currentPriceDifference)),
            ("C", "Toplam Tutar  ( A + B )", FormatMoney(cumulativeTotal)),
            ("D", "Bir Önceki Hakedişlerin Toplam Tutarı", FormatMoney(previousTotal)),
            ("E", "Bu Hakedişin Tutarı  ( C - D )", FormatMoney(currentTotal)),
            ("F", "KDV  %10 + %20", FormatMoney(latest.Calculation.Vat)),
            ("G", "Tahakkuk Tutarı", FormatMoney(currentTotal + latest.Calculation.Vat))
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
            ? CurrentContractorName.Trim()
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
        doc.Blocks.Add(CreateDocumentExampleFooter("M.Y.H.B.Y. Örnek No : 4/10"));

        return doc;
    }

    private static int CalculateContractDurationDays(ProgressPaymentHistoryData latest)
    {
        if (!latest.ContractStartDate.HasValue || !latest.ContractEndDate.HasValue)
            return 0;

        var start = latest.ContractStartDate.Value.ToDateTime(TimeOnly.MinValue);
        var end = latest.ContractEndDate.Value.ToDateTime(TimeOnly.MinValue);
        return Math.Max(0, (end - start).Days);
    }

    private static string FormatContractDateAndNumber(ProgressPaymentHistoryData latest)
    {
        var date = latest.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture);
        var number = latest.ContractNumber?.Trim();

        if (string.IsNullOrWhiteSpace(date) && string.IsNullOrWhiteSpace(number))
            return "-";

        if (string.IsNullOrWhiteSpace(number))
            return date!;

        if (string.IsNullOrWhiteSpace(date))
            return number;

        return $"{date} / {number}";
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
            ("Yüklenicinin Adı / Ticari Unvanı", string.IsNullOrWhiteSpace(latest.ContractorName) ? CurrentContractorName : latest.ContractorName),
            ("Sözleşme Bedeli", FormatMoney(latest.Input.ContractAmount)),
            ("İhale Tarihi", latest.TenderDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme Tarihi ve No.su", FormatContractDateAndNumber(latest)),
            ("Sözleşmenin Sayıştayca Tescil Tarihi ve No'su", "Tescile Dahil Değil"),
            ("Tescilin Yükleniciye Tebliğ Tarihi", "Tescile Dahil Değil"),
            ("İşyeri Teslim Tarihi", latest.ContractStartDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşmeye Göre İşin Süresi", $"{CalculateContractDurationDays(latest)} Gün"),
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
        doc.Blocks.Add(CreateDocumentExampleFooter("M.Y.H.B.Y. Örnek No: 4"));
        return doc;
    }

    private FlowDocument BuildKabulTutanağı(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var doc = CreateBaseDocument("HİZMET İŞLERİ KABUL TUTANAĞI", "");

        var tr = new CultureInfo("tr-TR");
        var contractStart = latest.ContractStartDate?.ToDateTime(TimeOnly.MinValue) ?? StartDatePicker.SelectedDate;
        var contractEnd = latest.ContractEndDate?.ToDateTime(TimeOnly.MinValue) ?? EndDatePicker.SelectedDate;
        var duration = Math.Max(0, (contractEnd - contractStart)?.Days ?? 0);

        var periodAnchor = latest.PeriodDate ?? DateOnly.FromDateTime(DateTime.Today);
        var acceptancePeriod = ProgressPaymentAcceptancePeriodService.BuildServicePeriod(periodAnchor);
        var serviceStart = acceptancePeriod.Start;
        var serviceEnd = acceptancePeriod.End;

        var contractAmount = latest.Input.ContractAmount;
        var contractor = string.IsNullOrWhiteSpace(latest.ContractorName)
            ? (string.IsNullOrWhiteSpace(CurrentContractorName)
                ? "Yüklenici bilgisi bulunamadı"
                : CurrentContractorName.Trim())
            : latest.ContractorName.Trim();

        doc.Blocks.Add(CreateDocumentTable(
            ("İşin adı", route.Name),
            ("Yüklenicinin adı/ticari unvanı", contractor),
            ("Sözleşme tarihi", latest.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
            ("Sözleşme bedeli", FormatMoney(contractAmount)),
            ("Sözleşmeye göre işin süresi (takvim günü)", $"{duration} Takvim Günü"),
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
        var rows = new List<(string, string, string, string, string)>();
        var totalA = 0m;
        var totalB = 0m;

        foreach (var month in ProgressPaymentIcmalService.BuildPeriods(latestDate))
        {
            var monthIndex = rows.Count;
            var monthPayment = payments.LastOrDefault(x =>
                x.PeriodDate.HasValue &&
                x.PeriodDate.Value.Year == month.Year &&
                x.PeriodDate.Value.Month == month.Month);

            var a = monthPayment?.Calculation.CombinedServiceAmount ?? 0m;
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

        var previousDeduction = ProgressPaymentIcmalService.FindPreviousDeduction(
            payments,
            latest);
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
        doc.Blocks.Add(CreateDocumentExampleFooter("M.Y.H.B.Y. Örnek No : 4/9"));

        return doc;
    }

    private static FlowDocument BuildCombinedRouteDocuments(IReadOnlyList<(FlowDocument Document, string Title)> documents)
    {
        var combined = CreateBaseDocument("OEHS — 4 BELGE", "Tek önizleme / tek yazdırma");
        combined.Blocks.Clear();

        for (var index = 0; index < documents.Count; index++)
        {
            var section = new Section { BreakPageBefore = index > 0 };
            var source = documents[index].Document;
            while (source.Blocks.Count > 0)
            {
                var block = source.Blocks.FirstBlock!;
                source.Blocks.Remove(block);
                section.Blocks.Add(block);
            }
            combined.Blocks.Add(section);
        }

        return combined;
    }

    private void CreateSaveAllRouteDocumentsButton_Click(object sender, RoutedEventArgs e)
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

        var combined = BuildCombinedRouteDocuments(documents);

        try
        {
            var targetFolder = OehsOutputPathService.BuildPdfDirectory(
                latest.TenderNumber ?? string.Empty,
                latest.RouteNumber ?? string.Empty);

            var targetPath = latest.ProgressPaymentNumber.HasValue && latest.PeriodDate.HasValue
                ? OehsOutputPathService.BuildPdfFilePath(
                    latest.TenderNumber ?? string.Empty,
                    latest.RouteNumber ?? string.Empty,
                    latest.ProgressPaymentNumber.Value,
                    latest.PeriodDate.Value)
                : Path.Combine(targetFolder, "OEHS_Hakediş.pdf");

            Directory.CreateDirectory(targetFolder);
            OehsPdfExportService.ExportFlowDocumentPages(combined, targetPath);
            SaveRouteDocumentsArchiveMetadata(latest);

            CalculationStatusTextBlock.Text =
                $"PDF başarıyla kaydedildi: {Path.GetFileName(targetPath)}";

            ShowPdfSaveInfoDialog(targetFolder, Path.GetFileName(targetPath), targetPath);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"PDF oluşturulurken hata oluştu.\n\n{ex.Message}",
                "OEHS PDF Çıktı Hatası",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
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

    private void ConfigureZeroValueInputs()
    {
        foreach (var textBox in new[]
        {
            WorkDaysTextBox,
            DailyPriceTextBox,
            GuideWorkDaysTextBox,
            GuideDailyPriceTextBox,
            GuideDiscountTextBox,
            GuidePriceDifferenceTextBox,
            TaxDebtTextBox,
            SocialSecurityDebtTextBox,
            MinimumWageSupportTextBox,
            InspectionPenaltyTextBox,
            DiscountTextBox,
            PriceDifferenceTextBox,
            RouteTransportWorkDaysTextBox,
            RouteGuideWorkDaysTextBox,
            RouteGuideDailyPriceTextBox
        })
        {
            textBox.GotFocus += ZeroValueTextBox_GotFocus;
            textBox.LostFocus += ZeroValueTextBox_LostFocus;
        }
    }

    private static void ZeroValueTextBox_GotFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && (textBox.Text.Trim() == "0" || textBox.Text.Trim() == "0,00" || textBox.Text.Trim() == "0.00"))
        {
            textBox.Clear();
            textBox.Select(0, 0);
        }
    }

    private static void ZeroValueTextBox_LostFocus(object sender, RoutedEventArgs e)
    {
        if (sender is TextBox textBox && string.IsNullOrWhiteSpace(textBox.Text))
            textBox.Text = "0";
    }

    private void ClearProgressValidationVisuals()
    {
        foreach (var control in new Control[]
        {
            DailyPriceTextBox, GuideDailyPriceTextBox, DiscountTextBox, GuideDiscountTextBox,
            PriceDifferenceTextBox, GuidePriceDifferenceTextBox, ContractAmountTextBox,
            TaxDebtTextBox, SocialSecurityDebtTextBox, MinimumWageSupportTextBox, InspectionPenaltyTextBox
        })
            ClearInvalid(control);
    }

    private static void MarkProgressInvalid(Control control)
    {
        MarkInvalid(control);
    }

    private void ConfigureContractValidationInputs()
    {
        foreach (var textBox in new[]
        {
            TenderNumberTextBox,
            ContractNumberTextBox,
            WorkNameTextBox,
            ContractAmountTextBox,
            ContractWorkDaysTextBox
        })
        {
            textBox.TextChanged += ContractValidationInput_TextChanged;
        }

        StartDatePicker.SelectedDateChanged += ContractValidationDateChanged;
        EndDatePicker.SelectedDateChanged += ContractValidationDateChanged;
    }

    private void ContractValidationInput_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is Control control)
            ClearInvalid(control);
    }

    private void ContractValidationDateChanged(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is Control control)
            ClearInvalid(control);
    }

    private static void MarkInvalid(Control control)
    {
        control.Background = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(255, 235, 235));
        control.BorderBrush = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(220, 53, 69));
        control.BorderThickness = new Thickness(2);
        control.ToolTip = "Bu alanı doldurun veya düzeltin.";
    }

    private static void ClearInvalid(Control control)
    {
        control.ClearValue(Control.BackgroundProperty);
        control.ClearValue(Control.BorderBrushProperty);
        control.ClearValue(Control.BorderThicknessProperty);
        control.ClearValue(Control.ToolTipProperty);
    }

    private void ClearContractValidationVisuals()
    {
        foreach (var control in new Control[]
        {
            TenderNumberTextBox,
            ContractNumberTextBox,
            WorkNameTextBox,
            ContractAmountTextBox,
            ContractWorkDaysTextBox,
            StartDatePicker,
            EndDatePicker
        })
        {
            ClearInvalid(control);
        }

        ContractValidationBanner.Visibility = Visibility.Collapsed;
        ContractValidationMessageTextBlock.Text = string.Empty;
    }

    private void ShowContractValidation(string message)
    {
        ContractValidationMessageTextBlock.Text = message;
        ContractValidationBanner.Visibility = Visibility.Visible;
        ContractStatusTextBlock.Text = "Kaydetme durduruldu. Kırmızı işaretli alanları tamamlayın.";
        ContractStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(198, 40, 40));
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static decimal RoundMoney(decimal value) =>
        decimal.Round(value, 2, MidpointRounding.AwayFromZero);

    private static string FormatMoney(decimal value) => $"{RoundMoney(value):N2} ₺";
    private static string BuildPdfPrintTitle(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        if (!latest.ProgressPaymentNumber.HasValue || !latest.PeriodDate.HasValue)
            return "OEHS_Hakediş";

        return Path.GetFileNameWithoutExtension(
            OehsFileNameService.BuildProgressPdfFileName(
                latest.TenderNumber ?? string.Empty,
                route.RouteNumber,
                latest.ProgressPaymentNumber.Value,
                latest.PeriodDate.Value));
    }


    private sealed record RouteDraft(
        string RouteNumber,
        string Name,
        decimal DailyPrice,
        int GuideWorkDays = 0,
        decimal GuideDailyPrice = 0m,
        int TransportWorkDays = 0,
        string TenderNumber = "",
        string ContractorTaxOrIdentityNumber = "",
        string ContractorName = "")
    {
        public string TransportWorkDaysText => $"Taşıma İş Günü: {TransportWorkDays}";
        public decimal TransportAnnualPrice => TransportWorkDays * DailyPrice;
        public string TransportAnnualPriceText => $"Taşıma Yıllık: {TransportAnnualPrice:N2} ₺";
        public string DailyPriceText => $"Taşıma Günlük: {DailyPrice:N2} ₺";
        public string GuideDailyPriceText => $"Rehber: {GuideDailyPrice:N2} ₺";
        public string TransportSummaryText => $"Taşıma: {TransportWorkDays} gün × {DailyPrice:N2} ₺";
        public string GuideSummaryText => $"Rehber: {GuideWorkDays} gün × {GuideDailyPrice:N2} ₺";
        public string ServiceSummaryText => $"Taşıma {TransportWorkDays} gün • Rehber {GuideWorkDays} gün";
        public string RelationshipText => $"İhale/Dosya: {TenderNumber} • Yüklenici: {ContractorName}";
        public string DisplayText => string.IsNullOrWhiteSpace(ContractorName)
            ? $"Hat {RouteNumber} — {Name}"
            : $"Hat {RouteNumber} — {Name} • {ContractorName}";
    }

    private sealed record ContractorDraft(
        string Name,
        string TaxOrIdentityNumber,
        string Address = "",
        string Phone = "",
        string Email = "",
        string BankAccountNumber = "",
        string ContactPerson = "");

    private sealed record ArchiveRow(
        string FullPath,
        string DisplayTitle,
        string DetailText,
        string NetPayableText,
        string RevisionText);

    private sealed record HistoryRow(
        Guid Id,
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
