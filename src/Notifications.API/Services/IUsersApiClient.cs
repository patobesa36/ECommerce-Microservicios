namespace Notifications.API.Services;

public interface IUsersApiClient
{
    Task<bool> UserExistsAsync(Guid userId, CancellationToken cancellationToken = default);
}