using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Services.SMS
{
    public class TwilioClient : ISMSService
    {
        private string accountSID;
        private string authToken;
        private string dispatchContact;

        public TwilioClient(string accountSID , string authToken, string contact)
        {
            this.accountSID = accountSID;
            this.authToken = authToken;
            this.dispatchContact = contact;
        }

        public Task SendAsync(SendSMSOptions options)
        {
            return Task.FromResult(0);
        }
    }
}