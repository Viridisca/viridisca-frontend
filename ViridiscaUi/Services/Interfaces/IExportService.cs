using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для экспорта данных
    /// </summary>
    public interface IExportService
    {
        /// <summary>
        /// Экспортирует оценки в PDF
        /// </summary>
        Task<string?> ExportGradesToPdfAsync(IEnumerable<Grade> grades, string title);
        
        /// <summary>
        /// Экспортирует оценки в Excel
        /// </summary>
        Task<string?> ExportGradesToExcelAsync(IEnumerable<Grade> grades, string title);
        
        /// <summary>
        /// Экспортирует аналитический отчет
        /// </summary>
        Task<string?> ExportAnalyticsReportAsync(object analytics, string title);
        
        /// <summary>
        /// Экспортирует курсы в Excel
        /// </summary>
        Task<string?> ExportCoursesToExcelAsync(IEnumerable<Course> courses, string title);
        
        /// <summary>
        /// Экспортирует преподавателей в Excel
        /// </summary>
        Task<string?> ExportTeachersToExcelAsync(IEnumerable<Teacher> teachers, string title);
        
        /// <summary>
        /// Экспортирует студентов в Excel
        /// </summary>
        Task<string?> ExportStudentsToExcelAsync(IEnumerable<Student> students, string title);
        
        /// <summary>
        /// Экспортирует группы в Excel
        /// </summary>
        Task<string?> ExportGroupsToExcelAsync(IEnumerable<Group> groups, string title);
        
        /// <summary>
        /// Экспортирует задания в Excel
        /// </summary>
        Task<string?> ExportAssignmentsToExcelAsync(IEnumerable<Assignment> assignments, string title);
    }
} 