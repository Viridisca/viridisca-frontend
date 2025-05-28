using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Сервис для работы с диалоговыми окнами
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    Task ShowInfoAsync(string title, string message);
    
    /// <summary>
    /// Показывает сообщение об ошибке
    /// </summary>
    Task ShowErrorAsync(string title, string message);
    
    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    Task ShowWarningAsync(string title, string message);
    
    /// <summary>
    /// Показывает запрос подтверждения
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);
    
    /// <summary>
    /// Показывает запрос подтверждения с кастомными кнопками
    /// </summary>
    Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет");
    
    /// <summary>
    /// Показывает диалог с вводом текста
    /// </summary>
    Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
    
    /// <summary>
    /// Показывает диалог с вводом текста (альтернативное название)
    /// </summary>
    Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "");
    
    /// <summary>
    /// Показывает диалог выбора файла для открытия
    /// </summary>
    Task<string?> ShowFileOpenDialogAsync(string title, string[] filters);
    
    /// <summary>
    /// Показывает диалог с выбором из списка
    /// </summary>
    Task<T?> ShowSelectionDialogAsync<T>(string title, string message, T[] items);

    Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel);
    Task<Student?> ShowStudentEditorDialogAsync(Student? student = null);
    
    // Диалоги для студентов
    Task<Student?> ShowStudentEditDialogAsync(Student student);
    Task ShowStudentDetailsDialogAsync(Student student);
    
    // Диалоги для департаментов
    Task<Department?> ShowDepartmentEditDialogAsync(Department department);
    Task ShowDepartmentDetailsDialogAsync(Department department);
    
    // Диалоги для предметов
    Task<Subject?> ShowSubjectEditDialogAsync(Subject subject);
    Task ShowSubjectDetailsDialogAsync(Subject subject);
    
    // Диалоги для групп
    Task<Group?> ShowGroupEditDialogAsync(Group group);
    Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers);
    Task<object?> ShowGroupStudentsManagementDialogAsync(Group group, IEnumerable<Student> allStudents);
    
    // Диалоги для курсов
    Task<Course?> ShowCourseEditDialogAsync(Course course);
    Task<object?> ShowCourseEnrollmentDialogAsync(Course course, IEnumerable<Student> allStudents);
    Task<object?> ShowCourseContentManagementDialogAsync(Course course);
    Task<object?> ShowCourseStudentsManagementDialogAsync(Course course, IEnumerable<Student> allStudents);
    Task<object?> ShowCourseStatisticsDialogAsync(string courseName, CourseStatistics statistics);
    Task<Group?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups);
    
    // Диалоги для заданий
    Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment);
    Task<object?> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<Submission> submissions);
    Task<IEnumerable<object>?> ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions);
    
    // Диалоги для оценок
    Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments);
    Task<IEnumerable<Grade>?> ShowBulkGradingDialogAsync(IEnumerable<Course> courses, IEnumerable<Assignment> assignments);
    
    // Диалоги для преподавателей
    Task<Teacher?> ShowTeacherEditDialogAsync(Teacher teacher);
    Task<object?> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<Course> allCourses);
    Task<object?> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> allGroups);
    Task<object?> ShowTeacherStatisticsDialogAsync(string teacherName, object statistics);
    
    // Диалоги для уведомлений
    Task<NotificationTemplate?> ShowNotificationTemplateEditDialogAsync(NotificationTemplate template);
    Task<Dictionary<string, object>?> ShowTemplateParametersDialogAsync(NotificationTemplate template);
    Task<ReminderData?> ShowCreateReminderDialogAsync();
}

/// <summary>
/// Данные для создания напоминания
/// </summary>
public class ReminderData
{
    public string Title { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public DateTime RemindAt { get; set; }
    public TimeSpan? RepeatInterval { get; set; }
}
