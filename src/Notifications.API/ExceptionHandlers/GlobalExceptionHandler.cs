using Microsoft.AspNetCore.Diagnostics;
using Notifications.API.Extensions;

namespace Notifications.API.ExceptionHandlers;

public class GlobalExceptionHandler : IExceptionHandler
{
    public async ValueTask<bool> TryHandleAsync(
        HttpContext context,
        Exception exception,
        CancellationToken cancellationToken)
    {
        context.Response.StatusCode = StatusCodes.Status500InternalServerError;

        await context.Response.WriteAsJsonAsync(new
        {
            type = "https://tools.ietf.org/html/rfc7231#section-6.6.1",
            title = "Internal Server Error",
            status = 500,
            detail = "Se produjo un error inesperado.",
            instance = context.Request.Path.Value,
            errorCode = "NTF-004",
            errorMessage = "Error interno al procesar la notificación.",
            correlationId = context.GetCorrelationId()
        }, cancellationToken);

        return true;
    }
}