using System.Globalization;
using System.Windows;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow : Window
{
    public MainWindow()
    {
        InitializeComponent();
        StartDatePicker.SelectedDate = DateTime.Today;
        EndDatePicker.SelectedDate = DateTime.Today;
    }

    private void NavigationButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not System.Windows.Controls.Button button)
        {
            return;
        }

        var page = button.Tag?.ToString() ?? "Home";

        HomePanel.Visibility = page == "Home" ? Visibility.Visible : Visibility.Collapsed;
        ContractPanel.Visibility = page == "Contract" ? Visibility.Visible : Visibility.Collapsed;
        ProgressPanel.Visibility = page == "Progress" ? Visibility.Visible : Visibility.Collapsed;
        PlaceholderPanel.Visibility = page is "Home" or "Contract" or "Progress"
            ? Visibility.Collapsed
            : Visibility.Visible;

        if (PlaceholderPanel.Visibility == Visibility.Visible)
        {
            PlaceholderTitleTextBlock.Text = button.Content?.ToString() ?? "Modül";
        }
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

        ContractStatusTextBlock.Text =
            $"Taslak hazır: {WorkNameTextBox.Text.Trim()} — {contractAmount:N2} ₺. Kalıcı kayıt katmanı bir sonraki adımda eklenecek.";
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
        ContractStatusTextBlock.Text = string.Empty;
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
            CalculationStatusTextBlock.Text = "Tutar alanlarını geçerli sayılarla doldurun.";
            return;
        }

        try
        {
            var result = ProgressPaymentCalculator.Calculate(
                new ProgressPaymentInput(
                    workDays,
                    dailyPrice,
                    discount,
                    priceDifference,
                    contractAmount,
                    ProgressVatExemptCheckBox.IsChecked == true,
                    0m,
                    0m,
                    0m,
                    InspectionPenaltyCheckBox.IsChecked == true));

            ServiceAmountResultTextBlock.Text = FormatMoney(result.ServiceAmount);
            TotalAmountResultTextBlock.Text = FormatMoney(result.TotalAmount);
            VatResultTextBlock.Text = FormatMoney(result.Vat);
            DeductionsResultTextBlock.Text = FormatMoney(result.TotalDeductions);
            GrossResultTextBlock.Text = FormatMoney(result.GrossAmount);
            NetResultTextBlock.Text = FormatMoney(result.NetPayable);
            CalculationStatusTextBlock.Text = "Hesaplama başarıyla tamamlandı.";
        }
        catch (ArgumentOutOfRangeException ex)
        {
            CalculationStatusTextBlock.Text = ex.Message;
        }
    }

    private static bool TryParseDecimal(string? text, out decimal value)
    {
        return decimal.TryParse(text, NumberStyles.Number, CultureInfo.CurrentCulture, out value) ||
               decimal.TryParse(text, NumberStyles.Number, CultureInfo.InvariantCulture, out value);
    }

    private static string FormatMoney(decimal value) => $"{value:N2} ₺";
}
