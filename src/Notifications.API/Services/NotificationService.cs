using Notifications.API.Data;
using Notifications.API.DTOs;
using Notifications.API.Exceptions;
using Notifications.API.Models;

namespace Notifications.API.Services;

public class NotificationsService(
    INotificationRepository notificationRepository,
    IUsersApiClient usersApiClient) : INotificationsService
{
    private static readonly HashSet<string> AllowedTypes =
    [
        "Email",
        "Push",
        "SMS"
    ];

    public async Task<NotificationResponse> SendAsync(
        SendNotificationRequest request,
        CancellationToken cancellationToken = default)
    {
        if (request.UsuarioId == Guid.Empty ||
            string.IsNullOrWhiteSpace(request.Mensaje) ||
            string.IsNullOrWhiteSpace(request.Tipo))
        {
            throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");
        }

        if (request.Mensaje.Length > 500)
        {
            throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");
        }

        if (!AllowedTypes.Contains(request.Tipo))
        {
            throw new ValidationException("NTF-002", "Los datos de la notificación son inválidos.");
        }

        var userExists = await usersApiClient.UserExistsAsync(request.UsuarioId, cancellationToken);

        if (!userExists)
        {
            throw new NotFoundException("NTF-001", "El usuario destinatario no fue encontrado.");
        }

        var notification = new Notification
        {
            Id = Guid.NewGuid(),
            UsuarioId = request.UsuarioId,
            Mensaje = request.Mensaje,
            Tipo = request.Tipo,
            Estado = "Enviada",
            FechaEnvio = DateTime.UtcNow
        };

        await notificationRepository.CreateAsync(notification);

        return new NotificationResponse
        {
            Id = notification.Id,
            UsuarioId = notification.UsuarioId,
            Mensaje = notification.Mensaje,
            Tipo = notification.Tipo,
            Estado = notification.Estado,
            FechaEnvio = notification.FechaEnvio
        };
    }

    public async Task<IReadOnlyCollection<NotificationResponse>> GetByUserAsync(
        Guid userId,
        CancellationToken cancellationToken = default)
    {
        var notifications = await notificationRepository.GetByUserAsync(userId);

        if (notifications.Count == 0)
        {
            throw new NotFoundException("NTF-003", "No se encontraron notificaciones para el usuario.");
        }

        return notifications
            .Select(notification => new NotificationResponse
            {
                Id = notification.Id,
                UsuarioId = notification.UsuarioId,
                Mensaje = notification.Mensaje,
                Tipo = notification.Tipo,
                Estado = notification.Estado,
                FechaEnvio = notification.FechaEnvio
            })
            .ToList();
    }
}