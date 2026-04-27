using CNS.Domain.Users;

namespace CNS.Application.Abstractions.Security;

public interface IBasicAuthService
{
    Task<User?> ValidateAsync(string username, string password, CancellationToken ct);
}

