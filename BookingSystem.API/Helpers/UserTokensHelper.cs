using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Helpers
{
    public static class UserTokensHelper
    {
        public static string Generate(int length = 6)
        {
            return Guid.NewGuid().ToString("N").Substring(0, length);
        }
    }
}