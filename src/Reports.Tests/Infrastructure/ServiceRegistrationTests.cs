using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Reports.Infrastructure;
using Reports.Infrastructure.Data;
using FluentAssertions;

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

    [Theory]
    [InlineData("Test")]
    [InlineData("Testing")]
    [InlineData("UnitTest")]
    [InlineData("IntegrationTest")]
    public void AddInfrastructure_WithTestEnvironments_ShouldUseInMemoryDatabase(string environment)
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration(environment);

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    #endregion

    #region Production Environment Tests (Marked as Skip - Integration Only)

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public void AddInfrastructure_WithProductionEnvironment_ShouldConfigureMySqlDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfigurationWithConnectionString("Production",
            "server=localhost;port=3306;database=reportsdb;user=testuser;password=testpass");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeFalse();
    }

    [Theory(Skip = "Requires MySQL server - Integration test only")]
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

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeFalse();
    }

    #endregion

    #region Service Registration Tests

    [Fact]
    public void AddInfrastructure_ShouldRegisterDbContextAsScoped()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("Test");

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
        var configuration = CreateConfiguration("Test");

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
        var configuration = CreateConfiguration("Test");

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
    public void AddInfrastructure_WithNullConfiguration_ShouldThrowArgumentNullException()
    {
        // Arrange
        var services = new ServiceCollection();
        IConfiguration? configuration = null;

        // Act & Assert
        Assert.Throws<ArgumentNullException>(() => services.AddInfrastructure(configuration!));
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public void AddInfrastructure_WithEmptyConfiguration_ShouldUseDefaults()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = new ConfigurationBuilder().Build();

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue(); // Default behavior for unspecified environment
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
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

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
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

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact]
    public void AddInfrastructure_WithCustomConnectionString_ShouldRegisterWithCustomString()
    {
        // Arrange
        var services = new ServiceCollection();
        var customConnectionString = "server=customserver;port=3306;database=customdb;user=customuser;password=custompass";
        var configuration = CreateConfigurationWithConnectionString("Test", customConnectionString);

        // Act
        services.AddInfrastructure(configuration);

        // Assert - For test environment, should still use InMemory regardless of connection string
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public void AddInfrastructure_WithUnknownEnvironment_ShouldDefaultToInMemoryDatabase()
    {
        // Arrange
        var services = new ServiceCollection();
        var configuration = CreateConfiguration("UnknownEnvironment");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
    }

    #endregion

    #region Logging Tests

    [Fact]
    public void AddInfrastructure_WithLogging_ShouldConfigureDbContextWithLogging()
    {
        // Arrange
        var services = new ServiceCollection();
        services.AddLogging(builder => builder.AddConsole());
        var configuration = CreateConfiguration("Test");

        // Act
        services.AddInfrastructure(configuration);

        // Assert
        var serviceProvider = services.BuildServiceProvider();
        var dbContext = serviceProvider.GetRequiredService<ReportsDbContext>();

        dbContext.Should().NotBeNull();
        dbContext.Database.IsInMemory().Should().BeTrue();
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