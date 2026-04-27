namespace CNS.Application.Abstractions.Messaging;

public interface IMessageProviderResolver
{
    IMessageProvider Resolve(string channel, string? providerHint);
}

