using FluentAssertions;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Tests.Infrastructure;

public class DatabasePerformanceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private bool _disposed;

    public DatabasePerformanceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .EnableDetailedErrors()
            .Options;

        _context = new ReportsDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public async Task Context_ShouldHandleLargeDatasets()
    {
        // Arrange
        var reports = new List<Report>();
        var baseTime = DateTime.UtcNow;

        for (int i = 0; i < 1000; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i,
                Format = (ReportFormat)(i % 4), // Cycle through formats
                FilePath = $"/large/dataset/file_{i}.pdf",
                GenerationDate = baseTime.AddDays(i % 365),
                CreatedAt = baseTime,
                UpdatedAt = baseTime
            });
        }

        // Act
        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Assert
        var count = await _context.Reports.CountAsync();
        count.Should().Be(1000);

        // Test querying performance
        var pdfReports = await _context.Reports
            .Where(r => r.Format == ReportFormat.Pdf)
            .CountAsync();

        pdfReports.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task Context_ShouldHandleBulkOperations()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var reports = Enumerable.Range(1, 100)
            .Select(i => new Report
            {
                AnalysisId = i,
                Format = ReportFormat.Json,
                FilePath = $"/bulk/operation_{i}.json",
                GenerationDate = baseTime,
                CreatedAt = baseTime,
                UpdatedAt = baseTime
            }).ToList();

        // Act - Bulk Insert
        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Bulk Update
        foreach (var report in reports)
        {
            report.UpdatedAt = baseTime.AddHours(1);
        }
        await _context.SaveChangesAsync();

        // Bulk Delete
        var toDelete = reports.Take(50).ToList();
        _context.Reports.RemoveRange(toDelete);
        await _context.SaveChangesAsync();

        // Assert
        var remainingCount = await _context.Reports.CountAsync();
        remainingCount.Should().Be(50);

        var allUpdated = await _context.Reports
            .AllAsync(r => r.UpdatedAt == baseTime.AddHours(1));
        allUpdated.Should().BeTrue();
    }

    [Fact]
    public async Task Context_ShouldHandleComplexQueries()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var reports = new List<Report>();
        var histories = new List<History>();

        // Create test data
        for (int i = 1; i <= 50; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i,
                Format = (ReportFormat)(i % 4),
                FilePath = $"/complex/query_{i}.pdf",
                GenerationDate = baseTime.AddDays(-i),
                CreatedAt = baseTime.AddDays(-i),
                UpdatedAt = baseTime
            });

            histories.Add(new History
            {
                UserId = i % 10, // 10 different users
                AnalysisId = i,
                CreatedAt = baseTime.AddDays(-i),
                UpdatedAt = baseTime
            });
        }

        _context.Reports.AddRange(reports);
        _context.History.AddRange(histories);
        await _context.SaveChangesAsync();

        // Act & Assert - Complex query with joins
        var recentReports = await _context.Reports
            .Where(r => r.GenerationDate >= baseTime.AddDays(-30))
            .OrderByDescending(r => r.GenerationDate)
            .Take(10)
            .ToListAsync();

        recentReports.Should().HaveCount(10);
        recentReports.Should().BeInDescendingOrder(r => r.GenerationDate);

        // Grouped query
        var formatCounts = await _context.Reports
            .GroupBy(r => r.Format)
            .Select(g => new { Format = g.Key, Count = g.Count() })
            .ToListAsync();

        formatCounts.Should().NotBeEmpty();
        formatCounts.Sum(f => f.Count).Should().Be(50);
    }

    [Fact]
    public async Task Context_ShouldHandleTransactionLikeOperations()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var reportData = new[]
        {
            new { AnalysisId = 1, Format = ReportFormat.Pdf },
            new { AnalysisId = 2, Format = ReportFormat.Html },
            new { AnalysisId = 3, Format = ReportFormat.Excel }
        };

        // Act - Simulate transaction-like operations
        foreach (var data in reportData)
        {
            // Create report
            var report = new Report
            {
                AnalysisId = data.AnalysisId,
                Format = data.Format,
                FilePath = $"/transaction/file_{data.AnalysisId}.{data.Format.ToString().ToLower()}",
                GenerationDate = baseTime,
                CreatedAt = baseTime,
                UpdatedAt = baseTime
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();

            // Create corresponding history
            var history = new History
            {
                UserId = data.AnalysisId * 10,
                AnalysisId = data.AnalysisId,
                CreatedAt = baseTime,
                UpdatedAt = baseTime
            };

            _context.History.Add(history);
            await _context.SaveChangesAsync();
        }

        // Assert
        var reportCount = await _context.Reports.CountAsync();
        var historyCount = await _context.History.CountAsync();

        reportCount.Should().Be(3);
        historyCount.Should().Be(3);

        // Verify data consistency
        for (int i = 1; i <= 3; i++)
        {
            var reportExists = await _context.Reports.AnyAsync(r => r.AnalysisId == i);
            var historyExists = await _context.History.AnyAsync(h => h.AnalysisId == i);

            reportExists.Should().BeTrue();
            historyExists.Should().BeTrue();
        }
    }

    [Fact]
    public async Task Context_ShouldHandleParameterizedQueries()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var testReports = new[]
        {
            new Report { AnalysisId = 100, Format = ReportFormat.Pdf, FilePath = "/param/test1.pdf", GenerationDate = baseTime, CreatedAt = baseTime, UpdatedAt = baseTime },
            new Report { AnalysisId = 200, Format = ReportFormat.Html, FilePath = "/param/test2.html", GenerationDate = baseTime.AddDays(1), CreatedAt = baseTime, UpdatedAt = baseTime },
            new Report { AnalysisId = 300, Format = ReportFormat.Excel, FilePath = "/param/test3.xlsx", GenerationDate = baseTime.AddDays(2), CreatedAt = baseTime, UpdatedAt = baseTime }
        };

        _context.Reports.AddRange(testReports);
        await _context.SaveChangesAsync();

        // Act & Assert - Parameterized queries
        var analysisIds = new[] { 100, 200 };
        var matchingReports = await _context.Reports
            .Where(r => analysisIds.Contains(r.AnalysisId))
            .ToListAsync();

        matchingReports.Should().HaveCount(2);

        // Date range query
        var startDate = baseTime.Date;
        var endDate = baseTime.AddDays(1).Date;
        var dateRangeReports = await _context.Reports
            .Where(r => r.GenerationDate.Date >= startDate && r.GenerationDate.Date <= endDate)
            .ToListAsync();

        dateRangeReports.Should().HaveCount(2);

        // String contains query
        var pathContaining = "test";
        var pathMatches = await _context.Reports
            .Where(r => r.FilePath != null && r.FilePath.Contains(pathContaining))
            .ToListAsync();

        pathMatches.Should().HaveCount(3);
    }

    [Fact]
    public async Task Context_ShouldHandleAsyncOperations()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;

        // Act - Sequential async operations (avoiding concurrency issues with shared context)
        for (int i = 0; i < 10; i++)
        {
            var report = new Report
            {
                AnalysisId = i,
                Format = ReportFormat.Json,
                FilePath = $"/async/operation_{i}.json",
                GenerationDate = baseTime,
                CreatedAt = baseTime,
                UpdatedAt = baseTime
            };

            _context.Reports.Add(report);
            await _context.SaveChangesAsync();
        }

        // Assert
        var count = await _context.Reports.CountAsync();
        count.Should().Be(10);

        // Verify all reports were created correctly
        var allReports = await _context.Reports.ToListAsync();
        allReports.Should().OnlyContain(r => r.Format == ReportFormat.Json);
        allReports.Should().HaveCount(10);
    }
    protected virtual void Dispose(bool disposing)
    {
        if (!_disposed)
        {
            if (disposing)
            {
                _context.Dispose();
            }
            _disposed = true;
        }
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
