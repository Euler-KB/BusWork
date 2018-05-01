using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public static class PasswordHelpers
    {
        const int MaxIterations = 1000;
        const int SaltSize = 32;
        const int PwdSize = 32;


        static string GenerateSalt(int size, out byte[] rawSalt)
        {
            rawSalt = new byte[size];
            using (var rng = new System.Security.Cryptography.RNGCryptoServiceProvider())
            {
                rng.GetBytes(rawSalt);
            }

            return Convert.ToBase64String(rawSalt);
        }

        public static string HashPassword(string plainPassword, out string salt)
        {
            byte[] rawSalt;
            salt = GenerateSalt(SaltSize, out rawSalt);

            using (var rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(plainPassword, rawSalt))
            {
                rfc.IterationCount = MaxIterations;
                return Convert.ToBase64String(rfc.GetBytes(PwdSize));
            }

        }

        public static bool AreEqual(string hashedPassword, string salt, string plainPassword)
        {
            using (var rfc = new System.Security.Cryptography.Rfc2898DeriveBytes(plainPassword, Convert.FromBase64String(salt)))
            {
                rfc.IterationCount = MaxIterations;
                return rfc.GetBytes(PwdSize).SequenceEqual(Convert.FromBase64String(hashedPassword));
            }

        }
    }
}