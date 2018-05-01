using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class ChangeEmailModel
    {
        [Required]
        public string Token { get; set; }

        [Required]
        public string Email { get; set; }
    }
}
