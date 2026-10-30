using System.Diagnostics;
using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private bool _outputRefreshing;

    private bool TryGetOutputRoute(out RouteDraft? route)
    {
        route = ReportRouteComboBox.SelectedItem as RouteDraft;
        return route is not null && OutputTenderListBox.SelectedItem is ContractDraftData contract && IsRouteForContract(route, contract);
    }

    private static bool IsOutputPaymentForRoute(ProgressPaymentHistoryData payment, RouteDraft route) =>
        PaymentOperationsService.RouteKey(payment) == HybridRouteKey(route);

    private ProgressPaymentHistoryData? GetSelectedOutputPayment()
    {
        if (!TryGetOutputRoute(out var route) || ReportItemsControl.SelectedItem is not ReportRow row) return null;
        var current = _progressHistory.FirstOrDefault(payment => payment.Id == row.Payment.Id && IsOutputPaymentForRoute(payment, route!));
        return current == row.Payment ? current : null;
    }

    private static string OutputDirectoryForPayment(ProgressPaymentHistoryData payment)
    {
        var current = OehsOutputPathService.BuildOfficialPdfDirectory(OehsRuntimePaths.DesktopRoot, payment);
        if (Directory.Exists(current) && Directory.EnumerateFiles(current, "*.pdf").Any()) return current;
        var legacy = OehsOutputPathService.BuildPdfDirectoryForContractor(payment.TenderNumber ?? string.Empty,
            payment.ContractorName ?? string.Empty, payment.ContractorTaxOrIdentityNumber ?? string.Empty,
            payment.RouteNumber ?? string.Empty, payment.Input.ProgramType, payment.BatchRouteNumbers, payment.PeriodDate);
        return Directory.Exists(legacy) ? legacy : current;
    }

    private static string OutputPeriodText(ProgressPaymentHistoryData payment) => payment.PeriodDate.HasValue
        ? PanelSearchCulture.TextInfo.ToTitleCase(payment.PeriodDate.Value.ToString("MMMM yyyy", PanelSearchCulture))
        : $"Dönemi belirtilmemiş • {payment.CreatedAt:dd.MM.yyyy}";

    private void RefreshOutputRecords()
    {
        if (_outputRefreshing) return;
        _outputRefreshing = true;
        try
        {
            var previous = (ReportItemsControl.SelectedItem as ReportRow)?.Payment;
            ReportItemsControl.ItemsSource = _reportRows;
            _reportRows.Clear();
            foreach (var payment in GetSelectedRoutePayments().OrderByDescending(p => p.PeriodDate ?? DateOnly.MinValue).ThenByDescending(p => p.CreatedAt))
            {
                var title = $"Hakediş No {payment.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "—"}";
                if (!string.IsNullOrWhiteSpace(payment.BatchDisplayName)) title += $" • {payment.BatchDisplayName}";
                _reportRows.Add(new ReportRow(payment, title, OutputPeriodText(payment), FormatMoney(payment.Calculation.GrossAmount), FormatMoney(payment.Calculation.NetPayable)));
            }
            ReportItemsControl.SelectedItem = _reportRows.FirstOrDefault(row => previous is not null && row.Payment.Id == previous.Id && row.Payment.Input.ProgramType == previous.Input.ProgramType)
                ?? _reportRows.FirstOrDefault();
        }
        finally { _outputRefreshing = false; }
        RefreshOutputSelection();
    }

    private void ReportPayment_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (IsInitialized && !_outputRefreshing && !_suppressSelectionEvents) RefreshOutputSelection();
    }

    private void RefreshOutputSelection(bool clearOperationStatus = true)
    {
        var payment = GetSelectedOutputPayment();
        ReportCountTextBlock.Text = _reportRows.Count.ToString(CultureInfo.CurrentCulture);
        ReportItemsControl.Visibility = _reportRows.Count == 0 ? Visibility.Collapsed : Visibility.Visible;
        ReportGrossTextBlock.Text = FormatMoney(payment?.Calculation.GrossAmount ?? 0m);
        ReportNetTextBlock.Text = FormatMoney(payment?.Calculation.NetPayable ?? 0m);
        ServiceReportSheetNameTextBlock.Text = OfficialTemplateProfile.For(payment?.Input.ProgramType ??
            (ReportRouteComboBox.SelectedItem as RouteDraft)?.ProgramType ?? GetSelectedTransportationProgramType()).ServiceReportSheetName;
        ReportEmptyStateBorder.Visibility = _reportRows.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        ReportEmptyStateTextBlock.Text = TryGetOutputRoute(out _) ? "Bu hatta kayıtlı hakediş yok. Önce Hakediş Oluştur ekranından bir kayıt oluşturun."
            : "Kayıtları görmek için üstten ihale ve hat seçin.";
        if (clearOperationStatus) ReportOperationStatusTextBlock.Text = string.Empty;

        var ready = false;
        if (payment is null)
        {
            ReportDetailTitleTextBlock.Text = "Hakediş seçilmedi";
            ReportDetailMetaTextBlock.Text = "Dönem listesinden çıktı alacağınız kaydı seçin.";
            ReportPdfTargetTextBlock.Text = "Kayıt seçildiğinde PDF klasörü burada gösterilir.";
            ReportPdfTargetTextBlock.ToolTip = null;
            ReportReadinessBorder.Background = Brushes.AliceBlue;
            ReportReadinessTextBlock.Foreground = Brushes.SlateGray;
            ReportReadinessTextBlock.Text = "Belge işlemleri kayıt seçildiğinde kullanılabilir.";
            OpenReportsFolderButton.IsEnabled = false;
        }
        else
        {
            var issues = PaymentIssues(payment);
            ready = issues.Count == 0;
            ReportDetailTitleTextBlock.Text = $"{OutputPeriodText(payment)} • Hakediş No {payment.ProgressPaymentNumber?.ToString(CultureInfo.CurrentCulture) ?? "—"}";
            var program = payment.Input.ProgramType == TransportationProgramType.SpecialEducation ? "Özel Eğitim Taşıma" : "Temel Eğitim/Ortaöğretim Taşıma";
            ReportDetailMetaTextBlock.Text = $"{program}\nYüklenici: {payment.ContractorName ?? "—"}\nHat {payment.RouteNumber}: {payment.RouteName ?? "—"}";
            var folder = OutputDirectoryForPayment(payment);
            ReportPdfTargetTextBlock.Text = $"Klasör: {folder}"; ReportPdfTargetTextBlock.ToolTip = folder;
            OpenReportsFolderButton.IsEnabled = Directory.Exists(folder);
            ReportReadinessBorder.Background = ready ? Brushes.Honeydew : Brushes.MistyRose;
            ReportReadinessTextBlock.Foreground = ready ? Brushes.DarkGreen : Brushes.Firebrick;
            ReportReadinessTextBlock.Text = ready ? "Seçili kayıt için dört belge hazır." : "Önce tamamlayın:\n" + string.Join("\n", issues.Select(issue => "• " + issue.Message));
        }
        PreviewReportsButton.IsEnabled = PrintReportsButton.IsEnabled = SaveReportsPdfsButton.IsEnabled = ready;
    }

    private void ShowOutputStatus(string message, bool warning = false)
    {
        ReportOperationStatusTextBlock.Text = message;
        ReportOperationStatusTextBlock.Foreground = warning ? Brushes.Gold : Brushes.LightSteelBlue;
    }

    private void OpenReportsFolder_Click(object sender, RoutedEventArgs e)
    {
        var payment = GetSelectedOutputPayment();
        if (payment is null) { ShowOutputStatus("Önce bir hakediş kaydı seçin.", warning: true); return; }
        var folder = OutputDirectoryForPayment(payment);
        if (!Directory.Exists(folder)) { ShowOutputStatus("Bu kayıt için klasör henüz oluşturulmadı. Önce birleşik PDF'yi kaydedin.", warning: true); return; }
        try { Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true }); }
        catch (Exception ex) when (ex is System.ComponentModel.Win32Exception or InvalidOperationException)
        { ShowOutputStatus($"Klasör açılamadı: {ex.Message}", warning: true); }
    }
}
