using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using BookingSystem.API.Models.DTO;
using BookingSystem.Android.Factory;
using BookingSystem.API.Models;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace BookingSystem.Android.ViewHolders
{
    public partial class ItemHolders
    {
        static string FormatPayStatus(PayStatus status)
        {
            switch (status)
            {
                case PayStatus.Failed:
                    return "Failed";
                case PayStatus.InitiatePay:
                    return "Payment Initiated";
                case PayStatus.InitiateRefund:
                    return "Initiate Refund";
                case PayStatus.Paid:
                    return "Paid";
                case PayStatus.Refunded:
                    return "Refunded";
            }

            throw new InvalidOperationException();
        }

        static void ShowItemPopupMenu(View anchor, ReservationInfo r)
        {
            var context = anchor.Context;
            var popupMenu = new PopupMenu(context, anchor, GravityFlags.Bottom | GravityFlags.Left);
            popupMenu.Inflate(Resource.Menu.actions_reservations);
            var menu = popupMenu.Menu;

            if (r.Category != ReservationCategory.Pending)
            {
                menu.FindItem(Resource.Id.action_cancel_reservation).SetEnabled(false);
            }

            popupMenu.MenuItemClick += (sender, evt) =>
            {
                switch (evt.Item.ItemId)
                {
                    case Resource.Id.action_cancel_reservation:
                        {
                            new AlertDialog.Builder(context)
                            .SetTitle("Cancel Reservation")
                            .SetMessage("Are you sure you want to cancel the reservation?")
                            .SetPositiveButton("Yes", async delegate
                            {
                                var proxy = ProxyFactory.GetProxyInstace();
                                var response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.CancelReservation(r.Id));
                                if (response.Successful)
                                {
                                    Toast.MakeText(context, "Ticket reservation cancelled successfully!", ToastLength.Short).Show();
                                }
                                else
                                {
                                    Toast.MakeText(context, "Failed cancelling reservation!", ToastLength.Short).Show();
                                }

                            })
                            .SetNegativeButton("No", delegate { })
                            .Show();
                        }
                        break;
                }
            };

            popupMenu.Show();
        }

        public static readonly IList<ViewBind> ReservationItemBindings = new List<ViewBind>()
        {
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_bus_name, (view,r) => view.Text = $"{r.Bus.Name} ({r.Bus.Model})" ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_from, (view,r) => view.Text = r.Route.From ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_to, (view,r) => view.Text = r.Route.Destination),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_departure_time, (view,r) => view.Text = r.Route.DepartureTime.ToString() ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_arrival_time, (view,r) => view.Text = r.Route.ArrivalTime.ToString() ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_booked_seats, (view,r) => view.Text = r.Seats ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_pickup_location, (view,r) => view.Text = r.PickupLocation ?? r.Route.From ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_reservation_cost, (view,r) => view.Text = r.Cost.ToString() ),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_reservation_pay_status, (view,r) => view.Text =  FormatPayStatus(r.PayStatus).ToString() ),
            new PropertyBind<View, ReservationInfo>(Resource.Id.btn_reservations_info, (btn,r) =>
            {
                  btn.SetOnClickListener(new ClickListener(delegate{ ShowItemPopupMenu(btn,r); }));
            }),
            new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_badge_status, (view,r) =>
            {
                view.Text = r.Category.ToString();
                switch (r.Category)
                {
                    case ReservationCategory.Active:
                        view.SetBackgroundResource(Resource.Drawable.badge_success);
                        view.SetTextColor(global::Android.Graphics.Color.White);
                        break;
                    case ReservationCategory.Cancelled:
                        view.SetBackgroundResource(Resource.Drawable.badge_danger);
                        view.SetTextColor(global::Android.Graphics.Color.White);
                        break;
                    case ReservationCategory.Completed:
                        view.SetBackgroundResource(Resource.Drawable.badge_accent);
                        view.SetTextColor(global::Android.Graphics.Color.White);
                        break;
                    case ReservationCategory.Pending:
                        view.SetBackgroundResource(Resource.Drawable.badge_idle);
                        view.SetTextColor(global::Android.Graphics.Color.Black);
                        break;
                }
            }),
        };


        public static readonly IList<ViewBind> ReservationSmallBindings = new List<ViewBind>()
        {
             new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_from, (view,r) => view.Text = r.Route.From ),
             new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_to, (view,r) => view.Text = r.Route.Destination),
             new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_date, (view,r) => view.Text = r.DateCreated.ToString("g")),
             new PropertyBind<TextView, ReservationInfo>(Resource.Id.lb_ticket_no, (view,r) => view.Text = $"#{r.ReferenceNo}"),
             new PropertyBind<View, ReservationInfo>(Resource.Id.btn_reservations_info, (btn,r) =>
             {
                  btn.SetOnClickListener(new ClickListener(delegate{ ShowItemPopupMenu(btn,r); }));
             })
        };
    }
}