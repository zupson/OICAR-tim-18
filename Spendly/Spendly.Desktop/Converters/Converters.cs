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

public class BoolToBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true
            ? new SolidColorBrush(Color.FromRgb(0x22, 0xC5, 0x5E))
            : new SolidColorBrush(Color.FromRgb(0xEF, 0x44, 0x44));

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => throw new NotSupportedException();
}
