using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Text.Json;
using App.Core.Enums;
using App.Core.Interfaces;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using MongoDB.Bson;

namespace App.Api.Handlers;

public class ReferenceTokenAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    private readonly ILogger<ReferenceTokenAuthHandler> _logger;
    private readonly IUserSessionRepository _sessionRepository;
    private readonly IUserBanRepository _userBanRepository;

    public ReferenceTokenAuthHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory loggerFactory,
        UrlEncoder encoder,
        ISystemClock clock,
        IUserSessionRepository sessionRepository,
        IUserBanRepository userBanRepository)
        : base(options, loggerFactory, encoder, clock)
    {
        _sessionRepository = sessionRepository;
        _userBanRepository = userBanRepository;
        _logger = loggerFactory.CreateLogger<ReferenceTokenAuthHandler>();
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        using (_logger.BeginScope("HandleAuthenticateAsync"))
        {
            _logger.LogInformation("Starting authentication for request {Path}", Request.Path);

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (string.IsNullOrEmpty(authHeader) || !authHeader.StartsWith("Bearer "))
            {
                _logger.LogWarning("Authorization header missing or invalid");
                return AuthenticateResult.NoResult();
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            _logger.LogTrace("Extracted token: {Token}", token);

            if (!ObjectId.TryParse(token, out var sessionId))
            {
                _logger.LogError("Invalid token format: {Token}", token);
                return AuthenticateResult.Fail("Invalid token format.");
            }

            _logger.LogDebug("Parsed sessionId: {SessionId}", sessionId);

            var session = await _sessionRepository.GetSessionAsync(sessionId);
            if (session == null)
            {
                _logger.LogWarning("Session not found for Id: {SessionId}", sessionId);
                return AuthenticateResult.Fail("Session not found.");
            }

            if (session.IsRevoked || session.ExpiresAt <= DateTime.UtcNow)
            {
                _logger.LogWarning(
                    "Session is revoked or expired. SessionId={SessionId}, ExpiresAt={ExpiresAt}, IsRevoked={IsRevoked}",
                    sessionId, session.ExpiresAt, session.IsRevoked);
                return AuthenticateResult.Fail("Session is invalid or expired.");
            }

            var userBans = await _userBanRepository.GetByUserIdAsync(session.UserId);
            if (userBans != null && userBans.Count > 0)
            {
                var activeBans = userBans
                    .Where(b => !b.BannedUntil.HasValue || b.BannedUntil.Value > DateTime.UtcNow)
                    .ToList();

                if (activeBans.Any(b => b.Types.HasFlag(BanType.Login)))
                {
                    _logger.LogInformation("User {UserId} is banned. ActiveBansCount={Count}", session.UserId,
                        activeBans.Count);

                    var banInfos = activeBans.Select(b => new
                    {
                        AdminId = b.AdminId.ToString(),
                        b.Reason,
                        b.Types,
                        b.BannedAt,
                        b.BannedUntil
                    }).ToList();

                    Context.Items["AuthError"] = JsonSerializer.Serialize(banInfos, new JsonSerializerOptions
                    {
                        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
                        WriteIndented = true
                    });

                    return AuthenticateResult.Fail("Banned");
                }
            }

            var claims = new List<Claim>
            {
                new(ClaimTypes.NameIdentifier, session.UserId.ToString()),
                new(ClaimTypes.Sid, session.Id.ToString()),
                new("Ip", session.DeviceInfo.Ip),
                new("Country", session.DeviceInfo.Country),
                new("City", session.DeviceInfo.City),
                new("Browser", session.DeviceInfo.Browser),
                new("Os", session.DeviceInfo.Os),
                new("Device", session.DeviceInfo.Device),
            };

            foreach (var role in session.Roles) claims.Add(new Claim(ClaimTypes.Role, role));

            _logger.LogDebug("Creating ClaimsIdentity for UserId={UserId} with Roles={Roles}", session.UserId,
                string.Join(",", session.Roles));

            var identity = new ClaimsIdentity(claims, Scheme.Name);
            var principal = new ClaimsPrincipal(identity);

            _logger.LogTrace("User claims for UserId={UserId}: {Claims}",
                session.UserId,
                string.Join(", ", principal.Claims.Select(c => $"{c.Type}={c.Value}"))
            );

            var ticket = new AuthenticationTicket(principal, Scheme.Name);

            _logger.LogInformation("Authentication successful for UserId={UserId}", session.UserId);

            return AuthenticateResult.Success(ticket);
        }
    }

    protected override Task HandleChallengeAsync(AuthenticationProperties properties)
    {
        using (_logger.BeginScope("HandleChallengeAsync"))
        {
            _logger.LogInformation("Handling challenge for request {Path}", Context.Request.Path);

            if (Context.Items.TryGetValue("AuthError", out var error))
            {
                _logger.LogWarning("Authorization failed due to ban. AuthError={AuthError}", error);

                object details;
                try
                {
                    details = JsonSerializer.Deserialize<object>(error.ToString()!);
                }
                catch
                {
                    details = error.ToString();
                }

                var problemDetails = new ValidationProblemDetails(new Dictionary<string, string[]>
                {
                    { "Authorization", new[] { "User is banned" } }
                })
                {
                    Type = "https://tools.ietf.org/html/rfc9110#section-15.5.1",
                    Title = "One or more authorization errors occurred.",
                    Status = 403
                };

                problemDetails.Extensions["traceId"] = Context.TraceIdentifier;
                problemDetails.Extensions["bans"] = details;

                Context.Response.StatusCode = 403;
                Context.Response.ContentType = "application/json";

                var json = JsonSerializer.Serialize(problemDetails, new JsonSerializerOptions
                {
                    PropertyNamingPolicy = JsonNamingPolicy.CamelCase
                });

                return Context.Response.WriteAsync(json);
            }

            return base.HandleChallengeAsync(properties);
        }
    }
}