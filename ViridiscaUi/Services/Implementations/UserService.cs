using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Infrastructure;
using ViridiscaUi.Services.Interfaces;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с пользователями
    /// </summary>
    public class UserService : IUserService
    {
        private readonly LocalDbContext _dbContext;

        public UserService(LocalDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public Task<User?> GetUserAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByUsernameAsync(string username)
        {
            throw new NotImplementedException();
        }

        public Task<User?> GetUserByEmailAsync(string email)
        {
            throw new NotImplementedException();
        }

        public Task<IEnumerable<User>> GetAllUsersAsync()
        {
            throw new NotImplementedException();
        }

        public Task AddUserAsync(User user, string password)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateUserAsync(User user)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeleteUserAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> ActivateUserAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> DeactivateUserAsync(Guid uid)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string middleName, string phoneNumber)
        {
            throw new NotImplementedException();
        }

        public Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl)
        {
            throw new NotImplementedException();
        }
    }
} 