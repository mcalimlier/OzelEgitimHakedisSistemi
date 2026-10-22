using System.Reflection;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;

namespace OzelEgitimHakedis.Wpf;

public partial class MainWindow
{
    private sealed record HybridPasteUndoCell(
        MultiRoutePaymentDraftRow Row, PropertyInfo Property, decimal Before, decimal Applied);

    private IReadOnlyList<HybridPasteUndoCell>? _hybridPasteUndo;

    private void HybridGrid_PreviewKeyDown(object sender, KeyEventArgs e)
    {
        if ((Keyboard.Modifiers & ModifierKeys.Control) == 0)
            return;

        if (e.Key == Key.V)
        {
            e.Handled = true;
            PasteHybridGridFromClipboard();
        }
        else if (e.Key == Key.Z && _hybridPasteUndo is { Count: > 0 })
        {
            e.Handled = true;
            UndoHybridGridPaste();
        }
    }

    private void HybridPasteButton_Click(object sender, RoutedEventArgs e) =>
        PasteHybridGridFromClipboard();

    private void HybridUndoButton_Click(object sender, RoutedEventArgs e) =>
        UndoHybridGridPaste();

    private void PasteHybridGridFromClipboard()
    {
        try
        {
            TryPasteHybridGridText(Clipboard.GetText());
        }
        catch (ExternalException)
        {
            ShowMultiRouteValidation("Windows panosundaki veriler okunamadı. Kopyalayıp yeniden deneyin.");
        }
    }

    private bool TryPasteHybridGridText(string clipboardText)
    {
        if (!TryCommitHybridGrid())
            return false;

        if (MultiRoutePaymentGrid.CurrentItem is not MultiRoutePaymentDraftRow startingRow
            || MultiRoutePaymentGrid.CurrentCell.Column is not DataGridColumn startColumn)
        {
            ShowMultiRouteValidation("Önce Excel-hibrit tabloda yapıştırmanın başlayacağı hücreyi seçin.");
            return false;
        }

        var firstRow = _multiRoutePaymentRows.IndexOf(startingRow);
        var columns = MultiRoutePaymentGrid.Columns
            .Where(column => column.Visibility == Visibility.Visible)
            .OrderBy(column => column.DisplayIndex)
            .ToArray();
        var firstColumn = Array.IndexOf(columns, startColumn);
        var paths = columns.Select(column =>
            column is DataGridBoundColumn bound && bound.Binding is Binding binding
                ? binding.Path?.Path ?? string.Empty
                : string.Empty).ToArray();

        var success = HybridClipboardPlan.TryCreate(
            clipboardText, paths, firstRow, firstColumn, _multiRoutePaymentRows.Count,
            (rowIndex, field) =>
            {
                var columnIndex = Array.IndexOf(paths, field);
                if (columnIndex < 0 || columns[columnIndex].IsReadOnly)
                    return false;
                var row = _multiRoutePaymentRows[rowIndex];
                if (field.StartsWith("Guide", StringComparison.Ordinal)
                    && row.CreateInput().ProgramType !=
                       Core.Models.TransportationProgramType.SpecialEducation)
                    return false;
                if (field == nameof(MultiRoutePaymentDraftRow.MinimumWageSupport)
                    && !row.MinimumWageSupportEnabled)
                    return false;
                return true;
            }, out var plan, out var error);

        if (!success)
        {
            ShowMultiRouteValidation(error);
            return false;
        }

        var changes = new List<HybridPasteUndoCell>();
        foreach (var cell in plan)
        {
            var row = _multiRoutePaymentRows[cell.RowIndex];
            var property = typeof(MultiRoutePaymentDraftRow).GetProperty(cell.FieldName);
            if (property is null || property.PropertyType != typeof(decimal)
                || !property.CanWrite || !property.CanRead)
            {
                ShowMultiRouteValidation("Tablodaki yapıştırma alanı tanımlı değil; hiçbir hücre değiştirilmedi.");
                return false;
            }
            changes.Add(new HybridPasteUndoCell(
                row, property, (decimal)property.GetValue(row)!, cell.Value));
        }

        var applied = 0;
        try
        {
            foreach (var change in changes)
            {
                change.Property.SetValue(change.Row, change.Applied);
                applied++;
            }
        }
        catch (Exception exception) when (exception is TargetInvocationException
                                           or ArgumentException or InvalidOperationException
                                           or OverflowException)
        {
            for (var index = applied - 1; index >= 0; index--)
                changes[index].Property.SetValue(changes[index].Row, changes[index].Before);
            ShowMultiRouteValidation("Yapıştırma tamamlanamadı; değişiklikler geri alındı.");
            return false;
        }

        _hybridPasteUndo = changes;
        ProgressValidationBanner.Visibility = Visibility.Collapsed;
        ProgressValidationMessageTextBlock.Text = string.Empty;
        RefreshHybridTotals();
        return true;
    }

    private void UndoHybridGridPaste()
    {
        if (_hybridPasteUndo is not { Count: > 0 } changes)
            return;

        if (changes.Any(change =>
                !_multiRoutePaymentRows.Contains(change.Row)
                || (decimal)change.Property.GetValue(change.Row)! != change.Applied))
        {
            _hybridPasteUndo = null;
            ShowMultiRouteValidation(
                "Yapıştırmadan sonra satırlar veya değerler değişti. Yeni düzenlemeleri silmemek için otomatik geri alma yapılmadı.");
            return;
        }

        foreach (var change in changes.Reverse())
            change.Property.SetValue(change.Row, change.Before);

        _hybridPasteUndo = null;
        RefreshHybridTotals();
    }
}
