using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    public class BookReservation : IIdentifiable<long>, ITimestamp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        [Column("RefNo")]
        [Required]
        public string ReferenceNo { get; set; }

        public string PickupLocation { get; set; }

        public bool Cancelled { get; set; }

        public virtual AppUser User { get; set; }

        [ForeignKey("Route")]
        public long RouteId { get; set; }

        public virtual BusRoute Route { get; set; }

        public string Seats { get; set; }

        public virtual IList<Transaction> Transactions { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }

        public BookReservation()
        {
            Transactions = new List<Transaction>();
        }
    }
}