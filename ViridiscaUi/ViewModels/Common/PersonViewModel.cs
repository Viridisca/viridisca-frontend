using System;
using ReactiveUI;
using ReactiveUI.Fody.Helpers;
using ViridiscaUi.Domain.Models.Auth;

namespace ViridiscaUi.ViewModels.Common;

/// <summary>
/// ViewModel для отображения человека
/// </summary>
public class PersonViewModel : ReactiveObject
{
    [Reactive] public Guid Uid { get; set; }
    [Reactive] public string FirstName { get; set; } = string.Empty;
    [Reactive] public string LastName { get; set; } = string.Empty;
    [Reactive] public string? MiddleName { get; set; }
    [Reactive] public string Email { get; set; } = string.Empty;
    [Reactive] public string? PhoneNumber { get; set; }
    [Reactive] public DateTime? DateOfBirth { get; set; }
    [Reactive] public string? Address { get; set; }
    [Reactive] public string? ProfileImageUrl { get; set; }
    [Reactive] public DateTime CreatedAt { get; set; }
    [Reactive] public DateTime? LastModifiedAt { get; set; }

    /// <summary>
    /// Полное имя
    /// </summary>
    public string FullName => string.IsNullOrWhiteSpace(MiddleName) 
        ? $"{FirstName} {LastName}" 
        : $"{FirstName} {MiddleName} {LastName}";

    /// <summary>
    /// Краткое имя (Фамилия И.О.)
    /// </summary>
    public string ShortName
    {
        get
        {
            var firstInitial = !string.IsNullOrEmpty(FirstName) ? FirstName[0] + "." : "";
            var middleInitial = !string.IsNullOrEmpty(MiddleName) ? MiddleName[0] + "." : "";
            return $"{LastName} {firstInitial}{middleInitial}".Trim();
        }
    }

    /// <summary>
    /// Возраст
    /// </summary>
    public int? Age => DateOfBirth?.Date != null 
        ? DateTime.Today.Year - DateOfBirth.Value.Year - (DateTime.Today.DayOfYear < DateOfBirth.Value.DayOfYear ? 1 : 0)
        : null;

    /// <summary>
    /// Есть ли фото профиля
    /// </summary>
    public bool HasProfileImage => !string.IsNullOrWhiteSpace(ProfileImageUrl);

    public PersonViewModel() { }

    public PersonViewModel(Person person)
    {
        Uid = person.Uid;
        FirstName = person.FirstName;
        LastName = person.LastName;
        MiddleName = person.MiddleName;
        Email = person.Email;
        PhoneNumber = person.PhoneNumber;
        DateOfBirth = person.DateOfBirth;
        Address = person.Address;
        ProfileImageUrl = person.ProfileImageUrl;
        CreatedAt = person.CreatedAt;
        LastModifiedAt = person.LastModifiedAt;
    }

    /// <summary>
    /// Преобразует ViewModel обратно в модель
    /// </summary>
    public Person ToModel()
    {
        return new Person
        {
            Uid = Uid,
            FirstName = FirstName,
            LastName = LastName,
            MiddleName = MiddleName,
            Email = Email,
            PhoneNumber = PhoneNumber,
            DateOfBirth = DateOfBirth,
            Address = Address,
            ProfileImageUrl = ProfileImageUrl,
            CreatedAt = CreatedAt,
            LastModifiedAt = LastModifiedAt
        };
    }
} 