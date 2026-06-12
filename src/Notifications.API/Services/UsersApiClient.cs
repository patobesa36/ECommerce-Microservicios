namespace Notifications.API.Services;

public class UsersApiClient(HttpClient httpClient, IConfiguration configuration) : IUsersApiClient
{
    public async Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default)
    {
        // Decisión de diseño:
        // Notifications.API valida la existencia del usuario destinatario consultando
        // un endpoint interno de soporte en Users.API: GET /api/users/{id}.
        //
        // Esta decisión evita acoplar microservicios por memoria o base de datos compartida
        // y mantiene la integración vía HTTP, coherente con la consigna.
        //
        // La ruta queda configurable por si el equipo necesita ajustar el contrato interno
        // sin reescribir este cliente.
        var template = configuration["UsersApi:UserLookupPath"] ?? "/api/users/{userId}";
        var requestPath = template.Replace("{userId}", userId.ToString());

        using var response = await httpClient.GetAsync(requestPath, cancellationToken);

        if (response.IsSuccessStatusCode)
            return true;

        if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
            return false;

        response.EnsureSuccessStatusCode();
        return false;
    }
}