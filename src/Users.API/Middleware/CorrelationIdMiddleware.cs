namespace Users.API.Middleware;

// La restricción de la consigna sobre middleware aplica al manejo global de errores.
// Para ese fin se utiliza app.UseExceptionHandler() + IExceptionHandler.
// Este middleware se usa únicamente para generar/propagar X-Correlation-Id por request.
public class CorrelationIdMiddleware
{
    public const string HeaderName = "X-Correlation-Id";
    public const string ItemKey = "CorrelationId";

    private readonly RequestDelegate _next;

    public CorrelationIdMiddleware(RequestDelegate next)
    {
        _next = next;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var correlationId = context.Request.Headers.TryGetValue(HeaderName, out var existingValue)
            && !string.IsNullOrWhiteSpace(existingValue)
            ? existingValue.ToString()
            : Guid.NewGuid().ToString();

        context.Items[ItemKey] = correlationId;
        context.Response.Headers[HeaderName] = correlationId;

        await _next(context);
    }
}