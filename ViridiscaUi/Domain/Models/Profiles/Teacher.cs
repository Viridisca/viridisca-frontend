using ReactiveUI;
using System;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.Domain.Models.Profiles
{
    /// <summary>
    /// Профиль преподавателя
    /// </summary>
    public class Teacher : Base.ViewModelBase
    {
        private Guid _uid;
        private Guid _userUid;
        private User? _user;
        private string _employeeId = string.Empty;
        private string _department = string.Empty;
        private string _position = string.Empty;
        private string _academicDegree = string.Empty;
        private string _academicTitle = string.Empty;
        private DateTime _hiringDate;

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
        /// Табельный номер сотрудника
        /// </summary>
        public string EmployeeId
        {
            get => _employeeId;
            set => this.RaiseAndSetIfChanged(ref _employeeId, value);
        }

        /// <summary>
        /// Кафедра
        /// </summary>
        public string Department
        {
            get => _department;
            set => this.RaiseAndSetIfChanged(ref _department, value);
        }

        /// <summary>
        /// Должность
        /// </summary>
        public string Position
        {
            get => _position;
            set => this.RaiseAndSetIfChanged(ref _position, value);
        }

        /// <summary>
        /// Ученая степень
        /// </summary>
        public string AcademicDegree
        {
            get => _academicDegree;
            set => this.RaiseAndSetIfChanged(ref _academicDegree, value);
        }

        /// <summary>
        /// Ученое звание
        /// </summary>
        public string AcademicTitle
        {
            get => _academicTitle;
            set => this.RaiseAndSetIfChanged(ref _academicTitle, value);
        }

        /// <summary>
        /// Дата приема на работу
        /// </summary>
        public DateTime HiringDate
        {
            get => _hiringDate;
            set => this.RaiseAndSetIfChanged(ref _hiringDate, value);
        }
    }
} 