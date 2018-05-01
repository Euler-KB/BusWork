using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("Wallets")]
    public class UserWallet : IIdentifiable<long> , ITimestamp , ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        [Required]
        public string Provider { get; set; }

        [Required]
        public string Value { get; set; }

        public virtual AppUser User { get; set; }

        public bool IsSoftDeleted { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}