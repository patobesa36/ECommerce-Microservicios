using Dapper;
using Notifications.API.Models;
using Microsoft.Data.Sqlite;

namespace Notifications.API.Data;

public class NotificationRepository(IConfiguration configuration) : INotificationRepository
{
    private readonly string _connectionString =
        configuration.GetConnectionString("DefaultConnection") ?? "Data Source=app.db";

    private SqliteConnection CreateConnection() => new(_connectionString);

    public async Task<Notification> CreateAsync(Notification notification)
    {
        using var connection = CreateConnection();

        await connection.ExecuteAsync(
            """
            INSERT INTO notifications (
                id,
                usuario_id,
                mensaje,
                tipo,
                estado,
                fecha_envio
            )
            VALUES (
                @Id,
                @UsuarioId,
                @Mensaje,
                @Tipo,
                @Estado,
                @FechaEnvio
            );
            """,
            new
            {
                Id = notification.Id.ToString(),
                UsuarioId = notification.UsuarioId.ToString(),
                notification.Mensaje,
                notification.Tipo,
                notification.Estado,
                FechaEnvio = notification.FechaEnvio.ToString("O")
            });

        return notification;
    }

    public async Task<IReadOnlyCollection<Notification>> GetByUserAsync(Guid userId)
    {
        using var connection = CreateConnection();

        var notifications = await connection.QueryAsync<Notification>(
            """
            SELECT
                id          AS Id,
                usuario_id  AS UsuarioId,
                mensaje     AS Mensaje,
                tipo        AS Tipo,
                estado      AS Estado,
                fecha_envio AS FechaEnvio
            FROM notifications
            WHERE usuario_id = @UsuarioId
            ORDER BY fecha_envio DESC;
            """,
            new { UsuarioId = userId.ToString() });

        return notifications.ToList();
    }
}