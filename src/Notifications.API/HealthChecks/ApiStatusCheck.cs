using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Notifications.API.HealthChecks;

public class ApiStatusCheck(AppRuntimeInfo runtimeInfo) : IHealthCheck
{
    public Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        var uptime = DateTime.UtcNow - runtimeInfo.StartedAtUtc;

        var data = new Dictionary<string, object>
        {
            ["startedAtUtc"] = runtimeInfo.StartedAtUtc,
            ["uptimeSeconds"] = Math.Round(uptime.TotalSeconds, 2),
            ["dotnetVersion"] = Environment.Version.ToString()
        };

        return Task.FromResult(
            HealthCheckResult.Healthy("API operativa.", data));
    }
}