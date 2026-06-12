namespace Notifications.API.DTOs;

/// <summary>
/// Respuesta de una notificación registrada o listada.
/// </summary>
public class NotificationResponse
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Identificador del usuario destinatario.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Mensaje enviado.
    /// </summary>
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación.
    /// </summary>
    public string Tipo { get; set; } = string.Empty;

    /// <summary>
    /// Estado de la notificación.
    /// </summary>
    public string Estado { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora UTC del envío o registro.
    /// </summary>
    public DateTime FechaEnvio { get; set; }
}