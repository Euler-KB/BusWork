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
using Newtonsoft.Json;
using BookingSystem.Android.Factory;

namespace BookingSystem.Android
{
    [Activity(Label = "Reservation Info")]
    public class ReservationInfoActivity : BaseActivity
    {
        ReservationInfo reservation;

        protected override int? GetMenuResource() => Resource.Menu.actions_reservations;

        public static void Navigate(Context context, ReservationInfo reservation)
        {
            Intent intent = new Intent(context, typeof(ReservationInfoActivity));
            intent.PutExtra("reservation", JsonConvert.SerializeObject(reservation));
            context.StartActivity(intent);
        }

        public ReservationInfoActivity() : base(Resource.Layout.reservation_info_layout)
        {
            OnLoaded += delegate
            {
                AllowBackNavigation();

                //
                reservation = JsonConvert.DeserializeObject<ReservationInfo>(Intent.GetStringExtra("reservation"));

                //  bind reservation to view
                foreach (var item in ViewHolders.ItemHolders.ReservationItemBindings)
                {
                    View view = FindViewById(item.Resource);
                    if (item.Resource == Resource.Id.btn_reservations_info)
                        view.Visibility = ViewStates.Gone;
                    else
                        item.OnBind.DynamicInvoke(view, reservation);
                }
            };
        }

        async void OnCancelReservation()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.CancelReservation(reservation.Id));
            if (response.Successful)
            {
                Toast.MakeText(this, "Reservation cancelled successfully!", ToastLength.Short).Show();
                Finish();
            }
            else
            {
                Toast.MakeText(this, "Failed cancelling reservation", ToastLength.Short).Show();
            }
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_cancel_reservation:
                    OnCancelReservation();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}