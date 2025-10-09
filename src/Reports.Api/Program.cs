using System;
using Prometheus;
using System.Linq;
using System.Text;
using System.Text.Json;
using FluentValidation;
using Reports.Application;
using Microsoft.OpenApi.Any;
using Reports.Api.Middleware;
using Reports.Infrastructure;
using Reports.Api.HealthChecks;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using Reports.Infrastructure.Data;
using FluentValidation.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using System.Text.Json.Serialization;
using Microsoft.IdentityModel.Tokens;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Reports.Application.Services.Report;
using Reports.Application.Services.History;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.Authentication.JwtBearer;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(); // .NET 9

// Registrar FluentValidation y controladores MVC
builder.Services.AddControllers(); // Controladores

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddInfrastructure(builder.Configuration); // Infraestructura

// Servicios de aplicación y dominio
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

// User Context Service - Extrae información del usuario de los headers X-User-* del Gateway
builder.Services.AddScoped<Reports.Application.Services.UserContext.IUserContext, Reports.Application.Services.UserContext.UserContext>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Explorador de API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reports API",
        Version = "v1"
    });

    // Configuración de autenticación JWT Bearer en Swagger
    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Ingrese el token JWT en el formato: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

// Localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .AddViewLocalization();

// --- JWT Authentication Configuration ---
var jwtSettings = builder.Configuration.GetSection("JwtSettings");
var secretKey = jwtSettings["SecretKey"];
var issuer = jwtSettings["Issuer"];
var audience = jwtSettings["Audience"];

if (string.IsNullOrEmpty(secretKey))
{
    throw new InvalidOperationException("JwtSettings:SecretKey is required");
}

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = issuer,
            ValidAudience = audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey)),
            ClockSkew = TimeSpan.Zero
        };
    });

builder.Services.AddAuthorization();

// --- Health Checks Configuration ---
var healthChecksConfig = builder.Configuration.GetSection("HealthChecks");
var memoryThresholdMB = healthChecksConfig.GetValue<long>("MemoryThresholdMB", 512);
var memoryThresholdBytes = memoryThresholdMB * 1024L * 1024L;

// Registrar health checks como servicios
builder.Services.AddSingleton<IHealthCheck>(sp =>
    new MemoryHealthCheck(
        sp.GetRequiredService<ILogger<MemoryHealthCheck>>(),
        memoryThresholdBytes));

var healthChecksBuilder = builder.Services.AddHealthChecks()
    // Health check básico de la aplicación
    .AddCheck<ApplicationHealthCheck>(
        "application",
        tags: new[] { "live", "ready" })

    // Health check de memoria  
    .AddCheck<MemoryHealthCheck>(
        "memory",
        tags: new[] { "live" })

    // Health check de base de datos personalizado
    .AddCheck<DatabaseHealthCheck>(
        "database",
        tags: new[] { "ready" })

    // Health check de EF Core
    .AddDbContextCheck<ReportsDbContext>(
        "reports_dbcontext",
        tags: new[] { "ready" });

// Health check de MySQL (opcional, requiere connection string)
var connectionString = builder.Configuration.GetConnectionString("Default");
if (!string.IsNullOrEmpty(connectionString))
{
    healthChecksBuilder.AddMySql(
        connectionString,
        name: "mysql",
        tags: new[] { "ready", "database" });
}

var app = builder.Build(); // Construcción de la aplicación

// Migración automática de la base de datos al iniciar la API
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    var env = scope.ServiceProvider.GetRequiredService<IWebHostEnvironment>();
    var environment = env.EnvironmentName;

    // Lista de entornos que usan InMemory database
    var testEnvironments = new[] { "TestEnvironment", "Testing", "Test", "UnitTest", "IntegrationTest", "Development" };

    if (testEnvironments.Contains(environment, StringComparer.OrdinalIgnoreCase))
    {
        // Para entornos de test/desarrollo con InMemory, solo crear la base de datos
        await db.Database.EnsureCreatedAsync();
    }
    else
    {
        // Para producción con MySQL, ejecutar migraciones
        await db.Database.MigrateAsync();
    }
}

