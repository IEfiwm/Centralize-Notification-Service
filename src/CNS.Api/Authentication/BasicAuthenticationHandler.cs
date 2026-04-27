using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using CNS.Application.Abstractions.Security;

namespace CNS.Api.Authentication;

public sealed class BasicAuthenticationHandler(
    IOptionsMonitor<AuthenticationSchemeOptions> options,
    ILoggerFactory logger,
    UrlEncoder encoder,
    IBasicAuthService authService
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
