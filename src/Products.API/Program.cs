using Serilog;
using Serilog.Events;
using HealthChecks.UI.Client;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Products.API.ExceptionHandlers;
using Products.API.Middlewares;

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

// 2. Controladores y Swagger
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    var xmlFile = $"{System.Reflection.Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    c.IncludeXmlComments(xmlPath);
});

// 3. Manejo Global de Errores (El orden importa)
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddProblemDetails();
// Inyección de dependencias de tus Servicios
builder.Services.AddScoped<Products.API.Services.IProductService, Products.API.Services.ProductService>();
builder.Services.AddScoped<Products.API.Data.IProductRepository, Products.API.Data.ProductRepository>();
builder.Services.AddSingleton<Products.API.Data.DatabaseInitializer>();


// // 4. Health Checks
// Configuración de Health Checks
builder.Services.AddHealthChecks()
    // El "live" sigue siendo un chequeo básico (la API prendió)
    .AddCheck("self", () => Microsoft.Extensions.Diagnostics.HealthChecks.HealthCheckResult.Healthy(), tags: new[] { "live" })
    // El "ready" ahora comprueba que la base de datos responda
    .AddCheck<Products.API.HealthChecks.SqliteHealthCheck>("ProductsDB", tags: new[] { "ready" });

builder.Services.AddHealthChecksUI(setup =>
{
    setup.SetEvaluationTimeInSeconds(600);
    setup.AddHealthCheckEndpoint("ProductsApi", "/health"); // Nombre ajustado para Products
}).AddInMemoryStorage();

var app = builder.Build();

// Ejecutamos la creación de la base de datos al arrancar
using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<Products.API.Data.DatabaseInitializer>();
    initializer.Initialize();
}

// 5. Configuración del Pipeline
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

app.UseExceptionHandler();


app.MapHealthChecks("/health", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Endpoint exclusivo de Liveness (¿Estoy vivo?)
app.MapHealthChecks("/health/live", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Endpoint exclusivo de Readiness (¿Estoy listo para recibir tráfico?)
app.MapHealthChecks("/health/ready", new Microsoft.AspNetCore.Diagnostics.HealthChecks.HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthChecks.UI.Client.UIResponseWriter.WriteHealthCheckUIResponse
});

// Interfaz gráfica de monitoreo
app.MapHealthChecksUI(setup => setup.UIPath = "/health-ui");

app.MapControllers();
app.Run();


