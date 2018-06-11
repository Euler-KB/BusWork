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
using Android.Webkit;
using BookingSystem.Android.Notifications;

namespace BookingSystem.Android
{
    [Activity(Label = "Payment Processing")]
    public class SlydePayOrderProcessing : Activity
    {
        public const int SlydePayPaymentOrderCode = 0x7890;

        public enum PaymentStatus
        {
            /// <summary>
            /// Payment was succesful
            /// </summary>
            Successful = 0,

            /// <summary>
            /// Payment failed
            /// </summary>
            Failed = -1
        }

        private long reservationId;

        public static void Show(Activity activity, string token, long reservationId, int requestCode)
        {
            var intent = new Intent(activity, typeof(SlydePayOrderProcessing));
            intent.PutExtra("token", token);
            intent.PutExtra("reservation", reservationId);
            activity.StartActivityForResult(intent, requestCode);
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            var intent = Intent;
            var token = intent.GetStringExtra("token");

            //
            reservationId = intent.GetLongExtra("reservation", -1);

            // Create your application here
            var webView = new WebView(this);
            webView.SetWebViewClient(new WebViewClient());
            SetContentView(webView, new ViewGroup.LayoutParams(ViewGroup.LayoutParams.MatchParent, ViewGroup.LayoutParams.MatchParent));

            webView.LoadUrl($"https://app.slydepay.com.gh/paylive/detailsnew.aspx?pay_token={token}");
        }

        protected override void OnResume()
        {
            RealtimeNotifications.OnReservationPaid += OnReservationPaid;
            RealtimeNotifications.OnPaymentFailed += OnReservationPaymentFailed;
            base.OnResume();
        }

        private void OnReservationPaymentFailed(object sender, long reservationId)
        {
            if (reservationId != this.reservationId)
                return;

            var intent = new Intent();
            intent.PutExtra("Status", (int)PaymentStatus.Failed);
            SetResult(Result.Canceled, intent);
            Finish();
        }

        private void OnReservationPaid(object sender, long reservationId)
        {
            if (reservationId != this.reservationId)
                return;

            var intent = new Intent();
            intent.PutExtra("Status", (int)PaymentStatus.Successful);
            SetResult(Result.Ok, intent);
            Finish();
        }

        protected override void OnPause()
        {
            RealtimeNotifications.OnReservationPaid -= OnReservationPaid;
            RealtimeNotifications.OnPaymentFailed -= OnReservationPaymentFailed;
            base.OnPause();
        }
    }
}