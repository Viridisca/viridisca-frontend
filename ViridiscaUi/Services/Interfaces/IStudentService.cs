using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы со студентами
    /// </summary>
    public interface IStudentService
    {
        /// <summary>
        /// Получает студента по идентификатору
        /// </summary>
        Task<Student?> GetStudentAsync(Guid uid);
        
        /// <summary>
        /// Получает всех студентов
        /// </summary>
        Task<IEnumerable<Student>> GetAllStudentsAsync();
        
        /// <summary>
        /// Получает студентов по идентификатору группы
        /// </summary>
        Task<IEnumerable<Student>> GetStudentsByGroupAsync(Guid groupUid);
        
        /// <summary>
        /// Добавляет нового студента
        /// </summary>
        Task AddStudentAsync(Student student);
        
        /// <summary>
        /// Обновляет существующего студента
        /// </summary>
        Task<bool> UpdateStudentAsync(Student student);
        
        /// <summary>
        /// Удаляет студента
        /// </summary>
        Task<bool> DeleteStudentAsync(Guid uid);
    }
} 