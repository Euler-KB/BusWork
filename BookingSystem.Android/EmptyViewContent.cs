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
    public class EmptyViewContent
    {
        public ImageView ImageView { get; }

        private TextView lbMessage;

        private TextView lbSubMessage;

        public View View { get; set; }

        public EmptyViewContent(View view)
        {
            View = view;
            ImageView = view.FindViewById<ImageView>(Resource.Id.img_empty);
            lbMessage = view.FindViewById<TextView>(Resource.Id.lb_msg);
            lbSubMessage = view.FindViewById<TextView>(Resource.Id.lb_sub_msg);
        }

      
        public string Message
        {
            get
            {
                return lbMessage.Text;
            }

            set
            {
                lbMessage.Text = value;
            }
        }

        public string SubMessage
        {
            get
            {
                return lbSubMessage.Text;
            }

            set
            {
                lbSubMessage.Text = value;
            }
        }

    }
}