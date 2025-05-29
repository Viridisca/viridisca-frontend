using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с диалогами
/// </summary>
public interface IDialogService
{
    /// <summary>
    /// Показывает диалог редактирования студента
    /// </summary>
    /// <param name="student">Студент для редактирования или null для создания нового</param>
    /// <returns>Обновленный или новый студент, или null если операция отменена</returns>
    Task<Student?> ShowStudentEditDialogAsync(Student? student = null);
    
    /// <summary>
    /// Показывает диалог с подробной информацией о студенте
    /// </summary>
    /// <param name="student">Студент для отображения</param>
    Task ShowStudentDetailsDialogAsync(Student student);
    
    /// <summary>
    /// Показывает диалог редактора студентов
    /// </summary>
    /// <param name="student">Студент для редактирования или null для создания нового</param>
    /// <returns>Результат редактирования</returns>
    Task<Student?> ShowStudentEditorDialogAsync(Student? student = null);
    
    /// <summary>
    /// Показывает диалог подтверждения
    /// </summary>
    /// <param name="title">Заголовок диалога</param>
    /// <param name="message">Сообщение</param>
    /// <param name="confirmText">Текст кнопки подтверждения</param>
    /// <param name="cancelText">Текст кнопки отмены</param>
    /// <returns>True если пользователь подтвердил, false если отменил</returns>
    Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет");
    
    /// <summary>
    /// Показывает диалог выбора файла для открытия
    /// </summary>
    /// <param name="title">Заголовок диалога</param>
    /// <param name="fileTypes">Типы файлов для фильтра</param>
    /// <returns>Путь к выбранному файлу или null если отменено</returns>
    Task<string?> ShowFileOpenDialogAsync(string title, string[] fileTypes);
    
    /// <summary>
    /// Показывает диалог сохранения файла
    /// </summary>
    /// <param name="title">Заголовок диалога</param>
    /// <param name="defaultFileName">Имя файла по умолчанию</param>
    /// <param name="fileTypes">Типы файлов для фильтра</param>
    /// <returns>Путь для сохранения файла или null если отменено</returns>
    Task<string?> ShowFileSaveDialogAsync(string title, string defaultFileName, string[] fileTypes);
    
    /// <summary>
    /// Показывает информационное сообщение
    /// </summary>
    /// <param name="title">Заголовок</param>
    /// <param name="message">Сообщение</param>
    Task ShowInfoAsync(string title, string message);
    
    /// <summary>
    /// Показывает сообщение об ошибке
    /// </summary>
    /// <param name="title">Заголовок</param>
    /// <param name="message">Сообщение об ошибке</param>
    Task ShowErrorAsync(string title, string message);
    
    /// <summary>
    /// Показывает предупреждение
    /// </summary>
    /// <param name="title">Заголовок</param>
    /// <param name="message">Предупреждение</param>
    Task ShowWarningAsync(string title, string message);
    
    /// <summary>
    /// Показывает запрос подтверждения
    /// </summary>
    Task<bool> ShowConfirmationAsync(string title, string message);
    
    /// <summary>
    /// Показывает диалог с вводом текста
    /// </summary>
    Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
    
    /// <summary>
    /// Показывает диалог с вводом текста (альтернативное название)
    /// </summary>
    Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "");
    
    /// <summary>
    /// Показывает диалог с выбором из списка
    /// </summary>
    Task<T?> ShowSelectionDialogAsync<T>(string title, string message, T[] items);

    Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel);
    
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
    Task<Teacher?> ShowTeacherEditDialogAsync(Teacher? teacher = null);
    Task<string?> ShowTeacherDetailsDialogAsync(Teacher teacher);
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
