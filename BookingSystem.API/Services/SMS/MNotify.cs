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
        static readonly string BaseUrl = "https://apps.mnotify.net/smsapi";

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
                    string contentEncoded = HttpUtility.UrlEncode(options.Message);
                    string msgSubject = options.Subject ?? subject;
                    string url = $"{BaseUrl}?key={key}&to={phone}&msg={contentEncoded}&sender_id={msgSubject}";
                    await httpClient.GetAsync(url);
                }

            }
        }
    }
}