namespace Notifications.API.HealthChecks;

public class AppRuntimeInfo
{
    public DateTime StartedAtUtc { get; } = DateTime.UtcNow;
}