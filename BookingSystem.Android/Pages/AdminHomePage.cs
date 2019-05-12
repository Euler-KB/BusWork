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
using BookingSystem.Android.Helpers;
using Android.Support.V4.Widget;
using System.Threading.Tasks;
using BookingSystem.Android.Factory;
using BookingSystem.API.Models;
using System.Timers;
using BookingSystem.Android.Views;

namespace BookingSystem.Android.Pages
{
    public class AdminHomePage : BasePage
    {
        public const int UpdateInterval = 953;

        protected bool isBusy;
        private SwipeRefreshLayout swipeRefreshLayout;
        private AdminDashboardModel dashboard;

        private Timer updateTimer = new Timer(UpdateInterval);

        private TextView lbTotalMoney,
            lbMoneyToday,
            lbMoneyYesterday,
            lbMoneyWeek;

        private TextView lbReservationsToday,
            lbReservationsYesterday,
            lbReservationsWeek,
            lbReservationsTotal,
            lbReservationsActive,
            lbReservationsPending,
            lbReservationsUsed;

        private TextView lbBusesTotal,
            lbBusesActive,
            lbBusesIdle,
            lbBusesActiveToday,
            lbBusesActiveYesterday,
            lbBusesActiveWeek,
            lbBusesIdleToday,
            lbBusesIdleYesterday,
            lbBusesIdleWeek,
            lbBusesCompleteToday,
            lbBusesCompleteYesterday,
            lbBusesCompleteWeek,
            lbRoutesTotal,
            lbUsersTotal,
            lbUsersRegisteredToday,
            lbUsersRegisteredYesterday,
            lbUsersRegisteredMonth,
            lbUsersRegisteredWeek,
            lbUsersPendingActivation;


