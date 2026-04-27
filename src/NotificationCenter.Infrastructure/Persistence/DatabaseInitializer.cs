using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using NotificationCenter.Application.Abstractions.Security;
using NotificationCenter.Domain.Users;

namespace NotificationCenter.Infrastructure.Persistence;

public static class DatabaseInitializer
{
    public static async Task InitializeAsync(IServiceProvider services, CancellationToken ct = default)
    {
        using var scope = services.CreateScope();
        var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
        await db.Database.MigrateAsync(ct);

        var config = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var seedUser = config["Seed:AdminUser"];
        var seedPass = config["Seed:AdminPassword"];
        if (string.IsNullOrWhiteSpace(seedUser) || string.IsNullOrWhiteSpace(seedPass))
            return;

        var existing = await db.Users.FirstOrDefaultAsync(x => x.Username == seedUser, ct);
        if (existing is not null)
            return;

        var hasher = scope.ServiceProvider.GetRequiredService<IPasswordHasher>();
        db.Users.Add(new User
        {
            Username = seedUser,
            PasswordHash = hasher.Hash(seedPass),
            IsActive = true
        });
        await db.SaveChangesAsync(ct);
    }
}

