using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class UserDashboardModel
    {
        public class ReservationSpec<T>
        {
            public T Total { get; set; }

            public T Today { get; set; }

            public T Yesterday { get; set; }

            public T Month { get; set; }

            public T Week { get; set; }

            public T Active { get; set; }

            public T Used { get; set; }

            public T Pending { get; set; }
        }

        public class MoneySpec<T>
        {
            public T Total { get; set; }

            public T Today { get; set; }

            public T Yesterday { get; set; }

            public T Month { get; set; }

            public T Week { get; set; }

            public T Refunded { get; set; }
        }

        public ReservationSpec<long> Reservations { get; set; }

        public MoneySpec<double> Money { get; set; }
    }
}
