using System;
using System.Collections.Generic;
using System.Linq;
using System.Web;
using BookingSystem.API.Models;
using System.Data.Entity;
using System.Web.Http;
using Microsoft.AspNet.SignalR;
using BookingSystem.API.Hubs;
using System.Threading.Tasks;

namespace BookingSystem.API.Services.Payment
{
    public class AdminPaymentNotification : IPaymentNotification
    {
        private Services.SMS.ISMSService SMSService
        {
            get
            {
                return (SMS.ISMSService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(SMS.ISMSService));
            }
        }

        private Services.Email.IEmailService EmailService
        {
            get
            {
                return (Email.IEmailService)GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(Email.IEmailService));
            }
        }

        public Task OnPaymentFail(BookReservation reservation, Transaction transaction)
        {
            //  Do nothing
            return Task.FromResult(0);
        }

        public async Task OnPaymentSuccessul(BookReservation reseravtion, Transaction transaction)
        {
            using (var db = new BookingContext())
            {
                var users = await db.Users.Where(x => !x.LockedOut && x.Claims.FirstOrDefault().Value == UserRoles.Admin).Select(x => new { x.Phone, x.Email, x.PhoneConfirmed, x.EmailConfirmed }).ToListAsync();

                var phoneTargets = users.Where(x => x.PhoneConfirmed);
                if (phoneTargets.Count() > 0)
                {
                    await SMSService.SendAsync(new SMS.SendSMSOptions()
                    {
                        Destinations = phoneTargets.Select(x => x.Phone).ToArray(),
                        Message = $"Payment of {transaction.IdealAmount} GHS has been successfully made for the reservation #{reseravtion.ReferenceNo}."
                    });
                }

                var emailTargets = users.Where(x => x.EmailConfirmed);
                if (emailTargets.Count() > 0)
                {
                    await EmailService.SendAsync(new Email.SendMailOptions()
                    {
                        Destinations = emailTargets.Select(x => x.Email).ToArray(),
                        Subject = "Payment Received",
                        IsHtml = true,
                        Message = $"<h3>Payment Received</h3><br/><p>Payment of <b>{transaction.IdealAmount} GHS</b></p> has been received for a new reservation #{reseravtion.ReferenceNo}",
                    });
                }


            }

            //  Broadcast signal for payment
            GlobalHost.ConnectionManager.GetHubContext<CoreHub>().Clients.All.OnReservationPaid(reseravtion.Id);

        }
    }
}