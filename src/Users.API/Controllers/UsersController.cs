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
    /// <param name="request">Datos necesarios para registrar un usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la request.</param>
    /// <response code="201">Usuario creado exitosamente.</response>
    /// <response code="400">Los datos del usuario son inválidos (USR-002).</response>
    /// <response code="409">El email ya está registrado (USR-001).</response>
    /// <response code="500">Error interno al procesar el usuario (USR-006).</response>
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
    ///
    /// Ejemplo de response 200:
    /// <code>
    /// {
    ///   "id": "a1b2c3d4-0000-0000-0000-111122223333",
    ///   "nombre": "María",
    ///   "apellido": "González",
    ///   "email": "maria@email.com"
    /// }
    /// </code>
    ///
    /// Ejemplo de response 401 (USR-003):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7235#section-3.1",
    ///   "title": "Unauthorized",
    ///   "status": 401,
    ///   "detail": "Las credenciales no son válidas.",
    ///   "instance": "/api/users/login",
    ///   "errorCode": "USR-003",
    ///   "errorMessage": "Credenciales incorrectas."
    /// }
    /// </code>
    ///
    /// Ejemplo de response 403 (USR-004):
    /// <code>
    /// {
    ///   "type": "https://tools.ietf.org/html/rfc7231#section-6.5.3",
    ///   "title": "Forbidden",
    ///   "status": 403,
    ///   "detail": "El acceso está prohibido.",
    ///   "instance": "/api/users/login",
    ///   "errorCode": "USR-004",
    ///   "errorMessage": "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte."
    /// }
    /// </code>
    /// </remarks>
    /// <param name="request">Credenciales del usuario.</param>
    /// <param name="cancellationToken">Token de cancelación de la request.</param>
    /// <response code="200">Login exitoso.</response>
    /// <response code="400">Los datos del usuario son inválidos (USR-002).</response>
    /// <response code="401">Credenciales incorrectas (USR-003).</response>
    /// <response code="403">Usuario bloqueado (USR-004 o USR-005).</response>
    /// <response code="500">Error interno al procesar el usuario (USR-006).</response>
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
}