using System;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using ViridiscaUi.Infrastructure;

namespace ViridiscaUi.Configuration;

/// <summary>
/// Конфигурация для настройки PostgreSQL базы данных
/// </summary>
public static class DatabaseConfiguration
{
    /// <summary>
    /// Добавляет PostgreSQL DbContext в DI контейнер
    /// </summary>
    /// <param name="services">Коллекция сервисов</param>
    /// <param name="configuration">Конфигурация приложения</param>
    /// <returns>Коллекция сервисов для цепочки вызовов</returns>
    public static IServiceCollection AddPostgreSqlDatabase(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = GetConnectionString(configuration);
        
        services.AddDbContext<ApplicationDbContext>(options =>
        {
            options.UseNpgsql(connectionString, npgsqlOptions =>
            {
                npgsqlOptions.MigrationsAssembly("ViridiscaUi");
            })
            .UseSnakeCaseNamingConvention()
            .EnableSensitiveDataLogging(configuration.GetSection("Logging:EnableSensitiveDataLogging").Value == "true")
            .EnableDetailedErrors(configuration.GetSection("Logging:EnableDetailedErrors").Value == "true");
        });

        return services;
    }

    /// <summary>
    /// Получает строку подключения к базе данных
    /// </summary>
    /// <param name="configuration">Конфигурация приложения</param>
    /// <returns>Строка подключения к PostgreSQL</returns>
    private static string GetConnectionString(IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? GetDefaultConnectionString();

        return connectionString;
    }

    /// <summary>
    /// Получает строку подключения по умолчанию для разработки
    /// </summary>
    /// <returns>Строка подключения по умолчанию</returns>
    private static string GetDefaultConnectionString()
    {
        // Минимальная строка подключения без лишних параметров
        return "Host=localhost;Database=viridisca_lms_production;Username=viridisca_user;Password=viridisca_password;";
    } 
} 