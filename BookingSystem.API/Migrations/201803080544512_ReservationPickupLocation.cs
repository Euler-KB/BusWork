namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ReservationPickupLocation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.BookReservations", "PickupLocation", c => c.String());
        }
        
        public override void Down()
        {
            DropColumn("dbo.BookReservations", "PickupLocation");
        }
    }
}
