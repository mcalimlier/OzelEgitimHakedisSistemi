using System.IO;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Microsoft.Win32;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private void HybridExportExcelButton_Click(object sender, RoutedEventArgs e)
    {
        if(!CommitHybridExportEdits()) return;
        var dialog=new SaveFileDialog
        {
            Filter="Excel çalışma kitabı (*.xlsx)|*.xlsx",DefaultExt=".xlsx",AddExtension=true,
            FileName="Secili-Hatlar.xlsx"
        };
        if(dialog.ShowDialog(this)==true) SaveHybridTableExcel(dialog.FileName);
    }

    private bool CommitHybridExportEdits() =>
        MultiRoutePaymentGrid.CommitEdit(DataGridEditingUnit.Cell,true) &&
        MultiRoutePaymentGrid.CommitEdit(DataGridEditingUnit.Row,true) && !HasGridValidationError(MultiRoutePaymentGrid);

    private bool SaveHybridTableExcel(string path)
    {
        if(!CommitHybridExportEdits() || _multiRoutePaymentRows.Count==0 || _multiRoutePaymentRows.Any(r=>r.Preview is null)) return false;
        var columns=MultiRoutePaymentGrid.Columns.Where(c=>c.Visibility==Visibility.Visible).OrderBy(c=>c.DisplayIndex).ToArray();
        if(columns.Length==0) return false;
        var properties=columns.Select(c=>(c as DataGridBoundColumn)?.Binding as Binding)
            .Select(b=>b?.Path?.Path).ToArray();
        if(properties.Any(p=>p is null || typeof(MultiRoutePaymentDraftRow).GetProperty(p) is null)) return false;
        IReadOnlyList<object?>[] values=_multiRoutePaymentRows.Select(row=>(IReadOnlyList<object?>)properties.Select(p=>
            p==nameof(MultiRoutePaymentDraftRow.NetPayableText)?(object?)row.Preview!.NetPayable:
            typeof(MultiRoutePaymentDraftRow).GetProperty(p!)!.GetValue(row)).ToArray()).ToArray();
        try
        {
            ExcelDocumentExporter.SaveTable(path,columns.Select(c=>c.Header?.ToString()??"").ToArray(),values);
            HybridSelectionSummaryTextBlock.Text="Görünen tablo Excel dosyasına aktarıldı.";
            return true;
        }
        catch(Exception ex) when(ex is IOException or UnauthorizedAccessException or ArgumentException or System.Xml.XmlException)
        {
            HybridSelectionSummaryTextBlock.Text="Excel dosyası kaydedilemedi: "+ex.Message;
            return false;
        }
    }
}
