using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class RouteInfo
    {
        public long Id { get; set; }

        public string Reference { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string From { get; set; }

        public decimal? FromLat { get; set; }

        public decimal? FromLng { get; set; }

        public double Cost { get; set; }

        public string Comments { get; set; }

        public string Destination { get; set; }

        public decimal? DestinationLat { get; set; }

        public decimal? DestinationLng { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }

    public class CreateRouteInfo
    {
        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public string From { get; set; }

        public decimal? FromLat { get; set; }

        public decimal? FromLng { get; set; }

        public double Cost { get; set; }

        public string Comments { get; set; }

        public string Destination { get; set; }

        public decimal? DestinationLat { get; set; }

        public decimal? DestinationLng { get; set; }
    }

    public class EditRouteInfo : CreateRouteInfo
    {
        public new DateTime ? ArrivalTime { get; set; }

        public new DateTime ? DepartureTime { get; set; }

        public new double ? Cost { get; set; }
        
    }
}