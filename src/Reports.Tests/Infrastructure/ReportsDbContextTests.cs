using FluentAssertions;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Reports.Tests.Infrastructure;

/// <summary>
/// Consolidated test suite for ReportsDbContext functionality.
/// Combines tests from DbContextTests.cs, ReportsDbContextAdvancedTests.cs, and ReportsDbContextComprehensiveTests.cs
/// to eliminate duplication while maintaining comprehensive coverage.
/// </summary>
public class ReportsDbContextConsolidatedTests : IDisposable
{
    private ReportsDbContext? _context;
    private bool _disposed;

    public ReportsDbContextConsolidatedTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
    }

    private static ReportsDbContext CreateContext()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new ReportsDbContext(options);
    }

    #region Basic DbSet Tests

    [Fact]
    public void ReportsDbContext_ShouldHaveReportsDbSet()
    {
        // Assert
        _context.Should().NotBeNull();
        _context!.Reports.Should().NotBeNull();
    }

    [Fact]
    public void ReportsDbContext_ShouldHaveHistoryDbSet()
    {
        // Assert
        _context.Should().NotBeNull();
        _context!.History.Should().NotBeNull();
    }

    #endregion

    #region Entity Configuration Tests

    [Fact]
    public void ReportsDbContext_ShouldConfigureReportEntity_WithCorrectTableName()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Report));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("reports"); // InMemory uses lowercase
    }

    [Fact]
    public void ReportsDbContext_ShouldConfigureHistoryEntity_WithCorrectTableName()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(History));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("history"); // InMemory uses lowercase
    }

    [Fact]
    public void ReportsDbContext_ReportEntity_ShouldHaveCorrectPrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Report));
        var primaryKey = entityType!.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void ReportsDbContext_HistoryEntity_ShouldHaveCorrectPrimaryKey()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(History));
        var primaryKey = entityType!.FindPrimaryKey();

        // Assert
        primaryKey.Should().NotBeNull();
        primaryKey!.Properties.Should().HaveCount(1);
        primaryKey.Properties[0].Name.Should().Be("Id");
    }

    [Fact]
    public void ReportsDbContext_ReportEntity_ShouldHaveCorrectColumnNames()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(Report));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(Report.Id))!.GetColumnName().Should().Be("id");
        entityType.FindProperty(nameof(Report.AnalysisId))!.GetColumnName().Should().Be("analysis_id");
        entityType.FindProperty(nameof(Report.Format))!.GetColumnName().Should().Be("format");
        entityType.FindProperty(nameof(Report.FilePath))!.GetColumnName().Should().Be("file_path");
        entityType.FindProperty(nameof(Report.GenerationDate))!.GetColumnName().Should().Be("generation_date");
        entityType.FindProperty(nameof(Report.CreatedAt))!.GetColumnName().Should().Be("created_at");
    }

    [Fact]
    public void ReportsDbContext_HistoryEntity_ShouldHaveCorrectColumnNames()
    {
        // Arrange
        using var context = CreateContext();

        // Act
        var entityType = context.Model.FindEntityType(typeof(History));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.FindProperty(nameof(History.Id))!.GetColumnName().Should().Be("id");
        entityType.FindProperty(nameof(History.UserId))!.GetColumnName().Should().Be("user_id");
        entityType.FindProperty(nameof(History.AnalysisId))!.GetColumnName().Should().Be("analysis_id");
        entityType.FindProperty(nameof(History.CreatedAt))!.GetColumnName().Should().Be("created_at");
    }

    #endregion

    #region Format Conversion Tests

    [Fact]
    public void ReportsDbContext_OnModelCreating_ShouldConfigureReportFormatConversion()
    {
        // Arrange & Act
        using var context = CreateContext();
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Html,
            FilePath = "/test.html",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        context.Reports.Add(report);
        context.SaveChanges();

        // Assert
        var savedReport = context.Reports.First();
        savedReport.Format.Should().Be(ReportFormat.Html);
    }

    [Fact]
    public void ReportsDbContext_ShouldHandleFormatConversion_ForAllEnumValues()
    {
        // Arrange
        using var context = CreateContext();

        // Act & Assert for each format
        foreach (ReportFormat format in Enum.GetValues<ReportFormat>())
        {
            var report = new Report
            {
                AnalysisId = 1,
                Format = format,
                FilePath = $"/test.{format.ToString().ToLower()}",
                GenerationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            context.Reports.Add(report);
            context.SaveChanges();

            var savedReport = context.Reports.OrderBy(r => r.Id).Last();
            savedReport.Format.Should().Be(format);

            context.Reports.Remove(savedReport);
            context.SaveChanges();
        }
    }

    #endregion

    #region Data Operations Tests

    [Fact]
    public void Report_ShouldMapToReportsTable()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "test.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context!.Reports.Add(report);
        _context.SaveChanges();

        // Assert
        var savedReport = _context.Reports.First();
        savedReport.Should().NotBeNull();
        savedReport.AnalysisId.Should().Be(1);
        savedReport.Format.Should().Be(ReportFormat.Pdf);
        savedReport.FilePath.Should().Be("test.pdf");
    }

    [Fact]
    public void History_ShouldMapToHistoryTable()
    {
        // Arrange
        var historyEntry = new History
        {
            UserId = 1,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context!.History.Add(historyEntry);
        _context.SaveChanges();

        // Assert
        var savedHistory = _context.History.First();
        savedHistory.Should().NotBeNull();
        savedHistory.UserId.Should().Be(1);
        savedHistory.AnalysisId.Should().Be(1);
    }

    [Fact]
    public void ReportsDbContext_ShouldSupportBulkInsert()
    {
        // Arrange
        var reports = new List<Report>();
        for (int i = 1; i <= 10; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i,
                Format = ReportFormat.Pdf,
                FilePath = $"test{i}.pdf",
                GenerationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Act
        _context!.Reports.AddRange(reports);
        _context.SaveChanges();

        // Assert
        _context.Reports.Should().HaveCount(10);
    }

    [Fact]
    public void ReportsDbContext_ShouldSupportComplexQueries()
    {
        // Arrange
        var reports = new List<Report>
        {
            new() { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "test1.pdf", GenerationDate = DateTime.UtcNow.AddDays(-5), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { AnalysisId = 2, Format = ReportFormat.Html, FilePath = "test2.html", GenerationDate = DateTime.UtcNow.AddDays(-3), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new() { AnalysisId = 3, Format = ReportFormat.Json, FilePath = "test3.json", GenerationDate = DateTime.UtcNow.AddDays(-1), CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context!.Reports.AddRange(reports);
        _context.SaveChanges();

        // Act
        var recentReports = _context.Reports
            .Where(r => r.GenerationDate > DateTime.UtcNow.AddDays(-4))
            .OrderByDescending(r => r.GenerationDate)
            .ToList();

        // Assert
        recentReports.Should().HaveCount(2);
        recentReports[0].Format.Should().Be(ReportFormat.Json);
    }

    #endregion

    #region Relationship Tests

    [Fact]
    public void ReportsDbContext_ShouldHandleReportAndHistoryRelationship()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "test.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var historyEntry = new History
        {
            UserId = 1,
            AnalysisId = 1, // Same AnalysisId as report
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context!.Reports.Add(report);
        _context.History.Add(historyEntry);
        _context.SaveChanges();

        // Assert
        var reportsWithHistory = _context.Reports
            .Where(r => _context.History.Any(h => h.AnalysisId == r.AnalysisId))
            .ToList();

        reportsWithHistory.Should().HaveCount(1);
        reportsWithHistory[0].AnalysisId.Should().Be(1);
    }

    #endregion

    #region Error Handling Tests

    [Fact]
    public void ReportsDbContext_ShouldHandleInvalidData()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 0, // Invalid AnalysisId
            Format = ReportFormat.Pdf,
            FilePath = "",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act & Assert
        _context!.Reports.Add(report);
        var action = () => _context.SaveChanges();

        // Note: InMemory provider is more lenient than real database
        // This test mainly verifies the context can handle edge cases
        action.Should().NotThrow();
    }

    [Fact]
    public void ReportsDbContext_ShouldHandleConcurrentAccess()
    {
        // Arrange
        using var context1 = CreateContext();
        using var context2 = CreateContext();

        var report1 = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "test1.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var report2 = new Report
        {
            AnalysisId = 2,
            Format = ReportFormat.Html,
            FilePath = "test2.html",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        context1.Reports.Add(report1);
        context2.Reports.Add(report2);

        context1.SaveChanges();
        context2.SaveChanges();

        // Assert
        context1.Reports.Should().HaveCount(1);
        context2.Reports.Should().HaveCount(1);
    }

    #endregion

    #region Performance Tests

    [Fact]
    public void ReportsDbContext_ShouldPerformEfficientQueries()
    {
        // Arrange
        var reports = new List<Report>();
        for (int i = 1; i <= 100; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i,
                Format = i % 2 == 0 ? ReportFormat.Pdf : ReportFormat.Html,
                FilePath = $"test{i}.{(i % 2 == 0 ? "pdf" : "html")}",
                GenerationDate = DateTime.UtcNow.AddDays(-i),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        _context!.Reports.AddRange(reports);
        _context.SaveChanges();

        // Act
        var pdfReports = _context.Reports
            .Where(r => r.Format == ReportFormat.Pdf)
            .Take(10)
            .ToList();

        // Assert
        pdfReports.Should().HaveCount(10);
        pdfReports.All(r => r.Format == ReportFormat.Pdf).Should().BeTrue();
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
                _context = null;
            }
            _disposed = true;
        }
    }
}
