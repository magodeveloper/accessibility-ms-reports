using Xunit;
using System.Net;
using FluentAssertions;
using System.Net.Http.Json;
using Reports.Application.Dtos;
using Reports.Tests.Infrastructure;

namespace Reports.Tests;

public class ReportsApiTests : IClassFixture<TestWebApplicationFactory<Reports.Api.Program>>
{
    private readonly TestWebApplicationFactory<Reports.Api.Program> _factory;
    private readonly HttpClient _client;

    public ReportsApiTests(TestWebApplicationFactory<Reports.Api.Program> factory)
    {
        _factory = factory;
        _client = _factory.CreateClient();
    }

    [Fact]
    public async Task GetByAnalysisId_NotFound()
    {
        var response = await _client.GetAsync("/api/report/by-analysis/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByDate_NotFound()
    {
        var date = "2099-01-01";
        var response = await _client.GetAsync($"/api/report/by-date/{date}");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetByFormat_NotFound()
    {
        var response = await _client.GetAsync("/api/report/by-format/INVALID");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task CreateAndDelete_Report_Success()
    {
        var dto = new ReportDto
        {
            AnalysisId = 1,
            Format = "PDF",
            FilePath = "test.pdf",
            GenerationDate = DateTime.UtcNow
        };
        var createResp = await _client.PostAsJsonAsync("/api/report", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResp.Content.ReadFromJsonAsync<ResponseWithData<ReportDto>>();
        created.Should().NotBeNull();
        created!.data.Id.Should().BeGreaterThan(0);

        // Delete
        var delResp = await _client.DeleteAsync($"/api/report/{created.data.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_Report_NotFound()
    {
        var response = await _client.DeleteAsync("/api/report/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistoryByUser_NotFound()
    {
        var response = await _client.GetAsync("/api/history/by-user/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task GetHistoryByAnalysis_NotFound()
    {
        var response = await _client.GetAsync("/api/history/by-analysis/9999");
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
        var createResp = await _client.PostAsJsonAsync("/api/history", dto);
        createResp.StatusCode.Should().Be(HttpStatusCode.OK);
        var created = await createResp.Content.ReadFromJsonAsync<ResponseWithData<HistoryDto>>();
        created.Should().NotBeNull();
        created!.data.Id.Should().BeGreaterThan(0);

        // Delete
        var delResp = await _client.DeleteAsync($"/api/history/{created.data.Id}");
        delResp.StatusCode.Should().Be(HttpStatusCode.OK);
    }

    [Fact]
    public async Task Delete_History_NotFound()
    {
        var response = await _client.DeleteAsync("/api/history/9999");
        response.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }

    private class ResponseWithData<T>
    {
        public T data { get; set; } = default!;
    }
}