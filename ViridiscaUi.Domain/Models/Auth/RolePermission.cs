using ViridiscaUi.Domain.Models.Base;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Связь между ролью и разрешением
/// </summary>
public class RolePermission : ViewModelBase
{
    /// <summary>
    /// Уникальный идентификатор связи
    /// </summary>
    public new Guid Uid { get; set; }

    /// <summary>
    /// Идентификатор роли
    /// </summary>
    public Guid RoleUid { get; set; }

    /// <summary>
    /// Идентификатор разрешения
    /// </summary>
    public Guid PermissionUid { get; set; }

    /// <summary>
    /// Роль
    /// </summary>
    public Role? Role { get; set; }

    /// <summary>
    /// Разрешение
    /// </summary>
    public Permission? Permission { get; set; }

    /// <summary>
    /// Флаг активности
    /// </summary>
    public bool IsActive { get; set; } = true;

    /// <summary>
    /// Дата назначения
    /// </summary>
    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
