using Moq;
using Xunit;
using FluentAssertions;
using Reports.Domain.Entities;
using Reports.Api.HealthChecks;
using Reports.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reports.Tests.HealthChecks;

/// <summary>
/// Tests unitarios para los Health Checks del microservicio Reports
/// </summary>
public class HealthChecksTests
{
    #region DatabaseHealthCheck Tests

    [Fact]
    public async Task DatabaseHealthCheck_WhenDatabaseIsAccessible_ReturnsHealthy()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ReportsDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Agregar algunos reportes de prueba
        context.Reports.AddRange(
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/reports/test1.pdf", GenerationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = ReportFormat.Html, FilePath = "/reports/test2.html", GenerationDate = DateTime.UtcNow, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        );
        await context.SaveChangesAsync();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database is accessible and responsive");
        result.Data.Should().ContainKey("reportCount");
        result.Data["reportCount"].Should().Be(2);
        result.Data.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenDatabaseIsEmpty_ReturnsHealthyWithZeroCount()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ReportsDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Data.Should().ContainKey("reportCount");
        result.Data["reportCount"].Should().Be(0);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ReportsDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenExceptionOccurs_ReturnsUnhealthy()
    {
        // Arrange - Usar un contexto que causará una excepción al intentar contar
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();

        // Dispose del contexto para forzar una excepción cuando se intente usarlo
        await context.DisposeAsync();

        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Database check failed");
        result.Data.Should().ContainKey("error");
        result.Data.Should().ContainKey("timestamp");
        result.Exception.Should().NotBeNull();
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenExceptionOccurs_LogsError()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);
        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();

        // Dispose del contexto para forzar una excepción
        await context.DisposeAsync();

        var healthCheck = new DatabaseHealthCheck(context, mockLogger.Object);

        // Act
        await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un error
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Error,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => true),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenHealthy_LogsDebugMessage()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ReportsDbContext(options);
        context.Reports.Add(new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "/test.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, mockLogger.Object);

        // Act
        await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Verificar que se registró un mensaje de debug
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Debug,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Database health check passed")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DatabaseHealthCheck_ReturnsTimestampInData()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        using var context = new ReportsDbContext(options);
        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        var beforeCheck = DateTime.UtcNow;

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        var afterCheck = DateTime.UtcNow;

        // Assert
        result.Data.Should().ContainKey("timestamp");
        var timestamp = (DateTime)result.Data["timestamp"];
        timestamp.Should().BeOnOrAfter(beforeCheck);
        timestamp.Should().BeOnOrBefore(afterCheck);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenCannotConnect_ReturnsUnhealthy()
    {
        // Arrange - Crear un DbContext que no puede conectar
        var mockContext = new Mock<ReportsDbContext>(
            new DbContextOptionsBuilder<ReportsDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options);

        var mockDatabase = new Mock<DatabaseFacade>(mockContext.Object);
        mockDatabase.Setup(d => d.CanConnectAsync(It.IsAny<CancellationToken>()))
                    .ReturnsAsync(false);

        mockContext.Setup(c => c.Database).Returns(mockDatabase.Object);

        var mockLogger = new Mock<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(mockContext.Object, mockLogger.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Cannot connect to the Reports database");
        result.Data.Should().ContainKey("database");
        result.Data["database"].Should().Be("unknown");

        // Verificar que se registró el warning
        mockLogger.Verify(
            x => x.Log(
                LogLevel.Warning,
                It.IsAny<EventId>(),
                It.Is<It.IsAnyType>((v, t) => v.ToString()!.Contains("Cannot connect to Reports database")),
                It.IsAny<Exception>(),
                It.Is<Func<It.IsAnyType, Exception?, string>>((v, t) => true)),
            Times.Once);
    }

    [Fact]
    public async Task DatabaseHealthCheck_WhenCanConnectThrowsInvalidOperation_ContinuesSuccessfully()
    {
        // Arrange - Simular InMemory DB que lanza InvalidOperationException en CanConnectAsync
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new ReportsDbContext(options);

        // Agregar datos para que el Count funcione después del catch
        context.Reports.Add(new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "/test.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        });
        await context.SaveChangesAsync();

        var logger = Mock.Of<ILogger<DatabaseHealthCheck>>();
        var healthCheck = new DatabaseHealthCheck(context, logger);

        // Act - Con InMemory DB, debería manejar cualquier excepción de CanConnectAsync
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert - Debería continuar y retornar Healthy porque InMemory DB funciona
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Database is accessible and responsive");
        result.Data.Should().ContainKey("reportCount");
        result.Data["reportCount"].Should().Be(1);
    }

    #endregion

    #region ApplicationHealthCheck Tests

    [Fact]
    public async Task ApplicationHealthCheck_WhenApplicationIsRunning_ReturnsHealthy()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        using var cts = new CancellationTokenSource();
        mockLifetime.Setup(l => l.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(logger, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Be("Application is running normally");
        result.Data.Should().ContainKey("status");
        result.Data["status"].Should().Be("running");
        result.Data.Should().ContainKey("environment");
        result.Data.Should().ContainKey("uptimeSeconds");
        result.Data.Should().ContainKey("uptimeFormatted");
        result.Data.Should().ContainKey("timestamp");
        result.Data.Should().ContainKey("machineName");
        result.Data.Should().ContainKey("processId");
    }

    [Fact]
    public async Task ApplicationHealthCheck_WhenApplicationIsStopping_ReturnsUnhealthy()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        using var cts = new CancellationTokenSource();
        await cts.CancelAsync(); // Simular que la aplicación está deteniéndose
        mockLifetime.Setup(l => l.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(logger, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Unhealthy);
        result.Description.Should().Be("Application is shutting down");
        result.Data.Should().ContainKey("status");
        result.Data["status"].Should().Be("stopping");
        result.Data.Should().ContainKey("timestamp");
    }

    [Fact]
    public async Task ApplicationHealthCheck_ReturnsValidUptime()
    {
        // Arrange
        var logger = Mock.Of<ILogger<ApplicationHealthCheck>>();
        var mockLifetime = new Mock<IHostApplicationLifetime>();
        using var cts = new CancellationTokenSource();
        mockLifetime.Setup(l => l.ApplicationStopping).Returns(cts.Token);

        var healthCheck = new ApplicationHealthCheck(logger, mockLifetime.Object);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().ContainKey("uptimeSeconds");
        var uptimeSeconds = (double)result.Data["uptimeSeconds"];
        uptimeSeconds.Should().BeGreaterThan(0);

        result.Data.Should().ContainKey("uptimeFormatted");
        var uptimeFormatted = result.Data["uptimeFormatted"].ToString();
        uptimeFormatted.Should().NotBeNullOrEmpty();
        uptimeFormatted.Should().MatchRegex(@"\d+d \d+h \d+m");
    }

    #endregion

    #region MemoryHealthCheck Tests

    [Fact]
    public async Task MemoryHealthCheck_WhenMemoryIsNormal_ReturnsHealthy()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var threshold = 10L * 1024L * 1024L * 1024L; // 10GB (muy alto para que no se alcance en tests)
        var healthCheck = new MemoryHealthCheck(logger, threshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Healthy);
        result.Description.Should().Contain("Memory usage is normal");
        result.Data.Should().ContainKey("allocatedMB");
        result.Data.Should().ContainKey("thresholdMB");
        result.Data.Should().ContainKey("gen0Collections");
        result.Data.Should().ContainKey("gen1Collections");
        result.Data.Should().ContainKey("gen2Collections");
    }

    [Fact]
    public async Task MemoryHealthCheck_WhenMemoryIsHigh_ReturnsDegraded()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var threshold = 1L; // Threshold muy bajo para simular uso alto de memoria
        var healthCheck = new MemoryHealthCheck(logger, threshold);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().Be(HealthStatus.Degraded);
        result.Description.Should().Contain("Memory usage is high");
        result.Data.Should().ContainKey("allocatedMB");
        result.Data.Should().ContainKey("thresholdMB");
    }

    [Fact]
    public async Task MemoryHealthCheck_WithDefaultThreshold_Works()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var healthCheck = new MemoryHealthCheck(logger); // Usar threshold por defecto (1GB)

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
        result.Data.Should().ContainKey("thresholdMB");
        var thresholdMB = (double)result.Data["thresholdMB"];
        thresholdMB.Should().Be(1024.0); // 1GB = 1024MB
    }

    [Fact]
    public async Task MemoryHealthCheck_ReturnsValidMemoryMetrics()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var healthCheck = new MemoryHealthCheck(logger);

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext());

        // Assert
        result.Should().NotBeNull();
        result.Data.Should().ContainKey("allocatedMB");
        var allocatedMB = (double)result.Data["allocatedMB"];
        allocatedMB.Should().BeGreaterThan(0);

        result.Data.Should().ContainKey("gen0Collections");
        var gen0 = (int)result.Data["gen0Collections"];
        gen0.Should().BeGreaterThanOrEqualTo(0);

        result.Data.Should().ContainKey("gen1Collections");
        result.Data.Should().ContainKey("gen2Collections");
    }

    [Fact]
    public async Task MemoryHealthCheck_WithCancellationToken_CompletesSuccessfully()
    {
        // Arrange
        var logger = Mock.Of<ILogger<MemoryHealthCheck>>();
        var healthCheck = new MemoryHealthCheck(logger);
        using var cts = new CancellationTokenSource();

        // Act
        var result = await healthCheck.CheckHealthAsync(new HealthCheckContext(), cts.Token);

        // Assert
        result.Should().NotBeNull();
        result.Status.Should().BeOneOf(HealthStatus.Healthy, HealthStatus.Degraded);
    }

    #endregion
}
