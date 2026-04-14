using System.Globalization;
using System.Windows;
using System.Windows.Data;
using System.Windows.Media;

namespace MTS.UI.Converters;

/// <summary>
/// Converts a bool (IsXxxActive) → White when true, TextSecondaryBrush when false.
/// Used for nav icon and label colour in the sidebar.
/// </summary>
[ValueConversion(typeof(bool), typeof(Brush))]
public sealed class NavIconBrushConverter : IValueConverter
{
    public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
    {
        bool isActive = value is bool b && b;

        if (isActive)
            return Brushes.White;

        // Fall back to the application-level TextSecondaryBrush resource
        if (Application.Current?.Resources["TextSecondaryBrush"] is Brush secondary)
            return secondary;

        return new SolidColorBrush(Color.FromRgb(0x90, 0x97, 0xB5));
    }

    public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => Binding.DoNothing;
}
