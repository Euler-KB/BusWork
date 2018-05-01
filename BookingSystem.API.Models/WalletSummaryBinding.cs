using BookingSystem.API.Models.DTO;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BookingSystem.API.Models
{
    public class WalletSummaryBinding
    {
        public WalletInfo Wallet { get; set; }

        public TransactionSummary Summary { get; set; }
    }
}
