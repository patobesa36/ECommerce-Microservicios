using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Exceptions;
using Notifications.API.Extensions;

namespace Notifications.API.ExceptionHandlers;

public class BusinessRuleExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        if (exception is not BusinessRuleException ex)
            return false;

        context.Response.StatusCode = ex.StatusCode;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
            title = "Conflict",
            status = ex.StatusCode,
            detail = "No se pudo procesar la regla de negocio.",
            instance = context.Request.Path.Value,
            errorCode = ex.ErrorCode,
            errorMessage = ex.Message,
            correlationId = context.GetCorrelationId()
        }, cancellationToken);

        return true;
    }
}