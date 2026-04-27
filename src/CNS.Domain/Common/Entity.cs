namespace CNS.Domain.Common;

public abstract class Entity
{
    public Guid Id { get; init; } = Guid.NewGuid();

    public DateTimeOffset CreatedAtUtc { get; init; } = DateTimeOffset.UtcNow;

    public DateTimeOffset? UpdatedAtUtc { get; private set; }

    public void Touch() => UpdatedAtUtc = DateTimeOffset.UtcNow;
}