// Configuración de localización
var supportedCultures = new[] { "es", "en" };
var localizationOptions = new RequestLocalizationOptions()
    .SetDefaultCulture("es")
    .AddSupportedCultures(supportedCultures)
    .AddSupportedUICultures(supportedCultures);
app.UseRequestLocalization(localizationOptions);

// --- Prometheus Metrics ---
// Configurar métricas HTTP automáticas
var metricsConfig = builder.Configuration.GetSection("Metrics");
var metricsEnabled = metricsConfig.GetValue<bool>("Enabled", true);

if (metricsEnabled)
{
    // Usar middleware de Prometheus para métricas HTTP
    app.UseHttpMetrics(options =>
    {
        // Personalizar rutas para evitar alta cardinalidad
        options.ReduceStatusCodeCardinality();

        // Agregar información de ruta sin parámetros dinámicos
        options.AddCustomLabel("endpoint", context =>
        {
            var endpoint = context.GetEndpoint();
            return endpoint?.DisplayName ?? "unknown";
        });
    });
}

// Gateway Secret Validation - Valida que las peticiones vengan del Gateway
app.UseGatewaySecretValidation();

// Habilitar autenticación y autorización JWT
app.UseAuthentication();
app.UseAuthorization();

// User Context Middleware - Extrae información del usuario de headers X-User-*
app.UseUserContext();

// Middleware global para manejo de errores uniformes
app.UseExceptionHandler(errorApp =>
{
    errorApp.Run(async context =>
    {
        context.Response.ContentType = "application/json";
        // Detectar idioma desde el header Accept-Language o default 'es'
        var lang = context.Request.Headers["Accept-Language"].FirstOrDefault()?.Split(',')[0] ?? "es";
        string Get(string key, string lang) => Reports.Application.Localization.Get(key, lang);
        var result = JsonSerializer.Serialize(new { error = Get("Error_InternalServer", lang) });
        context.Response.StatusCode = 500;
        await context.Response.WriteAsync(result);
    });
});

// app.UseHttpsRedirection();

// app.UseAuthorization();

// Habilitar el enrutamiento de controladores
app.MapControllers();

// --- Health Check Endpoints ---
// Endpoint de liveness: verifica que la aplicación está viva (no verifica dependencias)
app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Endpoint de readiness: verifica que la aplicación está lista para recibir tráfico
app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            checks = report.Entries.Select(e => new
            {
                name = e.Key,
                status = e.Value.Status.ToString(),
                description = e.Value.Description,
                duration = e.Value.Duration.TotalMilliseconds,
                data = e.Value.Data
            })
        });
        await context.Response.WriteAsync(result);
    }
});

// Endpoint de health general: devuelve el estado completo
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = async (context, report) =>
    {
        context.Response.ContentType = "application/json";
        var result = JsonSerializer.Serialize(new
        {
            status = report.Status.ToString(),
            timestamp = DateTime.UtcNow,
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.ToDictionary(
                e => e.Key,
                e => new
                {
                    status = e.Value.Status.ToString(),
                    description = e.Value.Description,
                    duration = e.Value.Duration.TotalMilliseconds,
                    tags = e.Value.Tags,
                    data = e.Value.Data,
                    exception = e.Value.Exception?.Message
                })
        }, new JsonSerializerOptions
        {
            WriteIndented = true,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull
        });
        await context.Response.WriteAsync(result);
    }
});

// --- Prometheus Metrics Endpoint ---
if (metricsEnabled)
{
    app.MapMetrics(); // Endpoint /metrics para Prometheus
}

// Swagger/OpenAPI
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
    app.UseSwagger();
    app.UseSwaggerUI();
}

await app.RunAsync();

public partial class Program
{
    protected Program() { }
}

// Necesario para tests de integración (WebApplicationFactory)
namespace Reports.Api
{
    public partial class Program { public Program() { } }
}