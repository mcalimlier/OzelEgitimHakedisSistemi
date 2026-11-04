using System.Globalization;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private static readonly DependencyProperty InvalidTenderDateInputProperty = DependencyProperty.RegisterAttached(
        "InvalidTenderDateInput", typeof(bool), typeof(MainWindow), new PropertyMetadata(false));

    private void TenderDate_InvalidInput(object? sender, DatePickerDateValidationErrorEventArgs e)
    {
        if (sender is not DatePicker picker) return;
        e.ThrowException = false;
        picker.SetValue(InvalidTenderDateInputProperty, true);
        picker.SelectedDate = null;
        MarkInvalid(picker);
        ShowContractValidation("Geçerli bir tarih girin veya takvimden seçin. Hatalı tarih düzeltilmeden kayıt yapılmaz.");
    }

    private void TenderDate_Changed(object? sender, SelectionChangedEventArgs e)
    {
        if (sender is DatePicker { SelectedDate: not null } picker)
        {
            picker.SetValue(InvalidTenderDateInputProperty, false);
            ClearInvalid(picker);
        }
    }

    private void ResetTenderDateValidation()
    {
        StartDatePicker.SetValue(InvalidTenderDateInputProperty, false);
        EndDatePicker.SetValue(InvalidTenderDateInputProperty, false);
        TenderDatePicker.SetValue(InvalidTenderDateInputProperty, false);
    }

    private void ClearTenderDate_Click(object sender, RoutedEventArgs e)
    {
        TenderDatePicker.SetValue(InvalidTenderDateInputProperty, false);
        TenderDatePicker.SelectedDate = null;
        TenderDatePicker.Text = string.Empty;
        ClearInvalid(TenderDatePicker);
    }
    private static bool TryReadLegacyContractAmount(string? text, out decimal amount)
    {
        amount = 0m;
        return string.IsNullOrWhiteSpace(text) || TurkishMoneyInput.TryParse(text, out amount) && amount >= 0m;
    }

    private static bool HasValidTenderDateText(DatePicker picker)
    {
        if ((bool)picker.GetValue(InvalidTenderDateInputProperty)) return false;
        if (string.IsNullOrWhiteSpace(picker.Text)) return true;
        if (picker.SelectedDate is not DateTime selected) return false;
        bool Matches(CultureInfo culture) => DateTime.TryParse(picker.Text, culture,
            DateTimeStyles.AllowWhiteSpaces, out var parsed) && parsed.Date == selected.Date;
        return Matches(picker.Language.GetSpecificCulture()) || Matches(CultureInfo.CurrentCulture);
    }

    private void TenderSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsInitialized && !_suppressSelectionEvents) RefreshTenderList();
    }

    private void RefreshTenderList()
    {
        var hasProgram = TryGetSelectedTransportationProgramType(out var program);
        var scope = _contracts.Where(c => !hasProgram || c.ProgramType == program).ToArray();
        var search = TenderSearchTextBox.Text.Trim();
        var shown = scope.Where(c => search.Length == 0 ||
            c.TenderNumber.Contains(search, StringComparison.OrdinalIgnoreCase) ||
            c.WorkName?.Contains(search, StringComparison.OrdinalIgnoreCase) == true ||
            _routes.Any(r => IsRouteForContract(r, c) && r.Name.Contains(search, StringComparison.OrdinalIgnoreCase)))
            .OrderBy(c => c.TenderNumber, StringComparer.OrdinalIgnoreCase).ToArray();
        ContractsListBox.ItemsSource = shown;
        TenderListCountTextBlock.Text = $"{shown.Length} / {scope.Length} ihale";
        TenderListEmptyBorder.Visibility = shown.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        TenderListEmptyTextBlock.Text = scope.Length == 0
            ? "Henüz kayıt yok. İhale bilgilerini doldurup kaydedin."
            : "Aramanıza uygun ihale bulunamadı.";
    }
}
