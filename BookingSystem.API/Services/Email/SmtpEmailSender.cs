using FluentEmail.Smtp;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Net.Mail;
using System.Threading.Tasks;
using System.Web;
using System.Web.Hosting;

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

            FluentEmail.Core.Email.DefaultSender = new SmtpSender(client);

        }

        public async Task SendAsync(SendMailOptions options)
        {

            var mail = global::FluentEmail.Core.Email.From(this.sourceEmail)
                .To(options.Destinations.Select(x => new FluentEmail.Core.Models.Address(x)).ToList())
                .Body(options.Message)
                .Subject(options.Subject);

            if (options.IsHtml)
                mail.BodyAsHtml();
            else
                mail.BodyAsPlainText();
                

            await Task.Run(async () =>
            {
                try
                {
                    await mail.SendAsync();
                }
                catch (Exception ex)
                {

                    //  Trace failure sending mail
                    File.WriteAllText(HostingEnvironment.MapPath("~/App_Data/MailError.txt"), ex.ToString());

                }

            });
        }
    }
}