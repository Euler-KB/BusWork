using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Configuration;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Text;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("UserTokens")]
    public class UserToken : IIdentifiable<long>, ITimestamp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        public virtual AppUser User { get; set; }

        public string Token { get; set; }

        public int AccessFailedCount { get; set; }

        public TokenType TokenType { get; set; }

        public DateTime? DispatchAfter { get; set; }

        public DateTime? LockoutEndDate { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }

    }

    public class UserTokenConfig : EntityTypeConfiguration<UserToken>
    {
        public UserTokenConfig()
        {
            HasRequired(x => x.User);
        }
    }
}