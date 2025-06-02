using HotelAuroraDreams.Api_Framework.IdentityModels;
using HotelAuroraDreams.Api_Framework.Providers;
using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security.Jwt;
using Microsoft.Owin.Security.OAuth;
using Microsoft.Owin;
using Owin;
using System.Configuration;
using System.Text;
using System.Web.Http;
using System;
using Microsoft.Owin.Security;

[assembly: OwinStartup(typeof(HotelAuroraDreams.Api_Framework.Startup))]
namespace HotelAuroraDreams.Api_Framework
{
    public class Startup
    {
        public void Configuration(IAppBuilder app)
        {
            HttpConfiguration httpConfig = new HttpConfiguration();

            app.UseCors(Microsoft.Owin.Cors.CorsOptions.AllowAll);
            ConfigureOAuth(app);
            WebApiConfig.Register(httpConfig);
            app.UseWebApi(httpConfig);
        }

        public void ConfigureOAuth(IAppBuilder app)
        {
            app.CreatePerOwinContext(ApplicationIdentityDbContext.Create);
            app.CreatePerOwinContext<ApplicationUserManager>(ApplicationUserManager.Create);
            app.CreatePerOwinContext<ApplicationRoleManager>(ApplicationRoleManager.Create);

            var issuer = ConfigurationManager.AppSettings["jwt:Issuer"];
            var audience = ConfigurationManager.AppSettings["jwt:Audience"];
            var secretKey = ConfigurationManager.AppSettings["jwt:SecretKey"];
            var tokenExpireTimeSpan = TimeSpan.FromDays(1);

            if (string.IsNullOrEmpty(issuer) || string.IsNullOrEmpty(audience) || string.IsNullOrEmpty(secretKey))
            {
                throw new ArgumentNullException("JWT Issuer, Audience, or SecretKey not configured in Web.config AppSettings.");
            }

            OAuthAuthorizationServerOptions OAuthServerOptions = new OAuthAuthorizationServerOptions()
            {
                AllowInsecureHttp = true,
                TokenEndpointPath = new PathString("/api/token"),
                AccessTokenExpireTimeSpan = tokenExpireTimeSpan,
                Provider = new ApplicationOAuthProvider("self"),
                AccessTokenFormat = new JwtTicketFormat(issuer, audience, secretKey, tokenExpireTimeSpan)
            };

            app.UseOAuthAuthorizationServer(OAuthServerOptions);

            app.UseJwtBearerAuthentication(
                new JwtBearerAuthenticationOptions
                {
                    AuthenticationMode = AuthenticationMode.Active,
                    TokenValidationParameters = new TokenValidationParameters()
                    {
                        ValidateIssuer = true,
                        ValidateAudience = true,
                        ValidateIssuerSigningKey = true,
                        ValidIssuer = issuer,
                        ValidAudience = audience,
                        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(secretKey))
                    }
                });
        }
    }
}