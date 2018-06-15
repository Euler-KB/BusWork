using BookingSystem.API.Hubs;
using BookingSystem.API.Models;
using BookingSystem.API.Models.DTO;
using BookingSystem.API.Services.Payment;
using Microsoft.AspNet.SignalR;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Web;
using System.Web.Http;

namespace BookingSystem.API.Handlers
{
    public class HubtelPaymentHandler : HttpTaskAsyncHandler
    {
        public static readonly string CallbackUrl = "payment/hubtel/callback";

        public override bool IsReusable => true;

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var request = context.Request;
            using (var sr = new StreamReader(request.InputStream))
            using (var dbContext = new BookingContext())
            {
                var body = JsonConvert.DeserializeObject<Services.Payment.Models.ResponseModel>(await sr.ReadToEndAsync());

                //
                var txn = dbContext.Transactions.Include(x => x.Reservation).FirstOrDefault(x => x.RefExternal == body.Data.TransactionId && x.Type == TransactionType.Charge);
                if (txn != null)
                {
                    if (Services.Payment.HubtelPaymentService.IsSuccessfulResponse(body.ResponseCode))
                        txn.Status = TransactionStatus.Successful;
                    else
                        txn.Status = TransactionStatus.Failed;

                    txn.Message = body.Data.Description;
                    txn.DateCompleted = DateTime.UtcNow;
                    await dbContext.SaveChangesAsync();

                    //  Broadcast notification
                    if (txn.Status == TransactionStatus.Successful)
                    {
                        var notifService = (IPaymentNotification) GlobalConfiguration.Configuration.DependencyResolver.GetService(typeof(IPaymentNotification));
                        await notifService.OnPaymentSuccessul(txn.Reservation, txn);
                    }

                }
            }


        }
    }
}