using System.Globalization;
using System.Windows;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;
public partial class MainWindow
{
    private static string DescribeRates(ProgressPaymentCalculationConstants c) => PaymentOperationsService.DescribeRates(c);
    private decimal ComparisonReportNet(ProgressPaymentHistoryData payment)
    {
        var history=_progressHistory.Where(p=>p.Id!=payment.Id).Append(payment).ToArray();
        return ProgressPaymentReportService.CalculateReportNetPayable(payment,ProgressPaymentReportService.Calculate(payment,history));
    }
    private MessageBoxResult ShowPaymentComparison(string title,string scope,ProgressPaymentHistoryData original,
        ProgressPaymentHistoryData proposed,IReadOnlyList<PaymentDifference>? extra=null)
    {
        var differences=PaymentOperationsService.Compare(original,proposed).ToList();
        if(extra is not null) differences.AddRange(extra);
        var oldReport=ComparisonReportNet(original);var newReport=ComparisonReportNet(proposed);
        if(oldReport!=newReport) differences.Add(new("PDF net ödeme",FormatMoney(oldReport),FormatMoney(newReport)));
        var totals=$"Hesaplanan net: {FormatMoney(original.Calculation.NetPayable)} → {FormatMoney(proposed.Calculation.NetPayable)}\n"+
            $"Fark: {FormatMoney(proposed.Calculation.NetPayable-original.Calculation.NetPayable)}\nPDF net ödeme: {FormatMoney(oldReport)} → {FormatMoney(newReport)}";
        var dialog=new PaymentComparisonDialog(title,scope,differences.DistinctBy(d=>d.Field).ToArray(),totals){Owner=this};
        return dialog.ShowDialog()==true?MessageBoxResult.Yes:MessageBoxResult.No;
    }
    private IReadOnlyList<PaymentDifference> LiveEditorDifferences(ProgressPaymentHistoryData original)
    {
        if(_historyCorrectionSection==HistoryCorrectionSection.Contract)
        {
            var c=FindContract(original.TenderNumber??"",original.ContractNumber??"",original.Input.ProgramType);
            if(c is not null)
            {
                var next=c with {TenderNumber=TenderNumberTextBox.Text.Trim(),
                    TenderDate=TenderDatePicker.SelectedDate is {} td?DateOnly.FromDateTime(td):null,
                    ContractDate=ContractDatePicker.SelectedDate is {} cd?DateOnly.FromDateTime(cd):null,
                    StartDate=StartDatePicker.SelectedDate is {} sd?DateOnly.FromDateTime(sd):c.StartDate,
                    EndDate=EndDatePicker.SelectedDate is {} ed?DateOnly.FromDateTime(ed):c.EndDate,
                    ContractWorkDays=int.TryParse(ContractWorkDaysTextBox.Text,out var days)?days:c.ContractWorkDays};
                return PaymentOperationsService.CompareFields(c,next,"Sözleşme");
            }
        }
        if(_historyCorrectionSection==HistoryCorrectionSection.Contractor)
        {
            var firm=_contractors.FirstOrDefault(f=>f.ProgramType==original.Input.ProgramType &&
                OehsIdentityService.NormalizeIdentity(f.TaxOrIdentityNumber)==OehsIdentityService.NormalizeIdentity(original.ContractorTaxOrIdentityNumber??""));
            if(firm is not null)
            {
                var old=new Contractor(firm.Name,firm.TaxOrIdentityNumber,firm.Address,NormalizePhoneNumber(firm.Phone),firm.Email,firm.BankAccountNumber,firm.ContactPerson,firm.ProgramType,firm.VatExempt);
                var next=old with {Name=ContractorNameTextBox.Text.Trim(),TaxOrIdentityNumber=ContractorTaxTextBox.Text.Trim(),
                    BankAccountNumber=ContractorBankTextBox.Text.Trim(),VatExempt=ContractorVatExemptCheckBox.IsChecked==true};
                return PaymentOperationsService.CompareFields(old,next,"Firma");
            }
        }
        return Array.Empty<PaymentDifference>();
    }
}
