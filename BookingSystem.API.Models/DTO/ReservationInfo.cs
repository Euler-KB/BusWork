using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class ReservationInfo
    {
        public long Id { get; set; }

        public string ReferenceNo { get; set; }

        public string PickupLocation { get; set; }

        public RouteInfo Route { get; set; }

        public BusInfo Bus { get; set; }

        public string Seats { get; set; }

        public double Cost { get; set; }

        public PayStatus PayStatus { get; set; }

        public ReservationCategory Category { get; set; }

        public string UserId { get; set; }

        public string UserFullName { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class CreateReservationInfo
    {

        public long RouteId { get; set; }

        public string Seats { get; set; }

        public string PickupLocation { get; set; }

        /// <summary>
        /// The wallet to use for payment
        /// </summary>
        public long WalletId { get; set; }


        public string AdditionalToken { get; set; }
    }
}