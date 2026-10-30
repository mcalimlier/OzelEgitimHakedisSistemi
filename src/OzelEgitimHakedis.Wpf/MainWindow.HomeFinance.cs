using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private void UpdateHomeFinanceCards(IReadOnlyList<MonthlyPaymentRow> rows)
    {
        // BuildMonth already selects the latest payment per full route identity and month.
        // The table's search/status filters intentionally do not change these period totals.
        var payments = rows.Where(row => row.Payment is not null).Select(row => row.Payment!).ToArray();
        HomeNetValue.Text = FormatMoney(payments.Sum(p => p.Calculation.NetPayable));
        HomeGrossValue.Text = FormatMoney(payments.Sum(p => p.Calculation.GrossAmount));
        HomeDeductionsValue.Text = FormatMoney(payments.Sum(p => p.Calculation.TotalDeductions));
        HomeVatValue.Text = FormatMoney(payments.Sum(p => p.Calculation.Vat));
        HomeTransportValue.Text = FormatMoney(payments.Sum(p => p.Calculation.ServiceAmount));
        // Show exactly the entered transport difference, independently of discounts and guide inputs.
        HomeDifferenceValue.Text = FormatMoney(payments.Sum(p => p.Input.PriceDifference));
        HomeRoutesValue.Text = $"{payments.Length} / {rows.Count}";
        var missing = rows.Count - payments.Length;
        HomeRoutesCaption.Text = rows.Count == 0 ? "Dönem kapsamında hat yok" : missing == 0
            ? "Tüm hatların hakedişi kayıtlı" : $"{missing} hat için kayıt yok";
        var firms = rows.Select(row => OehsIdentityService.NormalizeIdentity(row.Route.ContractorTaxOrIdentityNumber))
            .Where(key => key.Length > 0).Distinct(StringComparer.Ordinal).Count();
        HomeFirmsValue.Text = $"{firms} firma";
        HomeFinanceScopeText.Text = HomeProgram is null || HomeSelectedPeriod is null
            ? "Program ve hakediş dönemini seçin."
            : payments.Length == 0 ? "Seçili dönemde kayıtlı hakediş yok."
            : "Program · İKN · dönem toplamı — kayıtlı hakedişler";
    }
}
