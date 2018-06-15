using BookingSystem.API.Services.Email;
using BookingSystem.API.Services.SMS;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Threading.Tasks;
using System.Web.Http;

namespace BookingSystem.API.Controllers
{
    [RoutePrefix("api/test")]
    public class TestController : BaseController
    {
        public class SMSModel
        {
            public string Phone { get; set; }

            public string Content { get; set; }
        }

        public class EmailModel
        {
            public string Email { get; set; }

            public string Content { get; set; }

            public string Subject { get; set; }
        }

        private IEmailService emailService;

        private ISMSService smsService;

        public TestController(IEmailService email, ISMSService sms)
        {
            this.smsService = sms;
            this.emailService = email;
        }

        [HttpPost]
        [Route("sms")]
        public async Task SendSMS([FromBody]SMSModel model)
        {
            await smsService.SendAsync(new SendSMSOptions()
            {
                Destinations = new List<string>()
                {
                    model.Phone,
                },
                Message = model.Content
            });
        }

        [HttpPost]
        [Route("mail")]
        public async Task SendEmail([FromBody]EmailModel model)
        {
            await emailService.SendAsync(new SendMailOptions()
            {
                Destinations = new List<string>() { model.Email },
                IsHtml = false,
                Message = model.Content,
                Subject = model.Subject ?? "Test Email"
            });
        }
    }
}
