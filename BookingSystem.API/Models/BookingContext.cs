namespace BookingSystem.API.Models
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Linq;
    using System.Threading.Tasks;
    using Z.EntityFramework.Plus;

    public class SaveOptions
    {
        /// <summary>
        /// True to update timestamps changes
        /// </summary>
        public bool UpdateTimestamps { get; set; }

        /// <summary>
        /// The models to exclude from timestamp update
        /// </summary>
        public IEnumerable<ITimestamp> ExcludeTimestamps { get; set; }

    }

    public class BookingContext : DbContext
    {
        // Your context has been configured to use a 'BookingContext' connection string from your application's 
        // configuration file (App.config or Web.config). By default, this connection string targets the 
        // 'BookingSystem.API.Models.BookingContext' database on your LocalDb instance. 
        // 
        // If you wish to target a different database and/or database provider, modify the 'BookingContext' 
        // connection string in the application configuration file.
        public BookingContext()
            : base("name=BookingContext")
        {
            QueryFilterManager.InitilizeGlobalFilter(this);
        }

        void UpdateTimeStamps(SaveOptions saveOptions)
        {
            var ignore = saveOptions.ExcludeTimestamps;
            var entries = ChangeTracker.Entries<ITimestamp>();
            foreach (var entry in entries)
            {
                if (ignore?.Any(x => x == entry.Entity) == true)
                    continue;

                if (entry.State == EntityState.Added)
                    entry.Entity.DateCreated = DateTime.UtcNow;

                if (entry.State == EntityState.Added || entry.State == EntityState.Modified)
                    entry.Entity.LastUpdated = DateTime.UtcNow;
            }
        }

        private void UpdateContext(SaveOptions saveOptions)
        {
            if (!ChangeTracker.HasChanges())
                return;

            //
            if (saveOptions.UpdateTimestamps)
                UpdateTimeStamps(saveOptions);

        }

        protected override void OnModelCreating(DbModelBuilder modelBuilder)
        {
            modelBuilder.Configurations.AddFromAssembly(typeof(BookingContext).Assembly);
            base.OnModelCreating(modelBuilder);
        }

        public int SaveChanges(SaveOptions options)
        {
            UpdateContext(options);
            return base.SaveChanges();
        }

        public Task<int> SaveChangesAsync(SaveOptions options)
        {
            UpdateContext(options);
            return base.SaveChangesAsync();
        }

        public int SaveChanges(IEnumerable<ITimestamp> ignoreTimestamp)
        {
            return SaveChanges(new SaveOptions()
            {
                UpdateTimestamps = true,
                ExcludeTimestamps = ignoreTimestamp
            });
        }

        public Task<int> SaveChangesAsync(IEnumerable<ITimestamp> ignoreTimestamp)
        {
            return SaveChangesAsync(new SaveOptions()
            {
                UpdateTimestamps = true,
                ExcludeTimestamps = ignoreTimestamp
            });
        }

        public override int SaveChanges()
        {
            return SaveChanges(new SaveOptions() { UpdateTimestamps = true });
        }

        public override Task<int> SaveChangesAsync()
        {
            return SaveChangesAsync(new SaveOptions() { UpdateTimestamps = true });
        }

        public DbSet<Bus> Buses { get; set; }

        public DbSet<BusRoute> Routes { get; set; }

        public DbSet<BookReservation> Reservations { get; set; }

        public DbSet<AppUser> Users { get; set; }

        public DbSet<Media> Media { get; set; }

        public DbSet<UserClaim> UserClaims { get; set; }

        public DbSet<UserFeedBack> Feedback { get; set; }

        public DbSet<Transaction> Transactions { get; set; }

        public DbSet<UserWallet> Wallet { get; set; }

        public DbSet<UserToken> Tokens { get; set; }
    }


}