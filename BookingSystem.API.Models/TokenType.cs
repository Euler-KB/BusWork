using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Models
{
    public enum TokenType
    {
        PasswordReset,
        ChangeEmail,
        ChangePhone,
        ActivateAccount
    }
}
