using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Tests.Infrastructure;

public static class TestServiceRegistration
{
    public static IServiceCollection AddTestInfrastructure(this IServiceCollection services, IConfiguration config)
    {
        // Usar únicamente InMemory database para tests
        services.AddDbContext<ReportsDbContext>(opt =>
        {
            opt.UseInMemoryDatabase("TestReportsDb");
        });

        return services;
    }
}
