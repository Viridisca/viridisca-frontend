using System;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Domain.Models.Base;
using ViridiscaUi.Domain.Models.Education.Enums;

namespace ViridiscaUi.Domain.Models.Education
{
    /// <summary>
    /// Родитель/опекун студента
    /// </summary>
    public class StudentParent : AuditableEntity
    {
        /// <summary>
        /// Идентификатор студента
        /// </summary>
        public Guid StudentUid { get; set; }

        /// <summary>
        /// Идентификатор родителя (Person)
        /// </summary>
        public Guid ParentUid { get; set; }

        /// <summary>
        /// Тип родственного отношения
        /// </summary>
        public ParentRelationType RelationType { get; set; }

        /// <summary>
        /// Является ли контактом для экстренных случаев
        /// </summary>
        public bool IsEmergencyContact { get; set; }

        /// <summary>
        /// Имеет ли доступ к оценкам
        /// </summary>
        public bool HasAccessToGrades { get; set; }

        /// <summary>
        /// Имеет ли доступ к посещаемости
        /// </summary>
        public bool HasAccessToAttendance { get; set; }

        /// <summary>
        /// Студент
        /// </summary>
        public Student? Student { get; set; }

        /// <summary>
        /// Родитель
        /// </summary>
        public Person? Parent { get; set; }
    }
} 