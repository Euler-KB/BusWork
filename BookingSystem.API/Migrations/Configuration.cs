namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity;
    using System.Data.Entity.Migrations;
    using System.Data.Entity.Validation;
    using System.IO;
    using System.Linq;

    internal sealed class Configuration : DbMigrationsConfiguration<BookingSystem.API.Models.BookingContext>
    {
        public Configuration()
        {
            AutomaticMigrationsEnabled = false;
        }

        protected override void Seed(Models.BookingContext context)
        {
            //  Seed buses
            Seeding.DefaultSeedHandler.SeedBuses(context);

            //  Seed users
            Seeding.DefaultSeedHandler.SeedUsers(context);

        }
    }
}
