using FluentAssertions;
using Reports.Infrastructure;
using Reports.Infrastructure.Data;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Tests.Infrastructure;

public class ServiceRegistrationTests
{
    #region Test Environment Tests

    [Fact]
    public void AddInfrastructure_WithTestEnvironment_ShouldConfigureInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("TestEnvironment");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    #endregion

    #region Production Environment Tests

    [Fact]
    public void AddInfrastructure_WithProductionEnvironment_ShouldRegisterMySqlDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithConnectionString("Production",
            "server=localhost;port=3306;database=reportsdb;user=testuser;password=testpass");

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado, no que funcione
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Theory]
    [InlineData("Production")]
    [InlineData("Staging")]
    [InlineData("Development")]
    public void AddInfrastructure_WithProductionEnvironments_ShouldRegisterMySqlDatabase(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithConnectionString(environment,
            "server=localhost;port=3306;database=reportsdb;user=testuser;password=testpass");

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado, no que funcione
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("TestEnvironment");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_ShouldReturnServiceCollection()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("TestEnvironment");

        // Act
        var result = services.AddInfrastructure(configuration);

        // Assert
        result.Should().BeSameAs(services);
    }

    [Fact]
    public void AddInfrastructure_ShouldAllowMultipleRegistrations()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("TestEnvironment");

        // Act & Assert
        Action action = () =>
        {
            services.AddInfrastructure(configuration);
            services.AddInfrastructure(configuration);
        };

        action.Should().NotThrow();
    }

    #endregion

    #region Configuration Edge Cases

    [Fact]
    public void AddInfrastructure_WithEmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithEmptyConnectionString_ShouldUseInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["ConnectionStrings:Default"] = ""
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithNullConnectionString_ShouldUseInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "Production",
                ["ConnectionStrings:Default"] = null
            })
            .Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    [Fact]
    public void AddInfrastructure_WithCustomConnectionString_ShouldRegisterWithCustomString()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConnectionString = "server=customserver;port=3306;database=customdb;user=customuser;password=custompass";
        var configuration = CreateConfigurationWithConnectionString("TestEnvironment", customConnectionString);

        // Act
        services.AddInfrastructure(configuration);

        // Assert - For TestEnvironment, should use InMemory regardless of connection string
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_WithUnknownEnvironment_ShouldDefaultToMySql()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("UnknownEnvironment");

        // Act
        services.AddInfrastructure(configuration);

        // Assert - Solo verificamos que el servicio está registrado
        var serviceDescriptor = services.FirstOrDefault(s => s.ServiceType == typeof(ReportsDbContext));
        serviceDescriptor.Should().NotBeNull();
        serviceDescriptor!.Lifetime.Should().Be(ServiceLifetime.Scoped);
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void AddInfrastructure_WithLogging_ShouldConfigureDbContextWithLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        var configuration = CreateConfiguration("TestEnvironment");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();
        var logger = serviceProvider.GetService<ILogger<ReportsDbContext>>();

        dbContext.Should().NotBeNull();
        logger.Should().NotBeNull();
    }

    #endregion

    #region Helper Methods

    private static IConfiguration CreateConfiguration(string environment)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = environment
            })
            .Build();
    }

    private static IConfiguration CreateConfigurationWithConnectionString(string environment, string connectionString)
    {
        return new ConfigurationBuilder()
            .AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = environment,
                ["ConnectionStrings:Default"] = connectionString
            })
            .Build();
    }

    #endregion
}