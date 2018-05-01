using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class UserInfo
    {
        public string Id { get; set; }

        public string Username { get; set; }

        public string Email { get; set; }

        public string FullName { get; set; }

        public string Phone { get; set; }

        public AccountType AccountType { get; set; }

        public MediaInfo ProfileImage { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class UpdateUserInfo
    {
        public string Username { get; set; }

        public string FullName { get; set; }

    }
}
