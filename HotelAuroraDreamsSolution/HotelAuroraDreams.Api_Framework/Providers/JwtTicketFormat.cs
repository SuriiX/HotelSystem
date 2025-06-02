using Microsoft.IdentityModel.Tokens;
using Microsoft.Owin.Security;
using System.IdentityModel.Tokens.Jwt;
using System.Text;
using System;

namespace HotelAuroraDreams.Api_Framework.Providers
{
    public class JwtTicketFormat : ISecureDataFormat<AuthenticationTicket>
    {
        private readonly string _issuer;
        private readonly string _audience;
        private readonly SymmetricSecurityKey _signingKey;
        private readonly TimeSpan _accessTokenExpireTimeSpan;

        public JwtTicketFormat(string issuer, string audience, string secretKey, TimeSpan accessTokenExpireTimeSpan)
        {
            if (string.IsNullOrWhiteSpace(issuer)) throw new ArgumentNullException(nameof(issuer));
            if (string.IsNullOrWhiteSpace(audience)) throw new ArgumentNullException(nameof(audience));
            if (string.IsNullOrWhiteSpace(secretKey)) throw new ArgumentNullException(nameof(secretKey));

            _issuer = issuer;
            _audience = audience;
            var secretBytes = Encoding.UTF8.GetBytes(secretKey);
            _signingKey = new SymmetricSecurityKey(secretBytes);
            _accessTokenExpireTimeSpan = accessTokenExpireTimeSpan;
        }

        public string Protect(AuthenticationTicket data)
        {
            if (data == null)
            {
                throw new ArgumentNullException(nameof(data));
            }

            var signingCredentials = new SigningCredentials(_signingKey, SecurityAlgorithms.HmacSha256Signature);
            var identity = data.Identity;

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = identity,
                Expires = DateTime.UtcNow.Add(_accessTokenExpireTimeSpan),
                Issuer = _issuer,
                Audience = _audience,
                SigningCredentials = signingCredentials,
                IssuedAt = data.Properties.IssuedUtc?.UtcDateTime,
                NotBefore = data.Properties.IssuedUtc?.UtcDateTime
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            var securityToken = tokenHandler.CreateToken(tokenDescriptor);
            return tokenHandler.WriteToken(securityToken);
        }

        public AuthenticationTicket Unprotect(string protectedText)
        {
            throw new NotImplementedException();
        }
    }
}