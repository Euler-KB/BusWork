using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models.DTO
{
    public class RouteSummary
    {
        public int TotalReservations { get; set; }

        public int[] BookedSeats { get; set; }
    }
}