        public AdminHomePage()
        {
            OnLoaded += (s, rootFrame) =>
            {
                lbTotalMoney = rootFrame.FindViewById<TextView>(Resource.Id.lb_total_money);
                lbMoneyToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_today);
                lbMoneyYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_yesterday);
                lbMoneyWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_week);
                lbReservationsToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_today);

                lbReservationsYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_yesterday);
                lbReservationsWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_week);
                lbReservationsTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_total);
                lbReservationsActive = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_active);
                lbReservationsPending = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_pending);
                lbReservationsUsed = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_used);

                //
                lbRoutesTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_routes_total);

                //
                lbBusesTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_total);
                lbBusesActive = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_active);
                lbBusesIdle = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_idle);
                lbBusesCompleteToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_complete_today);
                lbBusesCompleteYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_complete_yesterday);
                lbBusesCompleteWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_complete_week);

                //
                lbBusesActiveToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_active_today);
                lbBusesActiveYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_active_yesterday);
                lbBusesActiveWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_active_week);
                lbBusesIdleToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_idle_today);
                lbBusesIdleYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_idle_yesterday);
                lbBusesIdleWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_buses_idle_week);

                //
                lbUsersTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_total);
                lbUsersRegisteredToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_registered_today);
                lbUsersRegisteredWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_registered_week);
                lbUsersRegisteredMonth = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_registered_month);
                lbUsersRegisteredYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_registered_yesterday);
                lbUsersPendingActivation = rootFrame.FindViewById<TextView>(Resource.Id.lb_users_pending_activation);

                //
                swipeRefreshLayout = rootFrame.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                swipeRefreshLayout.SetColorSchemeColors(AvatarDisplay.DefaultColors.Take(4).Select(t => t.ToArgb()).ToArray());
                swipeRefreshLayout.Refresh += async delegate
                {
                    if (isBusy)
                        return;

                    using (BusyState.Begin(() => updateTimer.Stop(), () => updateTimer.Start()))
                    using (Busy())
                    {
                        await LoadDashboardValues();
                    }

                };

                updateTimer.Elapsed += delegate
                {
                    if (isBusy)
                        return;

                    if(!IsActivePage)
                    {
                        updateTimer.Stop();
                        return;
                    }

                    Activity.RunOnUiThread(async delegate
                    {
                        using (Busy(false))
                        {
                            await LoadDashboardValues(true);
                        }

                    });


                };

                //
                dashboard = new AdminDashboardModel()
                {
                    Buses = new AdminDashboardModel.BusSpec(),
                    Money = new AdminDashboardModel.MoneySpec<double>(),
                    Reservations = new AdminDashboardModel.ReservationSpec<long>(),
                    Routes = new AdminDashboardModel.RoutesSpec(),
                    Users = new AdminDashboardModel.UsersSpec()
                };

                BindDashboardValues();

            };

          
        }

        void RestartTimer()
        {
            updateTimer.Stop();
            updateTimer.Start();
        }

        public override void OnLeavePage()
        {
            updateTimer.Stop();
        }

        protected IDisposable Busy(bool indicator = true)
        {
            return BusyState.Begin(delegate
            {
                isBusy = true;

                if (indicator)
                    swipeRefreshLayout.Refreshing = true;
            },
            delegate
            {
                isBusy = false;
                swipeRefreshLayout.Refreshing = false;

            });
        }

        protected async Task LoadDashboardValues(bool silent = false)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.AdminDashboard);
            if (!IsActivePage)
                return;

            if (response.Successful)
            {
                 //
                dashboard = await response.GetDataAsync<AdminDashboardModel>();

                //  
                BindDashboardValues();
               
            }
            else
            {
                if (!silent)
                    ShowApiError(response);
            }
        }

        private void BindDashboardValues()
        {
            //
            BindMoneyInfo();

            //
            BindReservationsInfo();

            //
            BindReservationsInfo();

            //
            BindBusesInfo();

            //
            BindRoutesInfo();

            //
            BindUsersInfo();

        }

        private void BindReservationsInfo()
        {
            lbReservationsToday.Text = dashboard.Reservations.Today.ToNumericStandard();
            lbReservationsYesterday.Text = dashboard.Reservations.Yesterday.ToNumericStandard();
            lbReservationsWeek.Text = dashboard.Reservations.Week.ToNumericStandard();
            lbReservationsTotal.Text = dashboard.Reservations.Total.ToNumericStandard();
            lbReservationsActive.Text = dashboard.Reservations.Active.ToNumericStandard();
            lbReservationsPending.Text = dashboard.Reservations.Pending.ToNumericStandard();
            lbReservationsUsed.Text = dashboard.Reservations.Used.ToNumericStandard();
        }

        private void BindBusesInfo()
        {
            lbBusesTotal.Text = dashboard.Buses.Total.ToNumericStandard();
            lbBusesActive.Text = dashboard.Buses.TotalActive.ToNumericStandard();
            lbBusesIdle.Text = dashboard.Buses.TotalIdle.ToNumericStandard();
            lbBusesActiveToday.Text = dashboard.Buses.ActiveToday.ToNumericStandard();
            lbBusesActiveYesterday.Text = dashboard.Buses.ActiveYesterday.ToNumericStandard();
            lbBusesActiveWeek.Text = dashboard.Buses.ActiveWeek.ToNumericStandard();
            lbBusesIdleToday.Text = dashboard.Buses.IdleToday.ToNumericStandard();
            lbBusesIdleWeek.Text = dashboard.Buses.IdleToday.ToNumericStandard();

            //
            lbBusesCompleteToday.Text = dashboard.Buses.CompleteToday.ToNumericStandard();
            lbBusesCompleteWeek.Text = dashboard.Buses.CompleteWeek.ToNumericStandard();
            lbBusesCompleteYesterday.Text = dashboard.Buses.CompleteYesterday.ToNumericStandard();
        }

        private void BindRoutesInfo()
        {
            lbRoutesTotal.Text = dashboard.Routes.Total.ToNumericStandard();
        }

        private void BindUsersInfo()
        {
            lbUsersTotal.Text = dashboard.Users.TotalUsers.ToNumericStandard();
            lbUsersRegisteredToday.Text = dashboard.Users.RegisteredToday.ToNumericStandard();
            lbUsersRegisteredWeek.Text = dashboard.Users.RegisteredWeek.ToNumericStandard();
            lbUsersRegisteredMonth.Text = dashboard.Users.RegisteredMonth.ToNumericStandard();
            lbUsersRegisteredYesterday.Text = dashboard.Users.RegisteredYesterday.ToNumericStandard();
            lbUsersPendingActivation.Text = dashboard.Users.PendingActivation.ToNumericStandard();
        }

        private void BindMoneyInfo()
        {
            lbTotalMoney.Text = dashboard.Money.Total.ToNumericStandard(2);
            lbMoneyToday.Text = dashboard.Money.Today.ToNumericStandard();
            lbMoneyYesterday.Text = dashboard.Money.Yesterday.ToNumericStandard();
            lbMoneyWeek.Text = dashboard.Money.Week.ToNumericStandard();
        }

        public override void OnResume()
        {
            base.OnResume();
            updateTimer.Start();
        }

        public override void OnPause()
        {
            base.OnPause();
            updateTimer.Stop();
        }
    }
}