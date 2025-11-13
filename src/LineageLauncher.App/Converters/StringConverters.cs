using System;
using System.Globalization;
using System.Windows;
using System.Windows.Data;

namespace LineageLauncher.App.Converters;

/// <summary>
/// Converts a string to Visibility (empty/null = Collapsed, non-empty = Visible).
/// </summary>
[ValueConversion(typeof(string), typeof(Visibility))]
public class StringToVisibilityConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return string.IsNullOrWhiteSpace(stringValue) ? Visibility.Collapsed : Visibility.Visible;
        }
        return Visibility.Collapsed;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}

/// <summary>
/// Converts a string to boolean (empty/null = false, non-empty = true).
/// </summary>
[ValueConversion(typeof(string), typeof(bool))]
public class StringToBoolConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        if (value is string stringValue)
        {
            return !string.IsNullOrWhiteSpace(stringValue);
        }
        return false;
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
