using System;
using System.Collections.Generic;
using System.Globalization;
using Avalonia.Data.Converters;
using Avalonia.Media;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для определения цвета индикатора статуса
/// </summary>
public class StatusColorConverter : IMultiValueConverter
{
    public object? Convert(IList<object?> values, Type targetType, object? parameter, CultureInfo culture)
    {
        if (values == null || values.Count < 2)
            return Colors.Green;

        var errorsCount = values[0] as int? ?? 0;
        var warningsCount = values[1] as int? ?? 0;

        if (errorsCount > 0)
            return Colors.Red;
        
        if (warningsCount > 0)
            return Colors.Orange;
        
        return Colors.Green;
    }

    public object?[] ConvertBack(object? value, Type[] targetTypes, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
} 