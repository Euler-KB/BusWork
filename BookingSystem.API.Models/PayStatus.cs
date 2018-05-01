using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public enum PayStatus
    {
        /// <summary>
        /// Item has been successfully paid for
        /// </summary>
        Paid,

        /// <summary>
        /// Item failed payment due to insuficient funds or other reasons
        /// </summary>
        Failed,

        /// <summary>
        /// Item payment has been initiated, waiting for confirmation
        /// </summary>
        InitiatePay,
        
        /// <summary>
        /// Item refund has been initiated
        /// </summary>
        InitiateRefund,

        /// <summary>
        /// Reservation has been refunded
        /// </summary>
        Refunded
    }
}
