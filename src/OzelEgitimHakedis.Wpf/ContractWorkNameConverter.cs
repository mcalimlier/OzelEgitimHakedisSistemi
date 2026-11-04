using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Windows.Data;
using OzelEgitimHakedis.Core.Models;

namespace OzelEgitimHakedis.Wpf;

public sealed class ContractWorkNameConverter : IMultiValueConverter, INotifyPropertyChanged
{
    public event PropertyChangedEventHandler? PropertyChanged;
    public IReadOnlyDictionary<ContractDraftData, string> Titles { get; private set; } =
        new Dictionary<ContractDraftData, string>();

    internal void UpdateTitles(IReadOnlyDictionary<ContractDraftData, string> titles)
    {
        Titles = titles;
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Titles)));
    }

    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values.Length == 0 || values[0] is not ContractDraftData contract)
            return string.Empty;

        return values.Length > 1 && values[1] is IReadOnlyDictionary<ContractDraftData, string> titles
            && titles.TryGetValue(contract, out var title)
            ? title
            : contract.WorkName;
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture) =>
        throw new NotSupportedException("Contract work titles are display-only.");
}
