using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.DependencyInjection;
using Orders.API.ExceptionHandlers;
using Orders.API.Middlewares;
using Orders.API.Services;
using Serilog;
using Serilog.Events;
using System.IO;
using System;

var builder = WebApplication.CreateBuilder(args);

// 1. Configuración de Serilog
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

// 2. Controladores, Swagger e Inyección de Dependencias
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// ---------------------------------------------------------
// INYECCIÓN DE SERVICIOS Y HTTP CLIENTS 
// ---------------------------------------------------------
// Registra el servicio de órdenes y le inyecta el HttpClient para Productos
builder.Services.AddHttpClient<Orders.API.Services.IOrderServices, Orders.API.Services.OrderService>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7001/"); // Puerto de Products.API
});

// Registra el cliente para comunicarse con Users.API
builder.Services.AddHttpClient<Orders.API.Services.IUsersApiClient, Orders.API.Services.UsersApiClient>(client =>
{
    client.BaseAddress = new Uri("https://localhost:7003/"); // Puerto de Users.API
});


// ---------------------------------------------------------
// PERSISTENCIA: REPOSITORIO E INICIALIZADOR SQLITE 
// ---------------------------------------------------------
builder.Services.AddScoped<Orders.API.Data.IOrderRepository, Orders.API.Data.OrderRepository>();
builder.Services.AddSingleton<Orders.API.Data.DatabaseInitializer>();

// 3. Manejo Global de Errores
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<UnprocessableEntityExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Health Checks 
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<Orders.API.HealthChecks.SqliteHealthCheck>("OrdersDB", tags: new[] { "ready" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("OrderApi", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

app.UseExceptionHandler();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Orders API V1");
});

app.UseRouting();
app.UseAuthorization();


// Inicializar la BD
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Orders.API.Data.DatabaseInitializer>();
    initializer.Initialize();
}

// Mapeos de Health Checks
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapControllers();

app.Run();
