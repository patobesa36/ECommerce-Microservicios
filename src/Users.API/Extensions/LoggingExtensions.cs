using Serilog;
using Serilog.Events;
using Users.API.Middleware;

namespace ECommerce.Users.API.Extensions;

// Consigna TP: el archivo de logs debe ser estructurado e incluir contexto del request.
// Guía MiniApi: se excluyen /health y /swagger del archivo para reducir ruido.
// Decisión: registrar requests útiles en archivo, excluyendo endpoints de monitoreo.
public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", "Users.API")
            .WriteTo.Logger(lc => lc
                .Filter.ByIncludingOnly(logEvent => logEvent.Level >= LogEventLevel.Error)
                .WriteTo.Console(
                    outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Servicio} {CorrelationId} {RequestMethod} {RequestPath} {Message:lj}{NewLine}{Exception}"))
            .WriteTo.Logger(lc => lc
                .Filter.ByExcluding(logEvent =>
                    logEvent.Properties.TryGetValue("RequestPath", out var pathValue) &&
                    (
                        pathValue.ToString().Contains("/health") ||
                        pathValue.ToString().Contains("/swagger")
                    ))
                .WriteTo.File(
                    path: "logs/users-api.log",
                    rollingInterval: RollingInterval.Day,
                    outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {Level:u3} | {Servicio} | {CorrelationId} | {RequestMethod} | {RequestPath} | {StatusCode} | {Elapsed:0.0000}{NewLine}{Exception}"))
            .CreateLogger();

        builder.Host.UseSerilog();
    }

    public static void UseAppRequestLogging(this WebApplication app)
    {
        app.UseSerilogRequestLogging(options =>
        {
            options.EnrichDiagnosticContext = (diagnosticContext, httpContext) =>
            {
                diagnosticContext.Set("Servicio", "Users.API");
                diagnosticContext.Set("Endpoint", httpContext.GetEndpoint()?.DisplayName ?? "N/A");

                var correlationId =
                    httpContext.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value) && value is string id
                        ? id
                        : httpContext.Request.Headers[CorrelationIdMiddleware.HeaderName].ToString();

                diagnosticContext.Set("CorrelationId", correlationId);
            };

            options.GetLevel = (httpContext, elapsed, ex) =>
            {
                if (ex != null)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 500)
                    return LogEventLevel.Error;

                if (httpContext.Response.StatusCode >= 400)
                    return LogEventLevel.Warning;

                if (httpContext.Request.Path.StartsWithSegments("/health"))
                    return LogEventLevel.Verbose;

                return LogEventLevel.Information;
            };
        });
    }
}