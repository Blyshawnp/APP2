using System.Globalization;
using System.Windows.Data;

namespace MTS.UI.Converters;

/// <summary>
/// Converts a bool? value for use with paired RadioButtons (Yes/No).
/// ConverterParameter: "true" → RadioButton represents Yes; "false" → represents No.
///
/// Convert:     bool?   → bool  (RadioButton.IsChecked)
/// ConvertBack: bool    → bool? (selected → stored value)
/// </summary>
[ValueConversion(typeof(bool?), typeof(bool))]
public sealed class NullableBoolConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (parameter is not string paramStr) return false;
        bool radioIsYes = string.Equals(paramStr, "true", StringComparison.OrdinalIgnoreCase);

        return value switch
        {
            true  => radioIsYes,
            false => !radioIsYes,
            _     => false   // null → neither selected
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not true) return Binding.DoNothing; // only process the "checked" event
        if (parameter is not string paramStr) return null;

        return string.Equals(paramStr, "true", StringComparison.OrdinalIgnoreCase)
            ? (bool?)true
            : (bool?)false;
    }
}
