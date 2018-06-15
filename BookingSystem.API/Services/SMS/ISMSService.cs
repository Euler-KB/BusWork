using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Services.SMS
{
    public class SendSMSOptions
    {
        public string Subject { get; set; }

        public string Message { get; set; }

        public IList<string> Destinations { get; set; }
    }

    public interface ISMSService
    {
        Task SendAsync(SendSMSOptions options);
    }

#if DEBUG

    public class DebugSMSService : ISMSService
    {
        public Task SendAsync(SendSMSOptions options)
        {
            Debug.WriteLine($"Send '{options.Message}' from {options.Subject ?? "Unspecified"} to {string.Join(",", options.Destinations)} ");
            return Task.FromResult(0);
        }
    }

#endif

}
