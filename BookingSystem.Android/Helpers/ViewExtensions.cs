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
using Android.Support.V4.Widget;
using BookingSystem.Android.Views;

namespace BookingSystem.Android.Helpers
{
    public static class ViewExtensions
    {
        public static void SetDefaultColorScheme(this SwipeRefreshLayout layout)
        {
            layout.SetColorSchemeColors(AvatarDisplay.DefaultColors.Take(4).Select(t => t.ToArgb()).ToArray());
        }
    }
}