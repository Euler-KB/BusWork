using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API
{
    public class DatabaseInitializer : System.Data.Entity.DropCreateDatabaseIfModelChanges<BookingContext>
    {
        protected override void Seed(BookingContext context)
        {
            //  Seed buses
            Seeding.DefaultSeedHandler.SeedBuses(context);

            //  Seed users
            Seeding.DefaultSeedHandler.SeedUsers(context);
        }
    }
}