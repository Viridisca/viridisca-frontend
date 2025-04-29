using ReactiveUI;
using System;

namespace ViridiscaUi.Domain.Models.Auth
{
    /// <summary>
    /// Модель связи пользователя и роли
    /// </summary>
    public class UserRole : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _userUid;
        private User? _user;
        private Guid _roleUid;
        private Role? _role;
        private bool _isActive = true;
        private DateTime _assignedAt = DateTime.UtcNow;

        /// <summary>
        /// Уникальный идентификатор связи
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Идентификатор пользователя
        /// </summary>
        public Guid UserUid
        {
            get => _userUid;
            set => this.RaiseAndSetIfChanged(ref _userUid, value);
        }

        /// <summary>
        /// Пользователь
        /// </summary>
        public User? User
        {
            get => _user;
            set => this.RaiseAndSetIfChanged(ref _user, value);
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
        /// Активна ли связь пользователя с ролью
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }

        /// <summary>
        /// Дата назначения роли пользователю
        /// </summary>
        public DateTime AssignedAt
        {
            get => _assignedAt;
            set => this.RaiseAndSetIfChanged(ref _assignedAt, value);
        }
    }
} 