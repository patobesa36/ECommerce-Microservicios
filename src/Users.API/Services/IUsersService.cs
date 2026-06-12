using Users.API.DTOs;

namespace Users.API.Services;

public interface IUsersService
{
    Task<UserResponse> RegisterAsync(RegisterUserRequest request, CancellationToken cancellationToken = default);
    Task<LoginResponse> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);

    // Endpoint técnico de soporte para integración entre microservicios.
    // Se devuelve un DTO seguro sin PasswordHash, respetando la consigna.
    Task<UserResponse> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
}