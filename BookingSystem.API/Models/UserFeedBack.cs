using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("Feedbacks")]
    public class UserFeedBack : IIdentifiable<long>, ITimestamp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        public virtual AppUser User { get; set; }

        [Required]
        public string Message { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }
}