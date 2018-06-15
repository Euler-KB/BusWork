using BookingSystem.API.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BookingSystem.API.Services.Payment
{
    public interface IPaymentNotification
    {
        /// <summary>
        /// Invoked when payment is successful
        /// </summary>
        Task OnPaymentSuccessul(API.Models.BookReservation reseravtion, Transaction transaction);

        /// <summary>
        /// Invoked when payment fails
        /// </summary>
        Task OnPaymentFail(API.Models.BookReservation reservation, Transaction transaction);
    }
}
