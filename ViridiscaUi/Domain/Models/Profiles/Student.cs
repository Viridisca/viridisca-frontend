using ReactiveUI;
using System;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.Profiles
{
    /// <summary>
    /// Профиль студента
    /// </summary>
    public class Student : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _userUid;
        private User? _user;
        private string _studentId = string.Empty;
        private string _group = string.Empty;
        private string _faculty = string.Empty;
        private int? _year;
        private DateTime _registrationDate;

        /// <summary>
        /// Уникальный идентификатор профиля
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
        /// Номер студенческого билета
        /// </summary>
        public string StudentId
        {
            get => _studentId;
            set => this.RaiseAndSetIfChanged(ref _studentId, value);
        }

        /// <summary>
        /// Группа
        /// </summary>
        public string Group
        {
            get => _group;
            set => this.RaiseAndSetIfChanged(ref _group, value);
        }

        /// <summary>
        /// Факультет
        /// </summary>
        public string Faculty
        {
            get => _faculty;
            set => this.RaiseAndSetIfChanged(ref _faculty, value);
        }

        /// <summary>
        /// Год обучения
        /// </summary>
        public int? Year
        {
            get => _year;
            set => this.RaiseAndSetIfChanged(ref _year, value);
        }

        /// <summary>
        /// Дата регистрации
        /// </summary>
        public DateTime RegistrationDate
        {
            get => _registrationDate;
            set => this.RaiseAndSetIfChanged(ref _registrationDate, value);
        }
    }
} 