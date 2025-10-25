using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Logging;
using System.Text.Encodings.Web;
using System.Security.Claims;

namespace tests;

public class TestAdminUserAuthHandler : AuthenticationHandler<AuthenticationSchemeOptions>
{
    public TestAdminUserAuthHandler(IOptionsMonitor<AuthenticationSchemeOptions> options,
        ILoggerFactory logger,
    UrlEncoder encoder)
    : base(options, logger, encoder)
    {
    }

    protected override Task<AuthenticateResult> HandleAuthenticateAsync()
    {
        var claims = new[] { new Claim(ClaimTypes.Name, "Test user"), new Claim(ClaimTypes.Role, "Admin") };
        var identity = new ClaimsIdentity(claims, "Test");
        var principal = new ClaimsPrincipal(identity);
        var ticket = new AuthenticationTicket(principal, "TestScheme");

        var result = AuthenticateResult.Success(ticket);
        return Task.FromResult(result);
    }
}
