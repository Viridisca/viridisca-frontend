using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с зачислениями
    /// </summary>
    public interface IEnrollmentService
    {
        /// <summary>
        /// Получает зачисление по идентификатору
        /// </summary>
        Task<Enrollment?> GetEnrollmentAsync(Guid uid);
        
        /// <summary>
        /// Получает все зачисления
        /// </summary>
        Task<IEnumerable<Enrollment>> GetAllEnrollmentsAsync();
        
        /// <summary>
        /// Получает зачисления студента
        /// </summary>
        Task<IEnumerable<Enrollment>> GetEnrollmentsByStudentAsync(Guid studentUid);
        
        /// <summary>
        /// Получает зачисления на курс
        /// </summary>
        Task<IEnumerable<Enrollment>> GetEnrollmentsByCourseAsync(Guid courseUid);
        
        /// <summary>
        /// Добавляет новое зачисление
        /// </summary>
        Task AddEnrollmentAsync(Enrollment enrollment);
        
        /// <summary>
        /// Обновляет существующее зачисление
        /// </summary>
        Task<bool> UpdateEnrollmentAsync(Enrollment enrollment);
        
        /// <summary>
        /// Удаляет зачисление
        /// </summary>
        Task<bool> DeleteEnrollmentAsync(Guid uid);
        
        /// <summary>
        /// Изменяет статус зачисления
        /// </summary>
        Task<bool> ChangeEnrollmentStatusAsync(Guid uid, EnrollmentStatus status);
    }
} 