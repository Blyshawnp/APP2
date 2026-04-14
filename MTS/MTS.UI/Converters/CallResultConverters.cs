using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;
using MTS.Core.Enums;

namespace MTS.UI.Converters;

[ValueConversion(typeof(CallResult?), typeof(Brush))]
public class CallResultToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            CallResult.Pass => new SolidColorBrush(Color.FromRgb(34, 197, 94)),  // green
            CallResult.Fail => new SolidColorBrush(Color.FromRgb(239, 68, 68)),  // red
            _               => new SolidColorBrush(Color.FromRgb(80, 80, 100))   // neutral
        };

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => null;
}

[ValueConversion(typeof(SessionStatus), typeof(Brush))]
public class SessionStatusToBrushConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => value switch
        {
            SessionStatus.Pass       => new SolidColorBrush(Color.FromRgb(34, 197, 94)),
            SessionStatus.Fail       => new SolidColorBrush(Color.FromRgb(239, 68, 68)),
            SessionStatus.Incomplete => new SolidColorBrush(Color.FromRgb(245, 158, 11)),
            _                        => new SolidColorBrush(Color.FromRgb(80, 80, 100))
        };

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => null;
}

[ValueConversion(typeof(bool?), typeof(bool))]
public class EnumEqualityConverter : IValueConverter
{
    public object Convert(object? value, Type targetType, object parameter, CultureInfo culture)
        => Equals(value, parameter);

    public object? ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        => value is true ? parameter : System.Windows.Data.Binding.DoNothing;
}
