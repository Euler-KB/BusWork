using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class PaymentDetails
    {
        public long WalletId { get; set; }
        
        public string AdditionalToken { get; set; }
    }
}
