using CNS.Application.Abstractions.Persistence;
using CNS.Application.Abstractions.Security;
using CNS.Domain.Users;

namespace CNS.Infrastructure.Security;

public sealed class BasicAuthService(
    IRepository<User> users,
    IPasswordHasher passwordHasher
) : IBasicAuthService
{
    public async Task<User?> ValidateAsync(string username, string password, CancellationToken ct)
    {
        var user = await users.FirstOrDefaultAsync(x => x.Username == username, ct);
        if (user is null || !user.IsActive)
            return null;

        return passwordHasher.Verify(password, user.PasswordHash) ? user : null;
    }
}

