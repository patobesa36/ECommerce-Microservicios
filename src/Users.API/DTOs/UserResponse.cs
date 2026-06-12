namespace Users.API.DTOs;

/// <summary>
/// Respuesta exitosa al registrar un usuario.
/// </summary>
public class UserResponse
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; }

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Fecha y hora UTC en la que se registró el usuario.
    /// </summary>
    public DateTime FechaRegistro { get; set; }

    /// <summary>
    /// Indica si el usuario se encuentra activo.
    /// </summary>
    public bool Activo { get; set; }
}