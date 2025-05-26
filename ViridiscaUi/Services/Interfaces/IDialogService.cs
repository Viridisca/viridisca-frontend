using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.Domain.Models.System;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.Services.Interfaces
{
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
        /// Показывает диалог с вводом текста
        /// </summary>
        Task<string?> ShowInputDialogAsync(string title, string message, string defaultValue = "");
        
        /// <summary>
        /// Показывает диалог с выбором из списка
        /// </summary>
        Task<T?> ShowSelectionDialogAsync<T>(string title, string message, T[] items);

        Task<TResult?> ShowDialogAsync<TResult>(ViewModelBase viewModel);
        Task<Student?> ShowStudentEditorDialogAsync(Student? student = null);
        
        // Диалоги для групп
        Task<Group?> ShowGroupEditDialogAsync(Group group);
        Task<Teacher?> ShowTeacherSelectionDialogAsync(IEnumerable<Teacher> teachers);
        Task<object?> ShowGroupStudentsManagementDialogAsync(Group group, IEnumerable<Student> allStudents);
        
        // Диалоги для курсов
        Task<Course?> ShowCourseEditDialogAsync(Course course);
        Task<object?> ShowCourseEnrollmentDialogAsync(Course course, IEnumerable<Student> allStudents);
        Task<Group?> ShowGroupSelectionDialogAsync(IEnumerable<Group> groups);
        
        // Диалоги для заданий
        Task<Assignment?> ShowAssignmentEditDialogAsync(Assignment assignment);
        Task<object?> ShowSubmissionsViewDialogAsync(Assignment assignment, IEnumerable<Submission> submissions);
        Task<IEnumerable<object>?> ShowBulkGradingDialogAsync(IEnumerable<Submission> submissions);
        
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
} 