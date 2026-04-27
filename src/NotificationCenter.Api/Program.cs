using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authorization;
using Microsoft.Extensions.Options;
using NotificationCenter.Application.Commands.SendMessage;
using NotificationCenter.Contracts.Api;
using NotificationCenter.Infrastructure;
using NotificationCenter.Infrastructure.Persistence;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddInfrastructure(builder.Configuration);
builder.Services.AddScoped<SendMessageCommandHandler>();

builder.Services
    .AddAuthentication("Basic")
    .AddScheme<AuthenticationSchemeOptions, BasicAuthenticationHandler>("Basic", _ => { });

builder.Services.AddAuthorization();

builder.Services.AddOpenApi();

var app = builder.Build();

await DatabaseInitializer.InitializeAsync(app.Services);

if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapPost("/messages", [Authorize] async (
    SendMessageRequestDto req,
    SendMessageCommandHandler handler,
    CancellationToken ct
) =>
{
    var requestId = Guid.NewGuid().ToString("N");
    await handler.HandleAsync(new SendMessageCommand(requestId, req), ct);
    return Results.Ok(new SendMessageResponseDto(requestId, "Queued"));
});

app.Run();

sealed class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    NotificationCenter.Application.Abstractions.Security.IBasicAuthService authService
) : AuthenticationHandler<AuthenticationSchemeOptions>(options, logger, encoder)
{
    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("Authorization", out var values))
            return AuthenticateResult.Fail("Missing Authorization header");

        var header = values.ToString();
        if (!header.StartsWith("Basic ", StringComparison.OrdinalIgnoreCase))
            return AuthenticateResult.Fail("Invalid Authorization scheme");

        string decoded;
        try
        {
            var base64 = header["Basic ".Length..].Trim();
            decoded = Encoding.UTF8.GetString(Convert.FromBase64String(base64));
        }
        catch
        {
            return AuthenticateResult.Fail("Invalid Basic token");
        }

        var idx = decoded.IndexOf(':');
        if (idx <= 0)
            return AuthenticateResult.Fail("Invalid Basic token format");

        var username = decoded[..idx];
        var password = decoded[(idx + 1)..];

        var user = await authService.ValidateAsync(username, password, Context.RequestAborted);
        if (user is null)
            return AuthenticateResult.Fail("Invalid username/password");

        var claims = new[]
        {
            new Claim(ClaimTypes.NameIdentifier, user.Id.ToString("N")),
            new Claim(ClaimTypes.Name, user.Username)
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);
        return AuthenticateResult.Success(ticket);
    }
}
