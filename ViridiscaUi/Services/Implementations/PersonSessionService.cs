using System;
using System.Reactive.Subjects;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для управления сессией пользователя (Singleton)
    /// </summary>
    public class PersonSessionService : IPersonSessionService
    {
        private readonly BehaviorSubject<Person?> _currentPersonSubject = new(null);
        private Account? _currentAccount;

        public IObservable<Person?> CurrentPersonObservable => _currentPersonSubject;

        public Person? CurrentPerson => _currentPersonSubject.Value;

        public Account? CurrentAccount => _currentAccount;

        public void SetCurrentPerson(Person? person)
        {
            _currentPersonSubject.OnNext(person);
        }

        public void SetCurrentAccount(Account? account)
        {
            _currentAccount = account;
        }

        public void ClearSession()
        {
            _currentPersonSubject.OnNext(null);
            _currentAccount = null;
        }

        public void Dispose()
        {
            _currentPersonSubject?.Dispose();
        }
    }
} 