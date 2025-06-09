using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.Domain.Models.Library;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Services.Interfaces;

/// <summary>
/// Интерфейс сервиса для работы с диалогами
/// </summary>
public interface IDialogService
{
    // Базовые диалоги - возвращают DialogResult
    Task<DialogResult> ShowMessageAsync(string title, string message);
    Task<DialogResult> ShowConfirmationAsync(string title, string message);
    Task<DialogResult> ShowConfirmationAsync(string title, string message, DialogButtons buttons);
    Task<string?> ShowInputAsync(string title, string message, string defaultValue = "");
    Task ShowErrorAsync(string title, string message);
    Task ShowWarningAsync(string title, string message);
    Task ShowInfoAsync(string title, string message);
    
    // Диалоги валидации
    Task ShowValidationErrorsAsync(string title, IEnumerable<string> errors);
    Task ShowValidationErrorsAsync(ValidationResult validationResult);
    
    // Специализированные диалоги редактирования - возвращают доменные объекты
    Task<Student?> ShowStudentEditDialogAsync(Student? student = null);
    Task<Teacher?> ShowTeacherEditDialogAsync(Teacher? teacher = null);
    Task<Group?> ShowGroupEditDialogAsync(Group? group = null);
    Task<Subject?> ShowSubjectEditDialogAsync(Subject? subject = null);
    Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment? assignment = null);
    Task<Grade?> ShowGradeEditDialogAsync(Grade grade, IEnumerable<Student> students, IEnumerable<Assignment> assignments);
    Task<Exam?> ShowExamEditDialogAsync(Exam? exam = null);
    Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot? scheduleSlot = null);
    Task<Curriculum?> ShowCurriculumEditDialogAsync(Curriculum? curriculum = null);
    Task<LibraryResource?> ShowLibraryResourceEditDialogAsync(LibraryResource? resource = null);
    
    // Диалоги конфликтов
    Task ShowScheduleConflictsDialogAsync(IEnumerable<object> conflicts);
    
    // Диалоги выбора файлов
    Task<string?> ShowOpenFileDialogAsync(string title, string filter = "");
    Task<string?> ShowSaveFileDialogAsync(string title, string filter = "", string defaultFileName = "");
    
    // Дополнительные диалоги
    Task<bool> ShowConfirmationDialogAsync(string title, string message, string confirmText = "Да", string cancelText = "Нет");
    Task<string?> ShowFileOpenDialogAsync(string title, string[] fileTypes);
    Task<string?> ShowFileSaveDialogAsync(string title, string defaultFileName, string[] fileTypes);
    Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
    Task<string?> ShowTextInputDialogAsync(string title, string message, string defaultValue = "");
    Task<T?> ShowSelectionDialogAsync<T>(string title, string message, T[] items);
    Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel) where TResult : class;
    
    // Диалоги для департаментов
    Task<Department?> ShowDepartmentEditDialogAsync(Department department);
    Task ShowDepartmentDetailsDialogAsync(Department department);
    
    // Диалоги для курсов
    Task<CourseInstance?> ShowCourseEditDialogAsync(CourseInstance courseInstance);
    Task<object?> ShowCourseDetailsDialogAsync(CourseInstance courseInstance);
    
    // Диалоги для уведомлений
    Task<NotificationTemplate?> ShowNotificationTemplateEditDialogAsync(NotificationTemplate template);
    Task<Dictionary<string, object>?> ShowTemplateParametersDialogAsync(NotificationTemplate template);
    Task<ReminderData?> ShowCreateReminderDialogAsync();

    /// <summary>
    /// Показывает диалог просмотра сдач заданий
    /// </summary>
    Task<object?> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<Submission> submissions);

    /// <summary>
    /// Показывает диалог массового оценивания сдач
    /// </summary>
    Task<IEnumerable<object>?> ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions);

    /// <summary>
    /// Показывает диалог создания займа библиотеки
    /// </summary>
    Task<object?> ShowCreateLoanDialogAsync();

    /// <summary>
    /// Показывает диалог продления займа
    /// </summary>
    Task<object?> ShowExtendLoanDialogAsync(object loan);

    /// <summary>
    /// Показывает диалог просроченных займов
    /// </summary>
    Task ShowOverdueLoansDialogAsync();

    /// <summary>
    /// Показывает диалог выбора преподавателя
    /// </summary>
    Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers);

    /// <summary>
    /// Показывает диалог управления курсами преподавателя
    /// </summary>
    Task<object?> ShowTeacherCoursesManagementDialogAsync(Teacher teacher, IEnumerable<CourseInstance> courseInstances);

    /// <summary>
    /// Показывает диалог управления группами преподавателя
    /// </summary>
    Task<object?> ShowTeacherGroupsManagementDialogAsync(Teacher teacher, IEnumerable<Group> groups);

    /// <summary>
    /// Показывает диалог статистики преподавателя
    /// </summary>
    Task<object?> ShowTeacherStatisticsDialogAsync(string title, object statistics);

    /// <summary>
    /// Показывает диалог деталей студента
    /// </summary>
    Task ShowStudentDetailsDialogAsync(Student student);

    /// <summary>
    /// Показывает диалог деталей преподавателя
    /// </summary>
    Task<string?> ShowTeacherDetailsDialogAsync(Teacher teacher);

    /// <summary>
    /// Показывает диалог статистики курса
    /// </summary>
    Task<object?> ShowCourseStatisticsDialogAsync(CourseInstance courseInstance);

    /// <summary>
    /// Показывает диалог записи на курс
    /// </summary>
    Task<object?> ShowCourseEnrollmentDialogAsync(CourseInstance courseInstance, IEnumerable<Student> allStudents);

    /// <summary>
    /// Показывает диалог управления содержимым курса
    /// </summary>
    Task<object?> ShowCourseContentManagementDialogAsync(CourseInstance courseInstance);

    /// <summary>
    /// Показывает диалог массового редактирования студентов
    /// </summary>
    Task<BulkEditResult?> ShowBulkEditDialogAsync(BulkEditOptions options);

    /// <summary>
    /// Показывает диалог выбора экземпляров курсов
    /// </summary>
    Task<IEnumerable<CourseInstance>?> ShowCourseInstanceSelectionDialogAsync(IEnumerable<CourseInstance> courseInstances);

    /// <summary>
    /// Показывает диалог выбора групп
    /// </summary>
    Task<IEnumerable<Group>?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups);

    /// <summary>
    /// Показывает диалог выбора файла
    /// </summary>
    Task<string?> ShowFilePickerAsync(string title, string[] fileTypes);

    /// <summary>
    /// Показывает диалог редактирования слота расписания с дополнительными параметрами
    /// </summary>
    Task<ScheduleSlot?> ShowScheduleSlotEditDialogAsync(ScheduleSlot? scheduleSlot, IEnumerable<CourseInstance> courseInstances, IEnumerable<string> rooms);
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
