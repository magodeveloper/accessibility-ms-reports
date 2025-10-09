using System.Text;
using System.Security.Claims;
using Reports.Infrastructure.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.DependencyInjection;

namespace Reports.Tests.Infrastructure;

public class TestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
{
    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        // Establecer el entorno ANTES de que se llame a ServiceRegistration
        builder.UseEnvironment("TestEnvironment");

        // Configurar la configuración ANTES de que se buildee la aplicación
        builder.ConfigureAppConfiguration((context, config) =>
        {
            // Limpiar configuraciones existentes para evitar conflictos con appsettings.json
            config.Sources.Clear();

            // Agregar configuración en memoria específica para tests
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["ASPNETCORE_ENVIRONMENT"] = "TestEnvironment",
                ["Environment"] = "TestEnvironment",
                ["Gateway:Secret"] = "",  // Deshabilitar validación de Gateway Secret (vacío)
                ["JwtSettings:SecretKey"] = "9b3e7ER@S^glvxPWKX8nN?DTqtrd%Yj!oVIfh+BG&piHwZz6ky4Q52MumOFA-Lc0",
                ["JwtSettings:Issuer"] = "https://api.accessibility.company.com/users",
                ["JwtSettings:Audience"] = "https://accessibility.company.com",
                ["JwtSettings:ExpiryHours"] = "24",
                ["HealthChecks:MemoryThresholdMB"] = "512",
                ["Metrics:Enabled"] = "true"
            });
        });

        builder.ConfigureServices(services =>
        {
            // NO intentar remover servicios de autenticación ya registrados
            // En su lugar, agregar una autenticación de prueba con menor prioridad
            // que sobrescribirá la autenticación JWT

            // Agregar autenticación de prueba ADICIONAL (no reemplazar)
            services.AddAuthentication(options =>
            {
                options.DefaultAuthenticateScheme = "Test";
                options.DefaultChallengeScheme = "Test";
            })
            .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

            // Crear la base de datos en memoria
            var serviceProvider = services.BuildServiceProvider();
            using var scope = serviceProvider.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
            context.Database.EnsureCreated();
        });
    }

    /// <summary>
    /// Crea un HttpClient con headers de autenticación X-User-* para simular
    /// peticiones autenticadas por el Gateway.
    /// </summary>
    /// <param name="userId">ID del usuario autenticado (default: 1)</param>
    /// <param name="email">Email del usuario autenticado (default: test@example.com)</param>
    /// <param name="role">Rol del usuario autenticado (default: Admin)</param>
    /// <param name="userName">Nombre del usuario autenticado (default: TestUser)</param>
    /// <returns>HttpClient configurado con headers de autenticación</returns>
    public HttpClient CreateAuthenticatedClient(int userId = 1, string email = "test@example.com",
        string role = "Admin", string userName = "TestUser")
    {
        var client = CreateClient();

        // Agregar headers X-User-* para compatibilidad con UserContextMiddleware
        client.DefaultRequestHeaders.Add("X-User-Id", userId.ToString());
        client.DefaultRequestHeaders.Add("X-User-Email", email);
        client.DefaultRequestHeaders.Add("X-User-Role", role);
        client.DefaultRequestHeaders.Add("X-User-Name", userName);

        // Agregar token JWT Bearer para autenticación real
        var token = GenerateJwtToken(userId, email, role, userName);
        client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");

        return client;
    }

    /// <summary>
    /// Genera un token JWT de prueba válido usando la misma configuración que la aplicación.
    /// </summary>
    private static string GenerateJwtToken(int userId, string email, string role, string userName)
    {
        var secretKey = "9b3e7ER@S^glvxPWKX8nN?DTqtrd%Yj!oVIfh+BG&piHwZz6ky4Q52MumOFA-Lc0";
        var issuer = "https://api.accessibility.company.com/users";
        var audience = "https://accessibility.company.com";

        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
            new Claim(JwtRegisteredClaimNames.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, userName),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: issuer,
            audience: audience,
            claims: claims,
            expires: DateTime.UtcNow.AddHours(24),
            signingCredentials: credentials
        );

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}

/// <summary>
/// Authentication handler de prueba que siempre autentica exitosamente.
/// </summary>
public class TestAuthenticationHandler : Microsoft.AspNetCore.Authentication.AuthenticationHandler<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions>
{
    public TestAuthenticationHandler(
        Microsoft.Extensions.Options.IOptionsMonitor<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions> options,
        Microsoft.Extensions.Logging.ILoggerFactory logger,
        System.Text.Encodings.Web.UrlEncoder encoder)
        : base(options, logger, encoder)
    {
    }

    protected override Task<Microsoft.AspNetCore.Authentication.AuthenticateResult> HandleAuthenticateAsync()
    {
        // Crear claims de prueba basados en headers X-User-* si existen
        var userId = Context.Request.Headers["X-User-Id"].FirstOrDefault() ?? "1";
        var email = Context.Request.Headers["X-User-Email"].FirstOrDefault() ?? "test@example.com";
        var role = Context.Request.Headers["X-User-Role"].FirstOrDefault() ?? "Admin";
        var userName = Context.Request.Headers["X-User-Name"].FirstOrDefault() ?? "TestUser";

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, userId),
            new Claim(ClaimTypes.Email, email),
            new Claim(ClaimTypes.Role, role),
            new Claim(ClaimTypes.Name, userName)
        };

        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new Microsoft.AspNetCore.Authentication.AuthenticationTicket(principal, "Test");

        return Task.FromResult(Microsoft.AspNetCore.Authentication.AuthenticateResult.Success(ticket));
    }
}
