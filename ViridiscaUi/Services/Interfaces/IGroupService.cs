using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services.Interfaces
{
    /// <summary>
    /// Сервис для работы с учебными группами
    /// </summary>
    public interface IGroupService
    {
        /// <summary>
        /// Получает группу по идентификатору
        /// </summary>
        Task<Group?> GetGroupByIdAsync(Guid id);
        
        /// <summary>
        /// Получает все группы
        /// </summary>
        Task<IEnumerable<Group>> GetGroupsAsync();
        
        /// <summary>
        /// Получает все группы (алиас для GetGroupsAsync)
        /// </summary>
        Task<IEnumerable<Group>> GetAllGroupsAsync();
        
        /// <summary>
        /// Добавляет новую группу
        /// </summary>
        Task<Group> CreateGroupAsync(Group group);
        
        /// <summary>
        /// Обновляет существующую группу
        /// </summary>
        Task<Group> UpdateGroupAsync(Group group);
        
        /// <summary>
        /// Удаляет группу
        /// </summary>
        Task DeleteGroupAsync(Guid id);
        
        /// <summary>
        /// Назначает куратора группе
        /// </summary>
        Task<bool> AssignCuratorAsync(Guid groupUid, Guid teacherUid);

        // === РАСШИРЕНИЯ ЭТАПА 1 ===
        
        /// <summary>
        /// Получает группы по году обучения
        /// </summary>
        Task<IEnumerable<Group>> GetGroupsByYearAsync(int year);
        
        /// <summary>
        /// Получает группы по куратору
        /// </summary>
        Task<IEnumerable<Group>> GetGroupsByCuratorAsync(Guid curatorUid);
        
        /// <summary>
        /// Добавляет студента в группу
        /// </summary>
        Task<bool> AddStudentToGroupAsync(Guid groupUid, Guid studentUid);
        
        /// <summary>
        /// Удаляет студента из группы
        /// </summary>
        Task<bool> RemoveStudentFromGroupAsync(Guid groupUid, Guid studentUid);
        
        /// <summary>
        /// Получает статистику группы (количество студентов, средний балл и т.д.)
        /// </summary>
        Task<GroupStatistics> GetGroupStatisticsAsync(Guid groupUid);
        
        /// <summary>
        /// Получает группы с пагинацией
        /// </summary>
        Task<(IEnumerable<Group> Groups, int TotalCount)> GetGroupsPagedAsync(int page, int pageSize, string? searchTerm = null);
    }

    /// <summary>
    /// Статистика группы
    /// </summary>
    public class GroupStatistics
    {
        public Guid GroupUid { get; set; }
        public int TotalStudents { get; set; }
        public int ActiveStudents { get; set; }
        public double AverageGrade { get; set; }
        public int TotalCourses { get; set; }
        public int CompletedAssignments { get; set; }
        public int PendingAssignments { get; set; }
        public DateTime? LastActivityDate { get; set; }
        public Dictionary<string, int> GradeDistribution { get; set; } = new();
        public Dictionary<string, double> SubjectPerformance { get; set; } = new();
        
        // Дополнительные свойства для совместимости с XAML
        public int StudentsCount => TotalStudents;
        public int ActiveCoursesCount => TotalCourses;
        public int CompletedAssignmentsCount => CompletedAssignments;
        public int PendingAssignmentsCount => PendingAssignments;
    }
} 