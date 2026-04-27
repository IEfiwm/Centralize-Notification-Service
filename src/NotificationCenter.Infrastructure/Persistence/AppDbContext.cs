using Microsoft.EntityFrameworkCore;
using NotificationCenter.Domain.Messaging;
using NotificationCenter.Domain.Users;

namespace NotificationCenter.Infrastructure.Persistence;

public sealed class AppDbContext(DbContextOptions<AppDbContext> options) : DbContext(options)
{
    public DbSet<User> Users => Set<User>();
    public DbSet<MessageLog> MessageLogs => Set<MessageLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<User>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.Username).HasMaxLength(200).IsRequired();
            b.Property(x => x.PasswordHash).HasMaxLength(500).IsRequired();
            b.HasIndex(x => x.Username).IsUnique();
        });

        modelBuilder.Entity<MessageLog>(b =>
        {
            b.HasKey(x => x.Id);
            b.Property(x => x.RequestId).HasMaxLength(100).IsRequired();
            b.Property(x => x.Channel).HasMaxLength(50).IsRequired();
            b.Property(x => x.Recipient).HasMaxLength(300).IsRequired();
            b.Property(x => x.Subject).HasMaxLength(500);
            b.Property(x => x.ProviderHint).HasMaxLength(100);
            b.Property(x => x.ProviderUsed).HasMaxLength(100);
            b.Property(x => x.Error).HasMaxLength(4000);
            b.HasIndex(x => x.RequestId).IsUnique();
            b.HasIndex(x => new { x.Channel, x.Status });
        });
    }
}

