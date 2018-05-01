using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;

namespace BookingSystem.Android
{
    public class ConnectionErrorContent
    {
        private Button btnRefresh;

        private TextView lbMessage;

        protected ImageView ImageView { get; }

        public event EventHandler OnRefresh;

        public View View { get; set; }

        public ConnectionErrorContent(View view)
        {
            View = view;

            lbMessage = view.FindViewById<TextView>(Resource.Id.lb_connection_err_msg);
            ImageView = view.FindViewById<ImageView>(Resource.Id.img_connection_error);
            btnRefresh = view.FindViewById<Button>(Resource.Id.btn_refresh);
            btnRefresh.Click += OnRefresh;
        }

        public string Message
        {
            get { return lbMessage.Text; }
            set
            {
                lbMessage.Text = value;
            }
        }

        public string RefreshLabel
        {
            get { return btnRefresh.Text; }
            set
            {
                btnRefresh.Text = value;
            }
        }

    }
}