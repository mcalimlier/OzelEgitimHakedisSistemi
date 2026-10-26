using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace OzelEgitimHakedis.Wpf;

public sealed class TurkishMoneyConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is decimal amount ? TurkishMoneyInput.Format(amount) : string.Empty;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture) =>
        value is string text && TurkishMoneyInput.TryParse(text, out var amount)
            ? amount : DependencyProperty.UnsetValue;
}
