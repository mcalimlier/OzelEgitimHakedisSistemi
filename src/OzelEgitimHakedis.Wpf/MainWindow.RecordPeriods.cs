using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private bool _updatingRecordPeriods;

    private void UpdateRecordPeriodOptions(bool archive)
    {
        var box=archive?ArchiveMonthFilter:HistoryMonthFilter;
        var previous=(box.SelectedItem as ProgressPaymentMonthOption)?.PeriodDate;
        var program=RecordsProgram;
        var tender=(archive?ArchiveTenderFilter:HistoryTenderFilter).Text;
        var periods=new List<ProgressPaymentMonthOption>();
        foreach(var contract in _contracts.Where(c=>c.EndDate>=c.StartDate && c.ProgramType==program && PaymentOperationsService.Contains(c.TenderNumber,tender)))
            periods.AddRange(ProgressPaymentMonthService.BuildOptions(contract.StartDate,contract.EndDate));
        IEnumerable<ProgressPaymentHistoryData> payments=_progressHistory;
        if(archive)
        {
            var snapshots=ReadOperationArchives().Where(a=>a.Contract.ProgramType==program && PaymentOperationsService.Contains(a.Contract.TenderNumber,tender)).ToArray();
            foreach(var snapshot in snapshots.Where(a=>a.Contract.EndDate>=a.Contract.StartDate))
                periods.AddRange(ProgressPaymentMonthService.BuildOptions(snapshot.Contract.StartDate,snapshot.Contract.EndDate));
            payments=snapshots.Select(a=>a.ProgressPayment);
        }
        foreach(var payment in payments.Where(p=>p.Input.ProgramType==program && PaymentOperationsService.Contains(p.TenderNumber,tender)))
        {
            if(payment.ContractStartDate is {} start && payment.ContractEndDate is {} end && end>=start)
                periods.AddRange(ProgressPaymentMonthService.BuildOptions(start,end));
            // A legacy payment with no contract dates must still be filterable.
            if(payment.PeriodDate is {} period && !periods.Any(p=>p.StartDate.Year==period.Year && p.StartDate.Month==period.Month))
                periods.AddRange(ProgressPaymentMonthService.BuildOptions(new DateOnly(period.Year,period.Month,1),new DateOnly(period.Year,period.Month,DateTime.DaysInMonth(period.Year,period.Month))));
        }
        var options=periods.GroupBy(p=>(p.StartDate.Year,p.StartDate.Month))
            .Select(g=>new ProgressPaymentMonthOption(g.Min(p=>p.StartDate),g.Max(p=>p.EndDate)))
            .OrderBy(p=>p.StartDate).ToArray();
        _updatingRecordPeriods=true;
        try
        {
            box.ItemsSource=options;
            box.SelectedItem=previous.HasValue?ProgressPaymentMonthService.FindForPeriod(options,previous.Value):null;
        }
        finally {_updatingRecordPeriods=false;}
    }
}
