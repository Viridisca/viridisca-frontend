using ReactiveUI;
using System;
using System.Collections.ObjectModel;

namespace ViridiscaUi.Domain.Models.Auth
{
    /// <summary>
    /// Модель роли пользователя
    /// </summary>
    public class Role : Base.ViewModelBase
    {
        private Guid _uid;
        private string _name = string.Empty;
        private string _description = string.Empty;
        private RoleType _roleType;
        private ObservableCollection<UserRole> _userRoles = new();
        private DateTime _createdAt;
        private DateTime _lastModifiedAt;

        /// <summary>
        /// Уникальный идентификатор роли
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
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
        /// Тип роли
        /// </summary>
        public RoleType RoleType
        {
            get => _roleType;
            set => this.RaiseAndSetIfChanged(ref _roleType, value);
        }

        /// <summary>
        /// Связи роли с пользователями
        /// </summary>
        public ObservableCollection<UserRole> UserRoles
        {
            get => _userRoles;
            set => this.RaiseAndSetIfChanged(ref _userRoles, value);
        }

        /// <summary>
        /// Дата создания роли
        /// </summary>
        public new DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        /// <summary>
        /// Дата последнего изменения роли
        /// </summary>
        public new DateTime LastModifiedAt
        {
            get => _lastModifiedAt;
            set => this.RaiseAndSetIfChanged(ref _lastModifiedAt, value);
        }
    }

    /// <summary>
    /// Тип роли пользователя
    /// </summary>
    public enum RoleType
    {
        /// <summary>
        /// Системный администратор
        /// </summary>
        SystemAdmin,
        
        /// <summary>
        /// Преподаватель
        /// </summary>
        Teacher,
        
        /// <summary>
        /// Студент
        /// </summary>
        Student
    }
} 