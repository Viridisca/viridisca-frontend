using ReactiveUI;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using ViridiscaUi.Domain.Models.Education;

namespace ViridiscaUi.Domain.Models.Auth
{
    /// <summary>
    /// Модель пользователя системы
    /// </summary>
    public class User : Base.ViewModelBase
    {
        private Guid _uid;
        private string _username = string.Empty;
        private string _email = string.Empty;
        private string _firstName = string.Empty;
        private string _lastName = string.Empty;
        private string _middleName = string.Empty;
        private string _passwordHash = string.Empty;
        private bool _isActive;
        private ObservableCollection<UserRole> _userRoles = new();
        private Student? _studentProfile;
        private Teacher? _teacherProfile;
        private DateTime _createdAt;
        private DateTime _lastModifiedAt;
        private Guid _roleId;
        private Role? _role;

        /// <summary>
        /// Уникальный идентификатор пользователя
        /// </summary>
        public new Guid Uid
        {
            get => _uid;
            set => this.RaiseAndSetIfChanged(ref _uid, value);
        }

        /// <summary>
        /// Имя пользователя (логин)
        /// </summary>
        public string Username
        {
            get => _username;
            set => this.RaiseAndSetIfChanged(ref _username, value);
        }

        /// <summary>
        /// Электронная почта пользователя
        /// </summary>
        public string Email
        {
            get => _email;
            set => this.RaiseAndSetIfChanged(ref _email, value);
        }

        /// <summary>
        /// Имя пользователя
        /// </summary>
        public string FirstName
        {
            get => _firstName;
            set => this.RaiseAndSetIfChanged(ref _firstName, value);
        }

        /// <summary>
        /// Фамилия пользователя
        /// </summary>
        public string LastName
        {
            get => _lastName;
            set => this.RaiseAndSetIfChanged(ref _lastName, value);
        }

        /// <summary>
        /// Отчество пользователя
        /// </summary>
        public string MiddleName
        {
            get => _middleName;
            set => this.RaiseAndSetIfChanged(ref _middleName, value);
        }

        /// <summary>
        /// Хеш пароля пользователя
        /// </summary>
        public string PasswordHash
        {
            get => _passwordHash;
            set => this.RaiseAndSetIfChanged(ref _passwordHash, value);
        }

        /// <summary>
        /// Флаг активности пользователя
        /// </summary>
        public bool IsActive
        {
            get => _isActive;
            set => this.RaiseAndSetIfChanged(ref _isActive, value);
        }
        
        /// <summary>
        /// Идентификатор основной роли пользователя
        /// </summary>
        public Guid RoleId
        {
            get => _roleId;
            set => this.RaiseAndSetIfChanged(ref _roleId, value);
        }
        
        /// <summary>
        /// Основная роль пользователя
        /// </summary>
        public Role? Role
        {
            get => _role;
            set => this.RaiseAndSetIfChanged(ref _role, value);
        }

        /// <summary>
        /// Связи пользователя с ролями
        /// </summary>
        public ObservableCollection<UserRole> UserRoles
        {
            get => _userRoles;
            set => this.RaiseAndSetIfChanged(ref _userRoles, value);
        }

        /// <summary>
        /// Профиль студента (если пользователь является студентом)
        /// </summary>
        public Student? StudentProfile
        {
            get => _studentProfile;
            set => this.RaiseAndSetIfChanged(ref _studentProfile, value);
        }

        /// <summary>
        /// Профиль преподавателя (если пользователь является преподавателем)
        /// </summary>
        public Teacher? TeacherProfile
        {
            get => _teacherProfile;
            set => this.RaiseAndSetIfChanged(ref _teacherProfile, value);
        }

        /// <summary>
        /// Дата создания пользователя
        /// </summary>
        public new DateTime CreatedAt
        {
            get => _createdAt;
            set => this.RaiseAndSetIfChanged(ref _createdAt, value);
        }

        /// <summary>
        /// Дата последнего изменения пользователя
        /// </summary>
        public new DateTime LastModifiedAt
        {
            get => _lastModifiedAt;
            set => this.RaiseAndSetIfChanged(ref _lastModifiedAt, value);
        }
    }
} 