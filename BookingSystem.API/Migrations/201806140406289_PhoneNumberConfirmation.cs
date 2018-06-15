namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class PhoneNumberConfirmation : DbMigration
    {
        public override void Up()
        {
            AddColumn("dbo.Users", "PhoneConfirmed", c => c.Boolean(nullable: false));
        }
        
        public override void Down()
        {
            DropColumn("dbo.Users", "PhoneConfirmed");
        }
    }
}
