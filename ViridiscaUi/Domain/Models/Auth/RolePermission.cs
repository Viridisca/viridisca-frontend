using ReactiveUI;
using System;

namespace ViridiscaUi.Domain.Models.Auth
{
    /// <summary>
    /// Связь роли с разрешением
    /// </summary>
    public class RolePermission : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _roleUid;
        private Role? _role;
        private Guid _permissionUid;
        private Permission? _permission;
        private DateTime _createdAt;
        private DateTime _lastModifiedAt;

        /// <summary>
        /// Уникальный идентификатор связи
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Идентификатор роли
        /// </summary>
        public Guid RoleUid
        {
            get => _roleUid;
            set => this.RaiseAndSetIfChanged(ref _roleUid, value);
        }

        /// <summary>
        /// Роль
        /// </summary>
        public Role? Role
        {
            get => _role;
            set => this.RaiseAndSetIfChanged(ref _role, value);
        }

        /// <summary>
        /// Идентификатор разрешения
        /// </summary>
        public Guid PermissionUid
        {
            get => _permissionUid;
            set => this.RaiseAndSetIfChanged(ref _permissionUid, value);
        }

        /// <summary>
        /// Разрешение
        /// </summary>
        public Permission? Permission
        {
            get => _permission;
            set => this.RaiseAndSetIfChanged(ref _permission, value);
        }

        /// <summary>
        /// Дата создания связи
        /// </summary>
        public new DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        /// <summary>
        /// Дата последнего изменения связи
        /// </summary>
        public new DateTime LastModifiedAt
        {
            get => _lastModifiedAt;
            set => this.RaiseAndSetIfChanged(ref _lastModifiedAt, value);
        }
    }
} 