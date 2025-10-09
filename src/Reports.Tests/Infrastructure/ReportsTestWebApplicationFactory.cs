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

namespace Reports.Tests.Infrastructure
{
    public class ReportsTestWebApplicationFactory<TStartup> : WebApplicationFactory<TStartup> where TStartup : class
    {
        protected override void ConfigureWebHost(IWebHostBuilder builder)
        {
            builder.UseEnvironment("TestEnvironment");

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
                // Agregar autenticación de prueba que sobrescribe JWT
                services.AddAuthentication(options =>
                {
                    options.DefaultAuthenticateScheme = "Test";
                    options.DefaultChallengeScheme = "Test";
                })
                .AddScheme<Microsoft.AspNetCore.Authentication.AuthenticationSchemeOptions, TestAuthenticationHandler>("Test", options => { });

                // Buscar y remover TODOS los DbContext relacionados
                var descriptors = services.Where(d => d.ServiceType.Name.Contains("DbContext") ||
                                                     d.ServiceType == typeof(DbContextOptions<ReportsDbContext>) ||
                                                     d.ServiceType == typeof(ReportsDbContext)).ToList();

                foreach (var descriptor in descriptors)
                {
                    services.Remove(descriptor);
                }

                // Configurar explícitamente el DbContext para usar InMemory
                services.AddDbContext<ReportsDbContext>(options =>
                    options.UseInMemoryDatabase("TestDatabase"));

                // Crear la base de datos en memoria
                var serviceProvider = services.BuildServiceProvider();
                using var scope = serviceProvider.CreateScope();
                var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
                context.Database.EnsureCreated();
            });
        }

        public ReportsDbContext GetDbContext()
        {
            var scope = Services.CreateScope();
            return scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
        }

        public void ResetDatabase()
        {
            using var scope = Services.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ReportsDbContext>();
            context.Database.EnsureDeleted();
            context.Database.EnsureCreated();
        }

        /// <summary>
        /// Crea un HttpClient con headers de autenticación X-User-* y token JWT Bearer
        /// para simular peticiones autenticadas por el Gateway.
        /// </summary>
        /// <param name="userId">ID del usuario autenticado (default: 1)</param>
        /// <param name="email">Email del usuario autenticado (default: test@example.com)</param>
        /// <param name="role">Rol del usuario autenticado (default: Admin)</param>
        /// <param name="userName">Nombre del usuario autenticado (default: TestUser)</param>
        /// <returns>HttpClient configurado con headers de autenticación y token JWT</returns>
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
                expires: DateTime.UtcNow.AddHours(1),
                signingCredentials: credentials
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }
    }
}
