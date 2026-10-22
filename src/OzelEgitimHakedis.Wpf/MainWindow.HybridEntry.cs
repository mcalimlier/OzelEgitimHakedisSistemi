using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using OzelEgitimHakedis.Core.Models;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private bool _refreshingHybridTotals;
    private void ConfigureHybridRow(MultiRoutePaymentDraftRow row,RouteDraft route)
    {
        row.PropertyChanged-=HybridRow_PropertyChanged;
        row.DailyPrice=route.DailyPrice;
        row.GuideDailyPrice=HasGuideStaff(route.ProgramType)?route.GuideDailyPrice:0m;
        var contract=ProgressTenderListBox.SelectedItem as ContractDraftData;
        var firm=FindContractor(route.ContractorTaxOrIdentityNumber,route.ProgramType);
        row.Configure(route.ProgramType,contract?.ContractAmount??0m,firm?.VatExempt??contract?.VatExempt??false,
            _calculationConstants??ProgressPaymentCalculationConstants.Defaults);
        row.PropertyChanged+=HybridRow_PropertyChanged;
    }
    private void HybridRow_PropertyChanged(object? sender,PropertyChangedEventArgs e)
    {
        if(e.PropertyName==nameof(MultiRoutePaymentDraftRow.NetPayableText))
        {RefreshHybridTotals();RefreshCalculationTraceLive();}
    }
    private void RefreshHybridCalculationTrace()
    {
        var lines=new List<string>{"HAT BAZINDA HAKEDİŞ — KAYDEDİLMEMİŞ TASLAKLAR","Her satır kendi gün, fiyat farkı ve kesinti değerleriyle hesaplanır."};
        foreach(var row in _multiRoutePaymentRows)
        {
            lines.Add("");lines.Add(row.RouteDisplayText);
            var input=row.CreateInput();var result=row.Preview;
            if(result is null) {lines.Add("Girdiler geçerli değil; bu satır kaydedilemez.");continue;}
            lines.Add($"Taşıma: {input.WorkDays} gün × {input.DailyPrice} = {FormatMoney(result.ServiceAmount)}");
            lines.Add($"Taşıma fiyat farkı: {input.PriceDifference} − iskonto {input.DiscountAmount} = {FormatMoney(result.PriceDifferenceExcludingVat)}");
            if(input.ProgramType==TransportationProgramType.SpecialEducation)
            {
                lines.Add($"Rehber: {input.GuideWorkDays} gün × {input.GuideDailyPrice} = {FormatMoney(result.GuideServiceAmount)}");
                lines.Add($"Rehber fiyat farkı: {input.GuidePriceDifference} − iskonto {input.GuideDiscountAmount} = {FormatMoney(result.GuidePriceDifferenceExcludingVat)}");
            }
            lines.Add($"KDV: {FormatMoney(result.Vat)} · Tevkifat: {FormatMoney(result.Withholding)}");
            lines.Add($"Vergi borcu: {FormatMoney(input.TaxDebt)} · SGK: {FormatMoney(input.SocialSecurityDebt)}");
            lines.Add($"Asgari ücret desteği: {FormatMoney(input.MinimumWageSupport)} · Denetim cezası: {FormatMoney(input.InspectionPenalty)}");
            lines.Add($"Toplam kesinti: {FormatMoney(result.TotalDeductions)} · Net: {FormatMoney(result.NetPayable)}");
        }
        CalculationTraceLiveTextBox.Text=string.Join(Environment.NewLine,lines);
    }
    private static bool HasGridValidationError(DependencyObject parent)
    {
        if(Validation.GetHasError(parent)) return true;
        for(var i=0;i<VisualTreeHelper.GetChildrenCount(parent);i++)
            if(HasGridValidationError(VisualTreeHelper.GetChild(parent,i))) return true;
        return false;
    }
    private bool TryCommitHybridGrid()
    {
        if(!MultiRoutePaymentGrid.CommitEdit(DataGridEditingUnit.Cell,true) ||
           !MultiRoutePaymentGrid.CommitEdit(DataGridEditingUnit.Row,true) || HasGridValidationError(MultiRoutePaymentGrid))
        {
            ShowMultiRouteValidation("Tabloda geçersiz sayı veya tamamlanmamış hücre var. İşaretli hücreyi düzeltin; hiçbir hakediş oluşturulmadı.");
            return false;
        }
        return true;
    }
    private void TryCalculateHybridSelection(ContractDraftData contract)
    {
        if(!TryCommitHybridGrid()) return;
        UpdateHybridRouteDraftRows();
        var routes=ProgressRouteComboBox.SelectedItems.OfType<RouteDraft>().ToArray();
        if(routes.Length==0) return;
        var period=(ProgressPeriodMonthComboBox.SelectedItem as ProgressPaymentMonthOption)?.PeriodDate;
        if(!period.HasValue) {ShowMultiRouteValidation("Önce hakediş ayını seçin.");return;}
        foreach(var row in _multiRoutePaymentRows) _multiRouteDraftCache[row.RouteKey]=row;
        try
        {
            var entries=SelectedRouteDrafts.SelectExact(routes.Select(r=>OehsIdentityService.BuildRouteKey(
                r.TenderNumber,r.RouteNumber,r.ContractorTaxOrIdentityNumber,r.ContractNumber,r.ProgramType)),_multiRouteDraftCache);
            SaveMultiRouteProgressPayment(contract,routes,entries,period);
        }
        catch(Exception ex) when(ex is ArgumentException or InvalidOperationException or OverflowException)
        {ShowMultiRouteValidation("Hat bazlı girdiler doğrulanamadı; hakediş kaydedilmedi. "+ex.Message);}
    }
    private void HybridColumnView_SelectionChanged(object sender,SelectionChangedEventArgs e) => ApplyHybridColumnView();
    private void ApplyHybridColumnView()
    {
        if(HybridColumnView is null || MultiRoutePaymentGrid is null) return;
        var view=(HybridColumnView.SelectedItem as ComboBoxItem)?.Tag?.ToString()??"All";
        var program=(ProgressTenderListBox?.SelectedItem as ContractDraftData)?.ProgramType;
        var hasGuide=program!=TransportationProgramType.BasicOrSecondary;
        if(!hasGuide && view=="Guide") {HybridColumnView.SelectedIndex=0;return;}
        foreach(var item in HybridColumnView.Items.OfType<ComboBoxItem>())
            if(item.Tag?.ToString()=="Guide") item.IsEnabled=hasGuide;
        foreach(var column in MultiRoutePaymentGrid.Columns.OfType<DataGridBoundColumn>())
        {
            var path=(column.Binding as Binding)?.Path?.Path??"";
            var guide=path.StartsWith("Guide",StringComparison.Ordinal);
            var deduction=path is "TaxDebt" or "SocialSecurityDebt" or "MinimumWageSupport" or "MinimumWageSupportEnabled" or "InspectionPenalty";
            var fixedColumn=path is "RouteDisplayText" or "NetPayableText";
            var visible=fixedColumn || (view=="All" || view=="Guide"&&guide || view=="Deductions"&&deduction || view=="Transport"&&!guide&&!deduction);
            if(guide&&!hasGuide) visible=false;
            if(path=="MinimumWageSupportEnabled"&&program!=TransportationProgramType.BasicOrSecondary) visible=false;
            column.Visibility=visible?Visibility.Visible:Visibility.Collapsed;
        }
    }
    private void HybridGrid_LoadingRow(object sender,DataGridRowEventArgs e) => e.Row.Header=(e.Row.GetIndex()+1).ToString(System.Globalization.CultureInfo.CurrentCulture);
    private void HybridGrid_CellEditEnding(object sender,DataGridCellEditEndingEventArgs e) =>
        Dispatcher.BeginInvoke(new Action(RefreshHybridTotals));
    private void RefreshHybridTotals()
    {
        if(_refreshingHybridTotals || HybridSelectionSummaryTextBlock is null ||
           MultiRouteEntryBorder.Visibility!=Visibility.Visible || _multiRoutePaymentRows.Count==0) return;
        _refreshingHybridTotals=true;
        try
        {
            var period=ProgressPeriodMonthComboBox?.SelectedItem as ProgressPaymentMonthOption;
            if (period is not null)
            {
                foreach (var row in _multiRoutePaymentRows)
                {
                    var input=row.CreateInput();
                    var calendarError=ProgressPaymentMonthService.GetCalendarCapacityError(
                        period, input.WorkDays, input.GuideWorkDays);
                    if (calendarError is null) continue;
                    HybridSelectionSummaryTextBlock.Text=$"{row.RouteDisplayText}: {calendarError} Hakediş kaydedilemez.";
                    ProgressCalculationSummaryTextBlock.Text="GÜN SAYISINI KONTROL EDİN";
                    NetResultTextBlock.Text="—";
                    ProgressNetSummaryTextBlock.Text="—";
                    return;
                }
            }
            var results=_multiRoutePaymentRows.Select(r=>r.Preview).ToArray();
            if(results.Any(r=>r is null) || HasGridValidationError(MultiRoutePaymentGrid))
            {
                HybridSelectionSummaryTextBlock.Text="Tabloda hatalı girdi var; taslak toplamı geçerli değil.";
                ProgressCalculationSummaryTextBlock.Text="GİRDİYİ KONTROL EDİN";
                NetResultTextBlock.Text="—";ProgressNetSummaryTextBlock.Text="—";
                return;
            }
            var valid=results.Select(r=>r!).ToArray();
            decimal Sum(Func<ProgressPaymentCalculation,decimal> f)=>valid.Sum(f);
            var deductions=new DeductionBreakdown(Sum(c=>c.Deductions.Withholding),Sum(c=>c.Deductions.StampTax),
                Sum(c=>c.Deductions.TaxDebt),Sum(c=>c.Deductions.InspectionPenalty),Sum(c=>c.Deductions.SocialSecurityDebt),
                Sum(c=>c.Deductions.MinimumWageSupport),Sum(c=>c.Deductions.AdditionalGuarantee));
            var total=new ProgressPaymentCalculation(Sum(c=>c.ServiceAmount),Sum(c=>c.PriceDifferenceExcludingVat),
                Sum(c=>c.TotalAmount),Sum(c=>c.Vat),Sum(c=>c.Withholding),Sum(c=>c.InvoiceAmountAfterWithholding),
                Sum(c=>c.StampTax),Sum(c=>c.GrossAmount),Sum(c=>c.TotalDeductions),Sum(c=>c.NetPayable),
                Sum(c=>c.AdditionalGuarantee),deductions,Sum(c=>c.GuideServiceAmount),Sum(c=>c.GuidePriceDifferenceExcludingVat),
                Sum(c=>c.TransportNetAmount),Sum(c=>c.GuideNetAmount),Sum(c=>c.TransportVat),Sum(c=>c.GuideVat),
                Sum(c=>c.TransportWithholding),Sum(c=>c.GuideWithholding),_calculationConstants);
            var days=_multiRoutePaymentRows.Sum(r=>r.WorkDays);
            UpdateProgressCalculationDisplay(total,days);
            ProgressWorkDaysCardTextBlock.Text=days+" gün";
            ProgressCalculationSummaryTextBlock.Text="TASLAK · KAYDEDİLMEDİ";
            HybridSelectionSummaryTextBlock.Text=$"{valid.Length} seçili hat · Taşıma: {FormatMoney(total.ServiceAmount)} · Rehber: {FormatMoney(total.GuideServiceAmount)} · Net: {FormatMoney(total.NetPayable)} · Henüz kaydedilmedi";
        }
        catch(OverflowException)
        {HybridSelectionSummaryTextBlock.Text="Girdiler veya toplamlar hesaplanabilir sayı aralığını aşıyor.";NetResultTextBlock.Text="—";}
        finally {_refreshingHybridTotals=false;}
    }
}
