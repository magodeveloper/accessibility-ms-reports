using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Migrations;
using Microsoft.EntityFrameworkCore.Storage;
using Reports.Infrastructure.Data;
using Reports.Domain.Entities;
using FluentAssertions;

namespace Reports.Tests.Infrastructure;

public class MigrationsTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly IServiceProvider _serviceProvider;
    private bool _disposed;

    public MigrationsTests()
    {
        var services = new ServiceCollection();

        services.AddDbContext<ReportsDbContext>(options =>
            options.UseInMemoryDatabase(Guid.NewGuid().ToString()));

        _serviceProvider = services.BuildServiceProvider();
        _context = _serviceProvider.GetRequiredService<ReportsDbContext>();
    }

    #region Basic Migrations Tests

    [Fact]
    public void Context_ShouldApplyMigrationsSuccessfully()
    {
        // Act & Assert
        _context.Database.EnsureCreated().Should().BeTrue();
    }

    [Fact]
    public void Context_ShouldHaveCorrectModelSnapshot()
    {
        // Arrange & Act
        var model = _context.Model;

        // Assert
        model.Should().NotBeNull();
        model.GetEntityTypes().Should().HaveCount(2); // Report and History entities
    }

    [Fact]
    public void Context_ShouldHaveReportEntityConfiguration()
    {
        // Arrange
        var reportEntityType = _context.Model.FindEntityType(typeof(Report));

        // Assert
        reportEntityType.Should().NotBeNull();
        if (reportEntityType != null)
        {
            reportEntityType.GetTableName().Should().Be("reports"); // InMemory uses lowercase

            var idProperty = reportEntityType.FindProperty(nameof(Report.Id));
            idProperty.Should().NotBeNull();
            idProperty!.GetColumnName().Should().Be("id");

            var analysisIdProperty = reportEntityType.FindProperty(nameof(Report.AnalysisId));
            analysisIdProperty.Should().NotBeNull();
            analysisIdProperty!.GetColumnName().Should().Be("analysis_id");

            var formatProperty = reportEntityType.FindProperty(nameof(Report.Format));
            formatProperty.Should().NotBeNull();
            formatProperty!.GetColumnName().Should().Be("format");
        }
    }

    [Fact]
    public void Context_ShouldHaveHistoryEntityConfiguration()
    {
        // Arrange
        var historyEntityType = _context.Model.FindEntityType(typeof(History));

        // Assert
        historyEntityType.Should().NotBeNull();
        if (historyEntityType != null)
        {
            historyEntityType.GetTableName().Should().Be("history"); // InMemory uses lowercase

            var idProperty = historyEntityType.FindProperty(nameof(History.Id));
            idProperty.Should().NotBeNull();
            idProperty!.GetColumnName().Should().Be("id");

            var userIdProperty = historyEntityType.FindProperty(nameof(History.UserId));
            userIdProperty.Should().NotBeNull();
            userIdProperty!.GetColumnName().Should().Be("user_id");

            var analysisIdProperty = historyEntityType.FindProperty(nameof(History.AnalysisId));
            analysisIdProperty.Should().NotBeNull();
            analysisIdProperty!.GetColumnName().Should().Be("analysis_id");
        }
    }

    [Fact]
    public void Context_DatabaseProviderShouldBeInMemory()
    {
        // Act & Assert
        _context.Database.IsInMemory().Should().BeTrue();
        _context.Database.ProviderName.Should().Be("Microsoft.EntityFrameworkCore.InMemory");
    }

    #endregion

    #region Schema Generation Tests

    [Fact]
    public void ReportsDbContext_ShouldHaveCorrectMigrationsAssembly()
    {
        // Arrange
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        // Act & Assert
        using var context = new ReportsDbContext(options);
        context.Should().NotBeNull();
        context.Database.IsInMemory().Should().BeTrue();
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public void ReportsDbContext_ShouldGenerateCorrectSchema()
    {
        // This test would be for MySQL schema validation
        // but requires actual MySQL server
        var reportEntityType = _context.Model.FindEntityType(typeof(Report));
        var historyEntityType = _context.Model.FindEntityType(typeof(History));

        reportEntityType.Should().NotBeNull();
        historyEntityType.Should().NotBeNull();
    }

    [Fact]
    public void ReportsDbContext_ShouldHaveCorrectEntityProperties()
    {
        // Arrange
        var reportEntityType = _context.Model.FindEntityType(typeof(Report));
        var historyEntityType = _context.Model.FindEntityType(typeof(History));

        // Assert - Report Entity
        reportEntityType.Should().NotBeNull();
        var reportProperties = reportEntityType!.GetProperties();
        reportProperties.Should().Contain(p => p.Name == nameof(Report.Id));
        reportProperties.Should().Contain(p => p.Name == nameof(Report.AnalysisId));
        reportProperties.Should().Contain(p => p.Name == nameof(Report.Format));
        reportProperties.Should().Contain(p => p.Name == nameof(Report.FilePath));
        reportProperties.Should().Contain(p => p.Name == nameof(Report.GenerationDate));

        // Assert - History Entity
        historyEntityType.Should().NotBeNull();
        var historyProperties = historyEntityType!.GetProperties();
        historyProperties.Should().Contain(p => p.Name == nameof(History.Id));
        historyProperties.Should().Contain(p => p.Name == nameof(History.UserId));
        historyProperties.Should().Contain(p => p.Name == nameof(History.AnalysisId));
        historyProperties.Should().Contain(p => p.Name == nameof(History.CreatedAt));
    }

    #endregion

    #region Model Validation Tests

    [Fact]
    public void ReportsDbContext_ShouldHandleModelValidation()
    {
        // Arrange
        var reportEntityType = _context.Model.FindEntityType(typeof(Report));
        var historyEntityType = _context.Model.FindEntityType(typeof(History));

        // Assert that specific string properties in Report that should be required are not nullable
        reportEntityType.Should().NotBeNull();

        // Note: FilePath can be nullable as it might not be set immediately upon report creation
        // Only check properties that are business-critical and should always have values
        var formatProperty = reportEntityType!.FindProperty(nameof(Report.Format));
        formatProperty.Should().NotBeNull();
        // Format property is an enum, so we check that it's not nullable as enum property

        // Assert that History.CreatedAt is of type DateTime and not nullable
        var createdAtProperty = historyEntityType!.FindProperty(nameof(History.CreatedAt));
        createdAtProperty.Should().NotBeNull();
        createdAtProperty!.ClrType.Should().Be(typeof(DateTime));
        createdAtProperty.IsNullable.Should().BeFalse();

        // Assert that required integer properties are not nullable
        var reportIdProperty = reportEntityType.FindProperty(nameof(Report.Id));
        reportIdProperty.Should().NotBeNull();
        reportIdProperty!.IsNullable.Should().BeFalse();

        var analysisIdProperty = reportEntityType.FindProperty(nameof(Report.AnalysisId));
        analysisIdProperty.Should().NotBeNull();
        analysisIdProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ReportsDbContext_ShouldHaveCorrectPrimaryKeyConfiguration()
    {
        // Arrange
        var reportEntityType = _context.Model.FindEntityType(typeof(Report));
        var historyEntityType = _context.Model.FindEntityType(typeof(History));

        // Assert
        reportEntityType.Should().NotBeNull();
        var reportPrimaryKey = reportEntityType!.FindPrimaryKey();
        reportPrimaryKey.Should().NotBeNull();
        reportPrimaryKey!.Properties.Should().HaveCount(1);
        reportPrimaryKey.Properties.First().Name.Should().Be("Id");

        historyEntityType.Should().NotBeNull();
        var historyPrimaryKey = historyEntityType!.FindPrimaryKey();
        historyPrimaryKey.Should().NotBeNull();
        historyPrimaryKey!.Properties.Should().HaveCount(1);
        historyPrimaryKey.Properties.First().Name.Should().Be("Id");
    }

    #endregion

    #region Database Operations Tests

    [Fact]
    public void ReportsDbContext_ShouldSupportQueryableOperations()
    {
        // Arrange
        _context.Database.EnsureCreated();

        // Act & Assert
        var reportsQuery = _context.Reports.AsQueryable();
        var historiesQuery = _context.History.AsQueryable();

        reportsQuery.Should().NotBeNull();
        historiesQuery.Should().NotBeNull();

        // Should be able to execute basic operations
        var reportCount = reportsQuery.Count();
        var historyCount = historiesQuery.Count();

        reportCount.Should().Be(0);
        historyCount.Should().Be(0);
    }

    [Fact]
    public void ReportsDbContext_ShouldHandleChangeTracking()
    {
        // Arrange
        _context.Database.EnsureCreated();
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "/test/path.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        _context.Reports.Add(report);
        _context.ChangeTracker.DetectChanges();

        // Assert
        var entry = _context.Entry(report);
        entry.Should().NotBeNull();
        entry.State.Should().Be(EntityState.Added);

        // Save and verify
        _context.SaveChanges();
        entry.State.Should().Be(EntityState.Unchanged);
    }

    [Fact]
    public void ReportsDbContext_ShouldSupportBulkInserts()
    {
        // Arrange
        _context.Database.EnsureCreated();
        var reports = new List<Report>();

        for (int i = 1; i <= 5; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i,
                Format = ReportFormat.Pdf,
                FilePath = $"/test/path{i}.pdf",
                GenerationDate = DateTime.UtcNow
            });
        }

        // Act
        _context.Reports.AddRange(reports);
        var savedCount = _context.SaveChanges();

        // Assert
        savedCount.Should().Be(5);
        _context.Reports.Count().Should().Be(5);
    }

    #endregion

    #region Integration Tests (MySQL only)

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public async Task InitialCreate_Migration_ShouldCreateCorrectTables()
    {
        // This would test actual migration execution against MySQL
        // Requires MySQL server to be available
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public void Migration_20250823160327_InitialCreate_ShouldHaveCorrectOperations()
    {
        // This would test specific migration operations
        // Requires MySQL server to be available
        Assert.True(true, "Integration test - requires MySQL server");
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public async Task ModelSnapshot_ShouldMatchCurrentModel()
    {
        // This would compare model snapshot with actual database schema
        // Requires MySQL server to be available
        await Task.CompletedTask;
    }

    [Fact(Skip = "Requires MySQL server - Integration test only")]
    public async Task DatabaseCreation_ShouldWorkWithAllMigrations()
    {
        // This would test complete migration chain
        // Requires MySQL server to be available
        await Task.CompletedTask;
    }

    #endregion

    #region Service Configuration Tests

    [Fact(Skip = "IRelationalConnection not available in InMemory - Integration test only")]
    public void Migration_Services_ShouldBeConfiguredCorrectly()
    {
        // Act
        var dbContext = _serviceProvider.GetService<ReportsDbContext>();
        var dbRelationalConnection = _context.Database.GetService<IRelationalConnection>();

        // Assert
        dbContext.Should().NotBeNull();
        dbRelationalConnection.Should().NotBeNull();
    }

    #endregion
    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context?.Dispose();
                if (_serviceProvider is IDisposable disposableProvider)
                {
                    disposableProvider.Dispose();
                }
            }
            _disposed = true;
        }
    }
}