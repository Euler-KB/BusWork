using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Data.Entity.ModelConfiguration;
using System.Linq;
using System.Web;

namespace BookingSystem.API.Models
{
    public class Transaction : IIdentifiable<long>, ITimestamp
    {
        [DatabaseGenerated(DatabaseGeneratedOption.Identity), Key]
        public long Id { get; set; }

        public TransactionType Type { get; set; }

        public TransactionStatus Status { get; set; }

        public double IdealAmount { get; set; }

        public double ChargedAmount { get; set; }

        public double FinalAmount { get; set; }

        /// <summary>
        /// The wallet used for this transaction
        /// </summary>
        public virtual UserWallet Wallet { get; set; }

        /// <summary>
        /// Local refernce
        /// </summary>
        public string RefLocal { get; set; }

        /// <summary>
        /// External reference
        /// </summary>
        public string RefExternal { get; set; }

        public string Message { get; set; }

        /// <summary>
        /// Additional meta data for transaction
        /// </summary>
        public string Meta { get; set; }

        public virtual BookReservation Reservation { get; set; }

        public DateTime ? DateCompleted { get; set; }

        public DateTime DateCreated { get; set; }

        public DateTime LastUpdated { get; set; }
    }
    
    public class TransactionConfig : EntityTypeConfiguration<Transaction>
    {
        public TransactionConfig()
        {
            HasRequired(x => x.Wallet);
            HasRequired(x => x.Reservation);
        }
    }
}