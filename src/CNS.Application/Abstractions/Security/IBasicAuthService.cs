using NotificationCenter.Domain.Users;

namespace NotificationCenter.Application.Abstractions.Security;

public interface IBasicAuthService
{
    Task<User?> ValidateAsync(string username, string password, CancellationToken ct);
}

