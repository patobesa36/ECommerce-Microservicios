using Users.API.Data;
using Users.API.DTOs;
using Users.API.Exceptions;
using Users.API.Models;
using Users.API.Services;

namespace Users.API.Services;

public class UsersService(IUserRepository userRepository) : IUsersService
{
    public async Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Nombre) ||
            string.IsNullOrWhiteSpace(request.Apellido) ||
            string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");
        }

        var existingUser = await userRepository.GetByEmailAsync(request.Email);

        if (existingUser is not null)
        {
            throw new BusinessRuleException(
                "USR-001",
                $"El email '{request.Email}' ya está registrado.",
                StatusCodes.Status409Conflict);
        }

        var user = new User
        {
            Id = Guid.NewGuid(),
            Nombre = request.Nombre,
            Apellido = request.Apellido,
            Email = request.Email,
            PasswordHash = request.Password,
            FechaRegistro = DateTime.UtcNow,
            Activo = true,
            IntentosFallidos = 0
        };

        await userRepository.CreateAsync(user);

        return new UserResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email,
            FechaRegistro = user.FechaRegistro,
            Activo = user.Activo
        };
    }

    public async Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
    {
        if (string.IsNullOrWhiteSpace(request.Email) ||
            string.IsNullOrWhiteSpace(request.Password))
        {
            throw new ValidationException("USR-002", "Los datos del usuario son inválidos.");
        }

        var user = await userRepository.GetByEmailAsync(request.Email);

        if (user is null)
        {
            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        // Convención temporal:
        // si Activo = false y IntentosFallidos < 3, interpretamos bloqueo manual/fraude.
        if (!user.Activo && user.IntentosFallidos < 3)
        {
            throw new BusinessRuleException(
                "USR-005",
                "Su cuenta fue suspendida por razones de seguridad. Contacte a soporte.",
                StatusCodes.Status403Forbidden);
        }

        if (!user.Activo && user.IntentosFallidos >= 3)
        {
            throw new BusinessRuleException(
                "USR-004",
                "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.",
                StatusCodes.Status403Forbidden);
        }

        if (user.PasswordHash != request.Password)
        {
            user.IntentosFallidos++;

            if (user.IntentosFallidos >= 3)
            {
                user.Activo = false;

                await userRepository.UpdateLoginStateAsync(user.Id, user.Activo, user.IntentosFallidos);

                throw new BusinessRuleException(
                    "USR-004",
                    "Su cuenta fue bloqueada por superar el máximo de intentos fallidos. Contacte a soporte.",
                    StatusCodes.Status403Forbidden);
            }

            await userRepository.UpdateLoginStateAsync(user.Id, user.Activo, user.IntentosFallidos);

            throw new UnauthorizedException("USR-003", "Credenciales incorrectas.");
        }

        user.IntentosFallidos = 0;
        user.Activo = true;

        await userRepository.UpdateLoginStateAsync(user.Id, user.Activo, user.IntentosFallidos);

        return new LoginResponse
        {
            Id = user.Id,
            Nombre = user.Nombre,
            Apellido = user.Apellido,
            Email = user.Email
        };
    }
}