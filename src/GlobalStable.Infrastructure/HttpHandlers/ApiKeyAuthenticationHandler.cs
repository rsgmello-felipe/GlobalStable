using System.Security.Claims;
using System.Text.Encodings.Web;
using GlobalStable.Domain.Entities;
using GlobalStable.Infrastructure.Persistence;
using GlobalStable.Infrastructure.Utilities;
using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using ISystemClock = Microsoft.AspNetCore.Authentication.ISystemClock;

namespace GlobalStable.Infrastructure.HttpHandlers;

public class ApiKeyAuthenticationHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public const string Scheme = "ApiKey";
    private readonly ServiceDbContext _db;
    private readonly IConfiguration _cfg;

    public ApiKeyAuthenticationHandler(
        IOptionsMonitor<AuthenticationSchemeOptions> options, 
        ILoggerFactory logger, 
        UrlEncoder encoder, 
        ISystemClock clock,
        ServiceDbContext db,
        IConfiguration cfg)
        : base(options, logger, encoder, clock)
    {
        _db = db;
        _cfg = cfg;
    }

    protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        if (!Request.Headers.TryGetValue("X-Api-Key", out var keyValues))
            return AuthenticateResult.NoResult();

        var apiKey = keyValues.ToString().Trim();
        if (string.IsNullOrEmpty(apiKey))
            return AuthenticateResult.Fail("Missing API key.");

        var pepper = _cfg["Security:ApiKeyPepper"] ?? "";
        var hash = ApiKeyHasher.Sha256(apiKey, pepper);

        var record = await _db.Set<CustomerApiKey>()
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.KeyHash == hash && x.Enabled);

        if (record is null)
            return AuthenticateResult.Fail("Invalid API key.");

        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, $"customer:{record.CustomerId}"),
            new Claim("customer_id", record.CustomerId.ToString()),
            new Claim(ClaimTypes.Role, "customer")
        };

        var identity = new ClaimsIdentity(claims, Scheme);
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, Scheme);

        return AuthenticateResult.Success(ticket);
    }
}
