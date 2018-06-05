using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using BookingSystem.API.Models;
using System.Security.Cryptography;
using System.Net.Http;
using System.Text;
using System.Net.Http.Headers;
using Newtonsoft.Json;

namespace BookingSystem.API.Services.Payment
{
    /// <summary>
    /// Apps & Mobile Solution Payment service
    /// </summary>
    public class AMSPaymentService : IPaymentService
    {

        class ChargeResponse
        {
            [JsonProperty("statusCode")]
            public string StatusCode { get; set; }

            [JsonProperty("statusMeessage")]
            public string Message { get; set; }
        }

        class ChargeInfo
        {
            public string OrderTxnRef { get; set; }
            public int TxnMode { get; set; }
            public string TxnType { get; set; }
            public string PhoneNumber { get; set; }
            public string TxnWallet { get; set; }
            public string MerchantTxnRef { get; set; }
            public double Amount { get; set; }
        }

        private string appId;

        private string apiKey;

        public bool CanRefund => false;

        public AMSPaymentService(string appId, string apiKey)
        {
            this.appId = appId;
            this.apiKey = apiKey;
        }

        public Task<double> CalculateCharges(double amount, UserWallet wallet, TransactionType type)
        {
            return Task.FromResult(0D);
        }

        public string ResolveOperator(UserWallet wallet)
        {
            if (wallet.Provider.IndexOf("tigo", StringComparison.OrdinalIgnoreCase) >= 0)
                return "TIG";
            else if (wallet.Provider.IndexOf("airtel", StringComparison.OrdinalIgnoreCase) >= 0)
                return "AIR";
            else if (wallet.Provider.IndexOf("mtn", StringComparison.OrdinalIgnoreCase) >= 0)
                return "MTN";

            return null;

        }

        private string CreateToken(string message, string secret)
        {
            secret = secret ?? "";
            var encoding = new ASCIIEncoding();
            byte[] keyByte = encoding.GetBytes(secret);
            byte[] messageBytes = encoding.GetBytes(message);
            using (var hmacsha256 = new HMACSHA256(keyByte))
            {
                byte[] hashmessage = hmacsha256.ComputeHash(messageBytes);
                return Convert.ToBase64String(hashmessage);
            }
        }


        private static string FormatPhoneNumber(string phone)
        {
            if (phone.StartsWith("233"))
            {
                return $"0{phone.Substring(3)}".Trim();
            }
            else if (phone.StartsWith("+233"))
            {
                return $"0{phone.Substring(4)}".Trim();
            }

            return phone;
        }

        public async Task<Transaction> Charge(ChargeOptions options, UserWallet wallet)
        {
            HttpResponseMessage response = null;
            string requestContentBase64String = string.Empty;
            string requestUri = HttpUtility.UrlEncode("");
            HttpRequestMessage request = new HttpRequestMessage(HttpMethod.Post, requestUri);

            string requestHttpMethod = request.Method.Method;

            //Calculate UNIX time
            DateTime epochStart = new DateTime(1970, 1, 1, 0, 0, 0, 0, DateTimeKind.Utc);
            TimeSpan timeSpan = DateTime.UtcNow - epochStart;
            string requestTimeStamp = Convert.ToUInt64(timeSpan.TotalSeconds).ToString();

            //create random nonce for each request
            string nonce = Guid.NewGuid().ToString("N");
            string merchantRef = $"BST-{Guid.NewGuid().ToString("N")}";

            ChargeInfo cInfo = new ChargeInfo()
            {
                Amount = options.Amount,
                TxnMode = 1,
                TxnType = "DR",
                MerchantTxnRef = merchantRef,
                OrderTxnRef = options.RefLocal,
                PhoneNumber = FormatPhoneNumber(wallet.Value),
                TxnWallet = ResolveOperator(wallet)
            };

            string rawContent = JsonConvert.SerializeObject(cInfo);
            request.Content = new StringContent(rawContent, Encoding.UTF8, "application/json");

            //Checking if the request contains body, usually will be null with HTTP GET and DELETE
            if (request.Content != null)
            {
                var stringToHash = await request.Content.ReadAsStringAsync();
                System.Diagnostics.Debug.WriteLine($"Request Content: {stringToHash}");

                var hasher = MD5.Create();
                var HashValue = hasher.ComputeHash(Encoding.ASCII.GetBytes(stringToHash));
                requestContentBase64String = string.Join("", HashValue.Select(b => b.ToString("x2")));
            }

            //Creating the raw signature string
            string signatureRawData = String.Format("{0}{1}{2}{3}{4}{5}", appId, requestHttpMethod, requestUri, requestTimeStamp, nonce, requestContentBase64String);

            string requestSignatureBase64String = CreateToken(signatureRawData, apiKey);

            string authHeader = string.Format("{0}:{1}:{2}:{3}", appId, requestSignatureBase64String, nonce, requestTimeStamp);
            request.Headers.Authorization = new AuthenticationHeaderValue("amx", authHeader);

            try
            {

                using (var client = new HttpClient())
                {
                    response = await client.SendAsync(request);

                    //  get charge response from server response
                    ChargeResponse chargeResponse = JsonConvert.DeserializeObject<ChargeResponse>(await response.Content.ReadAsStringAsync());

                    return new Transaction()
                    {
                        Type = TransactionType.Charge,
                        ChargedAmount = 0,
                        Status = (response.IsSuccessStatusCode && chargeResponse?.StatusCode == "010") ? TransactionStatus.Successful : TransactionStatus.Failed,
                        RefLocal = options.RefLocal,
                        Wallet = wallet,
                        IdealAmount = options.Amount,
                        FinalAmount = options.Amount,
                        DateCompleted = DateTime.UtcNow,
                        Message = chargeResponse.Message,
                        RefExternal = merchantRef,
                    };
                }
            }
            catch (HttpRequestException)
            {
                return new Transaction()
                {
                    Status = TransactionStatus.Failed,
                    Message = "An exception occured while processing transaction",
                    Wallet = wallet
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