using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Services.SMS
{
    public class MNotify : ISMSService
    {
        private string key;
        private string subject;

        public MNotify(string key, string defaultSubject = "BusWork")
        {
            this.key = key;
            this.subject = defaultSubject;
        }

        public async Task SendAsync(SendSMSOptions options)
        {
            using (var httpClient = new HttpClient())
            {
                foreach (var phone in options.Destinations)
                {

                    await httpClient.GetAsync($"https://apps.mnotify.net/smsapi?key={key}&to={phone}&msg={HttpUtility.UrlEncode(options.Message)}&sender_id={(options.Subject ?? subject)}");
                }

            }
        }
    }
}