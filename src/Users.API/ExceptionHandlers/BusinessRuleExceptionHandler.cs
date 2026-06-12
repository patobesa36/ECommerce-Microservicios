using Microsoft.AspNetCore.Diagnostics;
using Users.API.Exceptions;
using Users.API.Extensions;

namespace Users.API.ExceptionHandlers;

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
        var correlationId = context.GetCorrelationId();

        object payload = ex.StatusCode switch
        {
            StatusCodes.Status403Forbidden => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.3",
                title = "Forbidden",
                status = 403,
                detail = "El acceso está prohibido.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            },

            StatusCodes.Status409Conflict => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = "Conflict",
                status = 409,
                detail = "Ya existe un recurso con esos datos.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            },

            _ => new
            {
                type = "https://tools.ietf.org/html/rfc7231#section-6.5.9",
                title = "Conflict",
                status = ex.StatusCode,
                detail = "No se pudo procesar la regla de negocio.",
                instance = context.Request.Path.Value,
                errorCode = ex.ErrorCode,
                errorMessage = ex.Message,
                correlationId
            }
        };

        await context.Response.WriteAsJsonAsync(payload, cancellationToken);

        return true;
    }
}