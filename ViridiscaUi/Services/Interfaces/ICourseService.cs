using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с курсами
    /// </summary>
    public interface ICourseService
    {
        /// <summary>
        /// Получает курс по идентификатору
        /// </summary>
        Task<Course?> GetCourseAsync(Guid uid);
        
        /// <summary>
        /// Получает все курсы
        /// </summary>
        Task<IEnumerable<Course>> GetAllCoursesAsync();
        
        /// <summary>
        /// Получает курсы преподавателя
        /// </summary>
        Task<IEnumerable<Course>> GetCoursesByTeacherAsync(Guid teacherUid);
        
        /// <summary>
        /// Добавляет новый курс
        /// </summary>
        Task AddCourseAsync(Course course);
        
        /// <summary>
        /// Обновляет существующий курс
        /// </summary>
        Task<bool> UpdateCourseAsync(Course course);
        
        /// <summary>
        /// Удаляет курс
        /// </summary>
        Task<bool> DeleteCourseAsync(Guid uid);
        
        /// <summary>
        /// Публикует курс (изменяет статус)
        /// </summary>
        Task<bool> PublishCourseAsync(Guid uid);
        
        /// <summary>
        /// Архивирует курс (изменяет статус)
        /// </summary>
        Task<bool> ArchiveCourseAsync(Guid uid);
    }
} 