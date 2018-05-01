using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models.DTO
{
    public class ReservationCostInfo
    {
        /// <summary>
        /// The cost for the reservation including the number of seats
        /// </summary>
        public double ReservationCost { get; set; }

        /// <summary>
        /// The additional charges for the reservation
        /// </summary>
        public double Charges { get; set; }

    }
}
