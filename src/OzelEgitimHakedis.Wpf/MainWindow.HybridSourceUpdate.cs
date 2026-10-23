using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private List<HybridPaymentDraftData>? _savedHybridPaymentDrafts;
    private List<HybridPaymentDraftData> SavedHybridPaymentDrafts => _savedHybridPaymentDrafts ??= new();

    private static string HybridRouteKey(RouteDraft route) => OehsIdentityService.BuildRouteKey(
        route.TenderNumber, route.RouteNumber, route.ContractorTaxOrIdentityNumber,
        route.ContractNumber, route.ProgramType);

    private DateOnly? HybridPeriodMonth => ProgressPeriodMonthComboBox.SelectedItem is ProgressPaymentMonthOption month
        ? new DateOnly(month.StartDate.Year, month.StartDate.Month, 1) : null;

    private void RestoreSavedHybridInput(MultiRoutePaymentDraftRow row)
    {
        if (HybridPeriodMonth is not { } month) return;
        var saved = SavedHybridPaymentDrafts.LastOrDefault(draft =>
            draft.PeriodMonth == month && string.Equals(draft.RouteKey, row.RouteKey, StringComparison.Ordinal));
        if (saved is not null) row.LoadInput(saved.Input, saved.MinimumWageSupportEnabled);
    }

    private void RefreshHybridUpdateButton()
    {
        if (UpdateHybridRoutesButton is null) return;
        UpdateHybridRoutesButton.IsEnabled = !IsArchiveEditing && !_editingHistoryId.HasValue &&
            _multiRoutePaymentRows.Any(row => row.HasChanges);
    }

    private void SyncHybridSelectedRowFields()
    {
        if (IsArchiveEditing || _editingHistoryId.HasValue || _multiRoutePaymentRows.Count != 1) return;
        var row = _multiRoutePaymentRows[0];
        WorkDaysTextBox.Text = row.WorkDays.ToString(CultureInfo.CurrentCulture);
        DailyPriceTextBox.Text = FormatCalculationInput(row.DailyPrice);
        GuideWorkDaysTextBox.Text = row.GuideWorkDays.ToString(CultureInfo.CurrentCulture);
        GuideDailyPriceTextBox.Text = FormatCalculationInput(row.GuideDailyPrice);
    }

    private void UpdateHybridRoutesButton_Click(object sender, RoutedEventArgs e)
    {
        if (!TryCommitHybridGrid()) return;
        RefreshHybridUpdateButton();
        if (!UpdateHybridRoutesButton.IsEnabled) return;
        var changed = _multiRoutePaymentRows.Where(row => row.HasChanges).ToArray();
        var message = string.Join(Environment.NewLine, changed.Select(row => row.RouteDisplayText));
        var confirmed = MessageBox.Show(
            "Aşağıdaki hatların kaynak bilgileri ve seçilen aya ait tablo girdileri kaydedilsin mi?\n\n" +
            message + "\n\nÖnceki resmî hakedişler ve arşivler değişmez. Vazgeçerseniz tablo düzenlemeleri korunur.",
            "Tablo değişikliklerini kaydet", MessageBoxButton.YesNo, MessageBoxImage.Question,
            MessageBoxResult.No) == MessageBoxResult.Yes;
        ApplyHybridRouteChanges(confirmed);
    }

    private bool ApplyHybridRouteChanges(bool confirmed)
    {
        RefreshHybridUpdateButton();
        if (!confirmed || !UpdateHybridRoutesButton.IsEnabled) return false;
        if (!TryCommitHybridGrid()) return false;
        if (ProgressTenderListBox.SelectedItem is not ContractDraftData contract ||
            ProgressPeriodMonthComboBox.SelectedItem is not ProgressPaymentMonthOption period ||
            HybridPeriodMonth is not { } month)
        {
            ShowMultiRouteValidation("Önce ihale, yüklenici, hat ve hakediş ayını seçin. Kaynak kayıtlar değiştirilmedi.");
            return false;
        }

        var selected = ProgressRouteComboBox.SelectedItems.OfType<RouteDraft>()
            .Select(HybridRouteKey).ToHashSet(StringComparer.Ordinal);
        var changed = _multiRoutePaymentRows.Where(row => row.HasChanges).ToArray();
        var updates = new List<(int Index, RouteDraft Route, MultiRoutePaymentDraftRow Row)>();
        foreach (var row in changed)
        {
            var index = -1;
            for (var i = 0; i < _routes.Count; i++)
                if (string.Equals(HybridRouteKey(_routes[i]), row.RouteKey, StringComparison.Ordinal))
                { index = i; break; }
            if (index < 0 || !selected.Contains(row.RouteKey) || !IsRouteForContract(_routes[index], contract) ||
                _routes[index].ProgramType != contract.ProgramType ||
                FindContractor(_routes[index].ContractorTaxOrIdentityNumber, _routes[index].ProgramType) is null)
            {
                ShowMultiRouteValidation($"{row.RouteDisplayText}: kaynak hat, sözleşme veya firma eşleşmesi bulunamadı. Hiçbir kayıt değiştirilmedi.");
                return false;
            }
            var input = row.CreateInput();
            var calendarError = ProgressPaymentMonthService.GetCalendarCapacityError(period, input.WorkDays, input.GuideWorkDays);
            if (string.IsNullOrWhiteSpace(row.RouteName) || row.Preview is null || calendarError is not null)
            {
                ShowMultiRouteValidation($"{row.RouteDisplayText}: {calendarError ?? "Hat adı ve sayısal girdileri kontrol edin."} Hiçbir kayıt değiştirilmedi.");
                return false;
            }
            updates.Add((index, _routes[index] with
            {
                Name = row.RouteName.Trim(), TransportWorkDays = input.WorkDays,
                DailyPrice = input.DailyPrice, GuideDailyPrice = input.GuideDailyPrice
            }, row));
        }
        if (updates.Count == 0) return false;

        var previousRoutes = _routes.ToArray();
        var previousDrafts = SavedHybridPaymentDrafts.ToArray();
        var wasSuppressed = _suppressSelectionEvents;
        _suppressSelectionEvents = true;
        try
        {
            foreach (var update in updates)
            {
                _routes[update.Index] = update.Route;
                SavedHybridPaymentDrafts.RemoveAll(draft => draft.PeriodMonth == month &&
                    string.Equals(draft.RouteKey, update.Row.RouteKey, StringComparison.Ordinal));
                SavedHybridPaymentDrafts.Add(new(update.Row.RouteKey, month, update.Row.CreateInput(),
                    update.Row.MinimumWageSupportEnabled));
            }
            if (!SavePersistedData(showErrorDialog: false))
            {
                _routes.Clear();
                foreach (var route in previousRoutes) _routes.Add(route);
                _savedHybridPaymentDrafts = previousDrafts.ToList();
                RefreshHybridSourceViews(selected);
                ShowMultiRouteValidation("Değişiklikler diske kaydedilemedi. Kaynak kayıtlar geri alındı; tablo düzenlemeleri korundu. Tekrar kaydedebilirsiniz.");
                return false;
            }
            foreach (var update in updates)
            {
                update.Row.RouteName = update.Route.Name;
                update.Row.AcceptChanges();
            }
            RefreshHybridSourceViews(selected);
            HybridSelectionSummaryTextBlock.Text = $"{updates.Count} hattın kaynak bilgileri ve {month:MM.yyyy} tablo girdileri kaydedildi.";
            ProgressValidationBanner.Visibility = Visibility.Collapsed;
            return true;
        }
        finally
        {
            _suppressSelectionEvents = wasSuppressed;
            RefreshHybridUpdateButton();
            SyncHybridSelectedRowFields();
        }
    }

    private void RefreshHybridSourceViews(IReadOnlySet<string> selected)
    {
        // Replace selector objects without triggering selection/month resets or losing row drafts.
        var choices = ProgressRouteComboBox.Items.OfType<RouteDraft>()
            .Select(old => _routes.FirstOrDefault(route => HybridRouteKey(route) == HybridRouteKey(old)) ?? old)
            .ToArray();
        if (ProgressRouteComboBox.ItemsSource is null)
        {
            ProgressRouteComboBox.Items.Clear();
            foreach (var route in choices) ProgressRouteComboBox.Items.Add(route);
        }
        else ProgressRouteComboBox.ItemsSource = choices;
        ProgressRouteComboBox.SelectedItems.Clear();
        foreach (var route in choices.Where(route => selected.Contains(HybridRouteKey(route))))
            ProgressRouteComboBox.SelectedItems.Add(route);

        if (_editingRoute is not null)
        {
            var updated = _routes.FirstOrDefault(route => HybridRouteKey(route) == HybridRouteKey(_editingRoute));
            if (updated is not null)
            {
                _editingRoute = updated;
                RouteNameTextBox.Text = updated.Name;
                RouteTransportWorkDaysTextBox.Text = updated.TransportWorkDays.ToString(CultureInfo.CurrentCulture);
                RouteGuideDailyPriceTextBox.Text = FormatCalculationInput(updated.GuideDailyPrice);
            }
        }
        UpdateRouteListView();
        UpdateRouteSummary();
        UpdateContractSummaryView();
        UpdateHomeSummaryView();
        UpdateReportView();
        if (ProgressTenderListBox.SelectedItem is ContractDraftData contract)
            UpdateOutputRoutesForContract(contract);
        UpdateProgressRouteDefaults();
    }
}
