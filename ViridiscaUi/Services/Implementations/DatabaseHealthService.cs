using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Services.Implementations;

/// <summary>
/// Сервис для проверки состояния базы данных
/// </summary>
public class DatabaseHealthService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly ILogger<DatabaseHealthService> _logger;

    public DatabaseHealthService(ApplicationDbContext dbContext, ILogger<DatabaseHealthService> logger)
    {
        _dbContext = dbContext ?? throw new ArgumentNullException(nameof(dbContext));
        _logger = logger ?? throw new ArgumentNullException(nameof(logger));
    }

    /// <summary>
    /// Проверяет подключение к базе данных
    /// </summary>
    public async Task<bool> CheckDatabaseConnectionAsync()
    {
        try
        {
            _logger.LogInformation("Проверка подключения к базе данных PostgreSQL...");
            
            var canConnect = await _dbContext.Database.CanConnectAsync();
            
            if (canConnect)
            {
                _logger.LogInformation("✅ Подключение к базе данных успешно установлено");
                
                // Проверяем наличие таблиц
                var tablesExist = await CheckTablesExistAsync();
                if (tablesExist)
                {
                    _logger.LogInformation("✅ Таблицы базы данных найдены");
                }
                else
                {
                    _logger.LogWarning("⚠️  Таблицы базы данных не найдены. Возможно, нужно выполнить миграции");
                }
                
                return true;
            }
            else
            {
                _logger.LogError("❌ Не удается подключиться к базе данных PostgreSQL");
                return false;
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Ошибка при проверке подключения к базе данных: {ErrorMessage}", ex.Message);
            return false;
        }
    }

    /// <summary>
    /// Проверяет наличие основных таблиц
    /// </summary>
    private async Task<bool> CheckTablesExistAsync()
    {
        try
        {
            // Проверяем существование таблицы Users
            var usersTableExists = await _dbContext.Database
                .ExecuteSqlRawAsync("SELECT 1 FROM information_schema.tables WHERE table_name = 'users' LIMIT 1") >= 0;
            
            return usersTableExists;
        }
        catch
        {
            return false;
        }
    }

    /// <summary>
    /// Выводит информацию о строке подключения (без чувствительных данных)
    /// </summary>
    public void LogConnectionInfo()
    {
        try
        {
            var connectionString = _dbContext.Database.GetConnectionString();
            if (!string.IsNullOrEmpty(connectionString))
            {
                // Маскируем пароль для безопасности
                var maskedConnectionString = MaskPassword(connectionString);
                _logger.LogInformation("Строка подключения: {ConnectionString}", maskedConnectionString);
            }
            else
            {
                _logger.LogError("❌ Строка подключения пуста или не задана");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Ошибка при получении информации о строке подключения: {ErrorMessage}", ex.Message);
        }
    }

    private string MaskPassword(string connectionString)
    {
        // Маскируем пароль в строке подключения
        return System.Text.RegularExpressions.Regex.Replace(
            connectionString, 
            @"Password=([^;]+)", 
            "Password=***", 
            System.Text.RegularExpressions.RegexOptions.IgnoreCase);
    }
} 