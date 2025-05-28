using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования строкового ключа иконки в MaterialIconKind enum
/// </summary>
public class IconKeyToMaterialKindConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is string iconKey && !string.IsNullOrEmpty(iconKey))
        {
            try
            {
                // Пытаемся преобразовать строку в MaterialIconKind enum
                if (Enum.TryParse<MaterialIconKind>(iconKey, true, out var result))
                {
                    return result;
                }
                
                // Попробуем некоторые альтернативные варианты
                var alternativeKey = iconKey switch
                {
                    "AccountGroup" => MaterialIconKind.AccountGroup,
                    "AccountTie" => MaterialIconKind.AccountTie,
                    "Account" => MaterialIconKind.Account,
                    "StarBox" => MaterialIconKind.StarBox,
                    "BookOpenPageVariant" => MaterialIconKind.BookOpenPageVariant,
                    "BookMultiple" => MaterialIconKind.BookMultiple,
                    "AccountMultiple" => MaterialIconKind.AccountMultiple,
                    "ClipboardText" => MaterialIconKind.ClipboardText,
                    "Lock" => MaterialIconKind.Lock,
                    "Home" => MaterialIconKind.Home,
                    "Settings" => MaterialIconKind.Settings,
                    "School" => MaterialIconKind.School,
                    "Book" => MaterialIconKind.Book,
                    "Assignment" => MaterialIconKind.Assignment,
                    "Grade" => MaterialIconKind.Grade,
                    "Group" => MaterialIconKind.Group,
                    "Person" => MaterialIconKind.Person,
                    "Dashboard" => MaterialIconKind.ViewDashboard,
                    "Building" => MaterialIconKind.Building,
                    "Profile" => MaterialIconKind.Account,
                    // Добавляем новые иконки для образовательной системы
                    "ViewDashboard" => MaterialIconKind.ViewDashboard,
                    "ChartLine" => MaterialIconKind.ChartLine,
                    "Calendar" => MaterialIconKind.Calendar,
                    "Folder" => MaterialIconKind.Folder,
                    "FileDocument" => MaterialIconKind.FileDocument,
                    "Email" => MaterialIconKind.Email,
                    "Bell" => MaterialIconKind.Bell,
                    "Cog" => MaterialIconKind.Cog,
                    "Exit" => MaterialIconKind.ExitToApp,
                    _ => MaterialIconKind.HelpCircle
                };
                
                return alternativeKey;
            }
            catch
            {
                // Игнорируем ошибки парсинга
            }
        }
        
        // Возвращаем иконку по умолчанию
        return MaterialIconKind.HelpCircle;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }
} 