using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Services.Payment
{
    public interface IPaymentService
    {
        bool CanRefund { get; }

        Task<Transaction> Charge(ChargeOptions options, UserWallet wallet);

        Task<Transaction> Refund(Transaction txn);

        Task<Transaction> Transfer(Transaction txn, UserWallet destination, double amount);

        /// <summary>
        /// Calculate charges incurred for a transaction
        /// </summary>
        /// <param name="amount">The amount of money</param>
        /// <param name="wallet">The wallet to be used in the transaction</param>
        /// <param name="type">The type of transaction</param>
        Task<double> CalculateCharges(double amount, UserWallet wallet, TransactionType type);
    }
}
