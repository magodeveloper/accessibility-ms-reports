using Xunit;
using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using Reports.Domain.Entities;
using Reports.Application.Dtos;
using Reports.Tests.Infrastructure;

namespace Reports.Tests;

public class ReportsApiTests : IClassFixture<TestWebApplicationFactory<Reports.Api.Program>>
{
    private readonly TestWebApplicationFactory<Reports.Api.Program> _factory;

    public ReportsApiTests(TestWebApplicationFactory<Reports.Api.Program> factory)
    {
        _factory = factory;

    }

    /// <summary>
    /// Helper method para crear un cliente HTTP con headers de usuario autenticado (simulando Gateway)
    /// </summary>
    private HttpClient CreateAuthenticatedClient(int userId = 1, string email = "test@test.com", string role = "user", string userName = "Test User")
    {
        var client = _factory.CreateClient();

        // Agregar headers X-User-* que el Gateway propaga después de validar JWT
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", email);
        client.DefaultRequestHeaders.Add("X-User-Role", role);
        client.DefaultRequestHeaders.Add("X-User-Name", userName);

        return client;
    }


    [Fact]
    public async Task GetByAnalysisId_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/report/by-analysis/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllReports_WhenEmpty_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/report");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByDate_NotFound()
    {
        var date = "2099-01-01";
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync($"/api/report/by-date/{date}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByFormat_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/report/by-format/INVALID");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDelete_Report_Success()
    {
        var dto = new ReportDto
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "test.pdf",
            GenerationDate = DateTime.UtcNow
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/report", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResp.Content.ReadFromJsonAsync<ResponseWithData<ReportDto>>();
        created.Should().NotBeNull();
        created!.Data.Id.Should().BeGreaterThan(0);

        // Delete
        var delResp = await client.DeleteAsync($"/api/report/{created.Data.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Report_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.DeleteAsync("/api/report/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAllReports_WhenEmpty_NotFound()
    {
        var client = CreateAuthenticatedClient();
        // First, ensure we delete any existing reports
        await client.DeleteAsync("/api/report/all");

        // Now try to delete all when empty
        var response = await client.DeleteAsync("/api/report/all");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistoryByUser_NotFound()
    {
        var client = CreateAuthenticatedClient();

        // Ensure no history exists for this user
        await client.DeleteAsync("/api/history/all");

        // Use authenticated user's ID (1) to avoid Forbid response
        var response = await client.GetAsync("/api/history/by-user/1");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllHistory_WhenEmpty_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/history");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistoryByAnalysis_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.GetAsync("/api/history/by-analysis/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDelete_History_Success()
    {
        var dto = new HistoryDto
        {
            UserId = 1,
            AnalysisId = 1
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/history", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResp.Content.ReadFromJsonAsync<ResponseWithData<HistoryDto>>();
        created.Should().NotBeNull();
        created!.data.Id.Should().BeGreaterThan(0);

        // Delete
        var delResp = await client.DeleteAsync($"/api/history/{created.Data.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_History_NotFound()
    {
        var client = CreateAuthenticatedClient();

        var response = await client.DeleteAsync("/api/history/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task DeleteAllHistory_WhenEmpty_NotFound()
    {
        var client = CreateAuthenticatedClient();
        // First, ensure we delete any existing history records
        await client.DeleteAsync("/api/history/all");

        // Now try to delete all when empty
        var response = await client.DeleteAsync("/api/history/all");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllReports_AfterCreate_Success()
    {
        // Create a report first
        var dto = new ReportDto
        {
            AnalysisId = 1,
            Format = ReportFormat.Pdf,
            FilePath = "test-getall.pdf",
            GenerationDate = DateTime.UtcNow
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/report", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get all reports
        var getAllResp = await client.GetAsync("/api/report");
        getAllResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var allReports = await getAllResp.Content.ReadFromJsonAsync<ResponseWithData<IEnumerable<ReportDto>>>();
        allReports.Should().NotBeNull();
        allReports!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteAllReports_AfterCreate_Success()
    {
        // Create a report first
        var dto = new ReportDto
        {
            AnalysisId = 2,
            Format = ReportFormat.Html,
            FilePath = "test-deleteall.html",
            GenerationDate = DateTime.UtcNow
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/report", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete all reports
        var deleteAllResp = await client.DeleteAsync("/api/report/all");
        deleteAllResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all reports are deleted
        var getAllResp = await client.GetAsync("/api/report");
        getAllResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetAllHistory_AfterCreate_Success()
    {
        // Create a history record first
        var dto = new HistoryDto
        {
            UserId = 2,
            AnalysisId = 2
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/history", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Get all history
        var getAllResp = await client.GetAsync("/api/history");
        getAllResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var allHistory = await getAllResp.Content.ReadFromJsonAsync<ResponseWithData<IEnumerable<HistoryDto>>>();
        allHistory.Should().NotBeNull();
        allHistory!.Data.Should().NotBeEmpty();
    }

    [Fact]
    public async Task DeleteAllHistory_AfterCreate_Success()
    {
        // Create a history record first
        var dto = new HistoryDto
        {
            UserId = 3,
            AnalysisId = 3
        };
        var client = CreateAuthenticatedClient();

        var createResp = await client.PostAsJsonAsync("/api/history", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Delete all history
        var deleteAllResp = await client.DeleteAsync("/api/history/all");
        deleteAllResp.StatusCode.Should().Be(HttpStatusCode.OK);

        // Verify all history is deleted
        var getAllResp = await client.GetAsync("/api/history");
        getAllResp.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private class ResponseWithData<T>
    {
        public T Data { get; set; } = default!;
    }
}
