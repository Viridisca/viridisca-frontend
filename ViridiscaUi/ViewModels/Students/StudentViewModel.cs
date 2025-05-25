using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Education;
using ViridiscaUi.ViewModels;

namespace ViridiscaUi.ViewModels.Students
{
    /// <summary>
    /// ViewModel для отображения данных студента в гриде
    /// </summary>
    public class StudentViewModel : ViewModelBase
    {
        [Reactive] public Guid Uid { get; set; }
        [Reactive] public string FirstName { get; set; } = string.Empty;
        [Reactive] public string LastName { get; set; } = string.Empty;
        [Reactive] public string? MiddleName { get; set; }
        [Reactive] public string? Email { get; set; }
        [Reactive] public string? GroupName { get; set; }
        [Reactive] public StudentStatus Status { get; set; }

        public string FullName => $"{LastName} {FirstName}" + (MiddleName != null ? $" {MiddleName}" : string.Empty);

        public StudentViewModel(Student student)
        {
            Uid = student.Uid;
            FirstName = student.FirstName;
            LastName = student.LastName;
            MiddleName = student.MiddleName;
            Email = student.Email;
            GroupName = student.Group?.Name;
            Status = student.Status;
        }
    }
} 