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
using BookingSystem.API.Models.DTO;
using BookingSystem.Android.Factory;
using System.Timers;
using Microsoft.AspNet.SignalR.Client;
using Newtonsoft.Json;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android.Notifications
{

    /// <summary>
    /// Manages notifications from the server
    /// </summary>
    public static class RealtimeNotifications
    {
        const int ConnectionCheckInterval = 2500;

        static bool isRegistered;

        static Timer connectionTimer;

        public static event EventHandler<long> OnReservationRefunded;

        public static event EventHandler<long> OnReservationPaid;

        public static event EventHandler<long> OnPaymentFailed;

        //
        static HubConnection hubConnection;
        static IHubProxy coreHub;

        internal static void RegisterServices()
        {
            if (isRegistered)
                return;

            var proxy = ProxyFactory.GetProxyInstace();

            //
            proxy.OnLogin += OnUserLogin;
            proxy.OnSignOut += OnUserSignOut;

            //
            connectionTimer = new Timer(ConnectionCheckInterval);
            connectionTimer.Elapsed += OnCheckConnection;

            isRegistered = true;
        }

        private static void InitConnection(API.AuthenticationInfo e)
        {
            //
            var handler = new Handler(Looper.MainLooper);

            hubConnection = new HubConnection(Resources.APIBaseAddress, true);

            if (hubConnection.Headers.ContainsKey("Authorization"))
                hubConnection.Headers["Authorization"] = $"Bearer {e.AccessToken}";
            else
                hubConnection.Headers.Add("Authorization", $"Bearer {e.AccessToken}");

            coreHub = hubConnection.CreateHubProxy("CoreHub");

            coreHub.On("OnReservationPaid", (long id) =>
            {
                handler.Post(() => OnReservationPaid?.Invoke(null, id));
            });

            coreHub.On("OnReservationRefunded", (long id) =>
            {
                handler.Post(() => OnReservationRefunded?.Invoke(null, id));
            });

            coreHub.On("OnReservationPaymentFailed", (long id) =>
            {
                handler.Post(() => OnPaymentFailed?.Invoke(null, id));
            });

#if DEBUG

            coreHub.On("OnTest", (int count) =>
            {
                LogHelpers.Write(nameof(RealtimeNotifications), $"Receive test notification: {count}");
            });

#endif

            hubConnection.Error += (err) =>
            {

            };

            hubConnection.StateChanged += (t) =>
            {
                if (!connectionTimer.Enabled)
                    connectionTimer.Start();

                if (t.NewState == ConnectionState.Connected)
                {
                    LogHelpers.Write(nameof(RealtimeNotifications), "Connected to hub successfully!");
                }
            };

            //
            isRegistered = true;

            hubConnection.Start().ContinueWith(t =>
            {
                if (t.IsFaulted)
                {
                    LogHelpers.Write(nameof(RealtimeNotifications), t.Exception.GetBaseException().ToString());
                }
                else
                {
                    LogHelpers.Write(nameof(RealtimeNotifications), "Connection was successful");
                }

            });


        }

        private static void OnUserLogin(object sender, API.AuthenticationInfo e)
        {
            InitConnection(e);
        }

        private static void OnUserSignOut(object sender, API.AuthenticationInfo e)
        {
            connectionTimer.Stop();
            hubConnection.Stop();
        }

        private static void OnCheckConnection(object sender, ElapsedEventArgs e)
        {
            LogHelpers.Write(nameof(RealtimeNotifications), $"Hub State: {hubConnection.State}");

            var proxy = ProxyFactory.GetProxyInstace();
            if (proxy.IsAuthenticated && !(hubConnection.State == ConnectionState.Connected || hubConnection.State == ConnectionState.Connecting ||
                hubConnection.State == ConnectionState.Reconnecting))
            {
                LogHelpers.Write(nameof(RealtimeNotifications), $"Reconnecting hub in '{nameof(OnCheckConnection)}'");

                //  Try reconnect
                hubConnection.Stop();

                //  Init authentication
                InitConnection(proxy.AuthInfo);
            }
        }


    }
}