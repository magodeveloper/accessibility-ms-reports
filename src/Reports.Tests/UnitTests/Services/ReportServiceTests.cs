using FluentAssertions;
using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Services.Report;

namespace Reports.Tests.UnitTests.Services;

public class ReportServiceTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportService _reportService;
    private bool _disposed;

    public ReportServiceTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _reportService = new ReportService(_context);
    }

    [Fact]
    public async Task CreateAsync_ValidReport_ShouldCreateSuccessfully()
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test-report.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _reportService.CreateAsync(reportDto);

        // Assert
        result.Should().NotBeNull();
        result.AnalysisId.Should().Be(100);
        result.Format.Should().Be(ReportFormat.Pdf);
        result.FilePath.Should().Be("/reports/test-report.pdf");
        result.Id.Should().BeGreaterThan(0);
    }

    [Fact]
    public async Task GetAllAsync_WhenEmpty_ShouldReturnEmptyList()
    {
        // Act
        var result = await _reportService.GetAllAsync();

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetAllAsync_WithReports_ShouldReturnAllReports()
    {
        // Arrange
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = ReportFormat.Html, FilePath = "/report2.html", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 3, Format = ReportFormat.Json, FilePath = "/report3.json", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetAllAsync();

        // Assert
        result.Should().HaveCount(3);
        result.Should().Contain(r => r.Format == ReportFormat.Pdf);
        result.Should().Contain(r => r.Format == ReportFormat.Html);
        result.Should().Contain(r => r.Format == ReportFormat.Json);
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_ExistingAnalysisId_ShouldReturnReports()
    {
        // Arrange
        var analysisId = 100;
        var reports = new[]
        {
            new Report { AnalysisId = analysisId, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = analysisId, Format = ReportFormat.Json, FilePath = "/report2.json", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 200, Format = ReportFormat.Html, FilePath = "/report3.html", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetByAnalysisIdAsync(analysisId);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.AnalysisId == analysisId);
    }

    [Fact]
    public async Task GetByAnalysisIdAsync_NonExistingAnalysisId_ShouldReturnEmpty()
    {
        // Act
        var result = await _reportService.GetByAnalysisIdAsync(999);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByGenerationDateAsync_ExistingDate_ShouldReturnReports()
    {
        // Arrange
        var testDate = new DateTime(2024, 12, 19, 10, 0, 0, DateTimeKind.Utc);
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = testDate },
            new Report { AnalysisId = 2, Format = ReportFormat.Json, FilePath = "/report2.json", GenerationDate = testDate },
            new Report { AnalysisId = 3, Format = ReportFormat.Html, FilePath = "/report3.html", GenerationDate = testDate.AddDays(1) }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetByGenerationDateAsync(testDate);

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.GenerationDate.Date == testDate.Date);
    }

    [Fact]
    public async Task GetByGenerationDateAsync_NonExistingDate_ShouldReturnEmpty()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _reportService.GetByGenerationDateAsync(testDate);

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByFormatAsync_ExistingFormat_ShouldReturnReports()
    {
        // Arrange
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = ReportFormat.Pdf, FilePath = "/report2.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 3, Format = ReportFormat.Json, FilePath = "/report3.json", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.GetByFormatAsync("PDF");

        // Assert
        result.Should().HaveCount(2);
        result.Should().OnlyContain(r => r.Format == ReportFormat.Pdf);
    }

    [Fact]
    public async Task GetByFormatAsync_NonExistingFormat_ShouldReturnEmpty()
    {
        // Act
        var result = await _reportService.GetByFormatAsync("XLSX");

        // Assert
        result.Should().NotBeNull();
        result.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAsync_ExistingReport_ShouldReturnTrue()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/report.pdf",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.DeleteAsync(report.Id);

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var deletedReport = await _context.Reports.FindAsync(report.Id);
        deletedReport.Should().BeNull();
    }

    [Fact]
    public async Task DeleteAsync_NonExistingReport_ShouldReturnFalse()
    {
        // Act
        var result = await _reportService.DeleteAsync(999);

        // Assert
        result.Should().BeFalse();
    }

    [Fact]
    public async Task DeleteAllAsync_WithReports_ShouldReturnTrue()
    {
        // Arrange
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = ReportFormat.Json, FilePath = "/report2.json", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _reportService.DeleteAllAsync();

        // Assert
        result.Should().BeTrue();

        // Verify deletion
        var remainingReports = await _context.Reports.ToListAsync();
        remainingReports.Should().BeEmpty();
    }

    [Fact]
    public async Task DeleteAllAsync_WhenEmpty_ShouldReturnFalse()
    {
        // Act
        var result = await _reportService.DeleteAllAsync();

        // Assert
        result.Should().BeFalse();
    }

    [Theory]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Html)]
    [InlineData(ReportFormat.Excel)]
    [InlineData(ReportFormat.Pdf)]
    public async Task CreateAsync_WithDifferentFormats_ShouldCreateSuccessfully(ReportFormat format)
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = format,
            FilePath = $"/reports/test-report.{format.ToString().ToLower()}",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _reportService.CreateAsync(reportDto);

        // Assert
        result.Should().NotBeNull();
        result.Format.Should().Be(format);
        result.FilePath.Should().Be(reportDto.FilePath);
    }

    [Fact]
    public async Task Service_ShouldHandleMultipleOperationsSequentially()
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/sequential-test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act & Assert - Create
        var created = await _reportService.CreateAsync(reportDto);
        created.Should().NotBeNull();
        created.Id.Should().BeGreaterThan(0);

        // Act & Assert - Get All
        var allReports = await _reportService.GetAllAsync();
        allReports.Should().HaveCount(1);

        // Act & Assert - Get by Analysis ID
        var reportsByAnalysis = await _reportService.GetByAnalysisIdAsync(100);
        reportsByAnalysis.Should().HaveCount(1);

        // Act & Assert - Delete
        var deleted = await _reportService.DeleteAsync(created.Id);
        deleted.Should().BeTrue();

        // Verify deletion
        var finalReports = await _reportService.GetAllAsync();
        finalReports.Should().BeEmpty();
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
