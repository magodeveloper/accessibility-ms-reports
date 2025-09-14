using FluentAssertions;
using Reports.Domain.Entities;

namespace Reports.Tests.Domain;

public class DomainEntitiesTests
{
    [Fact]
    public void Report_Properties_SetAndGet_ShouldWork()
    {
        // Arrange & Act
        var report = new Report
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
        report.Id.Should().Be(1);
        report.AnalysisId.Should().Be(100);
        report.Format.Should().Be(ReportFormat.Pdf);
        report.FilePath.Should().Be("/reports/test-report.pdf");
        report.GenerationDate.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
        report.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        report.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void Report_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var report = new Report();

        // Assert
        report.Id.Should().Be(0);
        report.AnalysisId.Should().Be(0);
        report.Format.Should().Be(default(ReportFormat));
        report.FilePath.Should().BeNull();
        report.GenerationDate.Should().Be(default(DateTime));
        report.CreatedAt.Should().Be(default(DateTime));
        report.UpdatedAt.Should().Be(default(DateTime));
    }

    [Fact]
    public void History_Properties_SetAndGet_ShouldWork()
    {
        // Arrange & Act
        var history = new History
        {
            Id = 1,
            UserId = 200,
            AnalysisId = 300,
            CreatedAt = DateTime.UtcNow.AddDays(-2),
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        history.Id.Should().Be(1);
        history.UserId.Should().Be(200);
        history.AnalysisId.Should().Be(300);
        history.CreatedAt.Should().BeBefore(DateTime.UtcNow);
        history.UpdatedAt.Should().BeCloseTo(DateTime.UtcNow, TimeSpan.FromMinutes(1));
    }

    [Fact]
    public void History_DefaultValues_ShouldBeSet()
    {
        // Arrange & Act
        var history = new History();

        // Assert
        history.Id.Should().Be(0);
        history.UserId.Should().Be(0);
        history.AnalysisId.Should().Be(0);
        history.CreatedAt.Should().Be(default(DateTime));
        history.UpdatedAt.Should().Be(default(DateTime));
    }

    [Theory]
    [InlineData(ReportFormat.Pdf)]
    [InlineData(ReportFormat.Html)]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Excel)]
    public void ReportFormat_Enum_ShouldHaveValidValues(ReportFormat format)
    {
        // Assert
        Enum.IsDefined(typeof(ReportFormat), format).Should().BeTrue();
    }

    [Fact]
    public void ReportFormat_ToString_ShouldReturnCorrectValues()
    {
        // Assert
        ReportFormat.Pdf.ToString().Should().Be("Pdf");
        ReportFormat.Html.ToString().Should().Be("Html");
        ReportFormat.Json.ToString().Should().Be("Json");
        ReportFormat.Excel.ToString().Should().Be("Excel");
    }

    [Fact]
    public void Report_WithAllFormats_ShouldBeValid()
    {
        // Arrange
        var formats = Enum.GetValues<ReportFormat>();

        // Act & Assert
        foreach (var format in formats)
        {
            var report = new Report
            {
                Id = 1,
                AnalysisId = 100,
                Format = format,
                FilePath = $"/reports/test.{format.ToString().ToLower()}",
                GenerationDate = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            report.Format.Should().Be(format);
            report.FilePath.Should().Contain(format.ToString().ToLower());
        }
    }

    [Fact]
    public void Report_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var report1 = new Report { Id = 1, AnalysisId = 100 };
        var report2 = new Report { Id = 1, AnalysisId = 100 };
        var report3 = new Report { Id = 2, AnalysisId = 100 };

        // Assert
        report1.Id.Should().Be(report2.Id);
        report1.Id.Should().NotBe(report3.Id);
    }

    [Fact]
    public void History_Equality_ShouldWorkCorrectly()
    {
        // Arrange
        var history1 = new History { Id = 1, UserId = 200, AnalysisId = 300 };
        var history2 = new History { Id = 1, UserId = 200, AnalysisId = 300 };
        var history3 = new History { Id = 2, UserId = 200, AnalysisId = 300 };

        // Assert
        history1.Id.Should().Be(history2.Id);
        history1.Id.Should().NotBe(history3.Id);
    }

    [Fact]
    public void Report_WithValidData_ShouldPassValidation()
    {
        // Arrange & Act
        var report = new Report
        {
            Id = 1,
            AnalysisId = 100,
            Format = ReportFormat.Pdf,
            FilePath = "/valid/path/report.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        report.Id.Should().BePositive();
        report.AnalysisId.Should().BePositive();
        report.FilePath.Should().NotBeNullOrEmpty();
        report.GenerationDate.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
    }

    [Fact]
    public void History_WithValidData_ShouldPassValidation()
    {
        // Arrange & Act
        var history = new History
        {
            Id = 1,
            UserId = 200,
            AnalysisId = 300,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert
        history.Id.Should().BePositive();
        history.UserId.Should().BePositive();
        history.AnalysisId.Should().BePositive();
        history.CreatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
        history.UpdatedAt.Should().BeBefore(DateTime.UtcNow.AddMinutes(1));
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void Report_WithInvalidAnalysisId_ShouldStillBeCreated(int invalidAnalysisId)
    {
        // Arrange & Act
        var report = new Report
        {
            Id = 1,
            AnalysisId = invalidAnalysisId,
            Format = ReportFormat.Pdf,
            FilePath = "/path/report.pdf",
            GenerationDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert - Entity creation doesn't validate business rules
        report.AnalysisId.Should().Be(invalidAnalysisId);
    }

    [Theory]
    [InlineData(0)]
    [InlineData(-1)]
    [InlineData(int.MinValue)]
    public void History_WithInvalidIds_ShouldStillBeCreated(int invalidId)
    {
        // Arrange & Act
        var history = new History
        {
            Id = 1,
            UserId = invalidId,
            AnalysisId = invalidId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        // Assert - Entity creation doesn't validate business rules
        history.UserId.Should().Be(invalidId);
        history.AnalysisId.Should().Be(invalidId);
    }
}
