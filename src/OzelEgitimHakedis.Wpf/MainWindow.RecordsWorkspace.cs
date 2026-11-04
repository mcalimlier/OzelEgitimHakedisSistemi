using System.Diagnostics;
using System.IO;
using System.Windows;
using System.Windows.Controls;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private bool _changingRecordsTab;

    private TransportationProgramType? RecordsProgram =>
        Enum.TryParse<TransportationProgramType>(Choice(RecordsProgramComboBox), out var program)
            ? program
            : HomeProgram ?? GetSelectedTransportationProgramType();

    private void SelectRecordsSection(string page)
    {
        if (page is not ("History" or "Archives")) return;
        _changingRecordsTab = true;
        try { RecordsTabs.SelectedIndex = page == "Archives" ? 1 : 0; }
        finally { _changingRecordsTab = false; }
    }

    private void RecordsTabs_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!IsInitialized || _changingRecordsTab || !ReferenceEquals(e.OriginalSource, RecordsTabs)) return;
        if (RecordsTabs.SelectedItem is TabItem { Tag: string page })
            NavigationButton_Click(new Button { Tag = page }, new RoutedEventArgs());
    }

    private void NavigateToArchives() =>
        NavigationButton_Click(new Button { Tag = "Archives" }, new RoutedEventArgs());

    private void RecordsGrid_SizeChanged(object sender, SizeChangedEventArgs e)
    {
        if (sender is DataGrid grid) FitRecordColumns(grid);
    }

    private void RecordsGrid_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
    {
        if (sender is DataGrid grid && grid.IsVisible) FitRecordColumns(grid);
    }

    private static void FitRecordColumns(DataGrid grid)
    {
        if (grid.Columns.Count < 4 || grid.ActualWidth <= 0) return;
        var first = grid.Columns[0]; var second = grid.Columns[1];
        var fixedWidth = grid.Columns.Skip(2).Sum(column => column.MinWidth);
        var extra = Math.Max(0d, grid.ActualWidth - fixedWidth - SystemParameters.VerticalScrollBarWidth - 2d - first.MinWidth - second.MinWidth);
        first.Width = new DataGridLength(first.MinWidth + extra / 2d);
        second.Width = new DataGridLength(second.MinWidth + extra / 2d);
    }

    private void OpenRecordsFolder(string? folder, bool archive)
    {
        var status = archive ? ArchiveStatusTextBlock : HistoryOperationStatusTextBlock;
        status.Visibility = Visibility.Visible;
        if (string.IsNullOrWhiteSpace(folder) || !Directory.Exists(folder))
        {
            status.Text = archive ? "Arşiv klasörü bulunamadı. Listeyi yenileyip kaydı kontrol edin."
                : "Bu hakediş için çıktı klasörü henüz yok. Çıktılar ekranından PDF'leri kaydedebilirsiniz.";
            return;
        }
        try { Process.Start(new ProcessStartInfo { FileName = folder, UseShellExecute = true }); }
        catch (Exception error) when (error is IOException or UnauthorizedAccessException or System.ComponentModel.Win32Exception or InvalidOperationException)
        { status.Text = $"Klasör açılamadı: {error.Message}"; }
    }
}
