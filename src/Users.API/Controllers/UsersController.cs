using Microsoft.AspNetCore.Mvc;
using Users.API.DTOs;
using Users.API.Services;

namespace Users.API.Controllers;

/// <summary>
/// Endpoints relacionados con el registro y autenticación de usuarios.
/// </summary>
[ApiController]
[Route("api/users")]
public class UsersController(IUsersService usersService) : ControllerBase
{
    /// <summary>
    /// Registra un nuevo usuario en el sistema.
    /// </summary>
    /// <remarks>
    /// Ejemplo de request:
    /// <code>
    /// {
    ///   "nombre": "María",
    ///   "apellido": "González",
    ///   "email": "maria@email.com",
    ///   "password": "MiPassword123!"
    /// }
    /// </code>
    ///
    /// Ejemplo de response 201:
    /// <code>
    /// {
    ///   "id": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "nombre": "María",
    ///   "apellido": "González",
    ///   "email": "maria@email.com",
    ///   "fechaRegistro": "2024-03-10T09:00:00Z",
    ///   "activo": true
    /// }
    /// </code>
    ///
    /// Ejemplo de response 409 (USR-001):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.9",
    ///   "title": "Conflict",
    ///   "status": 409,
    ///   "detail": "Ya existe un recurso con esos datos.",
    ///   "instance": "/api/users/register",
    ///   "errorCode": "USR-001",
    ///   "errorMessage": "El email 'maria@email.com' ya está registrado."
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("register")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest request, CancellationToken cancellationToken)
    {
        var response = await usersService.RegisterAsync(request, cancellationToken);
        return Created($"/api/users/{response.Id}", response);
    }

    /// <summary>
    /// Autentica un usuario con email y contraseña.
    /// </summary>
    /// <remarks>
    /// Ejemplo de request:
    /// <code>
    /// {
    ///   "email": "maria@email.com",
    ///   "password": "MiPassword123!"
    /// }
    /// </code>
    /// </remarks>
    [HttpPost("login")]
    [ProducesResponseType(typeof(LoginResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken cancellationToken)
    {
        var response = await usersService.LoginAsync(request, cancellationToken);
        return Ok(response);
    }

    /// <summary>
    /// Obtiene un usuario por identificador.
    /// </summary>
    /// <remarks>
    /// Decisión de diseño:
    /// este endpoint se incorpora como soporte interno para integración entre microservicios,
    /// principalmente para que Notifications.API pueda validar la existencia del usuario
    /// destinatario mediante comunicación HTTP, tal como exige la consigna.
    ///
    /// La respuesta reutiliza un DTO seguro y nunca expone PasswordHash.
    ///
    /// Ejemplo de response 200:
    /// <code>
    /// {
    ///   "id": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "nombre": "María",
    ///   "apellido": "González",
    ///   "email": "maria@email.com",
    ///   "fechaRegistro": "2024-03-10T09:00:00Z",
    ///   "activo": true
    /// }
    /// </code>
    ///
    /// Ejemplo de response 404 (USR-007):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.4",
    ///   "title": "Not Found",
    ///   "status": 404,
    ///   "detail": "El recurso solicitado no fue encontrado.",
    ///   "instance": "/api/users/a1b2c3d4-0000-0000-0000-111122223333",
    ///   "errorCode": "USR-007",
    ///   "errorMessage": "Usuario no encontrado."
    /// }
    /// </code>
    /// </remarks>
    /// <param name="id">Identificador del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la request.</param>
    /// <response code="200">Usuario encontrado.</response>
    /// <response code="404">Usuario no encontrado (USR-007).</response>
    /// <response code="500">Error interno al procesar el usuario (USR-006).</response>
    [HttpGet("{id:guid}")]
    [ProducesResponseType(typeof(UserResponse), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
    {
        var response = await usersService.GetByIdAsync(id, cancellationToken);
        return Ok(response);
    }
}