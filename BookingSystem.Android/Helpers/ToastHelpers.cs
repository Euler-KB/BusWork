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
using BookingSystem.Android.API;

namespace BookingSystem.Android.Helpers
{
    public static class ToastHelpers
    {
        public static void ToastMessage(this Context context , ApiResponse response)
        {
            ToastMessage(context, response.GetErrorDescription());
        }

        public static void ToastMessage(this Context context , string message , ToastLength length = ToastLength.Short)
        {
            Toast.MakeText(context, message, length).Show();
        }
    }
}