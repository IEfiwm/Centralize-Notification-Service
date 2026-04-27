namespace NotificationCenter.Application.Abstractions.Messaging;

public interface IMessageProviderResolver
{
    IMessageProvider Resolve(string channel, string? providerHint);
}

