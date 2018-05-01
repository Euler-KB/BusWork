using BookingSystem.API.Helpers;
using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IdentityModel.Claims;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BookingSystem.API.Handlers
{
    public class AuthenticationHandler : DelegatingHandler
    {
        protected override Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            var auth = request.Headers.Authorization;
            if (auth != null && (auth.Scheme == "Bearer"))
            {
                var principal = JwtHelper.DecodeToken(auth.Parameter, TokenAudiences.Universal);
                if (principal != null)
                {
                    var id = principal.FindFirst(ClaimTypes.NameIdentifier).Value;
                    using (var dbContext = new BookingContext())
                    {
                        if (dbContext.Users.Any(x => x.Id == id && x.EmailConfirmed))
                        {
                            Thread.CurrentPrincipal = principal;

                            if (HttpContext.Current != null)
                                HttpContext.Current.User = principal;
                        }
                    }
                }
            }

            return base.SendAsync(request, cancellationToken);
        }
    }
}