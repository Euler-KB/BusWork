using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class BusInfo
    {
        public long Id { get; set; }

        public string Name { get; set; }

        public string Model { get; set; }

        public string Seats { get; set; }

        public MediaInfo Photo { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }


    }

    public class CreateBusInfo
    {
        public string Name { get; set; }

        public string Seats { get; set; }

        public string Model { get; set; }
    }

    public class EditBusInfo
    {
        public string Name { get; set; }

        public string Model { get; set; }

        public string Seats { get; set; }
    }
}