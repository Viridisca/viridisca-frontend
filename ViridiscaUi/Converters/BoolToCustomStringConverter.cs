using System;
using System.Globalization;
using Avalonia.Data.Converters;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования boolean значений в пользовательские строки
/// </summary>
public class BoolToCustomStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool boolValue)
            return string.Empty;

        if (parameter is not string parameterString)
            return boolValue.ToString();

        // Параметр должен быть в формате "TrueString|FalseString"
        var parts = parameterString.Split('|');
        if (parts.Length != 2)
            return boolValue.ToString();

        return boolValue ? parts[0] : parts[1];
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        throw new NotImplementedException();
    }
} 