using FluentAssertions;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Reports.Api.Controllers;
using Reports.Application.Services.Report;
using Reports.Application.Dtos;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;

namespace Reports.Tests.Controllers;

public class ReportControllerTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportService _reportService;
    private readonly ReportController _controller;
    private bool _disposed;

    public ReportControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _reportService = new ReportService(_context);
        _controller = new ReportController(_reportService);
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetAll_WithReports_ShouldReturnOk()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
        okResult.Value.Should().NotBeNull();
    }

    [Fact]
    public async Task GetByAnalysisId_ExistingId_ShouldReturnOk()
    {
        // Arrange
        var analysisId = 100;
        var report = new Report
        {
            AnalysisId = analysisId,
            Format = ReportFormat.Json,
            FilePath = "/reports/test.json",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByAnalysisId_NonExistingId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByAnalysisId(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByGenerationDate_ExistingDate_ShouldReturnOk()
    {
        // Arrange
        var testDate = new DateTime(2024, 12, 19, 10, 0, 0, DateTimeKind.Utc);
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Html,
            FilePath = "/reports/test.xml",
            GenerationDate = testDate
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByGenerationDate(testDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByGenerationDate_NonExistingDate_ShouldReturnNotFound()
    {
        // Arrange
        var testDate = new DateTime(2024, 1, 1, 0, 0, 0, DateTimeKind.Utc);

        // Act
        var result = await _controller.GetByGenerationDate(testDate);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByFormat_ExistingFormat_ShouldReturnOk()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Excel,
            FilePath = "/reports/test.excel",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByFormat("excel");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByFormat_NonExistingFormat_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByFormat("XLSX");

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_ValidReport_ShouldReturnOk()
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/create-test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_ExistingReport_ShouldReturnOk()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Json,
            FilePath = "/reports/delete-test.json",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(report.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_NonExistingReport_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.Delete(999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task DeleteAll_WithReports_ShouldReturnOk()
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
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task DeleteAll_WhenEmpty_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Html)]
    [InlineData(ReportFormat.Excel)]
    [InlineData(ReportFormat.Pdf)]
    public async Task Create_WithDifferentFormats_ShouldReturnOk(ReportFormat format)
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = format,
            FilePath = $"/reports/test.{format.ToString().ToLower()}",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Controller_ShouldHandleMultipleOperationsSequentially()
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
        var createResult = await _controller.Create(reportDto);
        createResult.Should().BeOfType<OkObjectResult>();

        // Get the created report's ID from the database
        var createdReport = await _context.Reports.FirstAsync(r => r.AnalysisId == 100);

        // Act & Assert - Get All
        var getAllResult = await _controller.GetAll();
        getAllResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Get by Analysis ID
        var getByAnalysisResult = await _controller.GetByAnalysisId(100);
        getByAnalysisResult.Should().BeOfType<OkObjectResult>();

        // Act & Assert - Delete
        var deleteResult = await _controller.Delete(createdReport.Id);
        deleteResult.Should().BeOfType<OkObjectResult>();

        // Verify deletion
        var getAllAfterDeleteResult = await _controller.GetAll();
        getAllAfterDeleteResult.Should().BeOfType<NotFoundObjectResult>();
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
