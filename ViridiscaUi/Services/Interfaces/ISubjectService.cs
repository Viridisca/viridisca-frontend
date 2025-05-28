using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с предметами
    /// </summary>
    public interface ISubjectService
    {
        /// <summary>
        /// Получает предмет по идентификатору
        /// </summary>
        Task<Subject?> GetSubjectAsync(Guid uid);

        /// <summary>
        /// Получает все предметы
        /// </summary>
        Task<IEnumerable<Subject>> GetAllSubjectsAsync();

        /// <summary>
        /// Получает активные предметы
        /// </summary>
        Task<IEnumerable<Subject>> GetActiveSubjectsAsync();

        /// <summary>
        /// Получает предметы по департаменту
        /// </summary>
        Task<IEnumerable<Subject>> GetSubjectsByDepartmentAsync(Guid departmentUid);

        /// <summary>
        /// Создает новый предмет
        /// </summary>
        Task<Subject> CreateSubjectAsync(Subject subject);

        /// <summary>
        /// Добавляет новый предмет
        /// </summary>
        Task AddSubjectAsync(Subject subject);

        /// <summary>
        /// Обновляет существующий предмет
        /// </summary>
        Task<bool> UpdateSubjectAsync(Subject subject);

        /// <summary>
        /// Удаляет предмет
        /// </summary>
        Task<bool> DeleteSubjectAsync(Guid uid);

        /// <summary>
        /// Поиск предметов по названию или коду
        /// </summary>
        Task<IEnumerable<Subject>> SearchSubjectsAsync(string searchTerm);

        /// <summary>
        /// Получает предметы с пагинацией
        /// </summary>
        Task<(IEnumerable<Subject> Subjects, int TotalCount)> GetSubjectsPagedAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null,
            Guid? departmentUid = null);

        /// <summary>
        /// Проверяет существование предмета с указанным кодом
        /// </summary>
        Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null);

        /// <summary>
        /// Получает статистику по предмету
        /// </summary>
        Task<SubjectStatistics> GetSubjectStatisticsAsync(Guid subjectUid);

        /// <summary>
        /// Активирует/деактивирует предмет
        /// </summary>
        Task<bool> SetSubjectActiveStatusAsync(Guid uid, bool isActive);
    }

    /// <summary>
    /// Статистика по предмету
    /// </summary>
    public class SubjectStatistics
    {
        public int TeachersCount { get; set; }
        public int CoursesCount { get; set; }
        public int StudentsCount { get; set; }
        public int ActiveCoursesCount { get; set; }
        public decimal AverageGrade { get; set; }
    }
} 