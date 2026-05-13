using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;
using Spendly.Desktop.Models;

namespace Spendly.Desktop.Converters;

public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is string s && !string.IsNullOrEmpty(s) ? Visibility.Visible : Visibility.Collapsed;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class AlertLevelToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is AlertLevel level ? level switch
        {
            AlertLevel.Critical => new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44)),
            AlertLevel.Warning  => new SolidColorBrush(Color.FromRgb(0xF5, 0x9E, 0x0B)),
            _                   => new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E)),
        } : Brushes.Gray;

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class StringToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        try { return new SolidColorBrush((Color)ColorConverter.ConvertFromString(value?.ToString() ?? "#3B82F6")); }
        catch { return Brushes.Gray; }
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

// values: [decimal amount, decimal exchangeRate, string symbol]
// optional 4th value: bool isIncome — adds +/- sign
public class CurrencyConverter : IMultiValueConverter
{
    public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
    {
        if (values[0] is not decimal amount || values[1] is not decimal rate || values[2] is not string symbol)
            return "";
        var converted = amount * rate;
        if (values.Length == 4 && values[3] is bool isIncome)
            return isIncome ? $"+{converted:N2} {symbol}" : $"-{converted:N2} {symbol}";
        return $"{converted:N2} {symbol}";
    }

    public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
