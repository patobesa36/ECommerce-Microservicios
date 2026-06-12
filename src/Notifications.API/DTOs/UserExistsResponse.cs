namespace Notifications.API.DTOs;

public class UserExistsResponse
{
    public Guid Id { get; set; }
    public bool Exists { get; set; }
}