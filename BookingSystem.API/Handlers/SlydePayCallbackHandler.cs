using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Web;
using System.Data.Entity;
using Microsoft.AspNet.SignalR;
using BookingSystem.API.Hubs;
using Newtonsoft.Json;

namespace BookingSystem.API.Handlers
{
    public class SlydePayCallbackHandler : HttpTaskAsyncHandler
    {
        class TransactionDetails
        {
            public string TransactionId { get; set; }

            public string Status { get; set; }

            public string LocalReference { get; set; }

            public bool IsSuccessful
            {
                get
                {
                    return true;
                }
            }
        }

        public override async Task ProcessRequestAsync(HttpContext context)
        {
            var request = context.Request;
            using (var stream = new StreamReader(request.InputStream))
            {
                var content = await stream.ReadToEndAsync();

                var sb = new StringBuilder();
                sb.AppendLine("<Response>");
                sb.AppendLine(content);
                sb.AppendLine("<Response>");
                sb.AppendLine();

                File.AppendAllText(HttpContext.Current.Server.MapPath("~/App_Data/SlydePayCallbackResponse.txt"), sb.ToString());

                //  TODO: 
                //  TransactionDetails details = null;
                //  ProcessTransaction(details);

            }
        }

        private async Task ProcessTransaction(TransactionDetails details)
        {
            using (var db = new BookingContext())
            {
                var txn = await db.Transactions.Include(x => x.Reservation).FirstOrDefaultAsync(x => x.RefLocal == details.LocalReference);
                if (txn != null)
                {
                    //
                    txn.DateCompleted = DateTime.UtcNow;
                    txn.Meta = JsonConvert.SerializeObject(details);
                    txn.Status = details.IsSuccessful ? TransactionStatus.Successful : TransactionStatus.Failed;

                    await db.SaveChangesAsync();

                    //
                    if(details.IsSuccessful)
                        GlobalHost.ConnectionManager.GetHubContext<CoreHub>().Clients.All.OnReservationPaid(txn.Reservation.Id);
                    else
                        GlobalHost.ConnectionManager.GetHubContext<CoreHub>().Clients.All.OnReservationPaymentFailed(txn.Reservation.Id);

                }

            }
        }
    }
}