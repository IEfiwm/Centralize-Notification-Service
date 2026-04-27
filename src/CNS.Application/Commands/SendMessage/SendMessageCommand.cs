using NotificationCenter.Contracts.Api;

namespace NotificationCenter.Application.Commands.SendMessage;

public sealed record SendMessageCommand(string RequestId, SendMessageRequestDto Request);

