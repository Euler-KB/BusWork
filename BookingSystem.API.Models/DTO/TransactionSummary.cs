using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models.DTO
{
    public class TransactionSummary
    {
        public double TotalAmountReceived { get; set; }

        public double TotalRefundedAmount { get; set; }

    }
}
