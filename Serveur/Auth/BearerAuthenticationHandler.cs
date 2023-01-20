using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using System;
using System.Net.Http.Headers;
using System.Security.Claims;
using System.Text;
using System.Text.Encodings.Web;
using System.Threading.Tasks;
using Server.Database;

namespace Server.Auth
{
    public class BearerAuthenticationHandler : AuthenticationHandler<AuthOptions>
    {
        public BearerAuthenticationHandler(
            IOptionsMonitor<AuthOptions> options,
            ILoggerFactory logger,
            UrlEncoder encoder,
            ISystemClock clock)
            : base(options, logger, encoder, clock)
        {

        }


        protected override async Task<AuthenticateResult> HandleAuthenticateAsync()
        {
            if (!Request.Headers.ContainsKey("Authorization"))
                return AuthenticateResult.Fail("Missing Authorization Header");
            String token = String.Empty;
            try
            {
                var authHeader = AuthenticationHeaderValue.Parse(Request.Headers["Authorization"]);
                token = authHeader.Parameter ?? "";
            }
            catch
            {
                return AuthenticateResult.Fail("Invalid Authorization Header");
            }

            int? maybeUserId = await Account.CheckTokenSession(token);
            if (maybeUserId is int userId)
            {
                var identity = new UserIdentity("Bearer", userId, true);
                var principal = new ClaimsPrincipal(identity);
                var ticket = new AuthenticationTicket(principal, Scheme.Name);
                return AuthenticateResult.Success(ticket);
            }
            else
            {
                return AuthenticateResult.Fail("Expired session");
            }
        }

    }
}