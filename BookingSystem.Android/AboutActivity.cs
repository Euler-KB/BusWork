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
using Android.Support.V7.App;

namespace BookingSystem.Android
{
    [Activity(Label = "About")]
    public class AboutActivity : BaseActivity
    {
        public AboutActivity():base(Resource.Layout.about_layout)
        {
            OnLoaded += delegate
            {
                AllowBackNavigation();

                FindViewById<Button>(Resource.Id.btn_send_feedback).Click += OnSendFeedback;
            };
        }

        private void OnSendFeedback(object sender, EventArgs e)
        {
            StartActivity(typeof(CreateFeedbackActivity));
        }
    }
}