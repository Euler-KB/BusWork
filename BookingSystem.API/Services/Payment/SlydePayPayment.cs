using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BookingSystem.API.Models;
using iWalletPayliveModule.gh.com.slydepay.app;
using System.Web.Http;

namespace BookingSystem.API.Services.Payment
{
    public class SlydePayPayment : IPaymentService
    {
        private PaymentService client;

        protected bool IsIntegrationMode { get; }

        public SlydePayPayment(string apiVersion, string merchantEmail, string apiKey, bool integrationMode, string serviceType = "C2B")
        {
            client = new PaymentService();

            IsIntegrationMode = integrationMode;

            if (client.PaymentHeaderValue == null)
                client.PaymentHeaderValue = new PaymentHeader();

            var header = client.PaymentHeaderValue;
            header.APIVersion = apiVersion;
            header.MerchantEmail = merchantEmail;
            header.MerchantKey = apiKey;
            header.UseIntMode = integrationMode;
            header.SvcType = serviceType;

        }

        public bool CanRefund => false;

        public Task<double> CalculateCharges(double amount, UserWallet wallet, TransactionType type)
        {
            return Task.FromResult(0D);
        }

        public async Task<Transaction> Charge(ChargeOptions options, UserWallet wallet)
        {
            var orderItems = new OrderItem[]
            {
                new OrderItem()
                {
                    ItemCode = "TKT",
                    ItemName = "Bus Ticket",
                    Quantity = (int)options.TotalSeats,
                    UnitPrice = (decimal)options.UnitSeatCost,
                    SubTotal = (decimal)options.Amount
                }
            };

            try
            {

                if (IsIntegrationMode)
                {
                    return new Transaction()
                    {
                        ChargedAmount = 0,
                        FinalAmount = options.Amount,
                        IdealAmount = options.Amount,
                        RefExternal = Guid.NewGuid().ToString("N"),
                        Message = "Payment was successfull",
                        Status = TransactionStatus.Successful,
                        DateCompleted = DateTime.UtcNow,
                        Wallet = wallet,
                        RefLocal = options.RefLocal,
                        Type = TransactionType.Charge,
                    };
                }
                else
                {

                    var result = await Task.Run(() => client.mobilePaymentOrder(options.RefLocal, (decimal)options.Amount, true, 0, true, 0, true, (decimal)options.Amount, true, "Payment for bus ticket", "", orderItems));

                    return new Transaction()
                    {
                        ChargedAmount = 0,
                        FinalAmount = options.Amount,
                        IdealAmount = options.Amount,
                        RefExternal = result.token,
                        Message = result.status,
                        Status = result.success ? TransactionStatus.Initiated : TransactionStatus.Failed,
                        DateCompleted = result.success ? (DateTime?)null : DateTime.UtcNow,
                        Wallet = wallet,
                        RefLocal = options.RefLocal,
                        Type = TransactionType.Charge,
                    };
                }

            }
            catch (Exception ex)
            {
                return new Transaction()
                {
                    DateCompleted = DateTime.UtcNow,
                    ChargedAmount = 0,
                    FinalAmount = 0,
                    IdealAmount = options.Amount,
                    RefExternal = null,
                    RefLocal = options.RefLocal,
                    Wallet = wallet,
                    Message = ex.Message,
                    Status = TransactionStatus.Failed,
                    Type = TransactionType.Charge
                };
            }
        }

        public Task<Transaction> Refund(Transaction txn)
        {
            throw new NotSupportedException();
        }

        public Task<Transaction> Transfer(Transaction txn, UserWallet destination, double amount)
        {
            throw new NotSupportedException();
        }
    }
}