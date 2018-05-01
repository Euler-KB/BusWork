using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public class JwtHelper
    {
        static readonly byte[] JwtSigningKey;

        static readonly string IssuerName;

        static JwtHelper()
        {
            JwtSigningKey = Encoding.UTF8.GetBytes(ConfigurationManager.AppSettings["JWT_SECRET"]);
            IssuerName = ConfigurationManager.AppSettings["JWT_ISSUER"];
        }

        public static string SignToken(AppUser user, IDictionary<string, string> properties = null, DateTime? expires = null, string audience = "*")
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            var descriptor = new SecurityTokenDescriptor()
            {
                Lifetime = new System.IdentityModel.Protocols.WSTrust.Lifetime(DateTime.UtcNow, expires ?? DateTime.UtcNow.AddDays(3)),
                SigningCredentials = new SigningCredentials(new InMemorySymmetricSecurityKey(JwtSigningKey), SecurityAlgorithms.HmacSha256Signature, SecurityAlgorithms.Sha256Digest),
                TokenIssuerName = IssuerName,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id),
                    new Claim(ClaimTypes.Role , user.Claims.FirstOrDefault(x => x.ClaimType == ClaimTypes.Role).Value),
                    new Claim(ClaimTypes.Name,user.Username),
                }),
                AppliesToAddress = audience,
            };

            if (properties != null)
            {
                foreach (var pair in properties)
                    descriptor.Properties.Add(pair.Key, pair.Value);
            }


            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public static ClaimsPrincipal DecodeToken(string token, string audience = "*", bool validateLifetime = true)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            SecurityToken securityToken;
            ClaimsPrincipal principal = null;
            try
            {
                principal = handler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidIssuer = IssuerName,
                    ValidateLifetime = validateLifetime,
                    IssuerSigningKey = new InMemorySymmetricSecurityKey(JwtSigningKey),
                    ValidateIssuer = true,
                    ValidAudience = audience
                }, out securityToken);
            }
            catch (Exception)
            {

            }

            return principal;
        }

    }
}