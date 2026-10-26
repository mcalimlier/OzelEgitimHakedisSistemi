using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private void ContractorSearch_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (IsInitialized && !_suppressSelectionEvents) RefreshContractorList();
    }

    private void RefreshContractorList()
    {
        var hasProgram = TryGetSelectedTransportationProgramType(out var program);
        var scope = _contractors.Where(firm => !hasProgram || firm.ProgramType == program).ToArray();
        var search = ContractorSearchTextBox.Text.Trim();
        var normalizedSearch = OehsIdentityService.NormalizeIdentity(search);
        var shown = scope.Where(firm => search.Length == 0 ||
            firm.Name.Contains(search, StringComparison.CurrentCultureIgnoreCase) ||
            normalizedSearch.Length > 0 && (
                OehsIdentityService.NormalizeIdentity(firm.TaxOrIdentityNumber).Contains(normalizedSearch, StringComparison.Ordinal) ||
                OehsIdentityService.NormalizeIdentity(firm.BankAccountNumber).Contains(normalizedSearch, StringComparison.Ordinal)))
            .OrderBy(firm => firm.Name, StringComparer.CurrentCultureIgnoreCase).ToArray();
        ContractorsListBox.ItemsSource = shown;
        ContractorCountTextBlock.Text = $"{shown.Length} / {scope.Length} firma";
        ContractorListEmptyBorder.Visibility = shown.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        ContractorListEmptyTextBlock.Text = scope.Length == 0
            ? "Henüz kayıt yok. Firma bilgilerini doldurup kaydedin."
            : "Aramanıza uygun firma bulunamadı.";
    }

    private void ClearContractorValidation()
    {
        ContractorValidationBanner.Visibility = Visibility.Collapsed;
        ContractorValidationMessageTextBlock.Text = string.Empty;
        ClearInvalid(ContractorNameTextBox);
        ClearInvalid(ContractorTaxTextBox);
        ClearInvalid(ContractorProgramComboBox);
    }

    private void ShowContractorValidation(string message)
    {
        ContractorValidationMessageTextBlock.Text = message;
        ContractorValidationBanner.Visibility = Visibility.Visible;
        ContractorStatusTextBlock.Text = message;
    }

    private void ClearContractorEditorFields()
    {
        ContractorNameTextBox.Clear();
        ContractorTaxTextBox.Clear();
        ContractorBankTextBox.Clear();
        ContractorVatExemptCheckBox.IsChecked = false;
        ClearContractorValidation();
    }
}
