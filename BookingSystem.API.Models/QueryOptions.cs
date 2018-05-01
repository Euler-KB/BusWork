using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models
{
    public class QueryOptions
    {
        public string SearchKeyword { get; set; }

        public string SearchFields { get; set; }

        public int? Limit { get; set; }

        public int? Offset { get; set; }

        public DateTime ? CreatedBefore { get; set; }

        public DateTime ? CreatedAfter { get; set; }

        public DateTime ? FromRange { get; set; }

        public DateTime ? ToRange { get; set; }
    }
}