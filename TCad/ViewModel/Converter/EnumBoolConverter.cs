using System;
using System.Windows.Data;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

namespace TCad.ViewModel;

public class EnumBoolConverter<T> : IValueConverter
{
    // Convert parameter to Enum
    private T ConvertParameter(object parameter)
    {
        string parameterString = parameter as string;
        return (T)Enum.Parse(typeof(T), parameterString);
    }

    // Enum -> bool
    public object Convert(object value, Type targetType, object parameter,
                          System.Globalization.CultureInfo culture)
    {
        T parameterValue = ConvertParameter(parameter);

        return parameterValue.Equals(value);
    }

    // bool -> Enum
    public object ConvertBack(object value, Type targetType, object parameter,
                              System.Globalization.CultureInfo culture)
    {
        // ignore case that true->false
        if (!(bool)value)
            return System.Windows.DependencyProperty.UnsetValue;

        return ConvertParameter(parameter);
    }
}
