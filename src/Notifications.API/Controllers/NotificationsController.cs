using Microsoft.AspNetCore.Mvc;
using Notifications.API.DTOs;
using Notifications.API.Services;

namespace Notifications.API.Controllers;

/// <summary>
/// Endpoints relacionados con el registro y consulta de notificaciones de usuarios.
/// </summary>
[ApiController]
[Route("api/notifications")]
public class NotificationsController(INotificationsService notificationsService) : ControllerBase
{
    /// <summary>
    /// Registra y simula el envío de una notificación.
    /// </summary>
    /// <remarks>
    /// Ejemplo de request:
    /// <code>
    /// {
    ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
    ///   "tipo": "Email"
    /// }
    /// </code>
    ///
    /// Ejemplo de response 201:
    /// <code>
    /// {
    ///   "id": "11112222-3333-4444-5555-666677778888",
    ///   "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "mensaje": "Su orden #f1e2d3c4 fue confirmada.",
    ///   "tipo": "Email",
    ///   "estado": "Enviada",
    ///   "fechaEnvio": "2024-03-10T12:01:00Z"
    /// }
    /// </code>
    ///
    /// Ejemplo de response 404 (NTF-001):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "El recurso solicitado no fue encontrado.",
    ///   "instance": "/api/notifications/send",
    ///   "errorCode": "NTF-001",
    ///   "errorMessage": "El usuario destinatario no fue encontrado."
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">Datos necesarios para registrar la notificación.</param>
    /// <param name="cancellationToken">Token de cancelación de la request.</param>
    /// <response code="201">Notificación registrada y enviada exitosamente.</response>
    /// <response code="400">Los datos de la notificación son inválidos (NTF-002).</response>
    /// <response code="404">El usuario destinatario no fue encontrado (NTF-001).</response>
    /// <response code="500">Error interno al procesar la notificación (NTF-004).</response>
    [HttpPost("send")]
    [ProducesResponseType(typeof(NotificationResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Send([FromBody] SendNotificationRequest request, CancellationToken cancellationToken)
    {
        var response = await notificationsService.SendAsync(request, cancellationToken);
        return Created($"/api/notifications/{response.UsuarioId}", response);
    }

    /// <summary>
    /// Lista las notificaciones registradas para un usuario.
    /// </summary>
    /// <remarks>
    /// Ejemplo de response 200:
    /// <code>
    /// [
    ///   {
    ///     "id": "11112222-3333-4444-5555-666677778888",
    ///     "usuarioId": "a1b2c3d4-0000-0000-0000-111122223333",
    ///     "mensaje": "Su orden fue confirmada.",
    ///     "tipo": "Email",
    ///     "estado": "Enviada",
    ///     "fechaEnvio": "2024-03-10T12:01:00Z"
    ///   }
    /// ]
    /// </code>
    ///
    /// Ejemplo de response 404 (NTF-003):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "El recurso solicitado no fue encontrado.",
    ///   "instance": "/api/notifications/a1b2c3d4-0000-0000-0000-111122223333",
    ///   "errorCode": "NTF-003",
    ///   "errorMessage": "No se encontraron notificaciones para el usuario."
    /// }
    /// </code>
    /// </remarks>
    /// <param name="userId">Identificador del usuario cuyas notificaciones se desean consultar.</param>
    /// <param name="cancellationToken">Token de cancelación de la request.</param>
    /// <response code="200">Listado de notificaciones obtenido correctamente.</response>
    /// <response code="404">No se encontraron notificaciones para el usuario (NTF-003).</response>
    /// <response code="500">Error interno al procesar la notificación (NTF-004).</response>
    [HttpGet("{userId:guid}")]
    [ProducesResponseType(typeof(IEnumerable<NotificationResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetByUser(Guid userId, CancellationToken cancellationToken)
    {
        var response = await notificationsService.GetByUserAsync(userId, cancellationToken);
        return Ok(response);
    }
}