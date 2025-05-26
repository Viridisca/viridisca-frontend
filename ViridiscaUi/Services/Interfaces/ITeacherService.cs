using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с преподавателями
    /// </summary>
    public interface ITeacherService
    {
        /// <summary>
        /// Получает преподавателя по идентификатору
        /// </summary>
        Task<Teacher?> GetTeacherAsync(Guid uid);
        
        /// <summary>
        /// Получает всех преподавателей
        /// </summary>
        Task<IEnumerable<Teacher>> GetAllTeachersAsync();
        
        /// <summary>
        /// Получает всех преподавателей (алиас для GetAllTeachersAsync)
        /// </summary>
        Task<IEnumerable<Teacher>> GetTeachersAsync();
        
        /// <summary>
        /// Добавляет нового преподавателя
        /// </summary>
        Task AddTeacherAsync(Teacher teacher);
        
        /// <summary>
        /// Обновляет существующего преподавателя
        /// </summary>
        Task<bool> UpdateTeacherAsync(Teacher teacher);
        
        /// <summary>
        /// Удаляет преподавателя
        /// </summary>
        Task<bool> DeleteTeacherAsync(Guid uid);
        
        /// <summary>
        /// Назначает преподавателя на курс
        /// </summary>
        Task<bool> AssignToCourseAsync(Guid teacherUid, Guid courseUid);
    }
} 