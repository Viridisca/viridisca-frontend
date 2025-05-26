using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для импорта данных
    /// </summary>
    public interface IImportService
    {
        /// <summary>
        /// Импортирует курсы из файла
        /// </summary>
        Task<IEnumerable<Course>?> ImportCoursesAsync(string filePath);
        
        /// <summary>
        /// Импортирует студентов из файла
        /// </summary>
        Task<IEnumerable<Student>?> ImportStudentsAsync(string filePath);
        
        /// <summary>
        /// Импортирует преподавателей из файла
        /// </summary>
        Task<IEnumerable<Teacher>?> ImportTeachersAsync(string filePath);
        
        /// <summary>
        /// Импортирует группы из файла
        /// </summary>
        Task<IEnumerable<Group>?> ImportGroupsAsync(string filePath);
        
        /// <summary>
        /// Импортирует задания из файла
        /// </summary>
        Task<IEnumerable<Assignment>?> ImportAssignmentsAsync(string filePath);
        
        /// <summary>
        /// Импортирует оценки из файла
        /// </summary>
        Task<IEnumerable<Grade>?> ImportGradesAsync(string filePath);
    }
} 