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
using BookingSystem.API.Models;
using System.Threading.Tasks;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android
{
    [Activity(MainLauncher = true, Theme = "@android:style/Theme.NoDisplay")]
    public class MainActivity : Activity
    {
        protected async void InitializeStateAsync()
        {
            try
            {
                //  Determine user state here
                var proxy = ProxyFactory.GetProxyInstace();

                if (AuthenticationManager.HasSession)
                {
                    var authInfo = AuthenticationManager.CurrentSession;

                    if (await proxy.RestoreAsync(authInfo, true))
                    {
                        NavigationHelper.NavigateUserHome(this);
                        Finish();
                        return;
                    }

                }
            }
            catch
            {
                global::Android.Util.Log.Debug("com.booking.system.Startup", "Failed restoring previous session");
            }

            //
            var intent = new Intent(this, typeof(LoginActivity));

            //  Navigate to other activity
            intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask | ActivityFlags.SingleTop);
            StartActivity(intent);

            //
            Finish();
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            base.OnCreate(savedInstanceState);

            //  Initialize state
            InitializeStateAsync();

        }
    }
}