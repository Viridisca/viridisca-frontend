using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Services
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
    }
} 