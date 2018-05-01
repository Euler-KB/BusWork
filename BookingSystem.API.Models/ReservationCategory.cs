using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public enum ReservationCategory
    {
        /// <summary>
        /// Reservation is currently active. Current time within departure and arrival time
        /// </summary>
        Active,

        /// <summary>
        /// Reservation is pending. Departure time not yet reached
        /// </summary>
        Pending,
        
        /// <summary>
        /// Reservation is cancelled
        /// </summary>
        Cancelled,

        /// <summary>
        /// The reservation is completed due to 
        /// </summary>
        Completed

    }
}
