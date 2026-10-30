using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    public sealed partial class MultiRoutePaymentDraftRow
    {
        private PaymentRatePeriod[]? _transportPeriods, _guidePeriods, _acceptedTransportPeriods, _acceptedGuidePeriods;
        private bool _hasPeriodChanges, _acceptedPeriodChanges;
        public IReadOnlyList<PaymentRatePeriod>? TransportPeriods => _transportPeriods;
        public IReadOnlyList<PaymentRatePeriod>? GuidePeriods => _guidePeriods;
        public bool HasTransportPeriods => _transportPeriods is { Length: > 1 };
        public bool HasGuidePeriods => _guidePeriods is { Length: > 1 };
        public bool HasPeriodChanges => _hasPeriodChanges || HasTransportPeriods || HasGuidePeriods;
        public string PeriodChangeButtonText => HasPeriodChanges ? "Değişiklikleri göster" : "Dönem içi değişiklik";
        public string PeriodCalculationText => $"Taşıma: {PaymentRatePeriodService.Formula(_transportPeriods,WorkDays,DailyPrice)} · Fiyat farkı: {PriceDifference:N2} TL" +
            (_program==TransportationProgramType.SpecialEducation?$" | Rehber: {PaymentRatePeriodService.Formula(_guidePeriods,GuideWorkDays,GuideDailyPrice)} · Fiyat farkı: {GuidePriceDifference:N2} TL":"");
        public string PeriodSummary => HasTransportPeriods || HasGuidePeriods
            ? $"Taşıma: {_transportPeriods?.Length ?? 1} bölüm · {WorkDays} gün · {FormatMoney(Preview?.ServiceAmount ?? 0)}\nRehber: {_guidePeriods?.Length ?? 1} bölüm · {GuideWorkDays} gün · {FormatMoney(Preview?.GuideServiceAmount ?? 0)}\nTabloda son günlük fiyat gösterilir. Bölüm gün, fiyat ve fiyat farkını bu pencereden düzenleyin."
            : "Bu hattın dönem içindeki gün, günlük fiyat ve fiyat farkı değişikliklerini düzenleyin.";
        private bool PeriodChangesDiffer() => _hasPeriodChanges != _acceptedPeriodChanges ||
            !(_transportPeriods ?? Array.Empty<PaymentRatePeriod>()).SequenceEqual(_acceptedTransportPeriods ?? Array.Empty<PaymentRatePeriod>()) ||
            !(_guidePeriods ?? Array.Empty<PaymentRatePeriod>()).SequenceEqual(_acceptedGuidePeriods ?? Array.Empty<PaymentRatePeriod>());
        private void AcceptPeriodChanges()
        { _acceptedTransportPeriods = _transportPeriods?.ToArray(); _acceptedGuidePeriods = _guidePeriods?.ToArray(); _acceptedPeriodChanges = _hasPeriodChanges; }
        public void ClearPeriodChanges() { _transportPeriods = null; _guidePeriods = null; _hasPeriodChanges=false; Changed(string.Empty); }
        public void ApplyPeriodInput(ProgressPaymentInput input)
        { PaymentRatePeriodService.Validate(input); LoadInput(input, MinimumWageSupportEnabled); }
        public bool ControlsPeriodField(string? name) =>
            HasTransportPeriods && name is nameof(WorkDays) or nameof(DailyPrice) or nameof(PriceDifference) ||
            HasGuidePeriods && name is nameof(GuideWorkDays) or nameof(GuideDailyPrice) or nameof(GuidePriceDifference);
    }

    private ProgressPaymentInput? _periodEditingInput;
    private Guid? _periodEditingId;
    private ProgressPaymentHistoryData? PeriodEditingPayment => IsArchiveCorrectionFor(ArchiveCorrectionSection.ProgressPayment)
        ? _editingArchiveSnapshot?.ProgressPayment : _editingHistoryId is Guid id
            ? _progressHistory.FirstOrDefault(p => p.Id == id) : null;
    private ProgressPaymentInput? EditingPeriodInput => PeriodEditingPayment is { } payment
        ? _periodEditingId == payment.Id ? _periodEditingInput ?? payment.Input : payment.Input : null;
    private ProgressPaymentInput AttachEditingPeriods(ProgressPaymentInput input) => EditingPeriodInput is { } saved
        ? input with { TransportPeriods = saved.TransportPeriods, GuidePeriods = saved.GuidePeriods, HasPeriodChanges = saved.HasPeriodChanges } : input;

    private void RefreshPeriodInputReadOnly()
    {
        var saved = EditingPeriodInput;
        var transport = saved?.TransportPeriods is { Count: > 1 };
        var guide = saved?.GuidePeriods is { Count: > 1 };
        foreach (var field in new[] { WorkDaysTextBox, DailyPriceTextBox, PriceDifferenceTextBox }) field.IsReadOnly = transport;
        foreach (var field in new[] { GuideWorkDaysTextBox, GuideDailyPriceTextBox, GuidePriceDifferenceTextBox }) field.IsReadOnly = guide;
    }

    private void HybridPeriodChange_Click(object sender, RoutedEventArgs e)
    {
        if (!TryCommitHybridGrid()) return;
        var payment = PeriodEditingPayment;
        var row = (sender as Button)?.Tag as MultiRoutePaymentDraftRow
            ?? MultiRoutePaymentGrid.CurrentItem as MultiRoutePaymentDraftRow
            ?? (_multiRoutePaymentRows.Count == 1 ? _multiRoutePaymentRows[0] : null);
        var input = payment is not null ? EditingPeriodInput : row?.CreateInput();
        var period = ProgressPeriodMonthComboBox.SelectedItem as ProgressPaymentMonthOption;
        if (input is null || period is null) { ShowMultiRouteValidation("Önce hat ve hakediş dönemini seçin."); return; }
        var dialog = new PaymentPeriodChangeWindow(input, period, row?.RouteDisplayText ?? payment?.RouteName ?? "Seçili hat", _calculationConstants) { Owner = this };
        if (dialog.ShowDialog() != true || dialog.ResultInput is not { } result) return;
        if (payment is not null)
        {
            _periodEditingId = payment.Id; _periodEditingInput = result;
            WorkDaysTextBox.Text = result.WorkDays.ToString(System.Globalization.CultureInfo.CurrentCulture);
            DailyPriceTextBox.Text = FormatCalculationInput(result.DailyPrice); PriceDifferenceTextBox.Text = FormatCalculationInput(result.PriceDifference);
            GuideWorkDaysTextBox.Text = result.GuideWorkDays.ToString(System.Globalization.CultureInfo.CurrentCulture);
            GuideDailyPriceTextBox.Text = FormatCalculationInput(result.GuideDailyPrice); GuidePriceDifferenceTextBox.Text = FormatCalculationInput(result.GuidePriceDifference);
            RefreshPeriodInputReadOnly(); RefreshCalculationTraceLive();
        }
        else if (row is not null)
        {
            row.ApplyPeriodInput(result);
            RefreshHybridUpdateButton(); SyncHybridSelectedRowFields(); RefreshHybridTotals();
        }
    }

    private void HybridGrid_BeginningEdit(object sender, DataGridBeginningEditEventArgs e)
    {
        if (e.Row.Item is MultiRoutePaymentDraftRow row && e.Column is DataGridBoundColumn column &&
            row.ControlsPeriodField((column.Binding as Binding)?.Path?.Path))
        {
            e.Cancel = true;
            HybridSelectionSummaryTextBlock.Text = "Bu hattın bölüm gün, fiyat ve fiyat farklarını Dönem içi değişiklik penceresinden düzenleyin.";
        }
    }

    private static DateTime PeriodChangeTimestamp(DateTime value) => value == default ? DateTime.MinValue : value.ToUniversalTime();
    private void SuggestPeriodDailyPrices(MultiRoutePaymentDraftRow row, RouteDraft route)
    {
        if (HybridPeriodMonth is not { } month) return;
        var candidates = _progressHistory.Where(p => PaymentOperationsService.RouteKey(p) == row.RouteKey && p.PeriodDate.HasValue &&
                new DateOnly(p.PeriodDate.Value.Year,p.PeriodDate.Value.Month,1) < month)
            .Select(p => (Month: new DateOnly(p.PeriodDate!.Value.Year,p.PeriodDate.Value.Month,1), RecordedAt: PeriodChangeTimestamp(p.CreatedAt), p.Input))
            .Concat(SavedHybridPaymentDrafts.Where(d => d.RouteKey == row.RouteKey && d.PeriodMonth < month)
                .Select(d => (Month: d.PeriodMonth, RecordedAt: PeriodChangeTimestamp(d.SavedAt), d.Input)))
            .OrderByDescending(p => p.Month).ThenByDescending(p => p.RecordedAt).ToArray();
        var transport = candidates.FirstOrDefault(c => c.Input.HasPeriodChanges || c.Input.TransportPeriods is { Count: > 1 }).Input;
        var guide = candidates.FirstOrDefault(c => c.Input.HasPeriodChanges || c.Input.GuidePeriods is { Count: > 1 }).Input;
        row.DailyPrice = transport is not null ? candidates[0].Input.DailyPrice : route.DailyPrice;
        row.GuideDailyPrice = HasGuideStaff(route.ProgramType) ? guide is not null ? candidates[0].Input.GuideDailyPrice : route.GuideDailyPrice : 0m;
    }
}
