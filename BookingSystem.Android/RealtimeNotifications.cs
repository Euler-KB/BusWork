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
            hubConnection = new HubConnection(Resources.APIBaseAddress, true);

            if (hubConnection.Headers.ContainsKey("Authorization"))
                hubConnection.Headers["Authorization"] = $"Bearer {e.AccessToken}";
            else
                hubConnection.Headers.Add("Authorization", $"Bearer {e.AccessToken}");

            coreHub = hubConnection.CreateHubProxy("CoreHub");

            var handler = new Handler(Looper.MainLooper);

            coreHub.On("OnReservationPaid", (long id) =>
            {
                handler.Post(() =>
                {
                    OnReservationPaid?.Invoke(null, id);
                });

            });

            coreHub.On("OnReservationRefunded", (long id) =>
            {
                handler.Post(() =>
                {
                    OnReservationRefunded?.Invoke(null, id);

                });
            });

            hubConnection.Start();

            hubConnection.Error += (err) =>
            {

            };

            hubConnection.StateChanged += (t) =>
            {
                if (!connectionTimer.Enabled)
                    connectionTimer.Start();

                if (t.NewState == ConnectionState.Connected)
                {
                    Toast.MakeText(Application.Context, "Notifications setup", ToastLength.Short).Show();
                }
            };

            //
            isRegistered = true;
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
            var proxy = ProxyFactory.GetProxyInstace();
            if (proxy.IsAuthenticated && hubConnection.State != ConnectionState.Connected ||
                hubConnection.State != ConnectionState.Reconnecting)
            {
                //  Try reconnect
                hubConnection.Stop();

                //  Init authentication
                InitConnection(proxy.AuthInfo);
            }
        }


    }
}