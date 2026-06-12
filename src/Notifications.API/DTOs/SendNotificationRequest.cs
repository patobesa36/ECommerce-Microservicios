using System.ComponentModel.DataAnnotations;

namespace Notifications.API.DTOs;

/// <summary>
/// Datos requeridos para registrar y simular el envío de una notificación.
/// </summary>
public class SendNotificationRequest
{
    /// <summary>
    /// Identificador del usuario destinatario.
    /// </summary>
    [Required]
    public Guid UsuarioId { get; set; }

    /// <summary>
    /// Mensaje de la notificación. Máximo 500 caracteres.
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string Mensaje { get; set; } = string.Empty;

    /// <summary>
    /// Tipo de notificación. Valores esperados: Email, Push o SMS.
    /// </summary>
    [Required]
    public string Tipo { get; set; } = string.Empty;
}