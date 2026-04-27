using CNS.Application.Abstractions.Messaging;

namespace CNS.Infrastructure.Messaging;

public sealed class MessageProviderResolver(IEnumerable<IMessageProvider> providers) : IMessageProviderResolver
{
    public IMessageProvider Resolve(string channel, string? providerHint)
    {
        var candidates = providers.Where(p => string.Equals(p.Channel, channel, StringComparison.OrdinalIgnoreCase)).ToList();
        if (candidates.Count == 0)
            throw new InvalidOperationException($"No providers registered for channel '{channel}'.");

        if (!string.IsNullOrWhiteSpace(providerHint))
        {
            var hinted = candidates.FirstOrDefault(p => string.Equals(p.Name, providerHint, StringComparison.OrdinalIgnoreCase));
            if (hinted is not null)
                return hinted;
        }

        return candidates[0];
    }
}

