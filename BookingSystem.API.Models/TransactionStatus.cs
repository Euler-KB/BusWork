using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Models
{
    public enum TransactionStatus
    {
        Initiated = 1,
        Successful = 2,
        Failed = -1,
        Unknown = 26
    }
}
