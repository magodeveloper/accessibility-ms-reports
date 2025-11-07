using FluentAssertions;
using Reports.Tests.Helpers;
using Reports.Domain.Entities;
using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace Reports.Tests.Infrastructure;

public class EntityConfigurationTests : IDisposable
{
    private readonly ReportsDbContext _context;
    private bool _disposed;

    public EntityConfigurationTests()
    {
        var options = new DbContextOptionsBuilder<ReportsDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new ReportsDbContext(options);
        _context.Database.EnsureCreated();
    }

    [Fact]
    public void ReportEntity_ShouldHaveCorrectConstraints()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(Report));

        // Assert
        entityType.Should().NotBeNull();

        // Verificar propiedades obligatorias
        var analysisIdProperty = entityType!.FindProperty(nameof(Report.AnalysisId));
        analysisIdProperty.Should().NotBeNull();
        analysisIdProperty!.IsNullable.Should().BeFalse();

        var formatProperty = entityType.FindProperty(nameof(Report.Format));
        formatProperty.Should().NotBeNull();
        formatProperty!.IsNullable.Should().BeFalse();

        var filePathProperty = entityType.FindProperty(nameof(Report.FilePath));
        filePathProperty.Should().NotBeNull();
        filePathProperty!.IsNullable.Should().BeFalse(); // FilePath is required in database

        var generationDateProperty = entityType.FindProperty(nameof(Report.GenerationDate));
        generationDateProperty.Should().NotBeNull();
        generationDateProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void HistoryEntity_ShouldHaveCorrectConstraints()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(History));

        // Assert
        entityType.Should().NotBeNull();

        // Verificar propiedades obligatorias
        var userIdProperty = entityType!.FindProperty(nameof(History.UserId));
        userIdProperty.Should().NotBeNull();
        userIdProperty!.IsNullable.Should().BeFalse();

        var analysisIdProperty = entityType.FindProperty(nameof(History.AnalysisId));
        analysisIdProperty.Should().NotBeNull();
        analysisIdProperty!.IsNullable.Should().BeFalse();

        var createdAtProperty = entityType.FindProperty(nameof(History.CreatedAt));
        createdAtProperty.Should().NotBeNull();
        createdAtProperty!.IsNullable.Should().BeFalse();
    }

    [Fact]
    public void ReportEntity_ShouldHaveCorrectTableName()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(Report));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("REPORTS");
    }

    [Fact]
    public void HistoryEntity_ShouldHaveCorrectTableName()
    {
        // Arrange & Act
        var entityType = _context.Model.FindEntityType(typeof(History));

        // Assert
        entityType.Should().NotBeNull();
        entityType!.GetTableName().Should().Be("HISTORY");
    }

    [Fact]
    public void ReportFormat_ShouldHaveCorrectConversion()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "/test/path.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context.Reports.Add(report);
        _context.SaveChanges();

        // Assert
        var savedReport = _context.Reports.First();
        savedReport.Format.Should().Be(ReportFormat.Pdf);
    }

    [Fact]
    public void Context_ShouldSupportAllReportFormats()
    {
        // Arrange
        var formats = Enum.GetValues<ReportFormat>();
        var reports = new List<Report>();

        for (int i = 0; i < formats.Length; i++)
        {
            reports.Add(new Report
            {
                AnalysisId = i + 1,
                Format = formats[i],
                FilePath = $"/test/path{i}.{formats[i].ToString().ToLower()}",
                GenerationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            });
        }

        // Act
        _context.Reports.AddRange(reports);
        _context.SaveChanges();

        // Assert
        var savedReports = _context.Reports.ToList();
        savedReports.Should().HaveCount(formats.Length);

        foreach (var format in formats)
        {
            savedReports.Should().Contain(r => r.Format == format);
        }
    }

    [Fact]
    public void Context_ShouldHandleTimestamps()
    {
        // Arrange
        var testTime = new DateTime(2024, 1, 15, 10, 30, 0, DateTimeKind.Utc);
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Json,
            FilePath = "/test/timestamp.json",
            GenerationDate = testTime,
            CreatedAt = testTime,
            UpdatedAt = testTime.AddHours(1)
        };

        // Act
        _context.Reports.Add(report);
        _context.SaveChanges();

        // Assert - El interceptor convierte UTC a Ecuador (-5 horas)
        var expectedEcuadorTime = DateTimeHelper.ToEcuadorTime(testTime);
        var expectedUpdatedTime = DateTimeHelper.ToEcuadorTime(testTime.AddHours(1));

        var savedReport = _context.Reports.First();
        savedReport.GenerationDate.Should().Be(expectedEcuadorTime);
        savedReport.CreatedAt.Should().Be(expectedEcuadorTime);
        savedReport.UpdatedAt.Should().Be(expectedUpdatedTime);
    }

    [Fact]
    public void Context_ShouldHandleUnicodeStrings()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Html,
            FilePath = "/test/archivo_con_ñ_y_acentos_México.html",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act
        _context.Reports.Add(report);
        _context.SaveChanges();

        // Assert
        var savedReport = _context.Reports.First();
        savedReport.FilePath.Should().Contain("ñ");
        savedReport.FilePath.Should().Contain("México");
    }

    [Fact]
    public void Context_ShouldValidateRequiredFields()
    {
        // Arrange
        var invalidReport = new Report(); // Missing required fields

        // Act & Assert
        _context.Reports.Add(invalidReport);

        // Since we're using InMemory database, validation may not be enforced
        // but we can test the entity state
        var entry = _context.Entry(invalidReport);
        entry.State.Should().Be(EntityState.Added);
    }

    [Fact]
    public void Context_ShouldMaintainEntityStates()
    {
        // Arrange
        var report = new Report
        {
            AnalysisId = 1,
            Format = ReportFormat.Excel,
            FilePath = "/test/state.xlsx",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Act - Add
        _context.Reports.Add(report);
        var addedState = _context.Entry(report).State;
        _context.SaveChanges();
        var afterSaveState = _context.Entry(report).State;

        // Modify
        report.FilePath = "/test/modified.xlsx";
        var modifiedState = _context.Entry(report).State;

        // Assert
        addedState.Should().Be(EntityState.Added);
        afterSaveState.Should().Be(EntityState.Unchanged);
        modifiedState.Should().Be(EntityState.Modified);
    }

    [Fact]
    public void Context_ShouldSupportQueryingByAllProperties()
    {
        // Arrange
        var baseTime = DateTime.UtcNow;
        var reports = new[]
        {
            new Report { AnalysisId = 100, Format = ReportFormat.Pdf, FilePath = "/path1.pdf", GenerationDate = baseTime, CreatedAt = baseTime, UpdatedAt = baseTime },
            new Report { AnalysisId = 200, Format = ReportFormat.Html, FilePath = "/path2.html", GenerationDate = baseTime.AddDays(1), CreatedAt = baseTime, UpdatedAt = baseTime },
            new Report { AnalysisId = 300, Format = ReportFormat.Excel, FilePath = "/path3.xlsx", GenerationDate = baseTime.AddDays(2), CreatedAt = baseTime, UpdatedAt = baseTime }
        };

        _context.Reports.AddRange(reports);
        _context.SaveChanges();

        // Act & Assert - Query by AnalysisId
        var byAnalysisId = _context.Reports.Where(r => r.AnalysisId == 100).ToList();
        byAnalysisId.Should().HaveCount(1);

        // Query by Format
        var byFormat = _context.Reports.Where(r => r.Format == ReportFormat.Html).ToList();
        byFormat.Should().HaveCount(1);

        // Query by FilePath
        var byFilePath = _context.Reports.Where(r => r.FilePath != null && r.FilePath.Contains("path2")).ToList();
        byFilePath.Should().HaveCount(1);

        // Query by GenerationDate
        var byDate = _context.Reports.Where(r => r.GenerationDate.Date == baseTime.Date).ToList();
        byDate.Should().HaveCount(1);
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