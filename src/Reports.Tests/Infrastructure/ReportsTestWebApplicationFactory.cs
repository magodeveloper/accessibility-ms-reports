using Reports.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Tests.Infrastructure
{
    public class ReportsTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("TestEnvironment");

            builder.ConfigureAppConfiguration((context, config) =>
            {
                // Limpiar configuraciones existentes y agregar configuración específica de tests
                config.Sources.Clear();
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment",
                    ["Environment"] = "TestEnvironment"
                });

                // Agregar configuración de test específica
                config.AddJsonFile("appsettings.Test.json", optional: true);
            });

            builder.ConfigureServices(services =>
            {
                // Buscar y remover TODOS los DbContext relacionados
                var descriptors = services.Where(d => d.ServiceType.Name.Contains("DbContext") ||
                                                     d.ServiceType == typeof(DbContextOptions<ReportsDbContext>) ||
                                                     d.ServiceType == typeof(ReportsDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Configurar explícitamente el DbContext para usar InMemory
                services.AddDbContext<ReportsDbContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase"));

                // Crear la base de datos en memoria
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
                context.Database.EnsureCreated();
            });
        }

        public ReportsDbContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
        }

        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }
    }
}
