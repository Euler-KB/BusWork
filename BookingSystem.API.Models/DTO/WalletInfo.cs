using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class WalletInfo
    {
        public long Id { get; set; }

        public string Provider { get; set; }

        public string Value { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class CreateWalletInfo
    {
        [Required]
        public string Provider { get; set; }

        [Required]
        public string Value { get; set; }
    }

    public class EditWalletInfo
    {
        public string Provider { get; set; }

        public string Value { get; set; }
    }
}