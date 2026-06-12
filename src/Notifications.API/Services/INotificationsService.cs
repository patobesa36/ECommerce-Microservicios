using Notifications.API.DTOs;

namespace Notifications.API.Services;

public interface INotificationsService
{
    Task<NotificationResponse> SendAsync(SendNotificationRequest request, CancellationToken cancellationToken = default);
    Task<IReadOnlyCollection<NotificationResponse>> GetByUserAsync(Guid userId, CancellationToken cancellationToken = default);
}
