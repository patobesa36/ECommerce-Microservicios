using Notifications.API.Middleware;

namespace Notifications.API.Extensions;

public static class CorrelationIdExtensions
{
    public static string GetCorrelationId(this HttpContext context)
    {
        if (context.Items.TryGetValue(CorrelationIdMiddleware.ItemKey, out var value) &&
            value is string correlationId &&
            !string.IsNullOrWhiteSpace(correlationId))
        {
            return correlationId;
        }

        if (context.Request.Headers.TryGetValue(CorrelationIdMiddleware.HeaderName, out var headerValue) &&
            !string.IsNullOrWhiteSpace(headerValue))
        {
            return headerValue.ToString();
        }

        return string.Empty;
    }
}