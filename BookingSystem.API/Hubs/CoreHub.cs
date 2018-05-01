using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using BookingSystem.API.Helpers;

namespace BookingSystem.API.Hubs
{
    public class CoreHub : Hub
    {
        public override Task OnConnected()
        {
            var auth = this.Context.Headers["Authentication"];
            if (auth?.StartsWith("Bearer") == true)
            {
                var userAuth = JwtHelper.DecodeToken(auth.Split(' ').Last());
                if (userAuth != null)
                {
                    if (userAuth.IsInRole(UserRoles.Admin))
                    {
                        Groups.Add(Context.ConnectionId, UserRoles.Admin);
                    }
                    else if (userAuth.IsInRole(UserRoles.User))
                    {
                        Groups.Add(Context.ConnectionId, UserRoles.User);
                    }
                }
            }

            return Task.FromResult(0);
        }
    }
}