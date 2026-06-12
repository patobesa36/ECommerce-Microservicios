using Notifications.API.Models;

namespace Notifications.API.Data;

public interface INotificationRepository
{
    Task<Notification> CreateAsync(Notification notification);
    Task<IReadOnlyCollection<Notification>> GetByUserAsync(Guid userId);
}