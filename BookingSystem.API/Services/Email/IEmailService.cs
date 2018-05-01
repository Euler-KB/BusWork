using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Services.Email
{
    public class SendMailOptions
    {
        public string Subject { get; set; }

        public IList<string> Destinations { get; set; }

        public string Message { get; set; }

        public bool IsHtml { get; set; }
    }

    public interface IEmailService
    {
        Task SendAsync(SendMailOptions options);
    }
}
