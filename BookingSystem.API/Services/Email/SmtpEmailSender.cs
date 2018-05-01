using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Services.Email
{
    public class SmtpEmailSender : IEmailService
    {
        private string sourceEmail;
        private string password;
        private string hostAddress;
        private int hostPort;
        private SmtpClient client;

        public SmtpEmailSender(string sourceEmail, string password, string host, int hostPort)
        {
            this.sourceEmail = sourceEmail;
            this.password = password;
            this.hostAddress = host;
            this.hostPort = hostPort;

            //
            client = new SmtpClient(this.hostAddress, this.hostPort);
            client.UseDefaultCredentials = false;
            client.EnableSsl = true;
            client.Credentials = new NetworkCredential(this.sourceEmail, this.password);
        }

        public async Task SendAsync(SendMailOptions options)
        {
            MailMessage message = new MailMessage();
            message.IsBodyHtml = options.IsHtml;
            message.Body = options.Message;
            message.Subject = options.Subject;
            message.From = new MailAddress(this.sourceEmail);
            foreach (var email in options.Destinations)
                message.To.Add(email);

            await Task.Run(() =>
            {
                try
                {
                    client.Send(message);
                }
                catch(Exception)
                {

                    //  Trace failure sending mail

                }

            });
        }
    }
}