namespace ViridiscaUi.Converters;

/// <summary>
/// Статические экземпляры конвертеров для строк
/// </summary>
public static class StringConverters
{
    /// <summary>
    /// Конвертер для преобразования boolean в пользовательские строки
    /// </summary>
    public static readonly BoolToCustomStringConverter BoolToCustomString = new();
} 