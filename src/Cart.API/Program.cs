using Cart.API.ExceptionHandlers;
using Cart.API.Middlewares;
using Cart.API.Services;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Cors.Infrastructure;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Serilog.Events;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Serilog [cite: 1150, 1151]
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.AspNetCore.Hosting.Diagnostics", LogEventLevel.Information)
    .Enrich.FromLogContext()
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le => le.Level >= LogEventLevel.Error)
        .WriteTo.Console(outputTemplate: "[{Timestamp:HH:mm:ss} {Level:u3}] {Message:lj}{NewLine}{Exception}"))
    .WriteTo.Logger(lc => lc
        .Filter.ByIncludingOnly(le =>
        {
            var isHttp = le.Properties.TryGetValue("RequestPath", out var pathValue);
            if (isHttp && pathValue?.ToString().Contains("/health") == true) return false;
            if (isHttp && pathValue?.ToString().Contains("/swagger") == true) return false;
            return true;
        })
        .WriteTo.File("logs/audit.log", rollingInterval: RollingInterval.Day,
            outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} | {RequestMethod} | {RequestPath} | {StatusCode}{NewLine}"))
    .CreateLogger();

builder.Host.UseSerilog();

// 2. Controladores, Swagger e Inyección de Dependencias [cite: 1141]
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// Inyección de servicios para Cart 
builder.Services.AddHttpClient();
builder.Services.AddScoped<ICartService, CartService>();

// 3. Manejo Global de Errores (Orden estricto de específico a genérico) [cite: 1145, 1180]
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<UnprocessableEntityExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Health Checks 
builder.Services.AddHealthChecks();
builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("CartApi", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

// 5. Configuración del Pipeline [cite: 1155]
app.UseMiddleware<CorrelationIdMiddleware>();

app.UseSerilogRequestLogging(options =>
{
    options.GetLevel = (httpContext, elapsed, ex) =>
        ex != null ? LogEventLevel.Error :
        httpContext.Request.Path.StartsWithSegments("/health") ? LogEventLevel.Verbose :
        LogEventLevel.Information;
});

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseExceptionHandler(); // [cite: 1145]

app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = UIResponseWriter.WriteHealthCheckUIResponse
});
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapControllers();

app.Run();