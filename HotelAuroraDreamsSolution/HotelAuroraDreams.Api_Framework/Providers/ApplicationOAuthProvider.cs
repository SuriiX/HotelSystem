using HotelAuroraDreams.Api_Framework.IdentityModels;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin.Security;
using System.Collections.Generic;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNet.Identity.Owin;

namespace HotelAuroraDreams.Api_Framework.Providers
{
    public class ApplicationOAuthProvider : OAuthAuthorizationServerProvider
    {
        private readonly string _publicClientId;

        public ApplicationOAuthProvider(string publicClientId)
        {
            _publicClientId = publicClientId;
        }

        public override Task ValidateClientAuthentication(OAuthValidateClientAuthenticationContext context)
        {
            context.Validated();
            return Task.FromResult<object>(null);
        }

        public override async Task GrantResourceOwnerCredentials(OAuthGrantResourceOwnerCredentialsContext context)
        {
            var userManager = context.OwinContext.GetUserManager<ApplicationUserManager>();
            ApplicationUser user = await userManager.FindAsync(context.UserName, context.Password);

            if (user == null)
            {
                context.SetError("invalid_grant", "El nombre de usuario o la contraseña no son válidos.");
                return;
            }

            ClaimsIdentity oAuthIdentity = await userManager.CreateIdentityAsync(user, OAuthDefaults.AuthenticationType);
            oAuthIdentity.AddClaim(new Claim(ClaimTypes.Name, user.UserName));
            oAuthIdentity.AddClaim(new Claim("nombre", user.Nombre));
            oAuthIdentity.AddClaim(new Claim("apellido", user.Apellido));
            if (user.HotelID.HasValue)
            {
                oAuthIdentity.AddClaim(new Claim("hotelId", user.HotelID.Value.ToString()));
            }
            if (user.CargoID.HasValue)
            {
                oAuthIdentity.AddClaim(new Claim("cargoId", user.CargoID.Value.ToString()));
            }

            var props = new AuthenticationProperties(new Dictionary<string, string>
            {
                { "userName", user.UserName },
                { "nombreCompleto", $"{user.Nombre} {user.Apellido}" }
            });

            AuthenticationTicket ticket = new AuthenticationTicket(oAuthIdentity, props);
            context.Validated(ticket);
        }

        public override Task TokenEndpoint(OAuthTokenEndpointContext context)
        {
            if (context.Properties.Dictionary != null)
            {
                foreach (var property in context.Properties.Dictionary)
                {
                    if (property.Key != ".issued" && property.Key != ".expires" &&
                        property.Key != "as:client_id" && property.Key != "audience" &&
                        property.Key != "client_id" &&
                        !context.AdditionalResponseParameters.ContainsKey(property.Key))
                    {
                        context.AdditionalResponseParameters.Add(property.Key, property.Value);
                    }
                }
            }
            return Task.FromResult<object>(null);
        }
    }
}