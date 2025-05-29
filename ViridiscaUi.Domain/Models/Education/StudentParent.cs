using System;
using ReactiveUI;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Родитель/опекун студента
    /// </summary>
    public class StudentParent : ViewModelBase
    {
        private Guid _studentUid;
        private Guid _parentUserUid;
        private ParentRelationType _relationType;
        private bool _isEmergencyContact;
        private bool _hasAccessToGrades;
        private bool _hasAccessToAttendance;

        /// <summary>
        /// Идентификатор студента
        /// </summary>
        public Guid StudentUid
        {
            get => _studentUid;
            set => this.RaiseAndSetIfChanged(ref _studentUid, value);
        }

        /// <summary>
        /// Идентификатор пользователя-родителя
        /// </summary>
        public Guid ParentUserUid
        {
            get => _parentUserUid;
            set => this.RaiseAndSetIfChanged(ref _parentUserUid, value);
        }

        /// <summary>
        /// Идентификатор родителя (алиас для ParentUserUid)
        /// </summary>
        public Guid ParentUid
        {
            get => _parentUserUid;
            set => this.RaiseAndSetIfChanged(ref _parentUserUid, value);
        }

        /// <summary>
        /// Тип родственного отношения
        /// </summary>
        public ParentRelationType RelationType
        {
            get => _relationType;
            set => this.RaiseAndSetIfChanged(ref _relationType, value);
        }

        /// <summary>
        /// Является ли контактом для экстренных случаев
        /// </summary>
        public bool IsEmergencyContact
        {
            get => _isEmergencyContact;
            set => this.RaiseAndSetIfChanged(ref _isEmergencyContact, value);
        }

        /// <summary>
        /// Имеет ли доступ к оценкам
        /// </summary>
        public bool HasAccessToGrades
        {
            get => _hasAccessToGrades;
            set => this.RaiseAndSetIfChanged(ref _hasAccessToGrades, value);
        }

        /// <summary>
        /// Имеет ли доступ к посещаемости
        /// </summary>
        public bool HasAccessToAttendance
        {
            get => _hasAccessToAttendance;
            set => this.RaiseAndSetIfChanged(ref _hasAccessToAttendance, value);
        }

        /// <summary>
        /// Отображаемый тип родственного отношения
        /// </summary>
        public string RelationTypeDisplayName => RelationType.GetDisplayName();

        /// <summary>
        /// Создает новый экземпляр родителя студента
        /// </summary>
        public StudentParent()
        {
        }

        /// <summary>
        /// Создает новый экземпляр родителя студента с указанными параметрами
        /// </summary>
        public StudentParent(Guid studentUid, Guid parentUserUid, ParentRelationType relationType)
        {
            Uid = Guid.NewGuid();
            _studentUid = studentUid;
            _parentUserUid = parentUserUid;
            _relationType = relationType;
            _hasAccessToGrades = true;
            _hasAccessToAttendance = true;
            _isEmergencyContact = false;
        }

        /// <summary>
        /// Обновляет права доступа родителя
        /// </summary>
        public void UpdateAccess(bool hasAccessToGrades, bool hasAccessToAttendance)
        {
            HasAccessToGrades = hasAccessToGrades;
            HasAccessToAttendance = hasAccessToAttendance;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Устанавливает флаг экстренного контакта
        /// </summary>
        public void SetEmergencyContact(bool isEmergencyContact)
        {
            IsEmergencyContact = isEmergencyContact;
            LastModifiedAt = DateTime.UtcNow;
        }

        /// <summary>
        /// Обновляет тип родственного отношения
        /// </summary>
        public void UpdateRelationType(ParentRelationType relationType)
        {
            RelationType = relationType;
            LastModifiedAt = DateTime.UtcNow;
        }
    }
} 