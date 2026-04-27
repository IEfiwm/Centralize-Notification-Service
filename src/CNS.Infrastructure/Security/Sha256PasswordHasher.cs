using System.Security.Cryptography;
using System.Text;
using NotificationCenter.Application.Abstractions.Security;

namespace NotificationCenter.Infrastructure.Security;

public sealed class Sha256PasswordHasher : IPasswordHasher
{
    public string Hash(string password)
    {
        var bytes = Encoding.UTF8.GetBytes(password);
        var hash = SHA256.HashData(bytes);
        return Convert.ToHexString(hash);
    }

    public bool Verify(string password, string passwordHash) =>
        string.Equals(Hash(password), passwordHash, StringComparison.OrdinalIgnoreCase);
}

