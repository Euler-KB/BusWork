using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;

namespace BookingSystem.API.Models.DTO
{
    public class TransactionInfo
    {
        public long Id { get; set; }

        public string RefLocal { get; set; }

        public string RefExternal { get; set; }

        public TransactionStatus Status { get; set; }

        public TransactionType Type { get; set; }

        public double Amount { get; set; }

    }
}