using System;
using System.Linq;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Validate parameters
        if (config == null)
            throw new ArgumentNullException(nameof(config));

        // Detectar entorno de tests desde ambas claves posibles
        var environment = config["ASPNETCORE_ENVIRONMENT"] ?? config["Environment"];

        // Lista de entornos que deben usar InMemory database para tests
        var testEnvironments = new[] { "TestEnvironment", "Testing", "Test", "UnitTest", "IntegrationTest", "Development" };

        if (testEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
        {
            // Usar InMemory database para tests (sin restricciones de clave externa)
            services.AddDbContext<ReportsDbContext>(opt =>
            {
                opt.UseInMemoryDatabase("TestReportsDb");
                // Configurar InMemory para ignorar restricciones relacionales
                opt.ConfigureWarnings(warnings => warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning));
            });
        }
        else
        {
            // Usar MySQL para desarrollo y producción
            var cs = config.GetConnectionString("Default")
                     ?? "server=127.0.0.1;port=3306;database=reportsdb;user=msuser;password=msapass;TreatTinyAsBoolean=false";

            services.AddDbContext<ReportsDbContext>(opt =>
            {
                opt.UseMySql(
                    cs,
                    new MySqlServerVersion(new Version(8, 4, 6)),
                    o => o.EnableRetryOnFailure(
                        maxRetryCount: 5,
                        maxRetryDelay: System.TimeSpan.FromSeconds(10),
                        errorNumbersToAdd: null
                    )
                );
            });
        }

        return services;
    }
}