using BookingSystem.API.Models;
using BookingSystem.API.Services.Payment.Models;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Services.Payment
{
    namespace Models
    {
        public static class NetworkProviders
        {
            public const string MTN = "mtn-gh";
            public const string Vodafone = "vodafone-gh";
            public const string Tigo = "tigo-gh";
            public const string Airtel = "airtel-gh";

            public static string[] AllProviders { get; } = new string[]
            {
                MTN,
                Vodafone,
                Tigo,
                Airtel
            };

        }

        public class ReceiveModel
        {
            public string CustomerName { get; set; }
            public string CustomerMsisdn { get; set; }
            public string CustomerEmail { get; set; }
            public string Channel { get; set; }
            public double Amount { get; set; }
            public string PrimaryCallbackUrl { get; set; }
            public string Description { get; set; }
            public string Token { get; set; }
            public string ClientReference { get; set; }
            public bool FeesOnCustomer { get; set; }
        }

        public class TransactionCycle
        {
            public DateTime Date { get; set; }
            public string Status { get; set; }
        }


        public class RefundModel
        {
            public string TransactionId { get; set; }
            public string Reason { get; set; }
            public string ClientReference { get; set; }
            public string Description { get; set; }
            public double Amount { get; set; }
            public bool Full { get; set; }
        }

        public class TransactionStatusModel
        {
            public DateTime StartDate { get; set; }
            public string TransactionStatus { get; set; }
            public string TransactionId { get; set; }
            public string NetworkTransactionId { get; set; }
            public string InvoiceToken { get; set; }
            public string TransactionType { get; set; }
            public string PaymentMethod { get; set; }
            public string ClientReference { get; set; }
            public string CountryCode { get; set; }
            public string CurrencyCode { get; set; }
            public double TransactionAmount { get; set; }
            public double Fee { get; set; }
            public double AmountAfterFees { get; set; }
            public object CardSchemeName { get; set; }
            public object CardNumber { get; set; }
            public string MobileNumber { get; set; }
            public string MobileChannelName { get; set; }
            public List<TransactionCycle> TransactionCycle { get; set; }
            public object RelatedTransactionId { get; set; }
            public object RelatedTransactionType { get; set; }
            public bool Disputed { get; set; }
            public int DisputedAmount { get; set; }
            public int DisputedAmountFee { get; set; }
            public int TotalAmountRefunded { get; set; }
        }

        public class SendModel
        {
            public string RecipientName { get; set; }
            public string RecipientMsisdn { get; set; }
            public string CustomerEmail { get; set; }
            public string Channel { get; set; }
            public double Amount { get; set; }
            public string PrimaryCallbackUrl { get; set; }
            public string SecondaryCallbackUrl { get; set; }
            public string Description { get; set; }
            public string ClientReference { get; set; }
        }

        public class ResponseModel
        {
            public class ResponseData
            {
                public double AmountAfterCharges { get; set; }
                public double AmountCharged { get; set; }
                public string TransactionId { get; set; }
                public string ClientReference { get; set; }
                public string Description { get; set; }
                public string ExternalTransactionId { get; set; }
                public int Amount { get; set; }
                public double Charges { get; set; }
            }

            [Required]
            public string ResponseCode { get; set; }

            public ResponseData Data { get; set; }
        }

    }

   

    public class HubtelPaymentService : IPaymentService
    {
        internal string BaseUrl = "https://api.hubtel.com/v1/merchantaccount";

        private string clientId;
        private string clientSecret;
        private string merchantAccountId;
        private HttpClient httpClient;

        public bool CanRefund => true;

        public HubtelPaymentService(string clientId, string secret, string merchantAccountNo)
        {
            this.clientId = clientId;
            this.clientSecret = secret;
            this.merchantAccountId = merchantAccountNo;

            //
            httpClient = new HttpClient() { BaseAddress = new Uri(BaseUrl, UriKind.Absolute) };
            httpClient.DefaultRequestHeaders.Authorization = new System.Net.Http.Headers.AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes($"{this.clientId}:{this.clientSecret}")));
        }

        protected string ResolveChannel(string networkOperator)
        {
            if (networkOperator.IndexOf("airtel", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NetworkProviders.Airtel;
            }
            else if (networkOperator.IndexOf("vodafone", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NetworkProviders.Vodafone;
            }
            else if (networkOperator.IndexOf("mtn", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NetworkProviders.MTN;
            }
            else if (networkOperator.IndexOf("tigo", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                return NetworkProviders.Tigo;
            }

            throw new InvalidOperationException("Cannot resolve network operator. Supported networks include Tigo, Airtel, MTN and Vodafone.");
        }

        public static bool IsSuccessfulResponse(string response)
        {
            return (response == "0001" || response == "0000");
        }

        private string FixPhone(string phone)
        {
            if (phone.StartsWith("0"))
            {
                return $"233{phone.Substring(1)}";
            }

            return phone;
        }

        public async Task<Transaction> Charge(ChargeOptions chargeOptions, UserWallet wallet)
        {
            var requestUri = HttpContext.Current.Request.Url;
            var response = await httpClient.PostAsJsonAsync($"merchants/{this.merchantAccountId}/send/mobilemoney", new ReceiveModel()
            {
                Amount = chargeOptions.Amount,
                Token = chargeOptions.AdditionalToken,
                CustomerEmail = chargeOptions.Email,
                Channel = ResolveChannel(wallet.Provider),
                ClientReference = chargeOptions.RefLocal,
                CustomerMsisdn = FixPhone(wallet.Value),
                CustomerName = chargeOptions.Name,
                Description = chargeOptions.Description ?? "Pay for booked reservation",
                PrimaryCallbackUrl = $"{requestUri.Scheme}://{requestUri.Authority}/{Handlers.HubtelPaymentHandler.CallbackUrl}",
                FeesOnCustomer = chargeOptions.FeesOnCustomer ?? true
            });

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsAsync<ResponseModel>();
                return new Transaction()
                {
                    ChargedAmount = body.Data.AmountCharged,
                    FinalAmount = body.Data.AmountAfterCharges,
                    IdealAmount = chargeOptions.Amount,
                    Message = body.Data.Description,
                    RefExternal = body.Data.TransactionId,
                    RefLocal = chargeOptions.RefLocal,
                    Status = IsSuccessfulResponse(body.ResponseCode) ? TransactionStatus.Initiated : TransactionStatus.Unknown,
                    Type = TransactionType.Charge,
                    Wallet = wallet
                };
            }
            else
            {
                return new Transaction()
                {
                    RefLocal = chargeOptions.RefLocal,
                    ChargedAmount = 0,
                    FinalAmount = 0,
                    IdealAmount = chargeOptions.Amount,
                    Status = TransactionStatus.Failed,
                    Type = TransactionType.Charge,
                    Message = $"Server didn't return successful status code: {response.ReasonPhrase}",
                    Meta = JsonConvert.SerializeObject(new
                    {
                        Response = await response.Content.ReadAsStringAsync(),
                        StatusCode = response.StatusCode
                    }),
                    Wallet = wallet
                };
            }

        }

        public async Task<Transaction> Refund(Transaction txn)
        {
            var response = await httpClient.PostAsJsonAsync($"merchants/{merchantAccountId}/transactions/refund", new RefundModel()
            {
                TransactionId = txn.RefExternal,
                Reason = "Cancel reservation",
                Description = "User cancelled booked reservation",
                ClientReference = txn.RefLocal,
                Full = true,
                Amount = txn.IdealAmount
            });

            if (response.IsSuccessStatusCode)
            {
                var body = await response.Content.ReadAsAsync<ResponseModel>();

                return new Transaction()
                {
                    Type = TransactionType.Refund,
                    Status = IsSuccessfulResponse(body.ResponseCode) ? TransactionStatus.Successful : TransactionStatus.Failed,
                    DateCompleted = DateTime.UtcNow,
                    Message = body.Data.Description,
                    IdealAmount = txn.IdealAmount,
                    ChargedAmount = 0,
                    FinalAmount = txn.IdealAmount,
                    Meta = JsonConvert.SerializeObject(new
                    {
                        Response = body

                    }, new JsonSerializerSettings() { NullValueHandling = NullValueHandling.Ignore })
                };

            }
            else
            {
                return new Transaction()
                {
                    Type = TransactionType.Refund,
                    Status = TransactionStatus.Failed,
                    Message = $"Server didn't return a successfull status code: {response.ReasonPhrase}",
                    Meta = JsonConvert.SerializeObject(new
                    {
                        Response = await response.Content.ReadAsStringAsync(),
                        Status = response.StatusCode
                    })
                };
            }
        }

        public Task<Transaction> Transfer(Transaction txn, UserWallet destination, double amount)
        {
            throw new NotImplementedException();
        }

        public async Task<double> CalculateCharges(double amount, UserWallet wallet, TransactionType type)
        {
            var body = new
            {
                channel = ResolveChannel(wallet.Provider),
                amount = amount
            };

            HttpResponseMessage response = null;
            switch (type)
            {
                case TransactionType.Charge:
                    response = await httpClient.PostAsJsonAsync($"merchants/{merchantAccountId}/charges/mobile/receive", body);
                    break;
                case TransactionType.Transfer:
                    response = await httpClient.PostAsJsonAsync($"merchants/{merchantAccountId}/charges/mobile/send", body);
                    break;
                default:
                    throw new InvalidOperationException("Unsupported transaction type");
            }

            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();

            }

            return 0;
        }
    }
}