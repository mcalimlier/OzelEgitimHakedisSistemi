using System.Collections.ObjectModel;
using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Text.Json;
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
    private const double DesignHeight = 1042.5;

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
    private const double DocumentContentWidth = 690d;
    private static readonly double[] ThreeColumnWidths = { 58d, 438d, 194d };
    private static readonly double[] ThreeValueWidths = { 45d, 360d, 135d };
    private static readonly double[] FourColumnWidths = { 55d, 308d, 163d, 164d };
    private static readonly double[] FiveColumnWidths = { 40d, 278d, 124d, 124d, 124d };
    private static readonly string[] MainReportHeaders = { "", "Kalem", "Tutar" };
    private static readonly string[] DeductionHeaders = { "", "KESİNTİLER VE MAHSUPLAR", "Tutar" };
    private static readonly string[] ContractHeaders = { "Sözleşme Bedeli", "Sözleşme Artış Onayının Tarihi No.su", "Ek Sözleşme Bedeli", "Toplam Sözleşme Bedeli" };
    private static readonly string[] ExtensionHeaders = { "Tarih", "Sayısı", "Verilen Süre", "İş Bitim Tarihi" };
    private static readonly string[] IcmalHeaders = { "Sıra No", "İşin Cinsi", "İş Tutarı (a)", "Fiyat Farkı Tutarı (b)", "Toplam (a+b)" };



    public MainWindow()
    {
        InitializeComponent();
        ConfigureWindow();
        ConfigureZeroValueInputs();
        ConfigureContractValidationInputs();
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
        ContractorSelectionComboBox.ItemsSource = _contractors;
        HomeTenderListBox.ItemsSource = _contracts;
        RouteTenderListBox.ItemsSource = _contracts;
        ProgressTenderListBox.ItemsSource = _contracts;
        HistoryItemsControl.ItemsSource = _historyRows;
        SummaryItemsControl.ItemsSource = _summaryRows;
        ReportItemsControl.ItemsSource = _reportRows;
        ProgressRouteComboBox.ItemsSource = _routes;
        ReportRouteComboBox.ItemsSource = _routes;
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
        var maxHeight = Math.Min(DesignHeight, workArea.Height * 0.985);
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
            "Contractors" => "Yüklenici Firmalar",
            "History" => "Hakediş Geçmişi",
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
            NavProgress, NavReports, NavHistory, NavAcceptance, NavSettings
        })
        {
            button.Uid = button.Tag?.ToString() == page ? "Active" : string.Empty;
        }
    }

    private ContractorDraft? FindContractor(string taxOrIdentityNumber)
    {
        if (string.IsNullOrWhiteSpace(taxOrIdentityNumber))
            return null;

        return _contractors.FirstOrDefault(x =>
            string.Equals(x.TaxOrIdentityNumber, taxOrIdentityNumber, StringComparison.OrdinalIgnoreCase));
    }

    private ContractDraftData? GetSelectedContract()
    {
        return HomeTenderListBox.SelectedItem as ContractDraftData
            ?? RouteTenderListBox.SelectedItem as ContractDraftData
            ?? ProgressTenderListBox.SelectedItem as ContractDraftData
            ?? _contracts.FirstOrDefault(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase));
    }

    private ContractorDraft? GetSelectedContractor()
    {
        return ContractorSelectionComboBox.SelectedItem as ContractorDraft;
    }

    private string CurrentContractorName =>
        GetSelectedContractor()?.Name
        ?? GetSelectedContract()?.ContractorName
        ?? string.Empty;

    private string CurrentContractorTax =>
        GetSelectedContractor()?.TaxOrIdentityNumber
        ?? GetSelectedContract()?.ContractorTaxOrIdentityNumber
        ?? string.Empty;

    private string CurrentContractorBank =>
        GetSelectedContractor()?.BankAccountNumber
        ?? GetSelectedContract()?.BankAccountNumber
        ?? string.Empty;

    private ContractDraftData? BuildContractFromFormOrNull()
    {
        if (string.IsNullOrWhiteSpace(TenderNumberTextBox.Text) ||
            !TryParseDecimal(ContractAmountTextBox.Text, out var amount))
            return null;

        var contractor = GetSelectedContractor();

        return new ContractDraftData(
            TenderNumberTextBox.Text.Trim(),
            ContractNumberTextBox.Text.Trim(),
            WorkNameTextBox.Text.Trim(),
            contractor?.Name ?? string.Empty,
            amount,
            DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
            DateOnly.FromDateTime(EndDatePicker.SelectedDate ?? DateTime.Today),
            VatExemptCheckBox.IsChecked == true,
            TenderDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(TenderDatePicker.SelectedDate.Value) : null,
            ContractDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(ContractDatePicker.SelectedDate.Value) : null,
            contractor?.TaxOrIdentityNumber ?? ContractorIdentityNumberTextBox.Text.Trim(),
            contractor?.BankAccountNumber ?? BankAccountNumberTextBox.Text.Trim(),
            int.TryParse(ContractWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays) ? workDays : 0,
            TryParseDecimal(ContractTransportDailyPriceTextBox.Text, out var transportPrice) ? transportPrice : 0m);
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

            var contractor = FindContractor(contract.ContractorTaxOrIdentityNumber);
            if (contractor is not null)
            {
                ContractorSelectionComboBox.SelectedItem = contractor;
                ContractorIdentityNumberTextBox.Text = contractor.TaxOrIdentityNumber;
                BankAccountNumberTextBox.Text = contractor.BankAccountNumber;
            }
            else
            {
                ContractorSelectionComboBox.SelectedItem = null;
                ContractorIdentityNumberTextBox.Text = contract.ContractorTaxOrIdentityNumber;
                BankAccountNumberTextBox.Text = contract.BankAccountNumber;
            }
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
        ContractorSelectionComboBox.SelectedItem = null;
        ContractorIdentityNumberTextBox.Clear();
        BankAccountNumberTextBox.Clear();
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
            }
            finally
            {
                _suppressSelectionEvents = false;
            }

            UpdateRouteContractorSelector(contract);
            UpdateProgressContractorAndRoutes(contract);
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
        var contractors = contract is null
            ? Array.Empty<ContractorDraft>()
            : _contractors.Where(x =>
                string.Equals(x.TaxOrIdentityNumber, contract.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
              .ToArray();

        _suppressSelectionEvents = true;
        try
        {
            RouteContractorListBox.ItemsSource = contractors;
            RouteContractorListBox.SelectedIndex = contractors.Length > 0 ? 0 : -1;
        }
        finally
        {
            _suppressSelectionEvents = false;
        }

        UpdateRouteListView();
        RouteSelectedTenderTextBlock.Text = contract is null ? "İhale: seçilmedi" : $"İhale: {contract.TenderNumber}";
        RouteSelectedContractorTextBlock.Text = contractors.Length > 0
            ? $"Yüklenici: {contractors[0].Name}"
            : "Yüklenici: firma seçilmemiş";
    }

    private void UpdateRouteListView()
    {
        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        var contractor = RouteContractorListBox.SelectedItem as ContractorDraft;

        RoutesListBox.ItemsSource = _routes.Where(x =>
            contract is not null &&
            string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            contractor is not null &&
            string.Equals(x.ContractorTaxOrIdentityNumber, contractor.TaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
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

        var contractor = FindContractor(contract.ContractorTaxOrIdentityNumber);
        ProgressContractorTextBlock.Text = contractor?.Name ?? contract.ContractorName;

        var routes = _routes.Where(x =>
            string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.ContractorTaxOrIdentityNumber, contract.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
            .ToArray();

        var previous = ProgressRouteComboBox.SelectedItem as RouteDraft;
        ProgressRouteComboBox.ItemsSource = routes;

        if (previous is not null)
            ProgressRouteComboBox.SelectedItem = routes.FirstOrDefault(x => x.RouteNumber == previous.RouteNumber);
        if (ProgressRouteComboBox.SelectedItem is null && routes.Length > 0)
            ProgressRouteComboBox.SelectedIndex = 0;

        var route = ProgressRouteComboBox.SelectedItem as RouteDraft;
        ProgressRouteRelationTextBlock.Text = route is null
            ? "Bu ihaleye bağlı hat tanımlanmamış."
            : $"İhale: {route.TenderNumber} • Yüklenici: {route.ContractorName} • Hat: {route.RouteNumber} — {route.Name}";
    }

    private void UpdateHomeTenderSummary(ContractDraftData? contract)
    {
        if (contract is null)
        {
            HomeSelectedTenderSummaryTextBlock.Text = "İhale seçiniz.";
            return;
        }

        HomeSelectedTenderSummaryTextBlock.Text =
            $"{contract.TenderNumber} • {contract.WorkName} • {contract.ContractorName} • " +
            $"Sözleşme {FormatMoney(contract.ContractAmount)} • " +
            $"{contract.StartDate:dd.MM.yyyy}–{contract.EndDate:dd.MM.yyyy} • " +
            $"{(_routes.Count(x => string.Equals(x.TenderNumber, contract.TenderNumber, StringComparison.OrdinalIgnoreCase)))} hat";
    }

    private void ShowAlreadyRegistered(string message)
    {
        MessageBox.Show(message, "OEHS", MessageBoxButton.OK, MessageBoxImage.Information);
    }

    private void SaveContractorButton_Click(object sender, RoutedEventArgs e)
    {
        var name = ContractorNameTextBox.Text.Trim();
        var tax = ContractorTaxTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(tax))
        {
            ContractorStatusTextBlock.Text = "Firma adı ve TC / Vergi No zorunludur.";
            return;
        }

        var duplicate = _contractors.FirstOrDefault(x =>
            (string.Equals(x.TaxOrIdentityNumber, tax, StringComparison.OrdinalIgnoreCase) ||
             (string.IsNullOrWhiteSpace(x.TaxOrIdentityNumber) && string.Equals(x.Name, name, StringComparison.OrdinalIgnoreCase))) &&
            !string.Equals(x.TaxOrIdentityNumber, _editingContractorKey, StringComparison.OrdinalIgnoreCase));

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
            var existing = _contractors.FirstOrDefault(x =>
                string.Equals(x.TaxOrIdentityNumber, _editingContractorKey, StringComparison.OrdinalIgnoreCase));

            if (existing is null)
                _contractors.Add(updated);
            else
            {
                var index = _contractors.IndexOf(existing);
                _contractors[index] = updated;
                UpdateReferencesForContractorEdit(existing, updated);
            }
        }

        _editingContractorKey = null;
        ContractorSelectionComboBox.ItemsSource = null;
        ContractorSelectionComboBox.ItemsSource = _contractors;
        ContractorSelectionComboBox.SelectedItem = updated;
        UpdateContractorSummary();
        SavePersistedData();
        UpdateContractSelectors();
        UpdateHomeSummaryView();
        ClearContractorButton_Click(sender, e);
        ContractorStatusTextBlock.Text = $"Firma kaydedildi: {name}.";
    }

    private void EditContractorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ContractorDraft contractor })
            return;

        _editingContractorKey = contractor.TaxOrIdentityNumber;
        ContractorNameTextBox.Text = contractor.Name;
        ContractorTaxTextBox.Text = contractor.TaxOrIdentityNumber;
        ContractorAddressTextBox.Text = contractor.Address;
        ContractorPhoneTextBox.Text = contractor.Phone;
        ContractorEmailTextBox.Text = contractor.Email;
        ContractorBankTextBox.Text = contractor.BankAccountNumber;
        ContractorContactTextBox.Text = contractor.ContactPerson;
        ContractorStatusTextBlock.Text = $"Düzenleme modu: {contractor.Name}.";
    }

    private void DeleteContractorButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: ContractorDraft contractor })
            return;

        var usedByContract = _contracts.Any(x =>
            string.Equals(x.ContractorTaxOrIdentityNumber, contractor.TaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase));

        if (usedByContract)
        {
            ShowAlreadyRegistered($"'{contractor.Name}' kayıtlı ihalelerde kullanılıyor. Önce ilişkili ihaleleri düzeltin; firma silinmedi.");
            return;
        }

        _contractors.Remove(contractor);
        SavePersistedData();
        UpdateContractorSummary();
        UpdateContractSelectors();
        UpdateHomeSummaryView();
        ContractorStatusTextBlock.Text = $"Firma silindi: {contractor.Name}.";
    }

    private void ClearContractorButton_Click(object sender, RoutedEventArgs e)
    {
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
        for (var i = 0; i < _contracts.Count; i++)
        {
            var contract = _contracts[i];
            if (string.Equals(contract.ContractorTaxOrIdentityNumber, oldContractor.TaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
            {
                _contracts[i] = contract with
                {
                    ContractorName = updated.Name,
                    ContractorTaxOrIdentityNumber = updated.TaxOrIdentityNumber,
                    BankAccountNumber = updated.BankAccountNumber
                };
            }
        }

        for (var i = 0; i < _routes.Count; i++)
        {
            var route = _routes[i];
            if (string.Equals(route.ContractorTaxOrIdentityNumber, oldContractor.TaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
            {
                _routes[i] = route with
                {
                    ContractorTaxOrIdentityNumber = updated.TaxOrIdentityNumber,
                    ContractorName = updated.Name
                };
            }
        }

        for (var i = 0; i < _progressHistory.Count; i++)
        {
            var item = _progressHistory[i];
            if (string.Equals(item.ContractorTaxOrIdentityNumber, oldContractor.TaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(item.ContractorName, oldContractor.Name, StringComparison.OrdinalIgnoreCase))
            {
                var updatedHistory = item with
                {
                    ContractorName = updated.Name,
                    BankAccountNumber = updated.BankAccountNumber,
                    ContractorTaxOrIdentityNumber = updated.TaxOrIdentityNumber
                };
                RecalculateHistoryItem(i, item.Input, updatedHistory);
            }
        }
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
            HomeContractorTextBlock.Text = contract.ContractorName;
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
            ? _progressHistory
            : _progressHistory.Where(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase)).ToList();

        HomeContractStatusTextBlock.Text = contract is null ? "İhale seçilmedi" : "KAYITLI";
        HomeRouteCountTextBlock.Text = contract is null
            ? "0"
            : _routes.Count(x => string.Equals(x.TenderNumber, tender, StringComparison.OrdinalIgnoreCase)).ToString(CultureInfo.CurrentCulture);

        HomeWorkNameTextBlock.Text = contract?.WorkName ?? "—";
        HomeContractorTextBlock.Text = contract?.ContractorName ?? "—";
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

        ContractSummaryContractorTextBlock.Text = string.IsNullOrWhiteSpace(CurrentContractorName)
            ? "Bekleniyor" : CurrentContractorName;

        ContractSummaryIdentityTextBlock.Text = string.IsNullOrWhiteSpace(CurrentContractorTax) ? "—" : CurrentContractorTax;
        ContractSummaryAmountTextBlock.Text = TryParseDecimal(ContractAmountTextBox.Text, out var summaryAmount)
            ? FormatMoney(summaryAmount) : FormatMoney(0);
        ContractSummaryBankTextBlock.Text = string.IsNullOrWhiteSpace(CurrentContractorBank) ? "—" : CurrentContractorBank;

        var workDaysText = int.TryParse(ContractWorkDaysTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var workDays)
            ? workDays.ToString(CultureInfo.CurrentCulture) : "0";
        var transportPriceText = TryParseDecimal(ContractTransportDailyPriceTextBox.Text, out var transportPrice)
            ? FormatMoney(transportPrice) : FormatMoney(0);

        ContractSummaryWorkDaysTextBlock.Text = $"{workDaysText} gün • {transportPriceText}/gün";
        ContractSummaryVatTextBlock.Text =
            $"{(VatExemptCheckBox.IsChecked == true ? "KDV İstisnası" : "KDV Uygulanır")} • {_routes.Count(x => string.Equals(x.TenderNumber, TenderNumberTextBox.Text.Trim(), StringComparison.OrdinalIgnoreCase))} hat";
    }

    private void ContractorSelectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (_suppressSelectionEvents)
            return;

        if (ContractorSelectionComboBox.SelectedItem is ContractorDraft contractor)
        {
            ContractorIdentityNumberTextBox.Text = contractor.TaxOrIdentityNumber;
            BankAccountNumberTextBox.Text = contractor.BankAccountNumber;
        }

        UpdateContractSummaryView();
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
            contractor!.TaxOrIdentityNumber,
            contractor.Name);

        var duplicate = _routes.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, proposed.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.ContractorTaxOrIdentityNumber, proposed.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase) &&
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
                _routes[index] = proposed;
                UpdateHistoryForRouteEdit(old, proposed);
            }
            _editingRoute = null;
        }

        SavePersistedData();
        UpdateRouteSummary();
        UpdateRouteListView();
        UpdateProgressContractorAndRoutes(contract);
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        ClearRouteButton_Click(sender, e);
        RouteStatusTextBlock.Text = $"Hat kaydedildi: {routeNumber} — {routeName}.";
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

        _routes.Remove(route);
        SavePersistedData();
        UpdateRouteSummary();
        UpdateRouteListView();
        UpdateHomeSummaryView();
        RouteStatusTextBlock.Text = $"Hat silindi: {route.RouteNumber}.";
    }

    private void EditRouteButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button { Tag: RouteDraft route })
            return;

        _editingRoute = route;

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
        RouteStatusTextBlock.Text = $"Düzenleme modu: {route.TenderNumber} / {route.RouteNumber}.";
    }

    private void ClearRouteButton_Click(object sender, RoutedEventArgs e)
    {
        _editingRoute = null;
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

    private void SaveContractButton_Click(object sender, RoutedEventArgs e)
    {
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
        if (GetSelectedContractor() is null)
            errors.Add("Yüklenici Firma");

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

        var contractor = GetSelectedContractor()!;
        var proposedContract = new ContractDraftData(
            TenderNumberTextBox.Text.Trim(),
            ContractNumberTextBox.Text.Trim(),
            WorkNameTextBox.Text.Trim(),
            contractor.Name,
            contractAmount,
            DateOnly.FromDateTime(StartDatePicker.SelectedDate ?? DateTime.Today),
            DateOnly.FromDateTime(EndDatePicker.SelectedDate ?? DateTime.Today),
            VatExemptCheckBox.IsChecked == true,
            TenderDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(TenderDatePicker.SelectedDate.Value) : null,
            ContractDatePicker.SelectedDate.HasValue ? DateOnly.FromDateTime(ContractDatePicker.SelectedDate.Value) : null,
            contractor.TaxOrIdentityNumber,
            contractor.BankAccountNumber,
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
                _contracts[index] = proposedContract;

                foreach (var route in _routes.Where(x => string.Equals(x.TenderNumber, oldTender, StringComparison.OrdinalIgnoreCase)).ToArray())
                {
                    var routeIndex = _routes.IndexOf(route);
                    _routes[routeIndex] = route with
                    {
                        TenderNumber = proposedContract.TenderNumber,
                        ContractorTaxOrIdentityNumber = proposedContract.ContractorTaxOrIdentityNumber,
                        ContractorName = proposedContract.ContractorName,
                        DailyPrice = proposedContract.TransportDailyPrice
                    };
                }

                RecalculateHistoriesForTenderChange(oldContract, proposedContract);
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
        ContractStatusTextBlock.Text = $"İhale kaydedildi: {proposedContract.TenderNumber}.";
    }

    private void SavePersistedData()
    {
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

        _dataStore.Save(data);
    }

    private void ClearContractButton_Click(object sender, RoutedEventArgs e)
    {
        _editingContractTender = null;
        ClearContractFormOnly();
        UpdateContractSummaryView();
        ContractStatusTextBlock.Text = "Yeni ihale/dosya kaydı için alanlar temizlendi.";
    }

    private static string OehsDesktopRoot =>
        Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.DesktopDirectory), "OzelEgitimHakedisSistemi");

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

    private static void SaveProgressArchive(ProgressPaymentHistoryData item)
    {
        var archiveFolder = Path.Combine(OehsDesktopRoot, "Arsiv", "Hakedisler");
        Directory.CreateDirectory(archiveFolder);

        var filePath = Path.Combine(archiveFolder, BuildArchiveKey(item) + ".json");
        var json = JsonSerializer.Serialize(item, JsonWriteOptions);
        File.WriteAllText(filePath, json);
    }

    private static void SaveFlowDocumentAsRtf(FlowDocument document, string filePath)
    {
        using var memory = new MemoryStream();
        var range = new TextRange(document.ContentStart, document.ContentEnd);
        range.Save(memory, System.Windows.DataFormats.Rtf);

        var rtfBytes = memory.ToArray();
        var marker = System.Text.Encoding.ASCII.GetBytes(@"\rtf1");
        var a4Controls = System.Text.Encoding.ASCII.GetBytes(@"\paperw11907\paperh16840\margl777\margr777\margt777\margb777");

        var markerIndex = rtfBytes.AsSpan().IndexOf(marker);
        if (markerIndex < 0)
        {
            File.WriteAllBytes(filePath, rtfBytes);
            return;
        }

        using var stream = File.Create(filePath);
        stream.Write(rtfBytes, 0, markerIndex + marker.Length);
        stream.Write(a4Controls, 0, a4Controls.Length);
        stream.Write(rtfBytes, markerIndex + marker.Length, rtfBytes.Length - markerIndex - marker.Length);
    }

    private string SaveRouteDocumentsArchive(RouteDraft route, ProgressPaymentHistoryData latest)
    {
        var folderName = BuildArchiveKey(latest);
        var documentFolder = Path.Combine(OehsDesktopRoot, "Belgeler", folderName);
        Directory.CreateDirectory(documentFolder);

        // Eski sürümden kalan Excel arşivi varsa yeni Word arşivine geçerken temizle.
        var obsoleteExcelPath = Path.Combine(documentFolder, "OEHS_Hakediş_Belgeleri.xlsx");
        if (File.Exists(obsoleteExcelPath))
            File.Delete(obsoleteExcelPath);

        var wordFiles = new[]
        {
            (Name: "Hakediş Raporu.rtf", Document: BuildHakedişRaporu(route, latest)),
            (Name: "Hizmet İşleri Hakediş Raporu.rtf", Document: BuildHizmetIsleriHakedisRaporu(route, latest)),
            (Name: "Hizmet İşleri Kabul Tutanağı.rtf", Document: BuildKabulTutanağı(route, latest)),
            (Name: "Hakediş İcmali.rtf", Document: BuildHakedişIcmali(route, latest))
        };

        foreach (var word in wordFiles)
            SaveFlowDocumentAsRtf(word.Document, Path.Combine(documentFolder, word.Name));

        var metadataPath = Path.Combine(documentFolder, "OEHS_Hakediş_Bilgi.json");
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
            WordFiles = wordFiles.Select(x => Path.Combine(documentFolder, x.Name)).ToArray(),
            SavedAt = DateTime.Now
        }, JsonWriteOptions);
        File.WriteAllText(metadataPath, metadata);

        return documentFolder;
    }

    private void RecalculateHistoryItem(int index, ProgressPaymentInput input, ProgressPaymentHistoryData metadataSource)
    {
        var result = ProgressPaymentCalculator.Calculate(input);
        _progressHistory[index] = metadataSource with
        {
            Input = input,
            Calculation = result
        };
        SaveProgressArchive(_progressHistory[index]);
    }

    private void RecalculateHistoriesForTenderChange(ContractDraftData oldContract, ContractDraftData updatedContract)
    {
        for (var i = 0; i < _progressHistory.Count; i++)
        {
            var item = _progressHistory[i];
            if (!string.Equals(item.TenderNumber, oldContract.TenderNumber, StringComparison.OrdinalIgnoreCase))
                continue;

            var input = item.Input with
            {
                ContractAmount = updatedContract.ContractAmount,
                IsVatExempt = updatedContract.VatExempt
            };
            var contractor = FindContractor(updatedContract.ContractorTaxOrIdentityNumber);
            var updated = item with
            {
                ContractorName = updatedContract.ContractorName,
                TenderNumber = updatedContract.TenderNumber,
                ContractNumber = updatedContract.ContractNumber,
                TenderDate = updatedContract.TenderDate,
                ContractDate = updatedContract.ContractDate,
                ContractStartDate = updatedContract.StartDate,
                ContractEndDate = updatedContract.EndDate,
                BankAccountNumber = contractor?.BankAccountNumber ?? updatedContract.BankAccountNumber,
                ContractorTaxOrIdentityNumber = updatedContract.ContractorTaxOrIdentityNumber
            };
            RecalculateHistoryItem(i, input, updated);
        }
    }

    private void UpdateHistoryForRouteEdit(RouteDraft oldRoute, RouteDraft updatedRoute)
    {
        for (var i = 0; i < _progressHistory.Count; i++)
        {
            var item = _progressHistory[i];
            if (!string.Equals(item.TenderNumber, oldRoute.TenderNumber, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(item.RouteNumber, oldRoute.RouteNumber, StringComparison.OrdinalIgnoreCase) ||
                !string.Equals(item.ContractorTaxOrIdentityNumber, oldRoute.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase))
                continue;

            var input = item.Input with
            {
                WorkDays = updatedRoute.TransportWorkDays,
                DailyPrice = updatedRoute.DailyPrice,
                GuideWorkDays = updatedRoute.GuideWorkDays,
                GuideDailyPrice = updatedRoute.GuideDailyPrice
            };
            var updated = item with
            {
                RouteNumber = updatedRoute.RouteNumber,
                RouteName = updatedRoute.Name,
                ContractorName = updatedRoute.ContractorName,
                ContractorTaxOrIdentityNumber = updatedRoute.ContractorTaxOrIdentityNumber
            };
            RecalculateHistoryItem(i, input, updated);
        }
    }

    private void CalculateProgressButton_Click(object sender, RoutedEventArgs e)
    {
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

        var selectedContract = ProgressTenderListBox.SelectedItem as ContractDraftData;
        if (selectedContract is null)
        {
            ShowAlreadyRegistered("Önce İhale / Dosya No seçin.");
            return;
        }

        try
        {
            RequireSelectedRoute();
            var selectedRoute = ProgressRouteComboBox.SelectedItem as RouteDraft;
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
                !selectedContract.VatExempt,
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

            int? progressNumber = null;
            if (int.TryParse(ProgressNumberTextBox.Text, NumberStyles.Integer, CultureInfo.CurrentCulture, out var parsedProgressNumber) &&
                parsedProgressNumber > 0)
                progressNumber = parsedProgressNumber;

            var periodDate = ProgressPeriodDatePicker.SelectedDate is DateTime selectedPeriodDate
                ? DateOnly.FromDateTime(selectedPeriodDate)
                : (DateOnly?)null;

            var contractor = FindContractor(selectedContract.ContractorTaxOrIdentityNumber);

            var historyItem = new ProgressPaymentHistoryData(
                _editingHistoryId ?? Guid.NewGuid(),
                _editingHistoryId.HasValue ? _progressHistory.First(x => x.Id == _editingHistoryId.Value).CreatedAt : DateTime.Now,
                input,
                result,
                progressNumber,
                periodDate,
                selectedRoute.RouteNumber,
                selectedRoute.Name,
                selectedContract.WorkName,
                selectedContract.ContractorName,
                selectedContract.TenderNumber,
                selectedContract.ContractNumber,
                selectedContract.TenderDate,
                selectedContract.ContractDate,
                selectedContract.StartDate,
                selectedContract.EndDate,
                contractor?.BankAccountNumber ?? selectedContract.BankAccountNumber,
                selectedContract.ContractorTaxOrIdentityNumber);

            if (_editingHistoryId.HasValue)
            {
                var editIndex = _progressHistory.IndexOf(_progressHistory.First(x => x.Id == _editingHistoryId.Value));
                _progressHistory[editIndex] = historyItem;
                _editingHistoryId = null;
            }
            else
            {
                var duplicate = _progressHistory.FirstOrDefault(x =>
                    string.Equals(x.TenderNumber, selectedContract.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.ContractorTaxOrIdentityNumber, selectedContract.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase) &&
                    string.Equals(x.RouteNumber, selectedRoute.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
                    x.ProgressPaymentNumber == progressNumber &&
                    x.PeriodDate == periodDate &&
                    x.Input.Equals(input));

                if (duplicate is not null)
                {
                    ShowAlreadyRegistered($"Hakediş No {progressNumber?.ToString(CultureInfo.CurrentCulture) ?? "-"} / {selectedContract.TenderNumber} / Hat {selectedRoute.RouteNumber} zaten kayıtlı. Mükerrer kayıt oluşturulmadı.");
                    return;
                }

                _progressHistory.Add(historyItem);
            }

            SavePersistedData();
            SaveProgressArchive(historyItem);
            UpdateHistoryView();
            UpdateSummaryView();
            UpdateReportView();
            UpdateHomeSummaryView();
            UpdateAcceptanceView();

            CalculationStatusTextBlock.Foreground = new System.Windows.Media.SolidColorBrush(System.Windows.Media.Color.FromRgb(36, 104, 216));
            CalculationStatusTextBlock.FontWeight = FontWeights.SemiBold;
            CalculationStatusTextBlock.Text = "Hesaplama başarıyla tamamlandı ve kayıt oluşturuldu.";

            NavigationButton_Click(NavHome, e);
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
        Path.Combine(OehsDesktopRoot, "Belgeler", BuildArchiveKey(item));

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

    private void EditHistoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { Tag: HistoryRow row })
            return;

        var item = _progressHistory.FirstOrDefault(x => x.Id == row.Id);
        if (item is null)
            return;

        var target = AskHistoryEditTarget(item);
        switch (target)
        {
            case "Contract":
                BeginContractHistoryEdit(item);
                break;
            case "Contractor":
                BeginContractorHistoryEdit(item);
                break;
            case "Route":
                BeginRouteHistoryEdit(item);
                break;
            case "Progress":
                BeginProgressHistoryEdit(item);
                break;
        }
    }

    private string? AskHistoryEditTarget(ProgressPaymentHistoryData item)
    {
        string? selected = null;
        var dialog = new Window
        {
            Title = "Hakediş Düzenleme",
            Owner = this,
            Width = 470,
            Height = 360,
            WindowStartupLocation = WindowStartupLocation.CenterOwner,
            ResizeMode = ResizeMode.NoResize,
            Background = System.Windows.Media.Brushes.White
        };

        var panel = new StackPanel { Margin = new Thickness(24) };
        panel.Children.Add(new TextBlock
        {
            Text = "Hangi bölümde düzeltme yapmak istiyorsunuz?",
            FontSize = 19,
            FontWeight = FontWeights.Bold,
            Foreground = System.Windows.Media.Brushes.DarkSlateGray,
            Margin = new Thickness(0, 0, 0, 6)
        });
        panel.Children.Add(new TextBlock
        {
            Text = $"İhale/Dosya: {item.TenderNumber ?? "-"} • Hat: {item.RouteNumber ?? "-"} • Hakediş: {item.ProgressPaymentNumber?.ToString() ?? "-"}",
            FontSize = 12.5,
            Foreground = System.Windows.Media.Brushes.DimGray,
            TextWrapping = TextWrapping.Wrap,
            Margin = new Thickness(0, 0, 0, 16)
        });

        foreach (var option in new[]
        {
            ("İhale Bilgileri", "Contract"),
            ("Yüklenici Firma Bilgileri", "Contractor"),
            ("Hat Bilgileri", "Route"),
            ("Hakediş Oluştur", "Progress")
        })
        {
            var button = new Button
            {
                Content = option.Item1,
                Width = 390,
                Height = 42,
                Margin = new Thickness(0, 4, 0, 4),
                Style = (Style)FindResource("ModernButton")
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
        var contract = _contracts.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase));
        if (contract is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu ihale kaydı bulunamadı.");
            return;
        }

        _editingContractTender = contract.TenderNumber;
        ApplyContractToForm(contract);
        NavigationButton_Click(NavContract, new RoutedEventArgs(Button.ClickEvent, NavContract));
        ContractStatusTextBlock.Text = "Hakedişten gelen düzenleme: yalnızca İhale Bilgileri alanlarını düzeltin ve KAYDET'e basın.";
    }

    private void BeginContractorHistoryEdit(ProgressPaymentHistoryData item)
    {
        var contractor = FindContractor(item.ContractorTaxOrIdentityNumber ?? string.Empty);
        if (contractor is null)
        {
            contractor = _contractors.FirstOrDefault(x => string.Equals(x.Name, item.ContractorName, StringComparison.OrdinalIgnoreCase));
        }

        if (contractor is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu yüklenici firma bulunamadı.");
            return;
        }

        _editingContractorKey = contractor.TaxOrIdentityNumber;
        ContractorNameTextBox.Text = contractor.Name;
        ContractorTaxTextBox.Text = contractor.TaxOrIdentityNumber;
        ContractorAddressTextBox.Text = contractor.Address;
        ContractorPhoneTextBox.Text = contractor.Phone;
        ContractorEmailTextBox.Text = contractor.Email;
        ContractorBankTextBox.Text = contractor.BankAccountNumber;
        ContractorContactTextBox.Text = contractor.ContactPerson;
        NavigationButton_Click(NavContractors, new RoutedEventArgs(Button.ClickEvent, NavContractors));
        ContractorStatusTextBlock.Text = "Hakedişten gelen düzenleme: firma bilgilerini düzeltin ve KAYDET'e basın.";
    }

    private void BeginRouteHistoryEdit(ProgressPaymentHistoryData item)
    {
        var route = _routes.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.RouteNumber, item.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.ContractorTaxOrIdentityNumber, item.ContractorTaxOrIdentityNumber, StringComparison.OrdinalIgnoreCase));

        if (route is null)
        {
            route = _routes.FirstOrDefault(x =>
                string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
                string.Equals(x.RouteNumber, item.RouteNumber, StringComparison.OrdinalIgnoreCase));
        }

        if (route is null)
        {
            ShowAlreadyRegistered("Bu hakedişin bağlı olduğu hat bulunamadı.");
            return;
        }

        _editingRoute = route;
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
        NavigationButton_Click(NavLines, new RoutedEventArgs(Button.ClickEvent, NavLines));
        RouteStatusTextBlock.Text = "Hakedişten gelen düzenleme: hat bilgilerini düzeltin ve KAYDET'e basın.";
    }

    private void BeginProgressHistoryEdit(ProgressPaymentHistoryData item)
    {
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
        ProgressRouteComboBox.SelectedItem = _routes.FirstOrDefault(x =>
            string.Equals(x.TenderNumber, item.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
            string.Equals(x.RouteNumber, item.RouteNumber, StringComparison.OrdinalIgnoreCase));

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

        NavigationButton_Click(NavProgress, new RoutedEventArgs(Button.ClickEvent, NavProgress));
        CalculationStatusTextBlock.Text = "Düzenleme modu: yalnızca hakediş girdilerini düzeltin ve HAKEDİŞİ HESAPLA'ya basın.";
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

        RouteTransportWorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
        DailyPriceTextBox.Text = route.DailyPrice.ToString("N2", CultureInfo.CurrentCulture);
        GuideDailyPriceTextBox.Text = route.GuideDailyPrice.ToString("N2", CultureInfo.CurrentCulture);

        WorkDaysTextBox.Text = route.TransportWorkDays.ToString(CultureInfo.CurrentCulture);

        if (string.IsNullOrWhiteSpace(GuideWorkDaysTextBox.Text) || GuideWorkDaysTextBox.Text == "0")
            GuideWorkDaysTextBox.Text = route.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
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
        table.Columns.Add(new TableColumn { Width = new GridLength(345) });
        table.Columns.Add(new TableColumn { Width = new GridLength(345) });
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
        foreach (var width in ThreeColumnWidths)
            table.Columns.Add(new TableColumn { Width = new GridLength(width) });

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
        table.Columns.Add(new TableColumn { Width = new GridLength(56) });
        table.Columns.Add(new TableColumn { Width = new GridLength(36) });
        table.Columns.Add(new TableColumn { Width = new GridLength(426) });
        table.Columns.Add(new TableColumn { Width = new GridLength(172) });

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
        table.Columns.Add(new TableColumn { Width = new GridLength(480) });
        table.Columns.Add(new TableColumn { Width = new GridLength(210) });

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
        table.Columns.Add(new TableColumn { Width = new GridLength(500) });
        table.Columns.Add(new TableColumn { Width = new GridLength(218.11) });

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

        table.Columns.Add(new TableColumn { Width = new GridLength(359.055) });
        table.Columns.Add(new TableColumn { Width = new GridLength(359.055) });

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
        const double a4Width = 793.7008;
        const double a4Height = 1122.5197;
        const double margin = 51.8504;
        const double contentWidth = DocumentContentWidth;
        // A4 sabit: 210 x 297 mm. Yaklaşık 13,7 mm güvenli kenar boşluğu; tablo içeriği 690 pt.

        var document = new FlowDocument
        {
            PageWidth = a4Width,
            PageHeight = a4Height,
            PagePadding = new Thickness(margin, margin, margin, margin),
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

        // A4 yazdırma kalıbı sabittir: 210 x 297 mm ve 1 cm kenar boşluğu.

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

        var combined = BuildCombinedRouteDocuments(documents);
        var viewer = new System.Windows.Controls.FlowDocumentScrollViewer
        {
            Document = combined,
            HorizontalScrollBarVisibility = ScrollBarVisibility.Disabled,
            VerticalScrollBarVisibility = ScrollBarVisibility.Auto,
            HorizontalContentAlignment = HorizontalAlignment.Center,
            VerticalContentAlignment = VerticalAlignment.Top,
            Margin = new Thickness(4),
            IsToolBarVisible = true
        };
        Grid.SetRow(viewer, 1);
        root.Children.Add(viewer);

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

        var doc = CreateBaseDocument("HAK EDİŞ RAPORU", "");

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
            ("Sözleşme Tarihi ve No.su", latest.ContractDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? StartDatePicker.SelectedDate?.ToString("dd.MM.yyyy", CultureInfo.CurrentCulture) ?? "-"),
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

        // Örnekteki kabul dönemi, seçilen hakediş tarihini takip eden takvim ayıdır.
        // Böylece 29.09.2026 tarihli kayıt için metin 01.10.2026 / 31.10.2026 şeklinde oluşur.
        var periodAnchor = latest.PeriodDate ?? DateOnly.FromDateTime(DateTime.Today);
        var serviceMonth = periodAnchor.AddMonths(1);
        var serviceStart = new DateOnly(serviceMonth.Year, serviceMonth.Month, 1);
        var serviceEnd = serviceStart.AddMonths(1).AddDays(-1);

        var contractAmount = latest.Input.ContractAmount;
        var contractor = string.IsNullOrWhiteSpace(latest.ContractorName)
            ? (string.IsNullOrWhiteSpace(CurrentContractorName)
                ? "S.S. Selçikler Min. Kar. Yolcu Taşıma Kooperatifi"
                : CurrentContractorName.Trim())
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

        try
        {
            var savedFolder = SaveRouteDocumentsArchive(route, latest);
            CalculationStatusTextBlock.Text = $"4 Word belgesi A4 uyumlu olarak arşivlendi: {savedFolder}";
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Word arşivi oluşturulurken hata oluştu. Yazdırma işlemi yine devam edecek.\n\n{ex.Message}",
                "OEHS Arşiv Uyarısı",
                MessageBoxButton.OK,
                MessageBoxImage.Warning);
        }

        var combined = BuildCombinedRouteDocuments(documents);
        TryShowDocument(combined, "OEHS — 4 Belge Tek Yazdırma");
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

    private static string FormatMoney(decimal value) => $"{value:N2} ₺";

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
        public string DisplayText => $"Hat {RouteNumber} — {Name}";
    }

    private sealed record ContractorDraft(
        string Name,
        string TaxOrIdentityNumber,
        string Address = "",
        string Phone = "",
        string Email = "",
        string BankAccountNumber = "",
        string ContactPerson = "");

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
