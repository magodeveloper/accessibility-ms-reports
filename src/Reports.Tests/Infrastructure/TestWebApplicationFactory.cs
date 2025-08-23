using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Reports.Infrastructure.Data;

namespace Reports.Tests.Infrastructure;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Establecer el entorno ANTES de que se llame a ServiceRegistration
        builder.UseEnvironment("TestEnvironment");

        // Configurar la configuración ANTES de que se buildee la aplicación
        builder.ConfigureAppConfiguration((context, config) =>
        {
            config.Sources.Clear();
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment",
                ["Environment"] = "TestEnvironment"
            });
        });

        builder.ConfigureServices(services =>
        {
            // En este punto ServiceRegistration ya se ejecutó con la configuración correcta
            // Solo necesitamos crear la base de datos
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
            context.Database.EnsureCreated();
        });
    }
}
