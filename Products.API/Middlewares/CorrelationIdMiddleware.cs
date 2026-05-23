namespace Products.API.Middlewares;

public class CorrelationIdMiddleware(RequestDelegate next)
{
  public async Task InvokeAsync(HttpContext context)
  {
    var correlationId = Guid.NewGuid().ToString();
    context.Request.Headers.TryAdd("X-Correlation-Id", correlationId);

    context.Response.OnStarting(() =>
    {
      context.Response.Headers.TryAdd("X-Correlation-Id", correlationId);
      return Task.CompletedTask;
    });

    await next(context);
  }
}