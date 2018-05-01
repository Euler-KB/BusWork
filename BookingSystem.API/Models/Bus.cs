using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    [Table("Buses")]
    public class Bus : IIdentifiable<long>, ITimestamp, ISoftDelete
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        [Required]
        public string Name { get; set; }

        public string Model { get; set; }

        public virtual Media ProfileImage { get; set; }

        public string Seats { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }

        [InverseProperty("Bus")]
        public virtual IList<BusRoute> Routes { get; set; }

        public bool IsSoftDeleted { get; set; }

        public Bus()
        {
            Routes = new List<BusRoute>();
        }
    }


}