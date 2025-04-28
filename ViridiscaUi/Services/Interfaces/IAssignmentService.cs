using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с заданиями
    /// </summary>
    public interface IAssignmentService
    {
        /// <summary>
        /// Получает задание по идентификатору
        /// </summary>
        Task<Assignment?> GetAssignmentAsync(Guid uid);
        
        /// <summary>
        /// Получает все задания
        /// </summary>
        Task<IEnumerable<Assignment>> GetAllAssignmentsAsync();
        
        /// <summary>
        /// Получает задания по курсу
        /// </summary>
        Task<IEnumerable<Assignment>> GetAssignmentsByCourseAsync(Guid courseUid);
        
        /// <summary>
        /// Получает задания по модулю
        /// </summary>
        Task<IEnumerable<Assignment>> GetAssignmentsByModuleAsync(Guid moduleUid);
        
        /// <summary>
        /// Добавляет новое задание
        /// </summary>
        Task AddAssignmentAsync(Assignment assignment);
        
        /// <summary>
        /// Обновляет существующее задание
        /// </summary>
        Task<bool> UpdateAssignmentAsync(Assignment assignment);
        
        /// <summary>
        /// Удаляет задание
        /// </summary>
        Task<bool> DeleteAssignmentAsync(Guid uid);
        
        /// <summary>
        /// Публикует задание
        /// </summary>
        Task<bool> PublishAssignmentAsync(Guid uid);
    }
} 