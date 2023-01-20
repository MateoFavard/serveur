using Microsoft.AspNetCore.Authentication;
using Microsoft.Extensions.Options;
using System.Security.Principal;
using System.Security.Claims;
using System.Text.Encodings.Web;
using System.Runtime.CompilerServices;

namespace Server.Auth
{
    public class UserIdentity : ClaimsIdentity
    {
        private String _authType;
        private String _name;

        public UserIdentity(String authType, int userId, bool validated)
        {
            this._authType = authType;
            this._name = userId.ToString();
            this.AddClaim(new Claim("userId", this._name));
            this.AddClaim(new Claim("isValidated", validated.ToString()));
        }

        public override string? AuthenticationType => this._authType;

        public override string? Name => _name;

        public override bool IsAuthenticated => true;
    }

    public class AuthOptions : AuthenticationSchemeOptions
    {

    }

    public static class UserIdentityExtensions
    {
        public static int? UserId(this ClaimsPrincipal @this)
        {
            try
            {
                Claim userIdClaim = @this.Claims.Single(claim => claim.Type == "userId");
                return Int32.Parse(userIdClaim.Value);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
            catch (OverflowException)
            {
                return null;
            }
        }

        public static bool? IsValidated(this ClaimsPrincipal @this)
        {
            try
            {
                Claim isValidatedClaim = @this.Claims.Single(claim => claim.Type == "isValidated");
                return Boolean.Parse(isValidatedClaim.Value);
            }
            catch (InvalidOperationException)
            {
                return null;
            }
            catch (FormatException)
            {
                return null;
            }
        }
    }
}