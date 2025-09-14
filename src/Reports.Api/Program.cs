using System;
using System.Linq;
using System.Text.Json;
using FluentValidation;
using Reports.Application;
using Microsoft.OpenApi.Any;
using Reports.Infrastructure;
using Microsoft.OpenApi.Models;
using Microsoft.AspNetCore.Http;
using FluentValidation.AspNetCore;
using Reports.Infrastructure.Data;
using Microsoft.AspNetCore.Builder;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.Extensions.Configuration;
using Reports.Application.Services.Report;
using Reports.Application.Services.History;
using Microsoft.Extensions.DependencyInjection;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddOpenApi(); // .NET 9

// Registrar FluentValidation y controladores MVC
builder.Services.AddControllers(); // Controladores

builder.Services.AddFluentValidationAutoValidation();

builder.Services.AddInfrastructure(builder.Configuration); // Infraestructura

// Servicios de aplicación y dominio
builder.Services.AddScoped<IReportService, ReportService>();
builder.Services.AddScoped<IHistoryService, HistoryService>();

// Swagger/OpenAPI
builder.Services.AddEndpointsApiExplorer(); // Explorador de API
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Reports API",
        Version = "v1"
    });
});

// Localización
builder.Services.AddLocalization(options => options.ResourcesPath = "Resources");
builder.Services.AddControllers()
    .AddDataAnnotationsLocalization()
    .AddViewLocalization();

var app = builder.Build(); // Construcción de la aplicación

// Migración automática de la base de datos al iniciar la API
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
    var environment = app.Configuration["ASPNETCORE_ENVIRONMENT"];

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