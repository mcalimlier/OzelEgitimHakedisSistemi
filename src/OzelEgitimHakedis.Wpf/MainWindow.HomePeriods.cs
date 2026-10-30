using OzelEgitimHakedis.Core.Services;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private DateOnly? HomeSelectedPeriod => (HomePeriodMonthComboBox.SelectedItem as ProgressPaymentMonthOption)?.PeriodDate;

    private void UpdateHomePeriodOptions()
    {
        var previous=HomeSelectedPeriod;
        var contracts=_contracts.Where(c=>c.ProgramType==HomeProgram);
        if(HomeTenderListBox.SelectedItem is ContractDraftData selected && selected.ProgramType==HomeProgram)
            contracts=contracts.Where(c=>c==selected);
        var options=contracts.SelectMany(c=>ProgressPaymentMonthService.BuildOptions(c.StartDate,c.EndDate))
            .GroupBy(p=>(p.StartDate.Year,p.StartDate.Month))
            .Select(g=>new ProgressPaymentMonthOption(g.Min(p=>p.StartDate),g.Max(p=>p.EndDate)))
            .OrderBy(p=>p.StartDate).ToArray();
        HomePeriodMonthComboBox.ItemsSource=options;
        HomePeriodMonthComboBox.SelectedItem=previous.HasValue?ProgressPaymentMonthService.FindForPeriod(options,previous.Value):null;
    }
}
