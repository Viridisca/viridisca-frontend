using System;
using System.Globalization;
using Avalonia.Data.Converters;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Converters;

/// <summary>
/// Конвертер для преобразования enum значений в локализованные строки
/// </summary>
public class EnumToLocalizedStringConverter : IValueConverter
{
    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return string.Empty;

        return value switch
        {
            // NotificationPriority
            NotificationPriority.Low => "Низкий",
            NotificationPriority.Normal => "Обычный", 
            NotificationPriority.High => "Высокий",
            NotificationPriority.Critical => "Критический",
            
            // NotificationType
            NotificationType.Info => "Информация",
            NotificationType.Warning => "Предупреждение",
            NotificationType.Error => "Ошибка",
            NotificationType.Success => "Успех",
            
            // RoleType (instead of UserRole)
            RoleType.Student => "Студент",
            RoleType.Teacher => "Преподаватель",
            RoleType.SystemAdmin => "Администратор",
            RoleType.SchoolDirector => "Супер-администратор",
            
            // StudentStatus
            StudentStatus.Active => "Активный",
            StudentStatus.Inactive => "Неактивный",
            StudentStatus.Graduated => "Выпускник",
            StudentStatus.Expelled => "Отчислен",
            StudentStatus.AcademicLeave => "В академическом отпуске",
            
            // TeacherStatus
            TeacherStatus.Active => "Активный",
            TeacherStatus.Inactive => "Неактивный",
            TeacherStatus.OnLeave => "В отпуске",
            TeacherStatus.Retired => "На пенсии",
            
            // CourseStatus
            CourseStatus.Draft => "Черновик",
            CourseStatus.Active => "Активный",
            CourseStatus.Completed => "Завершен",
            CourseStatus.Archived => "Архивный",
            
            // SubjectType
            SubjectType.Lecture => "Лекция",
            SubjectType.Practicum => "Практика",
            SubjectType.Laboratory => "Лабораторная",
            SubjectType.Seminar => "Семинар",
            
            // AssignmentType
            AssignmentType.Homework => "Домашнее задание",
            AssignmentType.Test => "Тест",
            AssignmentType.Exam => "Экзамен",
            AssignmentType.Project => "Проект",
            AssignmentType.Essay => "Эссе",
            
            // GradeType
            GradeType.Homework => "Домашнее задание",
            GradeType.Quiz => "Тест/Опрос",
            GradeType.Test => "Контрольная работа",
            GradeType.Exam => "Экзамен",
            GradeType.Project => "Проект",
            GradeType.FinalGrade => "Итоговая оценка",
            
            _ => value.ToString() ?? string.Empty
        };
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        // Обратное преобразование не поддерживается
        throw new NotImplementedException();
    }
} 