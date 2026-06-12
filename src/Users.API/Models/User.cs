namespace Users.API.Models;

/// <summary>
/// Entidad de dominio que representa a un usuario del sistema.
/// </summary>
public class User
{
    /// <summary>
    /// Identificador único del usuario.
    /// </summary>
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    public required string Nombre { get; set; }

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    public required string Apellido { get; set; }

    /// <summary>
    /// Correo electrónico del usuario. Debe ser único.
    /// </summary>
    public required string Email { get; set; }

    /// <summary>
    /// Hash de la contraseña. Nunca debe exponerse en responses.
    /// </summary>
    public required string PasswordHash { get; set; }

    /// <summary>
    /// Fecha y hora UTC del registro del usuario.
    /// </summary>
    public DateTime FechaRegistro { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Indica si el usuario se encuentra activo.
    /// </summary>
    public bool Activo { get; set; } = true;

    /// <summary>
    /// Cantidad de intentos fallidos consecutivos de login.
    /// </summary>
    public int IntentosFallidos { get; set; } = 0;
}