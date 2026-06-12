using System.Text.Json;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Users.API.HealthChecks;

public static class HealthCheckJsonResponseWriter
{
    public static async Task WriteResponseAsync(HttpContext context, HealthReport report)
    {
        context.Response.ContentType = "application/json";

        var payload = new
        {
            status = report.Status.ToString(),
            totalDuration = report.TotalDuration.TotalMilliseconds,
            entries = report.Entries.Select(entry => new
            {
                name = entry.Key,
                status = entry.Value.Status.ToString(),
                description = entry.Value.Description,
                duration = entry.Value.Duration.TotalMilliseconds,
                details = entry.Value.Data.ToDictionary(
                    item => item.Key,
                    item => item.Value)
            })
        };

        var json = JsonSerializer.Serialize(payload, new JsonSerializerOptions
        {
            WriteIndented = true
        });

        await context.Response.WriteAsync(json);
    }
}
