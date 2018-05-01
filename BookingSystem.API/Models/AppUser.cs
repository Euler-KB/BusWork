using BookingSystem.API.Helpers;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Security.Claims;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("Users")]
    public class AppUser : IIdentifiable<string>, ITimestamp, ISoftDelete
    {
        [Key]
        public string Id { get; set; }

        [Required]
        public string PasswordHash { get; set; }

        [Required]
        public string PasswordSalt { get; set; }

        [Required]
        public string Username { get; set; }

        [Required]
        public string Email { get; set; }

        [Required]
        public string Phone { get; set; }

        [Required]
        public string FullName { get; set; }

        public bool LockedOut { get; set; }

        [InverseProperty("User")]
        public virtual IList<UserClaim> Claims { get; set; }

        [InverseProperty("User")]
        public virtual IList<UserFeedBack> Feedback { get; set; }

        [InverseProperty("User")]
        public virtual IList<BookReservation> Reservations { get; set; }

        public virtual IList<UserWallet> Wallets { get; set; }

        public virtual IList<UserToken> Tokens { get; set; }

        public virtual Media ProfileImage { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }

        public bool EmailConfirmed { get; set; }

        public bool IsSoftDeleted { get; set; }

        public static IEnumerable<Claim> GenerateAdminClaims(AppUser user)
        {
            return new Claim[]
            {
                new Claim(ClaimTypes.Role , UserRoles.Admin),
            };
        }

        public static IEnumerable<Claim> GenerateUserClaims(AppUser user)
        {
            return new Claim[]
            {
                new Claim(ClaimTypes.Role , UserRoles.User)
            };
        }

        public AppUser()
        {
            Claims = new List<UserClaim>();
            Feedback = new List<UserFeedBack>();
            Reservations = new List<BookReservation>();
            Wallets = new List<UserWallet>();
            Tokens = new List<UserToken>();
        }

        public static AppUser New(string password)
        {
            string salt;
            string hash = PasswordHelpers.HashPassword(password, out salt);
            return new AppUser()
            {
                PasswordHash = hash,
                PasswordSalt = salt,
                Id = Guid.NewGuid().ToString()
            };

        }

    }
}