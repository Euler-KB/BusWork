using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Microsoft.AspNet.SignalR;
using BookingSystem.API.Helpers;
using System.Timers;

namespace BookingSystem.API.Hubs
{
    public class CoreHub : Hub
    {

#if DEBUG
        static CoreHub()
        {
            int count = 0;
            Timer timer = new Timer(2000);
            timer.Elapsed += delegate
            {
                try
                {
                    GlobalHost.ConnectionManager.GetHubContext<CoreHub>().Clients.All.OnTest(count++);
                }
                catch
                {

                }
            };
            timer.Start();
        }
#endif

        public override async Task OnConnected()
        {
            var auth = this.Context.Headers["Authorization"];
            if (auth?.StartsWith("Bearer") == true)
            {
                var userAuth = JwtHelper.DecodeToken(auth.Split(' ').Last(), validateLifetime: false);
                if (userAuth != null)
                {
                    if (userAuth.IsInRole(UserRoles.Admin))
                    {
                        await Groups.Add(Context.ConnectionId, UserRoles.Admin);
                    }
                    else if (userAuth.IsInRole(UserRoles.User))
                    {
                        await Groups.Add(Context.ConnectionId, UserRoles.User);
                    }
                }
            }

        }
    }
}