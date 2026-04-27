using CNS.Contracts.Api;

namespace CNS.Application.Commands.SendMessage;

public sealed record SendMessageCommand(string RequestId, SendMessageRequestDto Request);

