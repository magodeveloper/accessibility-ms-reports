using Reports.Application.Dtos;
using Reports.Domain.Entities;
using FluentAssertions;

namespace Reports.Tests.Dtos;

public class DtoInstantiationTests
{
    [Fact]
    public void ReportDto_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new ReportDto
        {
            Id = 1,
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/reports/test-report.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow.AddDays(-1),
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.AnalysisId.Should().Be(100);
        dto.Format.Should().Be(ReportFormat.Pdf);
        dto.FilePath.Should().Be("/reports/test-report.pdf");
        dto.GenerationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        dto.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void ReportDto_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var dto = new ReportDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(0);
        dto.AnalysisId.Should().Be(0);
        dto.Format.Should().Be(default(ReportFormat));
        dto.FilePath.Should().BeNull();
        dto.GenerationDate.Should().Be(default(DateTime));
        dto.CreatedAt.Should().Be(default(DateTime));
        dto.UpdatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void HistoryDto_CanBeInstantiated()
    {
        // Arrange & Act
        var dto = new HistoryDto
        {
            Id = 1,
            UserId = 200,
            AnalysisId = 300,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(1);
        dto.UserId.Should().Be(200);
        dto.AnalysisId.Should().Be(300);
        dto.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        dto.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void HistoryDto_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var dto = new HistoryDto();

        // Assert
        dto.Should().NotBeNull();
        dto.Id.Should().Be(0);
        dto.UserId.Should().Be(0);
        dto.AnalysisId.Should().Be(0);
        dto.CreatedAt.Should().Be(default(DateTime));
        dto.UpdatedAt.Should().Be(default(DateTime));
    }

    [Theory]
    [InlineData(ReportFormat.Pdf)]
    [InlineData(ReportFormat.Html)]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Excel)]
    public void ReportDto_WithAllFormats_ShouldBeValid(ReportFormat format)
    {
        // Arrange & Act
        var dto = new ReportDto
        {
            Id = 1,
            AnalysisId = 100,
            Format = format,
            FilePath = $"/reports/test.{format.ToString().ToLower()}",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        dto.Format.Should().Be(format);
        dto.FilePath.Should().Contain(format.ToString().ToLower());
    }

    [Fact]
    public void ReportDto_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var dto1 = new ReportDto { Id = 1, AnalysisId = 100 };
        var dto2 = new ReportDto { Id = 1, AnalysisId = 100 };
        var dto3 = new ReportDto { Id = 2, AnalysisId = 100 };

        // Assert
        dto1.Id.Should().Be(dto2.Id);
        dto1.Id.Should().NotBe(dto3.Id);
    }

    [Fact]
    public void HistoryDto_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var dto1 = new HistoryDto { Id = 1, UserId = 200, AnalysisId = 300 };
        var dto2 = new HistoryDto { Id = 1, UserId = 200, AnalysisId = 300 };
        var dto3 = new HistoryDto { Id = 2, UserId = 200, AnalysisId = 300 };

        // Assert
        dto1.Id.Should().Be(dto2.Id);
        dto1.Id.Should().NotBe(dto3.Id);
    }

    [Fact]
    public void ReportDto_PropertyAssignment_ShouldWorkCorrectly()
    {
        // Arrange
        var dto = new ReportDto();

        // Act
        dto.Id = 42;
        dto.AnalysisId = 999;
        dto.Format = ReportFormat.Excel;
        dto.FilePath = "/new/path/report.xlsx";
        dto.GenerationDate = new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Utc);
        dto.CreatedAt = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Utc);
        dto.UpdatedAt = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Utc);

        // Assert
        dto.Id.Should().Be(42);
        dto.AnalysisId.Should().Be(999);
        dto.Format.Should().Be(ReportFormat.Excel);
        dto.FilePath.Should().Be("/new/path/report.xlsx");
        dto.GenerationDate.Should().Be(new DateTime(2023, 12, 25, 0, 0, 0, DateTimeKind.Utc));
        dto.CreatedAt.Should().Be(new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Utc));
        dto.UpdatedAt.Should().Be(new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Utc));
    }

    [Fact]
    public void HistoryDto_PropertyAssignment_ShouldWorkCorrectly()
    {
        // Arrange
        var dto = new HistoryDto();

        // Act
        dto.Id = 42;
        dto.UserId = 777;
        dto.AnalysisId = 888;
        dto.CreatedAt = new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Utc);
        dto.UpdatedAt = new DateTime(2023, 12, 26, 0, 0, 0, DateTimeKind.Utc);

        // Assert
        dto.Id.Should().Be(42);
        dto.UserId.Should().Be(777);
        dto.AnalysisId.Should().Be(888);
        dto.CreatedAt.Should().Be(new DateTime(2023, 12, 24, 0, 0, 0, DateTimeKind.Unspecified));
        dto.UpdatedAt.Should().Be(new DateTime(2023, 12, 26));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void ReportDto_WithVariousIds_ShouldAcceptAllValues(int id)
    {
        // Arrange & Act
        var dto = new ReportDto { Id = id };

        // Assert
        dto.Id.Should().Be(id);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MaxValue)]
    [InlineData(int.MinValue)]
    public void HistoryDto_WithVariousIds_ShouldAcceptAllValues(int id)
    {
        // Arrange & Act
        var dto = new HistoryDto { Id = id, UserId = id, AnalysisId = id };

        // Assert
        dto.Id.Should().Be(id);
        dto.UserId.Should().Be(id);
        dto.AnalysisId.Should().Be(id);
    }
}
