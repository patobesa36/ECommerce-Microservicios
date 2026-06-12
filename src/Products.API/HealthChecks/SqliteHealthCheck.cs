namespace Products.API.HealthChecks
{
    using Microsoft.Extensions.Diagnostics.HealthChecks;
    using Microsoft.Data.Sqlite;
    using Dapper;
    using System.Threading;
    using System.Threading.Tasks;
    using Microsoft.Extensions.Configuration;
    using System;

    public class SqliteHealthCheck : IHealthCheck
    {
        private readonly string _connectionString;

        public SqliteHealthCheck(IConfiguration configuration)
        {
            _connectionString = configuration.GetConnectionString("DefaultConnection")!;
        }

        public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
        {
            try
            {
                using var connection = new SqliteConnection(_connectionString);
                // Hacemos una consulta súper liviana para ver si la BD está viva
                await connection.QueryFirstOrDefaultAsync<int>("SELECT 1");

                return HealthCheckResult.Healthy("Conexión a SQLite exitosa.");
            }
            catch (Exception ex)
            {
                return HealthCheckResult.Unhealthy("Fallo al conectar con la base de datos SQLite.", ex);
            }
        }
    }
}
