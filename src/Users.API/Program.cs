using ECommerce.Users.API.Extensions;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using System.Reflection;
using Users.API.Data;
using Users.API.ExceptionHandlers;
using Users.API.HealthChecks;
using Users.API.Middleware;
using Users.API.Services;

var builder = WebApplication.CreateBuilder(args);

// Logging
builder.AddAppLogging();

// Controllers
builder.Services.AddControllers();

// Swagger / OpenAPI + XML comments
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);

    if (File.Exists(xmlPath))
    {
        options.IncludeXmlComments(xmlPath, includeControllerXmlComments: true);
    }
});

// Problem Details + Exception Handlers
builder.Services.AddProblemDetails();
builder.Services.AddExceptionHandler<ValidationExceptionHandler>();
builder.Services.AddExceptionHandler<UnauthorizedExceptionHandler>();
builder.Services.AddExceptionHandler<BusinessRuleExceptionHandler>();
builder.Services.AddExceptionHandler<NotFoundExceptionHandler>();
builder.Services.AddExceptionHandler<GlobalExceptionHandler>();

// Health Checks
builder.Services.AddSingleton<AppRuntimeInfo>();
builder.Services.AddHealthChecks()
    .AddCheck<SqliteHealthCheck>(
        "sqlite-db",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "database" })
    .AddCheck<ApiStatusCheck>(
        "api-status",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "ready", "live", "api" });

// Data + Services
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IUsersService, UsersService>();
builder.Services.AddScoped<DatabaseInitializer>();

var app = builder.Build();

// Inicializar base de datos
using (var scope = app.Services.CreateScope())
{
    scope.ServiceProvider
        .GetRequiredService<DatabaseInitializer>()
        .Initialize();
}

// Correlation ID (antes de error handling y logging)
app.UseMiddleware<CorrelationIdMiddleware>();

// Global exception handling
app.UseExceptionHandler();

// Logging de requests
app.UseAppRequestLogging();

// Swagger
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Controllers
app.MapControllers();

// Health endpoints
app.MapHealthChecks("/health", new HealthCheckOptions
{
    ResponseWriter = HealthCheckJsonResponseWriter.WriteResponseAsync
});

app.MapHealthChecks("/health/ready", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("ready"),
    ResponseWriter = HealthCheckJsonResponseWriter.WriteResponseAsync
});

app.MapHealthChecks("/health/live", new HealthCheckOptions
{
    Predicate = check => check.Tags.Contains("live"),
    ResponseWriter = HealthCheckJsonResponseWriter.WriteResponseAsync
});

app.Run();