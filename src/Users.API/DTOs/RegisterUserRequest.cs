using System.ComponentModel.DataAnnotations;

namespace Users.API.DTOs;

/// <summary>
/// Datos requeridos para registrar un nuevo usuario.
/// </summary>
public class RegisterUserRequest
{
    /// <summary>
    /// Nombre del usuario.
    /// </summary>
    [Required]
    public string Nombre { get; set; } = string.Empty;

    /// <summary>
    /// Apellido del usuario.
    /// </summary>
    [Required]
    public string Apellido { get; set; } = string.Empty;

    /// <summary>
    /// Correo electrónico único del usuario.
    /// </summary>
    [Required]
    [EmailAddress]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// Contraseña en texto plano enviada al registrar.
    /// </summary>
    [Required]
    public string Password { get; set; } = string.Empty;
}