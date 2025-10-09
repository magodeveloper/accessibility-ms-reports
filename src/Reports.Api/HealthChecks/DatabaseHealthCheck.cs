using Reports.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Reports.Api.HealthChecks;

/// <summary>
/// Health check personalizado para verificar la conectividad y estado de la base de datos MySQL
/// </summary>
public class DatabaseHealthCheck : IHealthCheck
{
    private readonly ReportsDbContext _dbContext;
    private readonly ILogger<DatabaseHealthCheck> _logger;

    public DatabaseHealthCheck(ReportsDbContext dbContext, ILogger<DatabaseHealthCheck> logger)
    {
        _dbContext = dbContext;
        _logger = logger;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            // Verificar que la base de datos responda con una query simple
            // Para InMemory database, CanConnectAsync puede fallar con métodos relacionales
            bool canConnect;
            try
            {
                canConnect = await _dbContext.Database.CanConnectAsync(cancellationToken);
            }
            catch (InvalidOperationException)
            {
                // Si es InMemory database, simplemente verificamos que podemos hacer una query
                canConnect = true;
            }

            if (!canConnect)
            {
                _logger.LogWarning("Cannot connect to Reports database");
                return HealthCheckResult.Unhealthy(
                    "Cannot connect to the Reports database",
                    data: new Dictionary<string, object>
                    {
                        { "database", "unknown" }
                    });
            }

            // Contar reportes como verificación adicional
            var reportCount = await _dbContext.Reports.CountAsync(cancellationToken);

            _logger.LogDebug("Database health check passed. Report count: {ReportCount}", reportCount);

            return HealthCheckResult.Healthy(
                "Database is accessible and responsive",
                data: new Dictionary<string, object>
                {
                    { "reportCount", reportCount },
                    { "timestamp", DateTime.UtcNow }
                });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Database health check failed");

            return HealthCheckResult.Unhealthy(
                "Database check failed",
                exception: ex,
                data: new Dictionary<string, object>
                {
                    { "error", ex.Message },
                    { "timestamp", DateTime.UtcNow }
                });
        }
    }
}
