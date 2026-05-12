using Microsoft.AspNetCore.Mvc;
using CNS.Application.Commands.SendMessage;
using CNS.Contracts;
using CNS.Contracts.Api;

namespace CNS.Api.Controllers;

[Route("messages")]
public sealed class MessagesController : ApiControllerBase
{
    [HttpPost]
    [ProducesResponseType(typeof(SendMessageResponseDto), StatusCodes.Status200OK)]
    public async Task<ActionResult<SendMessageResponseDto>> SendAsync(
        [FromBody] SendMessageRequestDto request,
        [FromServices] SendMessageCommandHandler handler,
        CancellationToken cancellationToken)
    {
        var requestId = Guid.NewGuid().ToString("N");
        await handler.HandleAsync(new SendMessageCommand(requestId, request), cancellationToken);
        return Ok(new SendMessageResponseDto(requestId, "Queued"));
    }

    [HttpPost("sms/quick")]
    [ProducesResponseType(typeof(SendMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendMessageResponseDto>> QuickSmsAsync(
        [FromBody] QuickSendSmsRequestDto request,
        [FromServices] SendMessageCommandHandler handler,
        CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(request.Phone))
            return BadRequest("Phone is required.");
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var full = new SendMessageRequestDto(
            Channel: Channels.Sms,
            Recipient: request.Phone.Trim(),
            Subject: null,
            Body: request.Message.Trim(),
            Metadata: null,
            ProviderHint: string.IsNullOrWhiteSpace(request.ProviderHint) ? null : request.ProviderHint.Trim()
        );

        var requestId = Guid.NewGuid().ToString("N");
        await handler.HandleAsync(new SendMessageCommand(requestId, full), cancellationToken);
        return Ok(new SendMessageResponseDto(requestId, "Queued"));
    }

    [HttpPost("email/quick")]
    [ProducesResponseType(typeof(SendMessageResponseDto), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public async Task<ActionResult<SendMessageResponseDto>> QuickEmailAsync(
        [FromBody] QuickSendEmailRequestDto request,
        [FromServices] SendMessageCommandHandler handler,
        CancellationToken cancellationToken
        )
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest("Email is required.");
        if (string.IsNullOrWhiteSpace(request.Message))
            return BadRequest("Message is required.");

        var full = new SendMessageRequestDto(
            Channel: Channels.Email,
            Recipient: request.Email.Trim(),
            Subject: null,
            Body: request.Message.Trim(),
            Metadata: null,
            ProviderHint: string.IsNullOrWhiteSpace(request.ProviderHint) ? null : request.ProviderHint.Trim()
        );

        var requestId = Guid.NewGuid().ToString("N");
        await handler.HandleAsync(new SendMessageCommand(requestId, full), cancellationToken);
        return Ok(new SendMessageResponseDto(requestId, "Queued"));
    }

}