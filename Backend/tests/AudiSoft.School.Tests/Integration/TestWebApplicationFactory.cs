using AudiSoft.School.Api;
using AudiSoft.School.Infrastructure.Persistence;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using Microsoft.EntityFrameworkCore.InMemory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace AudiSoft.School.Tests.Integration;

/// <summary>
/// Factory personalizada para tests de integración que configura InMemory database
/// </summary>
public class TestWebApplicationFactory : WebApplicationFactory<Program>
{
    
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Configuración específica para tests - usar exactamente la misma configuración que development
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ConnectionStrings:DefaultConnection"] = "DataSource=:memory:",
                ["Jwt:SecretKey"] = "AudiSoft_School_JWT_Development_Secret_Key_2024_For_Development_Only_Do_Not_Use_In_Production",
                ["Jwt:Issuer"] = "AudiSoft.School.Api.Development",
                ["Jwt:Audience"] = "AudiSoft.School.Client.Development",
                ["Jwt:ExpiryMinutes"] = "120",
                ["Jwt:RefreshTokenExpiryDays"] = "30",
                ["Jwt:ClockSkewMinutes"] = "10"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remover el DbContext existente
            var descriptor = services.SingleOrDefault(
                d => d.ServiceType == typeof(DbContextOptions<AudiSoftSchoolDbContext>));
            if (descriptor != null)
            {
                services.Remove(descriptor);
            }

            // Agregar InMemory database para tests con nombre único
            var dbName = $"AudiSoftSchoolTestDb_{Guid.NewGuid()}";
            services.AddDbContext<AudiSoftSchoolDbContext>(options =>
            {
                options.UseInMemoryDatabase(dbName);
                options.EnableSensitiveDataLogging();
                // Configurar warnings para InMemory database  
                options.ConfigureWarnings(warnings =>
                {
                    warnings.Ignore(CoreEventId.ManyServiceProvidersCreatedWarning);
                    warnings.Ignore(InMemoryEventId.TransactionIgnoredWarning);
                });
            });

            // Configurar logging para tests
            services.AddLogging(builder =>
            {
                builder.ClearProviders();
                builder.AddConsole();
                builder.SetMinimumLevel(LogLevel.Warning);
            });
        });

        // Usar environment Development para mantener la configuración JWT
        builder.UseEnvironment("Development");
    }

    /// <summary>
    /// Inicializa la base de datos con datos de prueba
    /// </summary>
    public async Task InitializeDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AudiSoftSchoolDbContext>();
        
        // Asegurar que la DB esté creada
        await context.Database.EnsureCreatedAsync();
        
        // Solo seed si no tiene datos
        if (!context.Usuarios.Any())
        {
            await DataSeeder.SeedAsync(context);
        }
    }

    /// <summary>
    /// Limpia la base de datos
    /// </summary>
    public async Task CleanDatabaseAsync()
    {
        using var scope = Services.CreateScope();
        var context = scope.ServiceProvider.GetRequiredService<AudiSoftSchoolDbContext>();
        
        await context.Database.EnsureDeletedAsync();
    }
}