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
using AlertDialog = Android.Support.V7.App.AlertDialog;
using BookingSystem.Android.Notifications;
using System.Threading.Tasks;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android
{
    [Application(Label = "@string/AppName", Theme = "@style/Booking.System.Theme")]
    public class CustomApplication : Application, Application.IActivityLifecycleCallbacks
    {
        public static Activity CurrentActivity { get; private set; }

        protected CustomApplication(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

#if !DEBUG
            void ShowError(Exception ex)
            {
                if (CurrentActivity != null)
                {
                    new AlertDialog.Builder(CurrentActivity)
                        .SetTitle("Unhandled Exception")
                        .SetMessage(ex.ToString())
                        .SetPositiveButton("OK", delegate { })
                        .Show();
                }
            }

            AndroidEnvironment.UnhandledExceptionRaiser += (s, e) =>
            {
                e.Handled = true;
                ShowError(e.Exception);
            };

            AppDomain.CurrentDomain.UnhandledException += (s, e) =>
            {
                ShowError((Exception)e.ExceptionObject);
            };

#endif

        }

        public override void OnCreate()
        {

            //  Register
            RegisterActivityLifecycleCallbacks(this);

            //  Restore settings from store
            SettingsStore.Load();

            //  Setup proxy for communication
            SetupProxy();

            //  Setup notifications manager
            SetupNotificationsManager();

            base.OnCreate();
        }

        protected void SetupNotificationsManager()
        {
            RealtimeNotifications.RegisterServices();
        }

        protected void SetupProxy()
        {
            //
            var proxy = ProxyFactory.GetProxyInstace();
            proxy.OnUserUpdated += (s, authInfo) =>
            {
                AuthenticationManager.CurrentSession = authInfo;
            };

            proxy.OnRefreshToken += (s, authInfo) =>
            {
                AuthenticationManager.CurrentSession = authInfo;
            };

            proxy.OnLogin += async (s, authInfo) =>
            {
                //  Set current session
                AuthenticationManager.CurrentSession = authInfo;

                //  Load user preferences
                await UserPreferences.Load();

            };

            proxy.OnSignOut += (s, authInfo) =>
            {
                //  Destroy user authentication session
                AuthenticationManager.Destroy();

                //  Clear settings store
                SettingsStore.Clear();

                //
                if (CurrentActivity != null)
                {
                    //  Finish current activity
                    CurrentActivity.Finish();
                }

                //  Start the activity
                Intent intent = new Intent(this, typeof(LoginActivity));
                intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.SingleTop | ActivityFlags.NewTask);
                StartActivity(intent);
            };
        }

        #region LifeCylce Events

        public void OnActivityCreated(Activity activity, Bundle savedInstanceState)
        {
            CurrentActivity = activity;
        }

        public void OnActivityDestroyed(Activity activity)
        {

        }

        public void OnActivityPaused(Activity activity)
        {
            //  Save all changes before leaving
            SettingsStore.Save();
        }

        public void OnActivityResumed(Activity activity)
        {
            CurrentActivity = activity;
        }

        public void OnActivitySaveInstanceState(Activity activity, Bundle outState)
        {

        }

        public void OnActivityStarted(Activity activity)
        {

        }

        public void OnActivityStopped(Activity activity)
        {

        }

        #endregion
    }
}