using Notifications.API.Middleware;
using Serilog;
using Serilog.Events;

namespace Notifications.API.Extensions;

public static class LoggingExtensions
{
    public static void AddAppLogging(this WebApplicationBuilder builder)
    {
        // Decisión de diseño:
        // La consigna principal exige logging con Serilog, consola + archivo, y más contexto por request.
        // La guía MiniApi propone además una configuración centralizada, request logging y exclusión de
        // /health y /swagger para reducir ruido.
        //
        // Con las librerías explícitamente descriptas en los documentos, implementamos una base
        // compatible con ambos enfoques sin incorporar paquetes adicionales no listados.
        Log.Logger = new LoggerConfiguration()
            .MinimumLevel.Information()
            .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
            .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
            .Enrich.FromLogContext()
            .Enrich.WithProperty("Servicio", "Notifications.API")
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
                    path: "logs/notifications-api.log",
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
                diagnosticContext.Set("Servicio", "Notifications.API");
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