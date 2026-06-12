using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

/// <summary>
/// Credenciales necesarias para autenticar un usuario.
/// </summary>
public class LoginRequest
{
    /// <summary>
    /// Correo electrónico del usuario.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña del usuario.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}