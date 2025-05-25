using System;
using System.Threading.Tasks;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Education;
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
    }
} 