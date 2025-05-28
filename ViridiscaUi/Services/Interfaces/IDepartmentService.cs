using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.System;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с департаментами
    /// </summary>
    public interface IDepartmentService
    {
        /// <summary>
        /// Получает департамент по идентификатору
        /// </summary>
        Task<Department?> GetDepartmentAsync(Guid uid);

        /// <summary>
        /// Получает все департаменты
        /// </summary>
        Task<IEnumerable<Department>> GetAllDepartmentsAsync();

        /// <summary>
        /// Получает активные департаменты
        /// </summary>
        Task<IEnumerable<Department>> GetActiveDepartmentsAsync();

        /// <summary>
        /// Создает новый департамент
        /// </summary>
        Task<Department> CreateDepartmentAsync(Department department);

        /// <summary>
        /// Добавляет новый департамент
        /// </summary>
        Task AddDepartmentAsync(Department department);

        /// <summary>
        /// Обновляет существующий департамент
        /// </summary>
        Task<bool> UpdateDepartmentAsync(Department department);

        /// <summary>
        /// Удаляет департамент
        /// </summary>
        Task<bool> DeleteDepartmentAsync(Guid uid);

        /// <summary>
        /// Поиск департаментов по названию или коду
        /// </summary>
        Task<IEnumerable<Department>> SearchDepartmentsAsync(string searchTerm);

        /// <summary>
        /// Получает департаменты с пагинацией
        /// </summary>
        Task<(IEnumerable<Department> Departments, int TotalCount)> GetDepartmentsPagedAsync(
            int page,
            int pageSize,
            string? searchTerm = null,
            bool? isActive = null);

        /// <summary>
        /// Проверяет существование департамента с указанным кодом
        /// </summary>
        Task<bool> ExistsByCodeAsync(string code, Guid? excludeUid = null);

        /// <summary>
        /// Получает статистику по департаменту
        /// </summary>
        Task<DepartmentStatistics> GetDepartmentStatisticsAsync(Guid departmentUid);

        /// <summary>
        /// Активирует/деактивирует департамент
        /// </summary>
        Task<bool> SetDepartmentActiveStatusAsync(Guid uid, bool isActive);
    }

    /// <summary>
    /// Статистика по департаменту
    /// </summary>
    public class DepartmentStatistics
    {
        public int TeachersCount { get; set; }
        public int GroupsCount { get; set; }
        public int SubjectsCount { get; set; }
        public int StudentsCount { get; set; }
        public int ActiveCoursesCount { get; set; }
    }
} 