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

// ---------------------------------------------------------
// INYECCIÓN DE SERVICIOS Y HTTP CLIENT
// ---------------------------------------------------------
// Esto registra el servicio Y le inyecta automáticamente el HttpClient para hablar con Products
builder.Services.AddHttpClient<Cart.API.Services.ICartService, Cart.API.Services.CartService>();

// ---------------------------------------------------------
// PERSISTENCIA: REPOSITORIO E INICIALIZADOR SQLITE
// ---------------------------------------------------------
builder.Services.AddScoped<Cart.API.Data.ICartRepository, Cart.API.Data.CartRepository>();
builder.Services.AddSingleton<Cart.API.Data.DatabaseInitializer>();


// 3. Manejo Global de Errores (Orden estricto de específico a genérico) [cite: 1145, 1180]
builder.Services.AddExceptionHandler<BadRequestExceptionHandler>();
builder.Services.AddExceptionHandler<UnprocessableEntityExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddProblemDetails();

// 4. Health Checks 
// Configuración de Health Checks para el Carrito
builder.Services.AddHealthChecks()
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" })
    .AddCheck<Cart.API.HealthChecks.SqliteHealthCheck>("CartDB", tags: new[] { "ready" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    
    setup.AddHealthCheckEndpoint("CartApi", "/health");
}).AddInMemoryStorage();

var app = builder.Build();

app.UseExceptionHandler();


app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Cart API V1");
});

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Cart.API.Data.DatabaseInitializer>();
    initializer.Initialize();
}

// 4. Agregar enrutamiento básico para los controladores
app.UseRouting();
app.UseAuthorization();

// Mapeo general
app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Mapeo exclusivo de Liveness (¿Estoy vivo?)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Mapeo exclusivo de Readiness (¿Estoy listo para recibir peticiones?)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Mapeo de la interfaz gráfica
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapControllers();

app.Run();