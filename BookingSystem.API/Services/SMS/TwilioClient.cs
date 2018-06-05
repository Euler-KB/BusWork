using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using Twilio.Rest.Api.V2010.Account;
using Twilio.Types;

namespace BookingSystem.API.Services.SMS
{
    public class TwilioClient : ISMSService
    {
        private string accountSID;
        private string authToken;
        private PhoneNumber dispatchContact;

        public TwilioClient(string accountSID, string authToken, string contact)
        {
            this.accountSID = accountSID;
            this.authToken = authToken;
            this.dispatchContact = new PhoneNumber(contact);

            //  #
            Twilio.TwilioClient.Init(accountSID, authToken);
        }

        public Task SendAsync(SendSMSOptions options)
        {
            var from = options.Subject != null ? new PhoneNumber(options.Subject) : dispatchContact;
            return Task.WhenAll(options.Destinations.Select(x => MessageResource.CreateAsync(new CreateMessageOptions(new PhoneNumber(x))
            {
                Body = options.Message,
                From = from
            })));
        }

    }
}