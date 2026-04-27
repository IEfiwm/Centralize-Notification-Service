using NotificationCenter.Domain.Common;

namespace NotificationCenter.Domain.Users;

public sealed class User : Entity
{
    public required string Username { get; init; }
    public required string PasswordHash { get; init; }
    public bool IsActive { get; init; } = true;
}

