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

namespace BookingSystem.Android.Helpers
{
    public static class LogHelpers
    {
        public static void Write(string message)
        {
            Write("booking.system", message);
        }

        public static void Write(string tag , string message)
        {
            global::Android.Util.Log.Debug(tag, message);
        }
    }
}