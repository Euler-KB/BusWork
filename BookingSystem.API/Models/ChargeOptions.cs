using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    public class ChargeOptions
    {
        public double Amount { get; set; }

        public string Email { get; set; }

        public string Name { get; set; }

        public bool? FeesOnCustomer { get; set; }

        public string AdditionalToken { get; set; }

        public string Description { get; set; }

        public string RefLocal { get; set; }

        public long TotalSeats { get; set; }

        public double UnitSeatCost { get; set; }

        public double GatewayCharges { get; set; }
    }
}