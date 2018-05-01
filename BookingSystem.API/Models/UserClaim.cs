using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("UserClaims")]
    public class UserClaim
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity),Key]
        public long Id { get; set; }
        
        [Required]
        public string ClaimType { get; set; }

        public string Value { get; set; }

        public virtual AppUser User { get; set; }
    }
}