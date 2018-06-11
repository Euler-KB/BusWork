using BookingSystem.API.Models;
using Microsoft.IdentityModel.Tokens;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
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
            var descriptor = new Microsoft.IdentityModel.Tokens.SecurityTokenDescriptor()
            {
                Expires = expires ?? DateTime.UtcNow.AddDays(3),
                SigningCredentials = new Microsoft.IdentityModel.Tokens.SigningCredentials(new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(JwtSigningKey), Microsoft.IdentityModel.Tokens.SecurityAlgorithms.HmacSha256Signature),
                Issuer = IssuerName,
                Subject = new ClaimsIdentity(new Claim[]
                {
                    new Claim(ClaimTypes.NameIdentifier,user.Id),
                    new Claim(ClaimTypes.Role , user.Claims.FirstOrDefault(x => x.ClaimType == ClaimTypes.Role).Value),
                    new Claim(ClaimTypes.Name,user.Username),
                }),
                Audience = audience,
            };

            return handler.WriteToken(handler.CreateToken(descriptor));
        }

        public static ClaimsPrincipal DecodeToken(string token, string audience  = TokenAudiences.Universal, bool validateLifetime = true)
        {
            JwtSecurityTokenHandler handler = new JwtSecurityTokenHandler();
            Microsoft.IdentityModel.Tokens.SecurityToken securityToken;
            ClaimsPrincipal principal = null;
            try
            {
                principal = handler.ValidateToken(token, new TokenValidationParameters()
                {
                    ValidIssuer = IssuerName,
                    ValidateLifetime = validateLifetime,
                    IssuerSigningKey = new Microsoft.IdentityModel.Tokens.SymmetricSecurityKey(JwtSigningKey),
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