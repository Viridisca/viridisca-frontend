using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace ViridiscaUi.Domain.Models.Base;

/// <summary>
/// Базовый класс для всех моделей клиентского приложения
/// </summary>
public abstract class ViewModelBase : ReactiveObject
{
    private Guid _uid;
    
    /// <summary>
    /// Уникальный идентификатор сущности
    /// </summary>
    public Guid Uid
    {
        get => _uid;
        set => this.RaiseAndSetIfChanged(ref _uid, value);
    }
    
    /// <summary>
    /// Дата создания записи
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    /// <summary>
    /// Дата последнего изменения записи
    /// </summary>
    public DateTime? LastModifiedAt { get; set; }
    
    /// <summary>
    /// Устанавливает значение свойства, вызывает событие изменения и обновляет дату модификации
    /// </summary>
    protected bool SetProperty<T>(ref T field, T newValue, [CallerMemberName] string? propertyName = null)
    {
        if (EqualityComparer<T>.Default.Equals(field, newValue))
            return false;
        
        field = newValue;
        this.RaisePropertyChanged(propertyName);
        LastModifiedAt = DateTime.UtcNow;
        return true;
    }
} 