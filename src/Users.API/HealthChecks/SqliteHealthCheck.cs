using Dapper;
using Microsoft.Data.Sqlite;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

public class SqliteHealthCheck(IConfiguration configuration) : IHealthCheck
{
    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            var connectionString =
                configuration.GetConnectionString("DefaultConnection")
                ?? "Data Source=app.db";

            using var connection = new SqliteConnection(connectionString);
            await connection.OpenAsync(cancellationToken);

            await connection.ExecuteScalarAsync<int>("SELECT 1");

            return HealthCheckResult.Healthy("SELECT 1 ejecutado OK.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy(
                description: "No se pudo conectar a SQLite.",
                exception: ex);
        }
    }
}