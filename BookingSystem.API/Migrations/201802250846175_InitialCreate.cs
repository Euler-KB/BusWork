namespace BookingSystem.API.Migrations
{
    using System;
    using System.Data.Entity.Migrations;
    
    public partial class InitialCreate : DbMigration
    {
        public override void Up()
        {
            CreateTable(
                "dbo.Buses",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Model = c.String(),
                        Seats = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsSoftDeleted = c.Boolean(nullable: false),
                        ProfileImage_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Media", t => t.ProfileImage_Id)
                .Index(t => t.ProfileImage_Id);
            
            CreateTable(
                "dbo.Media",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Name = c.String(nullable: false),
                        Path = c.String(nullable: false),
                        MimeType = c.String(),
                        Tag = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                    })
                .PrimaryKey(t => t.Id);
            
            CreateTable(
                "dbo.Routes",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        From = c.String(nullable: false),
                        DepartureTime = c.DateTime(nullable: false),
                        ArrivalTime = c.DateTime(nullable: false),
                        FromLat = c.Decimal(precision: 18, scale: 2),
                        FromLng = c.Decimal(precision: 18, scale: 2),
                        Cost = c.Double(nullable: false),
                        Comments = c.String(),
                        Destination = c.String(nullable: false),
                        DestinationLat = c.Decimal(precision: 18, scale: 2),
                        DestinationLng = c.Decimal(precision: 18, scale: 2),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        IsSoftDeleted = c.Boolean(nullable: false),
                        Bus_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Buses", t => t.Bus_Id)
                .Index(t => t.Bus_Id);
            
            CreateTable(
                "dbo.Feedbacks",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Message = c.String(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Users",
                c => new
                    {
                        Id = c.String(nullable: false, maxLength: 128),
                        PasswordHash = c.String(nullable: false),
                        PasswordSalt = c.String(nullable: false),
                        Username = c.String(nullable: false),
                        Email = c.String(nullable: false),
                        Phone = c.String(nullable: false),
                        FullName = c.String(nullable: false),
                        LockedOut = c.Boolean(nullable: false),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        EmailConfirmed = c.Boolean(nullable: false),
                        IsSoftDeleted = c.Boolean(nullable: false),
                        ProfileImage_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Media", t => t.ProfileImage_Id)
                .Index(t => t.ProfileImage_Id);
            
            CreateTable(
                "dbo.UserClaims",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        ClaimType = c.String(nullable: false),
                        Value = c.String(),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.BookReservations",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        RefNo = c.String(nullable: false),
                        Cancelled = c.Boolean(nullable: false),
                        RouteId = c.Long(nullable: false),
                        Seats = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Routes", t => t.RouteId, cascadeDelete: true)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.RouteId)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Transactions",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Type = c.Int(nullable: false),
                        Status = c.Int(nullable: false),
                        IdealAmount = c.Double(nullable: false),
                        ChargedAmount = c.Double(nullable: false),
                        FinalAmount = c.Double(nullable: false),
                        RefLocal = c.String(),
                        RefExternal = c.String(),
                        Meta = c.String(),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        Reservation_Id = c.Long(),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.BookReservations", t => t.Reservation_Id)
                .Index(t => t.Reservation_Id);
            
            CreateTable(
                "dbo.UserTokens",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Token = c.String(),
                        AccessFailedCount = c.Int(nullable: false),
                        TokenType = c.Int(nullable: false),
                        DispatchAfter = c.DateTime(),
                        LockoutEndDate = c.DateTime(),
                        DateCreated = c.DateTime(nullable: false),
                        LastUpdated = c.DateTime(nullable: false),
                        User_Id = c.String(nullable: false, maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id, cascadeDelete: true)
                .Index(t => t.User_Id);
            
            CreateTable(
                "dbo.Wallets",
                c => new
                    {
                        Id = c.Long(nullable: false, identity: true),
                        Provider = c.String(nullable: false),
                        Value = c.String(nullable: false),
                        User_Id = c.String(maxLength: 128),
                    })
                .PrimaryKey(t => t.Id)
                .ForeignKey("dbo.Users", t => t.User_Id)
                .Index(t => t.User_Id);
            
        }
        
        public override void Down()
        {
            DropForeignKey("dbo.Wallets", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserTokens", "User_Id", "dbo.Users");
            DropForeignKey("dbo.BookReservations", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Transactions", "Reservation_Id", "dbo.BookReservations");
            DropForeignKey("dbo.BookReservations", "RouteId", "dbo.Routes");
            DropForeignKey("dbo.Users", "ProfileImage_Id", "dbo.Media");
            DropForeignKey("dbo.Feedbacks", "User_Id", "dbo.Users");
            DropForeignKey("dbo.UserClaims", "User_Id", "dbo.Users");
            DropForeignKey("dbo.Routes", "Bus_Id", "dbo.Buses");
            DropForeignKey("dbo.Buses", "ProfileImage_Id", "dbo.Media");
            DropIndex("dbo.Wallets", new[] { "User_Id" });
            DropIndex("dbo.UserTokens", new[] { "User_Id" });
            DropIndex("dbo.Transactions", new[] { "Reservation_Id" });
            DropIndex("dbo.BookReservations", new[] { "User_Id" });
            DropIndex("dbo.BookReservations", new[] { "RouteId" });
            DropIndex("dbo.UserClaims", new[] { "User_Id" });
            DropIndex("dbo.Users", new[] { "ProfileImage_Id" });
            DropIndex("dbo.Feedbacks", new[] { "User_Id" });
            DropIndex("dbo.Routes", new[] { "Bus_Id" });
            DropIndex("dbo.Buses", new[] { "ProfileImage_Id" });
            DropTable("dbo.Wallets");
            DropTable("dbo.UserTokens");
            DropTable("dbo.Transactions");
            DropTable("dbo.BookReservations");
            DropTable("dbo.UserClaims");
            DropTable("dbo.Users");
            DropTable("dbo.Feedbacks");
            DropTable("dbo.Routes");
            DropTable("dbo.Media");
            DropTable("dbo.Buses");
        }
    }
}
