using System;

namespace ViridiscaUi.Domain.Models.Education;

/// <summary>
/// Детальная информация об уроке
/// </summary>
public class LessonDetail
{
    /// <summary>
    /// Идентификатор урока
    /// </summary>
    public Guid LessonUid { get; set; }

    /// <summary>
    /// Тема урока
    /// </summary>
    public string Topic { get; set; } = string.Empty;

    /// <summary>
    /// Описание урока
    /// </summary>
    public string? Description { get; set; }
    
    /// <summary>
    /// Время начала урока
    /// </summary>
    public DateTime StartTime { get; set; }

    /// <summary>
    /// Время окончания урока
    /// </summary>
    public DateTime EndTime { get; set; }
    
    /// <summary>
    /// Имя преподавателя
    /// </summary>
    public string TeacherFirstName { get; set; } = string.Empty;

    /// <summary>
    /// Фамилия преподавателя
    /// </summary>
    public string TeacherLastName { get; set; } = string.Empty;

    /// <summary>
    /// Отчество преподавателя
    /// </summary>
    public string? TeacherMiddleName { get; set; }
    
    /// <summary>
    /// Название предмета
    /// </summary>
    public string SubjectName { get; set; } = string.Empty;

    /// <summary>
    /// Название группы
    /// </summary>
    public string GroupName { get; set; } = string.Empty;
    
    /// <summary>
    /// Признак отмены урока
    /// </summary>
    public bool IsCancelled { get; set; }

    /// <summary>
    /// Признак завершения урока
    /// </summary>
    public bool IsCompleted { get; set; }

    /// <summary>
    /// Полное имя преподавателя (Фамилия Имя Отчество)
    /// </summary>
    public string FullName => $"{TeacherLastName} {TeacherFirstName} {TeacherMiddleName}".Trim();

    /// <summary>
    /// Продолжительность урока в минутах
    /// </summary>
    public int DurationMinutes => (int)(EndTime - StartTime).TotalMinutes;

    /// <summary>
    /// Создает новый экземпляр детальной информации об уроке
    /// </summary>
    public LessonDetail()
    {
    }

    /// <summary>
    /// Создает новый экземпляр детальной информации об уроке с указанными параметрами
    /// </summary>
    public LessonDetail(
        Guid lessonUid,
        string topic,
        DateTime startTime,
        DateTime endTime,
        string teacherLastName,
        string teacherFirstName,
        string? teacherMiddleName,
        string subjectName,
        string groupName,
        string? description,
        bool isCancelled = false,
        bool isCompleted = false)
    {
        LessonUid = lessonUid;
        Topic = topic;
        StartTime = startTime;
        EndTime = endTime;
        TeacherLastName = teacherLastName;
        TeacherFirstName = teacherFirstName;
        TeacherMiddleName = teacherMiddleName;
        SubjectName = subjectName;
        GroupName = groupName;
        Description = description;
        IsCancelled = isCancelled;
        IsCompleted = isCompleted;
    }

    /// <summary>
    /// Создает детальную информацию об уроке на основе объекта урока
    /// </summary>
    public static LessonDetail FromLesson(Lesson lesson)
    {
        ArgumentNullException.ThrowIfNull(lesson);

        // Для новой модели Lesson используем доступные свойства
        return new LessonDetail(
            lesson.Uid,
            lesson.Title, // Используем Title вместо Topic
            DateTime.Now, // Временная заглушка для StartTime
            DateTime.Now.AddHours(1), // Временная заглушка для EndTime
            "Unknown", // Временная заглушка для LastName
            "Teacher", // Временная заглушка для FirstName
            "", // Временная заглушка для MiddleName
            "Unknown Subject", // Временная заглушка для Subject
            "Unknown Group", // Временная заглушка для Group
            lesson.Description,
            false, // Временная заглушка для IsCancelled
            false); // Временная заглушка для IsCompleted
    }
}
