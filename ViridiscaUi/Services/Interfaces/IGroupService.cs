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
        Task<Group?> GetGroupAsync(Guid uid);
        
        /// <summary>
        /// Получает все группы
        /// </summary>
        Task<IEnumerable<Group>> GetAllGroupsAsync();
        
        /// <summary>
        /// Добавляет новую группу
        /// </summary>
        Task AddGroupAsync(Group group);
        
        /// <summary>
        /// Обновляет существующую группу
        /// </summary>
        Task<bool> UpdateGroupAsync(Group group);
        
        /// <summary>
        /// Удаляет группу
        /// </summary>
        Task<bool> DeleteGroupAsync(Guid uid);
        
        /// <summary>
        /// Назначает куратора группе
        /// </summary>
        Task<bool> AssignCuratorAsync(Guid groupUid, Guid teacherUid);
    }
} 