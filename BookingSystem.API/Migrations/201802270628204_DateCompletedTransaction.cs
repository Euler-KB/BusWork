namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class DateCompletedTransaction : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Transactions", "DateCompleted", c => c.DateTime());
        }
        
        public override void Down()
        {
            DropColumn("dbo.Transactions", "DateCompleted");
        }
    }
}
