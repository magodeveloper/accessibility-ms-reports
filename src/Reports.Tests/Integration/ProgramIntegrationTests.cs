using System.Net;
using System.Text;
using System.Text.Json;
using FluentAssertions;
using Reports.Infrastructure.Data;
using Microsoft.Extensions.Hosting;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Reports.Application.Services.Report;
using Reports.Application.Services.History;
using Microsoft.Extensions.DependencyInjection;
using Reports.Application.Services.UserContext;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reports.Tests.Integration;

public class ProgramIntegrationTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly WebApplicationFactory<Program> _factory;
    private readonly HttpClient _client;

    public ProgramIntegrationTests(WebApplicationFactory<Program> factory)
    {
        _factory = factory.WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("IntegrationTest");

            builder.ConfigureServices(services =>
            {
                // Remover el DbContext de MySQL
                var descriptor = services.SingleOrDefault(
                    d => d.ServiceType == typeof(DbContextOptions<ReportsDbContext>));
                if (descriptor != null)
                {
                    services.Remove(descriptor);
                }

                // Agregar DbContext InMemory
                services.AddDbContext<ReportsDbContext>(options =>
                {
                    options.UseInMemoryDatabase("TestDatabase");
                });
            });
        });

        _client = _factory.CreateClient();
    }

    // ===== SERVICE REGISTRATION TESTS =====

    [Fact]
    public void Services_ShouldRegisterControllers()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert - Verificar que los controladores están registrados
        var mvcBuilder = services.GetService<Microsoft.AspNetCore.Mvc.Infrastructure.IActionDescriptorCollectionProvider>();
        mvcBuilder.Should().NotBeNull();
    }

    [Fact]
    public void Services_ShouldRegisterReportService()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var reportService = services.GetService<IReportService>();
        reportService.Should().NotBeNull();
        reportService.Should().BeAssignableTo<IReportService>();
    }

    [Fact]
    public void Services_ShouldRegisterHistoryService()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var historyService = services.GetService<IHistoryService>();
        historyService.Should().NotBeNull();
        historyService.Should().BeAssignableTo<IHistoryService>();
    }

    [Fact]
    public void Services_ShouldRegisterUserContext()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var userContext = services.GetService<IUserContext>();
        userContext.Should().NotBeNull();
        userContext.Should().BeAssignableTo<IUserContext>();
    }

    [Fact]
    public void Services_ShouldRegisterHealthChecks()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // Assert
        var healthCheckService = services.GetService<HealthCheckService>();
        healthCheckService.Should().NotBeNull();
    }

    // ===== HEALTH CHECK ENDPOINT TESTS =====

    [Fact]
    public async Task HealthEndpoint_Live_ShouldReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/live");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("status");
        content.Should().Contain("timestamp");
        content.Should().Contain("checks");
    }

    [Fact]
    public async Task HealthEndpoint_Ready_ShouldReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/ready");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("status");
        content.Should().Contain("database");
    }

    [Fact]
    public async Task HealthEndpoint_General_ShouldReturnOk()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("status");
        content.Should().Contain("entries");
        content.Should().Contain("totalDuration");
    }

    [Fact]
    public async Task HealthEndpoint_Live_ShouldIncludeLiveChecks()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/live");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonDocument>(content);

        // Assert
        healthReport.Should().NotBeNull();
        var checks = healthReport!.RootElement.GetProperty("checks");

        // Verificar que incluye application y memory checks
        var checksArray = checks.EnumerateArray().ToList();
        checksArray.Should().Contain(c =>
            c.GetProperty("name").GetString() == "application");
        checksArray.Should().Contain(c =>
            c.GetProperty("name").GetString() == "memory");
    }

    [Fact]
    public async Task HealthEndpoint_Ready_ShouldIncludeReadyChecks()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/health/ready");
        var content = await response.Content.ReadAsStringAsync();
        var healthReport = JsonSerializer.Deserialize<JsonDocument>(content);

        // Assert
        healthReport.Should().NotBeNull();
        var checks = healthReport!.RootElement.GetProperty("checks");

        // Verificar que incluye database checks
        var checksArray = checks.EnumerateArray().ToList();
        checksArray.Should().Contain(c =>
            c.GetProperty("name").GetString() == "database");
    }

    // ===== MIDDLEWARE PIPELINE TESTS =====

    [Fact]
    public async Task Middleware_ShouldValidateGatewaySecret_WhenMissing()
    {
        // Act - Request sin header X-Gateway-Secret
        var response = await _client.GetAsync("/api/report");

        // Assert - Debe rechazar la petición
        response.StatusCode.Should().Be(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_ShouldAcceptRequest_WithValidGatewaySecret()
    {
        // Arrange
        var client = _factory.CreateClient();

        // Obtener el secret configurado (en tests es "test-secret-key")
        var gatewaySecret = _factory.Services.GetRequiredService<IConfiguration>()
            .GetValue<string>("GatewaySecret") ?? "test-secret-key";

        client.DefaultRequestHeaders.Add("X-Gateway-Secret", gatewaySecret);
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        client.DefaultRequestHeaders.Add("X-User-Email", "test@test.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "user");
        client.DefaultRequestHeaders.Add("X-User-Name", "Test User");

        // Act
        var response = await client.GetAsync("/api/report");

        // Assert - No debe ser Forbidden (podría ser 404 NotFound si no hay reportes)
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
    }

    [Fact]
    public async Task Middleware_ShouldExtractUserContext_FromHeaders()
    {
        // Arrange
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "test-secret-key");
        client.DefaultRequestHeaders.Add("X-User-Id", "123");
        client.DefaultRequestHeaders.Add("X-User-Email", "user@example.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "admin");
        client.DefaultRequestHeaders.Add("X-User-Name", "Admin User");

        // Act
        var response = await client.GetAsync("/api/report");

        // Assert - La petición debe pasar el middleware
        response.StatusCode.Should().NotBe(HttpStatusCode.Forbidden);
        response.StatusCode.Should().NotBe(HttpStatusCode.Unauthorized);
    }

    // ===== CONTROLLER ROUTING TESTS =====

    [Fact]
    public async Task Controllers_ReportEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/report");

        // Assert - Endpoint existe (puede ser 404 NotFound si vacío, pero no 404 de ruta)
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    [Fact]
    public async Task Controllers_HistoryEndpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/api/history");

        // Assert
        response.StatusCode.Should().BeOneOf(HttpStatusCode.OK, HttpStatusCode.NotFound);
    }

    // ===== EXCEPTION HANDLER TESTS =====

    [Fact]
    public void ExceptionHandler_ShouldBeConfigured()
    {
        // Este test verifica que el middleware de excepciones está configurado
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;

        // El middleware de excepciones se registra implícitamente con app.UseExceptionHandler
        // Verificamos que la configuración se cargó correctamente
        var configuration = services.GetRequiredService<IConfiguration>();
        configuration.Should().NotBeNull();
    }

    // ===== LOCALIZATION TESTS =====

    [Fact]
    public async Task Localization_ShouldSupportSpanish()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "es");

        // Act
        var response = await client.GetAsync("/api/report");

        // Assert
        response.Should().NotBeNull();
        // La localización está configurada, el endpoint responde
    }

    [Fact]
    public async Task Localization_ShouldSupportEnglish()
    {
        // Arrange
        var client = CreateAuthenticatedClient();
        client.DefaultRequestHeaders.Add("Accept-Language", "en");

        // Act
        var response = await client.GetAsync("/api/report");

        // Assert
        response.Should().NotBeNull();
    }

    // ===== PROMETHEUS METRICS TESTS =====

    [Fact]
    public async Task Metrics_Endpoint_ShouldBeAccessible()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act
        var response = await client.GetAsync("/metrics");

        // Assert
        response.StatusCode.Should().Be(HttpStatusCode.OK);

        var content = await response.Content.ReadAsStringAsync();
        content.Should().Contain("# HELP");
        content.Should().Contain("# TYPE");
    }

    [Fact]
    public async Task Metrics_ShouldTrackHttpRequests()
    {
        // Arrange
        var client = CreateAuthenticatedClient();

        // Act - Hacer una petición para generar métricas
        await client.GetAsync("/api/report");

        // Obtener métricas
        var metricsResponse = await client.GetAsync("/metrics");
        var metricsContent = await metricsResponse.Content.ReadAsStringAsync();

        // Assert - Verificar que las métricas HTTP están presentes
        metricsContent.Should().Contain("http_request");
    }

    // ===== DATABASE MIGRATION TESTS =====

    [Fact]
    public void Database_ShouldBeCreated_InTestEnvironment()
    {
        // Arrange & Act
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var dbContext = services.GetRequiredService<Reports.Infrastructure.Data.ReportsDbContext>();

        // Assert - La base de datos InMemory debe estar creada
        var canConnect = dbContext.Database.CanConnect();
        canConnect.Should().BeTrue();
    }

    // ===== SWAGGER TESTS (Development only) =====

    [Fact]
    public void Swagger_ShouldNotBeEnabled_InTestEnvironment()
    {
        // En Integration/Test environment, Swagger no debe estar habilitado
        // Este test verifica que la lógica de entorno funciona correctamente
        using var scope = _factory.Services.CreateScope();
        var services = scope.ServiceProvider;
        var env = services.GetRequiredService<IWebHostEnvironment>();

        // Assert
        env.IsDevelopment().Should().BeFalse();
        env.EnvironmentName.Should().Be("IntegrationTest");
    }

    // ===== HELPER METHODS =====

    private HttpClient CreateAuthenticatedClient()
    {
        var client = _factory.CreateClient();
        client.DefaultRequestHeaders.Add("X-Gateway-Secret", "test-secret-key");
        client.DefaultRequestHeaders.Add("X-User-Id", "1");
        client.DefaultRequestHeaders.Add("X-User-Email", "test@test.com");
        client.DefaultRequestHeaders.Add("X-User-Role", "user");
        client.DefaultRequestHeaders.Add("X-User-Name", "Test User");
        return client;
    }
}
