using Microsoft.AspNetCore.Authentication;
using System.Security.Claims;
using System.Text.Encodings.Web;
using App.Core.Interfaces;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace App.Api.Handlers;

public class ReferenceTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly IUserSessionRepository _sessionRepository;

    public ReferenceTokenAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserSessionRepository sessionRepository)
        : base(options, logger, encoder, clock)
    {
        _sessionRepository = sessionRepository;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var authHeader = Request.Headers["Authorization"].FirstOrDefault();
        if (authHeader is null || !authHeader.StartsWith("Bearer ")) 
            return AuthenticateResult.NoResult();

        var token = authHeader.Substring("Bearer ".Length).Trim();

        if (!ObjectId.TryParse(token, out var sessionId))
            return AuthenticateResult.Fail("Invalid token format.");

        var session = await _sessionRepository.GetSessionAsync(sessionId);

        if (session == null || session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
            return AuthenticateResult.Fail("Session is invalid or expired.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, session.UserId.ToString()),
            new Claim(ClaimTypes.Sid, session.Id.ToString()),
            new Claim("DeviceInfo", session.DeviceInfo ?? "")
        };

        var identity = new ClaimsIdentity(claims, Scheme.Name);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme.Name);

        return AuthenticateResult.Success(ticket);
    }
}