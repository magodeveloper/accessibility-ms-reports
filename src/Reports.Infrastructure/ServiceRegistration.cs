using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Infrastructure;

public static class ServiceRegistration
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Detectar si estamos en entorno de tests
        var environmentName = config["ASPNETCORE_ENVIRONMENT"] ?? config["Environment"];

        if (environmentName == "TestEnvironment")
        {
            // Para tests, usar InMemory database
            services.AddDbContext<ReportsDbContext>(options =>
                options.UseInMemoryDatabase("TestDatabase"));
        }
        else
        {
            // Para producción/desarrollo, usar MySQL
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