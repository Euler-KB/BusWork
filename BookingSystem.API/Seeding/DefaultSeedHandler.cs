using BookingSystem.API.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Web;
using System.Web.Hosting;

namespace BookingSystem.API.Seeding
{
    public static class DefaultSeedHandler
    {
        class BusInfo
        {
            public int Id { get; set; }

            public string Name { get; set; }

            public string Seats { get; set; }

            public string Model { get; set; }

        }

        class RouteInfo
        {
            public int BusId { get; set; }

            public string From { get; set; }

            public string Destination { get; set; }

            public double Cost { get; set; }

            public DateTime DepartureTime { get; set; }

            public DateTime ArrivalTime { get; set; }

            public string Comments { get; set; }
        }

        class UserInfo
        {
            public string Username { get; set; }

            public string Email { get; set; }

            public string Password { get; set; }

            public string FullName { get; set; }

            public string Phone { get; set; }

            public string Role { get; set; }
        }

        static T LoadDocument<T>(string name)
        {

#if DEBUG
            string path = Path.Combine(AppDomain.CurrentDomain.GetData("DataDirectory").ToString(), "Seed", name);
#else
            string path = Path.Combine(HostingEnvironment.MapPath("~/App_Data/Seed"), name);
#endif

            return JsonConvert.DeserializeObject<T>(File.ReadAllText(path));
        }

        public static void SeedBuses(BookingContext db)
        {
            var routes = LoadDocument<IList<RouteInfo>>("Routes.json");

            foreach (var bus in LoadDocument<IList<BusInfo>>("Buses.json"))
            {
                if(db.Buses.Any(x => x.Name == bus.Name && x.Model == bus.Model))
                    continue;

                var dbBus = new Bus()
                {
                    Id = bus.Id,
                    Name = bus.Name,
                    Model = bus.Model,
                    Seats = bus.Seats
                };

                foreach (var rt in routes.Where(x => x.BusId == bus.Id))
                {
                    dbBus.Routes.Add(new BusRoute()
                    {
                        From = rt.From,
                        Destination = rt.Destination,
                        Cost = rt.Cost,
                        ArrivalTime = rt.ArrivalTime,
                        Comments = rt.Comments,
                        DepartureTime = rt.DepartureTime,
                    });
                }

                db.Buses.Add(dbBus);

            }

        }


        public static void SeedUsers(BookingContext db)
        {
            foreach (var user in LoadDocument<IList<UserInfo>>("Users.json"))
            {
                if (db.Users.Any(x => x.Username == user.Username))
                    continue;

                var dbUser = AppUser.New(user.Password);
                dbUser.Id = Guid.NewGuid().ToString();
                dbUser.Username = user.Username;
                dbUser.Email = user.Email;
                dbUser.Phone = user.Phone;
                dbUser.FullName = user.FullName;

                //  Email & Phone already confirmed
                dbUser.EmailConfirmed = true;
                dbUser.PhoneConfirmed = true;

                switch (user.Role)
                {
                    case "Admin":
                        {
                            db.UserClaims.AddRange(AppUser.GenerateAdminClaims(dbUser).Select(x => new UserClaim()
                            {
                                User = dbUser,
                                ClaimType = x.Type,
                                Value = x.Value,
                            }));
                        }
                        break;
                    case "User":
                        {
                            db.UserClaims.AddRange(AppUser.GenerateUserClaims(dbUser).Select(x => new UserClaim()
                            {
                                User = dbUser,
                                ClaimType = x.Type,
                                Value = x.Value
                            }));
                        }
                        break;
                }

                db.Users.Add(dbUser);
            }
        }
    }
}