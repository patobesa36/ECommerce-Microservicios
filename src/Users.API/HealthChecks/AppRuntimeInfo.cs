namespace Users.API.HealthChecks;

public class AppRuntimeInfo
{
    public DateTime StartedAtUtc { get; } = DateTime.UtcNow;
}
