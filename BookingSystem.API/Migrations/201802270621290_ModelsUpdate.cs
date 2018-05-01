namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class ModelsUpdate : DbMigration
    {
        public override void Up()
        {
            DropForeignKey("dbo.Transactions", "Reservation_Id", "dbo.BookReservations");
            DropIndex("dbo.Transactions", new[] { "Reservation_Id" });
            AddColumn("dbo.Transactions", "Message", c => c.String());
            AddColumn("dbo.Transactions", "Wallet_Id", c => c.Long(nullable: false));
            AddColumn("dbo.Wallets", "IsSoftDeleted", c => c.Boolean(nullable: false));
            AddColumn("dbo.Wallets", "DateCreated", c => c.DateTime(nullable: false));
            AddColumn("dbo.Wallets", "LastUpdated", c => c.DateTime(nullable: false));
            AlterColumn("dbo.Transactions", "Reservation_Id", c => c.Long(nullable: false));
            CreateIndex("dbo.Transactions", "Reservation_Id");
            CreateIndex("dbo.Transactions", "Wallet_Id");
            AddForeignKey("dbo.Transactions", "Wallet_Id", "dbo.Wallets", "Id", cascadeDelete: true);
            AddForeignKey("dbo.Transactions", "Reservation_Id", "dbo.BookReservations", "Id", cascadeDelete: true);
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Transactions", "Reservation_Id", "dbo.BookReservations");
            DropForeignKey("dbo.Transactions", "Wallet_Id", "dbo.Wallets");
            DropIndex("dbo.Transactions", new[] { "Wallet_Id" });
            DropIndex("dbo.Transactions", new[] { "Reservation_Id" });
            AlterColumn("dbo.Transactions", "Reservation_Id", c => c.Long());
            DropColumn("dbo.Wallets", "LastUpdated");
            DropColumn("dbo.Wallets", "DateCreated");
            DropColumn("dbo.Wallets", "IsSoftDeleted");
            DropColumn("dbo.Transactions", "Wallet_Id");
            DropColumn("dbo.Transactions", "Message");
            CreateIndex("dbo.Transactions", "Reservation_Id");
            AddForeignKey("dbo.Transactions", "Reservation_Id", "dbo.BookReservations", "Id");
        }
    }
}
