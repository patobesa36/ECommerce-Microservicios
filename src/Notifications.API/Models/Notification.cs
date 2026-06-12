namespace Notifications.API.Models;

/// <summary>
/// Entidad de dominio que representa una notificación enviada a un usuario.
/// </summary>
public class Notification
{
    /// <summary>
    /// Identificador único de la notificación.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Identificador del usuario destinatario.
    /// </summary>
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Mensaje de la notificación.
    /// </summary>
    public required string Mensaje { get; set; }

    /// <summary>
    /// Tipo de notificación. Valores esperados: Email, Push o SMS.
    /// </summary>
    public required string Tipo { get; set; }

    /// <summary>
    /// Estado de la notificación. Valores de negocio previstos: Pendiente, Enviada o Fallida.
    /// </summary>
    public string Estado { get; set; } = "Pendiente";

    /// <summary>
    /// Fecha y hora UTC en la que se registró o envió la notificación.
    /// </summary>
    public DateTime FechaEnvio { get; set; } = DateTime.UtcNow;
}