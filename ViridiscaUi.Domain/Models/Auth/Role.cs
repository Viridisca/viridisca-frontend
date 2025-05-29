using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Роль в системе
/// </summary>
public class Role : ViewModelBase
{
    private RoleType _roleType;

    private string _name = string.Empty;
    private string _description = string.Empty;

    /// <summary>
    /// Тип роли
    /// </summary>
    public RoleType RoleType
    {
        get => _roleType;
        set => this.RaiseAndSetIfChanged(ref _roleType, value);
    }

    /// <summary>
    /// Название роли
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание роли
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Отображаемое название роли
    /// </summary>
    public string DisplayName => RoleType.GetDisplayName();

    /// <summary>
    /// Создает новый экземпляр роли
    /// </summary>
    public Role()
    {
    }

    /// <summary>
    /// Создает новый экземпляр роли с указанными параметрами
    /// </summary>
    public Role(Guid uid, RoleType roleType, string name, string description)
    {
        Uid = uid;
        _roleType = roleType;
        _name = name;
        _description = description;
    }

    /// <summary>
    /// Создает новый экземпляр роли с типом
    /// </summary>
    public static Role Create(RoleType roleType)
    {
        return new Role
        {
            Uid = Guid.NewGuid(),
            RoleType = roleType,
            Name = roleType.ToString(),
            Description = roleType.GetDisplayName()
        };
    }
} 