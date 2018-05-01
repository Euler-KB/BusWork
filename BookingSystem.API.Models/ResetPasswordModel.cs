using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BookingSystem.API.Models
{
    public class ResetPasswordModel
    {
        [Required]
        public string UserIdentity { get; set; }

        [Required]
        public string Token { get; set; }

        [Required]
        [StringLength(16, MinimumLength = 6)]
        public string NewPassword { get; set; }
    }
}