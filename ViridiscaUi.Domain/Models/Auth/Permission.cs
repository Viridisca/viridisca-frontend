using ViridiscaUi.Domain.Models.Base;
using ReactiveUI;

namespace ViridiscaUi.Domain.Models.Auth;

/// <summary>
/// Разрешение в системе
/// </summary>
public class Permission : ViewModelBase
{
    private Guid _uid;

    private string _description = string.Empty;
    private string _name = string.Empty;
    
    private DateTime _lastModifiedAt;
    private DateTime _createdAt;

    /// <summary>
    /// Уникальный идентификатор разрешения
    /// </summary>
    public new Guid Uid
    {
        get => _uid;
        set => this.RaiseAndSetIfChanged(ref _uid, value);
    }

    /// <summary>
    /// Название разрешения
    /// </summary>
    public string Name
    {
        get => _name;
        set => this.RaiseAndSetIfChanged(ref _name, value);
    }

    /// <summary>
    /// Описание разрешения
    /// </summary>
    public string Description
    {
        get => _description;
        set => this.RaiseAndSetIfChanged(ref _description, value);
    }

    /// <summary>
    /// Дата создания разрешения
    /// </summary>
    public new DateTime CreatedAt
    {
        get => _createdAt;
        set => this.RaiseAndSetIfChanged(ref _createdAt, value);
    }

    /// <summary>
    /// Дата последнего изменения разрешения
    /// </summary>
    public new DateTime LastModifiedAt
    {
        get => _lastModifiedAt;
        set => this.RaiseAndSetIfChanged(ref _lastModifiedAt, value);
    }
}
