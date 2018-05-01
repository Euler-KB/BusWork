using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class ReservationCategoryBinding
    {
        public long ReservationId { get; set; }

        public ReservationCategory Category { get; set; }
    }
}
