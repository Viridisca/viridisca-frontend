using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Domain.Models.Auth;
using ViridiscaUi.Services.Interfaces;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Services.Implementations
{
    /// <summary>
    /// Реализация сервиса для работы с людьми (Person)
    /// </summary>
    public class PersonService : GenericCrudService<Person>, IPersonService
    {
        public PersonService(ApplicationDbContext dbContext, ILogger<PersonService> logger)
            : base(dbContext, logger)
        {
        }

        public async Task<Person?> GetPersonAsync(Guid uid)
        {
            return await GetByUidWithIncludesAsync(uid, p => p.PersonRoles);
        }

        public async Task<Person?> GetPersonByEmailAsync(string email)
        {
            return await _dbSet
                .Include(p => p.PersonRoles)
                .ThenInclude(pr => pr.Role)
                .FirstOrDefaultAsync(p => p.Email.ToLower() == email.ToLower());
        }

        public async Task<IEnumerable<Person>> GetAllPersonsAsync()
        {
            return await GetAllWithIncludesAsync(p => p.PersonRoles);
        }

        public async Task<Person> AddPersonAsync(Person person)
        {
            await CreateAsync(person);
            return person;
        }

        public async Task<bool> UpdatePersonAsync(Person person)
        {
            try
            {
                _logger.LogDebug("Updating person with UID: {PersonUid}", person.Uid);

                // Получаем существующую сущность из базы данных
                var existingPerson = await _dbSet
                    .FirstOrDefaultAsync(p => p.Uid == person.Uid);

                if (existingPerson == null)
                {
                    _logger.LogWarning("Person with UID {PersonUid} not found for update", person.Uid);
                    return false;
                }

                // Обновляем только измененные поля
                existingPerson.FirstName = person.FirstName;
                existingPerson.LastName = person.LastName;
                existingPerson.MiddleName = person.MiddleName;
                existingPerson.Email = person.Email;
                existingPerson.PhoneNumber = person.PhoneNumber;
                existingPerson.DateOfBirth = person.DateOfBirth;
                existingPerson.ProfileImageUrl = person.ProfileImageUrl;
                existingPerson.Address = person.Address;
                existingPerson.IsActive = person.IsActive;
                existingPerson.LastModifiedAt = DateTime.UtcNow;

                // Сохраняем изменения
                var result = await _dbContext.SaveChangesAsync() > 0;

                if (result)
                {
                    _logger.LogInformation("Successfully updated person with UID: {PersonUid}", person.Uid);
                    
                    // Копируем обновленные данные обратно в переданный объект
                    person.FirstName = existingPerson.FirstName;
                    person.LastName = existingPerson.LastName;
                    person.MiddleName = existingPerson.MiddleName;
                    person.Email = existingPerson.Email;
                    person.PhoneNumber = existingPerson.PhoneNumber;
                    person.DateOfBirth = existingPerson.DateOfBirth;
                    person.ProfileImageUrl = existingPerson.ProfileImageUrl;
                    person.Address = existingPerson.Address;
                    person.IsActive = existingPerson.IsActive;
                    person.LastModifiedAt = existingPerson.LastModifiedAt;
                }
                else
                {
                    _logger.LogWarning("No changes were made to person with UID: {PersonUid}", person.Uid);
                }

                return result;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating person with UID: {PersonUid}", person.Uid);
                return false;
            }
        }

        public async Task<bool> DeletePersonAsync(Guid uid)
        {
            return await DeleteAsync(uid);
        }

        public async Task<IEnumerable<Person>> GetPersonsByRoleAsync(string roleName)
        {
            return await _dbSet
                .Include(p => p.PersonRoles)
                .ThenInclude(pr => pr.Role)
                .Where(p => p.PersonRoles.Any(pr => pr.Role.Name == roleName && pr.IsActive))
                .ToListAsync();
        }

        public async Task<IEnumerable<Person>> GetPersonsByRoleAndContextAsync(string roleName, string? context = null)
        {
            var query = _dbSet
                .Include(p => p.PersonRoles)
                .ThenInclude(pr => pr.Role)
                .Where(p => p.PersonRoles.Any(pr => pr.Role.Name == roleName && pr.IsActive));

            if (!string.IsNullOrEmpty(context))
            {
                query = query.Where(p => p.PersonRoles.Any(pr => pr.Context == context));
            }

            return await query.ToListAsync();
        }

        public async Task<bool> AssignRoleAsync(Guid personUid, Guid roleUid, string? context = null, DateTime? expiresAt = null, Guid? assignedBy = null)
        {
            try
            {
                var personRole = new PersonRole
                {
                    Uid = Guid.NewGuid(),
                    PersonUid = personUid,
                    RoleUid = roleUid,
                    Context = context,
                    ExpiresAt = expiresAt,
                    AssignedBy = assignedBy,
                    AssignedAt = DateTime.UtcNow,
                    IsActive = true,
                    CreatedAt = DateTime.UtcNow,
                    LastModifiedAt = DateTime.UtcNow
                };

                _dbContext.PersonRoles.Add(personRole);
                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error assigning role {RoleUid} to person {PersonUid}", roleUid, personUid);
                return false;
            }
        }

        public async Task<bool> RevokeRoleAsync(Guid personUid, Guid roleUid, string? context = null)
        {
            try
            {
                var query = _dbContext.PersonRoles
                    .Where(pr => pr.PersonUid == personUid && pr.RoleUid == roleUid && pr.IsActive);

                if (!string.IsNullOrEmpty(context))
                {
                    query = query.Where(pr => pr.Context == context);
                }

                var personRoles = await query.ToListAsync();

                foreach (var personRole in personRoles)
                {
                    personRole.IsActive = false;
                    personRole.LastModifiedAt = DateTime.UtcNow;
                }

                await _dbContext.SaveChangesAsync();
                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error revoking role {RoleUid} from person {PersonUid}", roleUid, personUid);
                return false;
            }
        }

        public async Task<IEnumerable<PersonRole>> GetPersonRolesAsync(Guid personUid)
        {
            return await _dbContext.PersonRoles
                .Include(pr => pr.Role)
                .Where(pr => pr.PersonUid == personUid && pr.IsActive)
                .ToListAsync();
        }

        public async Task<bool> UpdateProfileAsync(Guid uid, string firstName, string lastName, string? middleName, string? phoneNumber, string? address)
        {
            try
            {
                var person = await GetByUidAsync(uid);
                if (person == null)
                    return false;

                person.FirstName = firstName;
                person.LastName = lastName;
                person.MiddleName = middleName;
                person.PhoneNumber = phoneNumber;
                person.Address = address;
                person.LastModifiedAt = DateTime.UtcNow;

                return await UpdateAsync(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile for person {PersonUid}", uid);
                return false;
            }
        }

        public async Task<bool> UpdateProfileImageAsync(Guid uid, string imageUrl)
        {
            try
            {
                var person = await GetByUidAsync(uid);
                if (person == null)
                    return false;

                person.ProfileImageUrl = imageUrl;
                person.LastModifiedAt = DateTime.UtcNow;

                return await UpdateAsync(person);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating profile image for person {PersonUid}", uid);
                return false;
            }
        }

        public async Task<IEnumerable<Person>> SearchPersonsAsync(string searchTerm)
        {
            if (string.IsNullOrWhiteSpace(searchTerm))
                return Enumerable.Empty<Person>();

            var lowerSearchTerm = searchTerm.ToLower();

            var query = _dbSet
                .Include(p => p.PersonRoles)
                .ThenInclude(pr => pr.Role)
                .Where(p => 
                    p.FirstName.ToLower().Contains(lowerSearchTerm) ||
                    p.LastName.ToLower().Contains(lowerSearchTerm) ||
                    p.Email.ToLower().Contains(lowerSearchTerm) ||
                    (p.MiddleName != null && p.MiddleName.ToLower().Contains(lowerSearchTerm)));

            // Поиск по ID - добавляем отдельное условие
            if (Guid.TryParse(searchTerm, out var personId))
            {
                query = query.Union(_dbSet
                    .Include(p => p.PersonRoles)
                    .ThenInclude(pr => pr.Role)
                    .Where(p => p.Uid == personId));
            }

            return await query.ToListAsync();
        }

        public async Task<Account?> GetPersonAccountAsync(Guid personUid)
        {
            return await _dbContext.Accounts
                .FirstOrDefaultAsync(a => a.PersonUid == personUid);
        }
    }
} 