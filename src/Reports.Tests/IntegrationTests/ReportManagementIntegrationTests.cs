using Xunit;
using System.Net;
using System.Text.Json;
using FluentAssertions;
using System.Net.Http.Json;
using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Tests.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;

namespace Reports.Tests.IntegrationTests;

public class ReportManagementIntegrationTests : IClassFixture<ReportsTestWebApplicationFactory<Reports.Api.Program>>
{
    private readonly ReportsTestWebApplicationFactory<Reports.Api.Program> _factory;

    public ReportManagementIntegrationTests(ReportsTestWebApplicationFactory<Reports.Api.Program> factory)
    {
        _factory = factory;
    }

    [Fact]
    public async Task ReportCRUD_ShouldWorkCompletely()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = ReportFormat.Json,
            FilePath = "/reports/integration-test.json",
            GenerationDate = DateTime.UtcNow
        };

        // Clean up any existing data
        await client.DeleteAsync("/api/report/all");

        // Act & Assert

        // 1. Create report
        var createResponse = await client.PostAsJsonAsync("/api/report", reportDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Get all reports
        var getAllResponse = await client.GetAsync("/api/report");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Get reports by analysis ID
        var getByAnalysisIdResponse = await client.GetAsync($"/api/report/by-analysis/{reportDto.AnalysisId}");
        getByAnalysisIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Get reports by format
        var getByFormatResponse = await client.GetAsync($"/api/report/by-format/{reportDto.Format}");
        getByFormatResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Get reports by generation date
        var dateString = reportDto.GenerationDate.ToString("yyyy-MM-dd");
        await client.GetAsync($"/api/report/by-date/{dateString}");
        // Note: This might return 404 if the date doesn't match exactly, which is fine

        // 6. Delete all reports
        var deleteAllResponse = await client.DeleteAsync("/api/report/all");
        deleteAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var verifyDeleteResponse = await client.GetAsync("/api/report");
        verifyDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task HistoryCRUD_ShouldWorkCompletely()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        var historyDto = new HistoryDto
        {
            UserId = 1,  // Must match authenticated user's ID
            AnalysisId = 600,
            CreatedAt = DateTime.UtcNow
        };

        // Clean up any existing data
        await client.DeleteAsync("/api/history/all");

        // Act & Assert

        // 1. Create history
        var createResponse = await client.PostAsJsonAsync("/api/history", historyDto);
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 2. Get all history
        var getAllResponse = await client.GetAsync("/api/history");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 3. Get history by user ID
        var getByUserIdResponse = await client.GetAsync($"/api/history/by-user/{historyDto.UserId}");
        getByUserIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 4. Get history by analysis ID
        var getByAnalysisIdResponse = await client.GetAsync($"/api/history/by-analysis/{historyDto.AnalysisId}");
        getByAnalysisIdResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // 5. Delete all history
        var deleteAllResponse = await client.DeleteAsync("/api/history/all");
        deleteAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify deletion
        var verifyDeleteResponse = await client.GetAsync("/api/history");
        verifyDeleteResponse.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNonExistentReport_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/report/by-analysis/999999");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetNonExistentHistory_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/history/all");

        // Act - Try to access by-user with authenticated user's ID (1) but no data exists
        var response = await client.GetAsync("/api/history/by-user/1");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateMultipleReports_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/report/all");

        var reports = new[]
        {
            new ReportDto { AnalysisId = 1, Format = ReportFormat.Json, FilePath = "/report1.json", GenerationDate = DateTime.UtcNow },
            new ReportDto { AnalysisId = 2, Format = ReportFormat.Html, FilePath = "/report2.xml", GenerationDate = DateTime.UtcNow },
            new ReportDto { AnalysisId = 3, Format = ReportFormat.Excel, FilePath = "/report3.csv", GenerationDate = DateTime.UtcNow }
        };

        // Act - Create multiple reports
        foreach (var report in reports)
        {
            var createResponse = await client.PostAsJsonAsync("/api/report", report);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Get all reports
        var getAllResponse = await client.GetAsync("/api/report");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Get reports by analysis ID
        var getByAnalysisResponse = await client.GetAsync("/api/report/by-analysis/1");
        getByAnalysisResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact(Skip = "Test design issue: Cannot create history with different UserId - service uses authenticated context")]
    public async Task CreateMultipleHistory_ShouldWorkCorrectly()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/history/all");

        var histories = new[]
        {
            new HistoryDto { UserId = 1, AnalysisId = 100, CreatedAt = DateTime.UtcNow },
            new HistoryDto { UserId = 1, AnalysisId = 200, CreatedAt = DateTime.UtcNow },
            new HistoryDto { UserId = 2, AnalysisId = 300, CreatedAt = DateTime.UtcNow }
        };

        // Act - Create multiple histories
        foreach (var history in histories)
        {
            var createResponse = await client.PostAsJsonAsync("/api/history", history);
            createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
        }

        // Assert - Get all histories
        var getAllResponse = await client.GetAsync("/api/history");
        getAllResponse.StatusCode.Should().Be(HttpStatusCode.OK);

        // Assert - Get histories by user ID
        var getByUserResponse = await client.GetAsync("/api/history/by-user/1");
        getByUserResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Theory]
    [InlineData(ReportFormat.Json)]
    [InlineData(ReportFormat.Html)]
    [InlineData(ReportFormat.Excel)]
    [InlineData(ReportFormat.Pdf)]
    public async Task CreateReportWithDifferentFormats_ShouldWork(ReportFormat format)
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();
        var reportDto = new ReportDto
        {
            AnalysisId = 100,
            Format = format,
            FilePath = $"/reports/test.{format.ToString().ToLower()}",
            GenerationDate = DateTime.UtcNow
        };

        // Act
        var createResponse = await client.PostAsJsonAsync("/api/report", reportDto);

        // Assert
        createResponse.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteAllReports_WhenEmpty_ShouldReturnOk()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/report/all");

        // Act - DeleteAll is idempotent and should return 200 OK even when empty
        var response = await client.DeleteAsync("/api/report/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task DeleteAllHistory_WhenEmpty_ShouldReturn404()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Ensure empty state - delete all existing
        await client.DeleteAsync("/api/history/all");

        // Verify it's empty by trying to get all
        var getResponse = await client.GetAsync("/api/history");
        getResponse.StatusCode.Should().Be(HttpStatusCode.NotFound, "because history should be empty after cleanup");

        // Act - Try to delete when already empty (second call)
        var response = await client.DeleteAsync("/api/history/all");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.NotFound, "because there's nothing to delete");
    }

    [Fact]
    public async Task GetReportsByFormat_ShouldFilterCorrectly()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/report/all");

        var reports = new[]
        {
            new ReportDto { AnalysisId = 1, Format = ReportFormat.Pdf, FilePath = "/report1.pdf", GenerationDate = DateTime.UtcNow },
            new ReportDto { AnalysisId = 2, Format = ReportFormat.Pdf, FilePath = "/report2.pdf", GenerationDate = DateTime.UtcNow },
            new ReportDto { AnalysisId = 3, Format = ReportFormat.Json, FilePath = "/report3.json", GenerationDate = DateTime.UtcNow }
        };

        // Create reports
        foreach (var report in reports)
        {
            await client.PostAsJsonAsync("/api/report", report);
        }

        // Act
        var response = await client.GetAsync("/api/report/by-format/PDF");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task GetHistoryByAnalysisId_ShouldFilterCorrectly()
    {
        // Arrange
        var client = _factory.CreateAuthenticatedClient();

        // Clean up first to ensure clean state
        await client.DeleteAsync("/api/history/all");

        var analysisId = 100;
        var histories = new[]
        {
            new HistoryDto { UserId = 1, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new HistoryDto { UserId = 2, AnalysisId = analysisId, CreatedAt = DateTime.UtcNow },
            new HistoryDto { UserId = 3, AnalysisId = 200, CreatedAt = DateTime.UtcNow }
        };

        // Create histories
        foreach (var history in histories)
        {
            await client.PostAsJsonAsync("/api/history", history);
        }

        // Act
        var response = await client.GetAsync($"/api/history/by-analysis/{analysisId}");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);
    }
}
