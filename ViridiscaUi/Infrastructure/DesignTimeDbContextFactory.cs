using System.IO;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;

namespace ViridiscaUi.Infrastructure;

/// <summary>
/// Фабрика для создания ApplicationDbContext во время разработки (design-time)
/// Используется EF Core Tools для создания миграций
/// </summary>
public class DesignTimeDbContextFactory : IDesignTimeDbContextFactory<ApplicationDbContext>
{
    public ApplicationDbContext CreateDbContext(string[] args)
    {
        // Создаем конфигурацию для чтения connection string
        var configuration = new ConfigurationBuilder()
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json", optional: false)
            .AddJsonFile("appsettings.Development.json", optional: true)
            .AddEnvironmentVariables()
            .Build();

        // Получаем connection string
        var connectionString = configuration.GetConnectionString("PostgreSQL")
            ?? configuration.GetConnectionString("DefaultConnection")
            ?? "Host=localhost;Port=5432;Database=viridisca_lms_production;Username=viridisca_user;Password=viridisca_password;Pooling=true;MinPoolSize=1;MaxPoolSize=20;Connection Lifetime=300;";

        // Настраиваем DbContext options
        var optionsBuilder = new DbContextOptionsBuilder<ApplicationDbContext>();
        optionsBuilder.UseNpgsql(connectionString, npgsqlOptions =>
        {
            // Настройка версии PostgreSQL
            npgsqlOptions.SetPostgresVersion(15, 0);
            
            // Настройка миграций
            npgsqlOptions.MigrationsAssembly("ViridiscaUi");
            
            // Настройка таймаута команд
            npgsqlOptions.CommandTimeout(30);
        })
        .UseSnakeCaseNamingConvention(); // Автоматическое преобразование в snake_case

        return new ApplicationDbContext(optionsBuilder.Options);
    }
} 