using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public static class Extensions
    {
        public static bool HasPassword(this AppUser user, string password)
        {
            return PasswordHelpers.AreEqual(user.PasswordHash, user.PasswordSalt, password);
        }

        public static void SetPassword(this AppUser user, string password)
        {
            string salt;
            user.PasswordHash = PasswordHelpers.HashPassword(password,out salt);
            user.PasswordSalt = salt;
        }
    }
}