using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private void RouteSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsInitialized && !_suppressSelectionEvents) UpdateRouteListView();
    }

    private void RefreshRoutePanelList()
    {
        var contract = RouteTenderListBox.SelectedItem as ContractDraftData;
        var firm = RouteContractorListBox.SelectedItem as ContractorDraft;
        var scope = _routes.Where(route => contract is not null && IsRouteForContract(route, contract)).ToArray();
        var search = RouteSearchTextBox.Text.Trim();
        var shown = scope.Where(route => (firm is null ||
                OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber) == OehsIdentityService.NormalizeIdentity(firm.TaxOrIdentityNumber)) &&
            (search.Length == 0 || route.RouteNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
                PanelSearchCulture.CompareInfo.IndexOf(route.Name, search, CompareOptions.IgnoreCase) >= 0 ||
                PanelSearchCulture.CompareInfo.IndexOf(route.ContractorName, search, CompareOptions.IgnoreCase) >= 0))
            .OrderBy(route => route.RouteNumber, PanelNameComparer).ThenBy(route => route.Name, PanelNameComparer).ToArray();
        RoutesListBox.ItemsSource = shown;
        RouteCountTextBlock.Text = $"{shown.Length} / {scope.Length} hat";
        RouteListEmptyBorder.Visibility = shown.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        RouteListEmptyTextBlock.Text = contract is null ? "Hatları görmek ve yeni hat eklemek için önce ihale seçin."
            : scope.Length == 0 ? "Bu ihalede henüz hat yok. Yükleniciyi seçip hat bilgilerini kaydedin."
            : "Seçili yükleniciye veya aramanıza uygun hat bulunamadı.";
    }

    private void RouteDate_InvalidInput(object? sender, DatePickerDateValidationErrorEventArgs e)
    {
        if (sender is not DatePicker picker) return;
        e.ThrowException = false;
        picker.SetValue(InvalidTenderDateInputProperty, true);
        picker.SelectedDate = null;
        MarkInvalid(picker);
        ShowRouteValidation("Geçerli bir sözleşme tarihi girin veya takvimden seçin; hatalı tarih düzeltilmeden kayıt yapılmaz.");
    }

    private void ResetRouteDateValidation() => RouteContractDatePicker.SetValue(InvalidTenderDateInputProperty, false);


    private void ClearRouteEditorFields()
    {
        RouteNumberTextBox.Clear(); RouteNameTextBox.Clear(); RouteDailyPriceTextBox.Clear();
        RouteContractAmountTextBox.Clear(); RouteGuideDailyPriceTextBox.Clear();
        RouteContractDatePicker.Text = string.Empty; RouteContractDatePicker.SelectedDate = null;
        ResetRouteDateValidation();
        ClearRouteValidationVisuals();
    }

    private int CountRouteHistoryReferences(RouteDraft route) => _progressHistory.Count(payment =>
        payment.Input.ProgramType == route.ProgramType &&
        string.Equals(payment.TenderNumber, route.TenderNumber, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(payment.ContractNumber, route.ContractNumber, StringComparison.OrdinalIgnoreCase) &&
        string.Equals(payment.RouteNumber, route.RouteNumber, StringComparison.OrdinalIgnoreCase) &&
        OehsIdentityService.NormalizeIdentity(payment.ContractorTaxOrIdentityNumber) == OehsIdentityService.NormalizeIdentity(route.ContractorTaxOrIdentityNumber));
}
