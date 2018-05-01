using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API
{
    public static class TokenAudiences
    {
        public const string Universal = "http://booking.system.com/*";

        public const string RefreshToken = "http://booking.system.com/refresh/token";

        public const string ResetPassword = "http://booking.system.com/api/account/resetpassword";

        public const string ActivateAccount = "http://booking.system.com/api/account/activate";
    }
}