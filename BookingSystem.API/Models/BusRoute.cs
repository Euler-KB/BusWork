using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("Routes")]
    public class BusRoute : IIdentifiable<long>, ITimestamp , ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        [Required]
        public string From { get; set; }

        public DateTime DepartureTime { get; set; }

        public DateTime ArrivalTime { get; set; }

        public decimal? FromLat { get; set; }

        public decimal? FromLng { get; set; }

        public double Cost { get; set; }

        public string Comments { get; set; }

        [Required]
        public string Destination { get; set; }

        public decimal? DestinationLat { get; set; }

        public decimal? DestinationLng { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }

        public virtual Bus Bus { get; set; }

        public bool IsSoftDeleted { get; set; }
    }

    public class BusRouteConfig : EntityTypeConfiguration<BusRoute>
    {
        public BusRouteConfig()
        {
            Property(x => x.Comments).IsOptional();
        }
    }
}