using System.IO;
using System.Windows;
using OzelEgitimHakedis.Core.Services;

namespace OzelEgitimHakedis.Wpf;
public partial class MainWindow
{
    private static OehsBackupService BackupService() => new(OehsRuntimePaths.DataRoot,OehsRuntimePaths.DesktopRoot);
    private async void BackupCreate_Click(object sender,RoutedEventArgs e)
    {
        var dialog=new Microsoft.Win32.SaveFileDialog {Filter="OEHS tam yedek|*.oehs-backup.zip",FileName=$"OEHS-{DateTime.Now:yyyy-MM-dd-HHmm}.oehs-backup.zip",AddExtension=true};
        if(dialog.ShowDialog(this)!=true) return;
        IsEnabled=false;
        try
        {
            WindowLayoutPreferenceStore.Save(_windowLayoutPreference);
            await Task.Run(()=>BackupService().Create(dialog.FileName));
            BackupStatusText.Text=$"Son başarılı yedek: {DateTime.Now:dd.MM.yyyy HH:mm}\n{dialog.FileName}";
            File.WriteAllText(Path.Combine(OehsRuntimePaths.DataRoot,"last-backup.txt"),BackupStatusText.Text);
            MessageBox.Show(this,"Tam yedek oluşturuldu. Kaydedilmemiş form girdileri pakete dahil değildir.","OEHS — Yedek",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        catch(Exception ex) {MessageBox.Show(this,"Yedek oluşturulamadı:\n"+ex.Message,"OEHS",MessageBoxButton.OK,MessageBoxImage.Error);}
        finally {IsEnabled=true;}
    }
    private bool HasOpenWorkForRestore() => IsArchiveEditing || _editingHistoryId.HasValue || _historyCorrectionTargetId.HasValue ||
        _editingContractTender is not null || _editingContractorKey is not null || _editingRoute is not null ||
        !string.IsNullOrWhiteSpace(TenderNumberTextBox.Text) || !string.IsNullOrWhiteSpace(RouteNumberTextBox.Text) ||
        !string.IsNullOrWhiteSpace(RouteNameTextBox.Text) || !string.IsNullOrWhiteSpace(ContractorNameTextBox.Text) ||
        !string.IsNullOrWhiteSpace(ContractorTaxTextBox.Text) ||
        TryParseDecimal(WorkDaysTextBox.Text,out var days)&&days!=0 ||
        TurkishMoneyInput.TryParse(PriceDifferenceTextBox.Text,out var difference)&&difference!=0 ||
        TryParseDecimal(GuideWorkDaysTextBox.Text,out var guideDays)&&guideDays!=0 ||
        _multiRoutePaymentRows.Count>0;
    private async void BackupRestore_Click(object sender,RoutedEventArgs e)
    {
        if(HasOpenWorkForRestore()) {MessageBox.Show(this,"Önce açık düzenlemeyi kaydedin veya formu temizleyin. Geri yükleme kaydedilmemiş girdileri değiştirebilir.","OEHS",MessageBoxButton.OK,MessageBoxImage.Information);return;}
        var dialog=new Microsoft.Win32.OpenFileDialog {Filter="OEHS tam yedek|*.oehs-backup.zip|ZIP|*.zip"};
        if(dialog.ShowDialog(this)!=true) return;
        IsEnabled=false;
        try
        {
            var service=BackupService();var summary=await Task.Run(()=>service.Inspect(dialog.FileName));
            var message=$"Yedek tarihi: {summary.CreatedAt.ToLocalTime():dd.MM.yyyy HH:mm}\n"+
                $"Sözleşme: {summary.ContractCount} · Hat: {summary.RouteCount} · Hakediş: {summary.PaymentCount}\n"+
                $"Arşiv revizyonu: {summary.ArchiveCount} · PDF: {summary.PdfCount} · Dosya: {summary.FileCount}\n\n"+
                "İki programın mevcut kayıtları ve çıktı klasörü bu yedekle değiştirilecek. Önce mevcut veriler için otomatik kurtarma yedeği alınır. Devam edilsin mi?";
            if(MessageBox.Show(this,message,"OEHS — Yedeği incele",MessageBoxButton.YesNo,MessageBoxImage.Warning,MessageBoxResult.No)!=MessageBoxResult.Yes) return;
            var recovery=await Task.Run(()=>
            {
                if(OutputEvidenceStore.FileHash(dialog.FileName)!=summary.PackageHash) throw new IOException("İncelenen paket değişmiş. Yeniden seçin.");
                return service.Restore(dialog.FileName);
            });
            _dataLoadFailed=false;LoadPersistedData();
            if(_dataLoadFailed) throw new IOException("Geri yüklenen veri açılamadı. Kurtarma yedeği: "+recovery);
            PopulateCalculationConstantsPanel();UpdateContractSelectors();UpdateRouteSummary();
            UpdateContractorSummary();UpdateContractSummaryView();UpdateSummaryView();UpdateReportView();UpdateAcceptanceView();
            UpdateHistoryView();UpdateArchiveView();RefreshHomeOperations();LoadSignatoriesIntoFields();
            _windowLayoutPreference=WindowLayoutPreferenceStore.Load();ApplyWindowLayoutPreference(_windowLayoutPreference);
            BackupStatusText.Text="Yedek geri yüklendi. Önceki verilerin kurtarma paketi:\n"+recovery;
            MessageBox.Show(this,BackupStatusText.Text,"OEHS — Geri yükleme",MessageBoxButton.OK,MessageBoxImage.Information);
        }
        catch(Exception ex) {MessageBox.Show(this,"Geri yükleme tamamlanamadı:\n"+ex.Message,"OEHS",MessageBoxButton.OK,MessageBoxImage.Error);}
        finally {IsEnabled=true;}
    }
}
