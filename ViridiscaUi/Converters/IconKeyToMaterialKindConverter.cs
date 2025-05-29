using System;
using System.Globalization;
using Avalonia.Data.Converters;
using Material.Icons;
using ViridiscaUi.Infrastructure;

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
                // Сначала логируем попытку конвертации
                StatusLogger.LogDebug($"Converting icon key: '{iconKey}'", "IconConverter");
                
                // Пытаемся преобразовать строку в MaterialIconKind enum
                if (Enum.TryParse<MaterialIconKind>(iconKey, true, out var result))
                {
                    StatusLogger.LogDebug($"Successfully converted '{iconKey}' to MaterialIconKind.{result}", "IconConverter");
                    return result;
                }
                
                // Попробуем некоторые альтернативные варианты
                var alternativeKey = iconKey switch
                {
                    // Основные иконки
                    "Home" => MaterialIconKind.Home,
                    "Settings" => MaterialIconKind.Settings,
                    "Account" => MaterialIconKind.Account,
                    "Lock" => MaterialIconKind.Lock,
                    "Building" => MaterialIconKind.Building,
                    "Profile" => MaterialIconKind.Account,
                    
                    // Образовательные иконки
                    "AccountGroup" => MaterialIconKind.AccountGroup,
                    "AccountTie" => MaterialIconKind.AccountTie,
                    "BookOpenPageVariant" => MaterialIconKind.BookOpenPageVariant,
                    "BookMultiple" => MaterialIconKind.BookMultiple,
                    "AccountMultiple" => MaterialIconKind.AccountMultiple,
                    "ClipboardText" => MaterialIconKind.ClipboardText,
                    "School" => MaterialIconKind.School,
                    "Book" => MaterialIconKind.Book,
                    "Assignment" => MaterialIconKind.Assignment,
                    "Grade" => MaterialIconKind.Grade,
                    "Group" => MaterialIconKind.Group,
                    "Person" => MaterialIconKind.Person,
                    
                    // Навигационные иконки
                    "Dashboard" => MaterialIconKind.ViewDashboard,
                    "ViewDashboard" => MaterialIconKind.ViewDashboard,
                    "ChartLine" => MaterialIconKind.ChartLine,
                    "Calendar" => MaterialIconKind.Calendar,
                    "Folder" => MaterialIconKind.Folder,
                    "FileDocument" => MaterialIconKind.FileDocument,
                    "Email" => MaterialIconKind.Email,
                    "Bell" => MaterialIconKind.Bell,
                    "Cog" => MaterialIconKind.Cog,
                    "Exit" => MaterialIconKind.ExitToApp,
                    
                    // Системные иконки
                    "Domain" => MaterialIconKind.Domain,
                    "DomainOff" => MaterialIconKind.DomainOff,
                    "Information" => MaterialIconKind.Information,
                    "InformationOutline" => MaterialIconKind.InformationOutline,
                    "AlertCircle" => MaterialIconKind.AlertCircle,
                    "CheckCircle" => MaterialIconKind.CheckCircle,
                    "PauseCircle" => MaterialIconKind.PauseCircle,
                    "CloseCircle" => MaterialIconKind.CloseCircle,
                    
                    // Действия
                    "Plus" => MaterialIconKind.Plus,
                    "Pencil" => MaterialIconKind.Pencil,
                    "Delete" => MaterialIconKind.Delete,
                    "Refresh" => MaterialIconKind.Refresh,
                    "Magnify" => MaterialIconKind.Magnify,
                    "Close" => MaterialIconKind.Close,
                    "ContentSave" => MaterialIconKind.ContentSave,
                    "Cancel" => MaterialIconKind.Cancel,
                    "Check" => MaterialIconKind.Check,
                    "Eye" => MaterialIconKind.Eye,
                    "Play" => MaterialIconKind.Play,
                    "Pause" => MaterialIconKind.Pause,
                    "Help" => MaterialIconKind.Help,
                    
                    // Файлы и экспорт
                    "FileExcel" => MaterialIconKind.FileExcel,
                    "FileDelimited" => MaterialIconKind.FileDelimited,
                    "Export" => MaterialIconKind.Export,
                    "Import" => MaterialIconKind.Import,
                    "FileExport" => MaterialIconKind.FileExport,
                    
                    // Обработка эмодзи иконок (фолбэк для старых записей)
                    "📚" => MaterialIconKind.BookOpenPageVariant,
                    "👨‍🏫" => MaterialIconKind.AccountTie,
                    "👤" => MaterialIconKind.Account,
                    "🔑" => MaterialIconKind.Key,
                    "📝" => MaterialIconKind.Pencil,
                    
                    // Иконка по умолчанию
                    _ => MaterialIconKind.HelpCircle
                };
                
                StatusLogger.LogDebug($"Converted '{iconKey}' using fallback mapping to MaterialIconKind.{alternativeKey}", "IconConverter");
                return alternativeKey;
            }
            catch (Exception ex)
            {
                StatusLogger.LogError($"Error converting icon key '{iconKey}': {ex.Message}", "IconConverter");
            }
        }
        
        // Возвращаем иконку по умолчанию
        StatusLogger.LogWarning($"Using default icon for key: '{value}'", "IconConverter");
        return MaterialIconKind.HelpCircle;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        return value?.ToString();
    }
} 