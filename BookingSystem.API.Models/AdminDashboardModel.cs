using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class AdminDashboardModel
    {
        public class ReservationSpec<T> where T : struct
        {
            public T Total { get; set; }

            public T Today { get; set; }

            public T Yesterday { get; set; }

            public T Week { get; set; }

            public T Month { get; set; }

            public T Used { get; set; }

            public T Active { get; set; }

            public T Pending { get; set; }
        }

        public class MoneySpec<T> where T: struct
        {
            public T Total { get; set; }

            public T Refunded { get; set; }

            public T Today { get; set; }

            public T Yesterday { get; set; }

            public T Week { get; set; }

            public T Month { get; set; }

        }

        public class BusSpec
        {
            public long Total { get; set; }

            public long TotalActive { get; set; }

            public long ActiveToday { get; set; }

            public long ActiveYesterday { get; set; }

            public long ActiveWeek { get; set; }

            public long CompleteWeek { get; set; }

            public long CompleteToday { get; set; }

            public long CompleteYesterday { get; set; }

            public long TotalIdle { get; set; }

            public long IdleToday { get; set; }

        }

        public class RoutesSpec
        {
            public long Total { get; set; }
        }

        public class UsersSpec
        {
            public long TotalUsers { get; set; }

            public long RegisteredToday { get; set; }

            public long RegisteredYesterday { get; set; }

            public long RegisteredWeek { get; set; }

            public long RegisteredMonth { get; set; }

            public long PendingActivation { get; set; }
        }

        public ReservationSpec<long> Reservations { get; set; }

        public MoneySpec<double> Money { get; set; }

        public BusSpec Buses { get; set; }

        public RoutesSpec Routes { get; set; }

        public UsersSpec Users { get; set; }
    }
}
