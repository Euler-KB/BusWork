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
using BookingSystem.Android.Factory;

namespace BookingSystem.Android.Helpers
{
    public static class NavigationHelper
    {
        public static void HideActivity(this Context context)
        {
            Intent intent = new Intent(Intent.ActionMain);
            intent.AddCategory(Intent.CategoryHome);
            intent.SetFlags(ActivityFlags.NewTask);
            context.StartActivity(intent);
        }

        public static void NavigateUserHome(Context context, Action<Intent> onPrepareIntent = null)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            if(proxy.User == null)
            {
                var intent = new Intent(context, typeof(LoginActivity));

                //  Navigate to other activity
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask | ActivityFlags.SingleTop);
                context.StartActivity(intent);
                return;
            }

            Type activityType = null;
            switch (proxy.User.AccountType)
            {
                case BookingSystem.API.Models.AccountType.Administrator:
                    activityType = typeof(AdminMainActivity);
                    break;
                case BookingSystem.API.Models.AccountType.User:
                    activityType = typeof(UserMainActivity);
                    break;
            }

            if (activityType != null)
            {
                var intent = new Intent(context, activityType);
                onPrepareIntent?.Invoke(intent);
                context.StartActivity(intent);
            }
        }
    }
}