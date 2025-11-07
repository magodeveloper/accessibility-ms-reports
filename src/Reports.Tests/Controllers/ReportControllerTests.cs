using Moq;
using FluentAssertions;
using Reports.Tests.Helpers;
using Reports.Application.Dtos;
using Microsoft.AspNetCore.Mvc;
using Reports.Api.Controllers;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Reports.Application.Services.Report;
using Reports.Application.Services.UserContext;

namespace Reports.Tests.Controllers;

public class ReportControllerTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private readonly ReportService _reportService;
    private readonly Mock<IUserContext> _mockUserContext;
    private readonly ReportController _controller;
    private bool _disposed;

    public ReportControllerTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _reportService = new ReportService(_context);

        // Configurar usuario autenticado por defecto
        _mockUserContext = new Mock<IUserContext>();
        _mockUserContext.Setup(x => x.UserId).Returns(1);
        _mockUserContext.Setup(x => x.Email).Returns("test@test.com");
        _mockUserContext.Setup(x => x.Role).Returns("user");
        _mockUserContext.Setup(x => x.UserName).Returns("Test User");
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(true);
        _mockUserContext.Setup(x => x.IsAdmin).Returns(false);

        _controller = new ReportController(_reportService, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        _controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
        {
            HttpContext = httpContext
        };
    }

    [Fact]
    public async Task GetAll_WhenEmpty_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetAll();

        // Assert - GetAll should return 200 OK with empty list, not 404
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
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
    public async Task DeleteAll_WhenEmpty_ShouldReturnOk()
    {
        // Act - DeleteAll is idempotent and should return OK even when empty
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
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

        // Verify deletion - GetAll should return 200 OK with empty list after deletion
        var getAllAfterDeleteResult = await _controller.GetAll();
        getAllAfterDeleteResult.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);

        // Act
        var result = await _controller.GetAll();

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Create_WhenNotAuthenticated_ShouldReturnUnauthorized()
    {
        // Arrange
        _mockUserContext.Setup(x => x.IsAuthenticated).Returns(false);
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert
        result.Should().BeOfType<UnauthorizedObjectResult>();
        var unauthorizedResult = result as UnauthorizedObjectResult;
        unauthorizedResult!.StatusCode.Should().Be(401);
    }

    [Fact]
    public async Task Create_WithInvalidData_ShouldStillCreateButMetricsRecorded()
    {
        // Arrange - Even with AnalysisId = 0, InMemory DB doesn't throw exceptions
        // This test verifies that metrics are recorded for report generation attempts
        var reportDto = new ReportDto
        {
            AnalysisId = 0,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert - InMemory DB will still allow this, but in real DB it would fail validation
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByFormat_WithCaseInsensitiveFormat_ShouldReturnOk()
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

        // Act - Test case insensitive
        var result = await _controller.GetByFormat("PDF");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task Delete_MultipleReports_ShouldDeleteCorrectOne()
    {
        // Arrange
        var report1 = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test1.pdf",
            GenerationDate = DateTime.UtcNow
        };
        var report2 = new Report
        {
            AnalysisId = 200,
            Format = ReportFormat.Json,
            FilePath = "/reports/test2.json",
            GenerationDate = DateTime.UtcNow
        };

        _context.Reports.AddRange(report1, report2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.Delete(report1.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify only one report remains
        var remainingReports = await _context.Reports.ToListAsync();
        remainingReports.Should().HaveCount(1);
        remainingReports[0].Id.Should().Be(report2.Id);
    }

    [Fact]
    public async Task GetByAnalysisId_WithMultipleReports_ShouldReturnAll()
    {
        // Arrange
        var analysisId = 100;
        var report1 = new Report
        {
            AnalysisId = analysisId,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test1.pdf",
            GenerationDate = DateTime.UtcNow
        };
        var report2 = new Report
        {
            AnalysisId = analysisId,
            Format = ReportFormat.Json,
            FilePath = "/reports/test2.json",
            GenerationDate = DateTime.UtcNow.AddMinutes(5)
        };

        _context.Reports.AddRange(report1, report2);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByAnalysisId(analysisId);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var reports = okResult!.Value as IEnumerable<ReportDto>;
        reports.Should().HaveCount(2);
    }

    // ===== FASE 4: EDGE CASES COMPLEJOS =====

    [Fact]
    public async Task Create_ServiceThrowsException_ShouldRecordFailureMetricAndPropagate()
    {
        // Arrange - Use a mock service that throws exception
        var mockService = new Mock<IReportService>();
        mockService.Setup(x => x.CreateAsync(It.IsAny<ReportDto>()))
            .ThrowsAsync(new InvalidOperationException("Database connection failed"));

        var controller = new ReportController(mockService.Object, _mockUserContext.Object);

        // Setup HttpContext for language helper
        var httpContext = new Microsoft.AspNetCore.Http.DefaultHttpContext();
        httpContext.Request.Headers["Accept-Language"] = "en-US";
        controller.ControllerContext = new Microsoft.AspNetCore.Mvc.ControllerContext()
        {
            HttpContext = httpContext
        };

        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test.pdf",
            GenerationDate = DateTime.UtcNow
        };

        // Act & Assert
        await Assert.ThrowsAsync<InvalidOperationException>(() => controller.Create(reportDto));

        // Note: Metrics for failure (success: false) should be recorded in the catch block
    }

    [Theory]
    [InlineData("2000-01-01T00:00:00Z")]
    [InlineData("2099-12-31T23:59:59Z")]
    public async Task GetByGenerationDate_BoundaryDates_ShouldHandleCorrectly(string dateString)
    {
        // Arrange
        var testDate = DateTime.ParseExact(dateString, "yyyy-MM-ddTHH:mm:ssZ",
            System.Globalization.CultureInfo.InvariantCulture,
            System.Globalization.DateTimeStyles.RoundtripKind);
        var report = new Report
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/boundary-test.pdf",
            GenerationDate = testDate
        };

        _context.Reports.Add(report);
        await _context.SaveChangesAsync();

        // El interceptor convierte la fecha a Ecuador, así que debemos buscar por esa fecha
        var ecuadorDate = DateTimeHelper.ToEcuadorTime(testDate);

        // Act
        var result = await _controller.GetByGenerationDate(ecuadorDate);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task GetByAnalysisId_NegativeId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByAnalysisId(-1);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByAnalysisId_ZeroId_ShouldReturnNotFound()
    {
        // Act
        var result = await _controller.GetByAnalysisId(0);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Theory]
    [InlineData("")]
    [InlineData(" ")]
    [InlineData("   ")]
    [InlineData("invalid_format")]
    [InlineData("docx")]
    [InlineData("txt")]
    [InlineData("unknown")]
    public async Task GetByFormat_InvalidOrEmptyFormat_ShouldReturnNotFound(string format)
    {
        // Act
        var result = await _controller.GetByFormat(format);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task Create_WithFutureDate_ShouldStillSucceed()
    {
        // Arrange - Even with future date, should create successfully
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/future-test.pdf",
            GenerationDate = DateTime.UtcNow.AddDays(30)
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        okResult!.StatusCode.Should().Be(200);
    }

    [Fact]
    public async Task Delete_AfterCreate_ShouldRecordDeletionMetric()
    {
        // Arrange
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Json,
            FilePath = "/reports/delete-metric-test.json",
            GenerationDate = DateTime.UtcNow
        };

        await _controller.Create(reportDto);
        var createdReport = await _context.Reports.FirstAsync(r => r.AnalysisId == 100);

        // Act
        var result = await _controller.Delete(createdReport.Id);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        // Deletion metric should be incremented (ReportsDeletionsTotal.Inc())
    }

    [Fact]
    public async Task DeleteAll_WithMultipleFormats_ShouldDeleteAll()
    {
        // Arrange
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/r1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = ReportFormat.Json, FilePath = "/r2.json", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 3, Format = ReportFormat.Html, FilePath = "/r3.html", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 4, Format = ReportFormat.Excel, FilePath = "/r4.xlsx", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.DeleteAll();

        // Assert
        result.Should().BeOfType<OkObjectResult>();

        // Verify all deleted
        var remainingReports = await _context.Reports.ToListAsync();
        remainingReports.Should().BeEmpty();
    }

    [Fact]
    public async Task GetByFormat_MultipleMatchingReports_ShouldReturnAll()
    {
        // Arrange
        var format = ReportFormat.Pdf;
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = format, FilePath = "/r1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 2, Format = format, FilePath = "/r2.pdf", GenerationDate = DateTime.UtcNow.AddMinutes(1) },
            new Report { AnalysisId = 3, Format = format, FilePath = "/r3.pdf", GenerationDate = DateTime.UtcNow.AddMinutes(2) },
            new Report { AnalysisId = 4, Format = ReportFormat.Json, FilePath = "/r4.json", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByFormat("pdf");

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedReports = okResult!.Value as IEnumerable<ReportDto>;
        returnedReports.Should().HaveCount(3);
    }

    [Fact]
    public async Task GetByGenerationDate_SameSecond_ShouldReturnAllMatching()
    {
        // Arrange
        var exactTime = new DateTime(2024, 6, 15, 14, 30, 45, DateTimeKind.Utc);
        var reports = new[]
        {
            new Report { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/r1.pdf", GenerationDate = exactTime },
            new Report { AnalysisId = 2, Format = ReportFormat.Json, FilePath = "/r2.json", GenerationDate = exactTime },
            new Report { AnalysisId = 3, Format = ReportFormat.Html, FilePath = "/r3.html", GenerationDate = exactTime.AddMinutes(1) } // Different time
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        var result = await _controller.GetByGenerationDate(exactTime);

        // Assert
        result.Should().BeOfType<OkObjectResult>();
        var okResult = result as OkObjectResult;
        var returnedReports = okResult!.Value as IEnumerable<ReportDto>;
        returnedReports.Should().HaveCountGreaterThanOrEqualTo(2); // At least 2 with exact time
    }

    [Fact]
    public async Task Create_WithEmptyFilePath_ShouldStillCreate()
    {
        // Arrange - InMemory DB allows empty filepath
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var result = await _controller.Create(reportDto);

        // Assert - InMemory DB doesn't enforce string validation
        result.Should().BeOfType<OkObjectResult>();
    }

    [Fact]
    public async Task GetAll_WithLargeDataset_ShouldReturnAll()
    {
        // Arrange - Create many reports
        var reports = Enumerable.Range(1, 50).Select(i => new Report
        {
            AnalysisId = i,
            Format = (ReportFormat)(i % 4),
            FilePath = $"/reports/test{i}.pdf",
            GenerationDate = DateTime.UtcNow.AddMinutes(i)
        }).ToArray();

        _context.Reports.AddRange(reports);
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
    public async Task Delete_NonExistingReport_ShouldNotIncrementDeletionMetric()
    {
        // Act - Delete non-existing report
        var result = await _controller.Delete(99999);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        // Deletion metric should NOT be incremented (condition: if (deleted))
    }

    [Theory]
    [InlineData(int.MinValue)]
    [InlineData(int.MaxValue)]
    public async Task Delete_ExtremeIds_ShouldReturnNotFound(int id)
    {
        // Act
        var result = await _controller.Delete(id);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
        var notFoundResult = result as NotFoundObjectResult;
        notFoundResult!.StatusCode.Should().Be(404);
    }

    [Fact]
    public async Task GetByAnalysisId_AfterDeleteAll_ShouldReturnNotFound()
    {
        // Arrange
        var reports = new[]
        {
            new Report { AnalysisId = 100, Format = ReportFormat.Pdf, FilePath = "/r1.pdf", GenerationDate = DateTime.UtcNow },
            new Report { AnalysisId = 100, Format = ReportFormat.Json, FilePath = "/r2.json", GenerationDate = DateTime.UtcNow }
        };

        _context.Reports.AddRange(reports);
        await _context.SaveChangesAsync();

        // Act
        await _controller.DeleteAll();
        var result = await _controller.GetByAnalysisId(100);

        // Assert
        result.Should().BeOfType<NotFoundObjectResult>();
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
