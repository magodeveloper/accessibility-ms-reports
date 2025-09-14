using Microsoft.EntityFrameworkCore;
using FluentAssertions;
using Reports.Infrastructure.Data;
using Reports.Domain.Entities;
using Reports.Application.Services.Report;
using Reports.Application.Dtos;

namespace Reports.Tests.Application;

public class ReportServiceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportService _service;
    private bool _disposed;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _context.Database.EnsureCreated();
        _service = new ReportService(_context);
    }

    [Fact]
    public async Task GetByFormatAsync_WithValidFormat_ShouldReturnReports()
    {
        // Arrange
        var report1 = new Report
        {
            Format = ReportFormat.Pdf,
            FilePath = "/test/report1.pdf",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var report2 = new Report
        {
            Format = ReportFormat.Html,
            FilePath = "/test/report2.html",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 2,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reports.AddRange(report1, report2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByFormatAsync("Pdf");

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().Format.Should().Be(ReportFormat.Pdf);
    }

    [Fact]
    public async Task GetByFormatAsync_WithInvalidFormat_ShouldReturnEmptyList()
    {
        // Arrange - no setup needed

        // Act
        var result = await _service.GetByFormatAsync("InvalidFormat");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Theory]
    [InlineData("pdf")]
    [InlineData("PDF")]
    [InlineData("Pdf")]
    [InlineData("html")]
    [InlineData("HTML")]
    [InlineData("Html")]
    public async Task GetByFormatAsync_WithDifferentCasing_ShouldWork(string format)
    {
        // Arrange
        var report = new Report
        {
            Format = format.ToLower() == "pdf" ? ReportFormat.Pdf : ReportFormat.Html,
            FilePath = $"/test/report.{format.ToLower()}",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByFormatAsync(format);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_WithExistingId_ShouldReturnReports()
    {
        // Arrange
        var report1 = new Report
        {
            Format = ReportFormat.Json,
            FilePath = "/test/report1.json",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var report2 = new Report
        {
            Format = ReportFormat.Excel,
            FilePath = "/test/report2.xlsx",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 100,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reports.AddRange(report1, report2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByAnalysisIdAsync(100);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(2);
        result.All(r => r.AnalysisId == 100).Should().BeTrue();
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_WithNonExistingId_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetByAnalysisIdAsync(9999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByGenerationDateAsync_WithExistingDate_ShouldReturnReports()
    {
        // Arrange
        var targetDate = new DateTime(2024, 1, 1, 12, 0, 0, DateTimeKind.Utc);

        var report = new Report
        {
            Format = ReportFormat.Pdf,
            FilePath = "/test/report.pdf",
            GenerationDate = targetDate,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetByGenerationDateAsync(targetDate);

        // Assert
        result.Should().NotBeEmpty();
        result.Should().HaveCount(1);
        result.First().GenerationDate.Date.Should().Be(targetDate.Date);
    }

    [Fact]
    public async Task GetByGenerationDateAsync_WithNonExistingDate_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetByGenerationDateAsync(new DateTime(2099, 1, 1, 0, 0, 0, DateTimeKind.Utc));

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task CreateAsync_WithValidDto_ShouldCreateAndReturnReport()
    {
        // Arrange
        var dto = new ReportDto
        {
            Format = ReportFormat.Html,
            FilePath = "/new/report.html",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 500
        };

        // Act
        var result = await _service.CreateAsync(dto);

        // Assert
        result.Should().NotBeNull();
        result.Id.Should().BeGreaterThan(0);
        result.Format.Should().Be(ReportFormat.Html);
        result.FilePath.Should().Be("/new/report.html");
        result.AnalysisId.Should().Be(500);

        // Verify it was saved to database
        var savedReport = await _context.Reports.FindAsync(result.Id);
        savedReport.Should().NotBeNull();
        savedReport!.Format.Should().Be(ReportFormat.Html);
    }

    [Fact]
    public async Task DeleteAsync_WithExistingId_ShouldDeleteReport()
    {
        // Arrange
        var report = new Report
        {
            Format = ReportFormat.Pdf,
            FilePath = "/delete/report.pdf",
            GenerationDate = DateTime.UtcNow,
            AnalysisId = 1,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();
        var reportId = report.Id;

        // Act
        var result = await _service.DeleteAsync(reportId);

        // Assert
        result.Should().BeTrue();

        // Verify it was deleted from database
        var deletedReport = await _context.Reports.FindAsync(reportId);
        deletedReport.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_WithNonExistingId_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteAsync(9999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllAsync_WithExistingReports_ShouldDeleteAllReports()
    {
        // Arrange
        var reports = new[]
        {
            new Report { Format = ReportFormat.Pdf, FilePath = "/1.pdf", GenerationDate = DateTime.UtcNow, AnalysisId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Report { Format = ReportFormat.Html, FilePath = "/2.html", GenerationDate = DateTime.UtcNow, AnalysisId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Report { Format = ReportFormat.Json, FilePath = "/3.json", GenerationDate = DateTime.UtcNow, AnalysisId = 3, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.DeleteAllAsync();

        // Assert
        result.Should().BeTrue();

        // Verify all reports were deleted
        var remainingReports = await _context.Reports.ToListAsync();
        remainingReports.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WithNoReports_ShouldReturnFalse()
    {
        // Act
        var result = await _service.DeleteAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task GetAllAsync_WithMultipleReports_ShouldReturnAllReports()
    {
        // Arrange
        var reports = new[]
        {
            new Report { Format = ReportFormat.Pdf, FilePath = "/1.pdf", GenerationDate = DateTime.UtcNow, AnalysisId = 1, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow },
            new Report { Format = ReportFormat.Html, FilePath = "/2.html", GenerationDate = DateTime.UtcNow, AnalysisId = 2, CreatedAt = DateTime.UtcNow, UpdatedAt = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().HaveCount(2);
        result.Should().Contain(r => r.Format == ReportFormat.Pdf);
        result.Should().Contain(r => r.Format == ReportFormat.Html);
    }

    [Fact]
    public async Task GetAllAsync_WithNoReports_ShouldReturnEmptyList()
    {
        // Act
        var result = await _service.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
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
