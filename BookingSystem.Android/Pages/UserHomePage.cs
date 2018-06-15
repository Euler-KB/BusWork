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
using System.Threading.Tasks;
using BookingSystem.Android.Factory;
using BookingSystem.API.Models;
using BookingSystem.Android.Helpers;
using Android.Support.V4.Widget;
using Android.Animation;
using Android.Views.Animations;
using BookingSystem.Android.Notifications;
using System.Timers;

namespace BookingSystem.Android.Pages
{
    public class UserHomePage : BasePage, IRefreshPage
    {
        public const int CreateReservationLayout = 0x34;
        public const int UpdateTimeInterval = 1275;

        public static readonly AccelerateDecelerateInterpolator ValueInterpolator = new AccelerateDecelerateInterpolator();

        private bool isBusy;
        private SwipeRefreshLayout swipeRefreshLayout;
        private UserDashboardModel dashboardModel;
        private Timer updateTimer = new Timer(UpdateTimeInterval);

        #region Reservations

        private TextView lbReservationsToday,
            lbReservationsYesterday,
            lbReservationsWeek,
            lbReservationsUsed,
            lbReservationsActive,
            lbReservationsPending,
            lbReservationsTotal;


        #endregion

        #region Money

        private TextView lbMoneyTotal,
            lbMoneyToday,
            lbMoneyYesterday,
            lbMoneyRefunded;

        #endregion

        public UserHomePage()
        {
            OnLoaded += async (s, rootFrame) =>
            {
                //  Reservations section
                lbReservationsToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_today);
                lbReservationsYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_yesterday);
                lbReservationsWeek = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_week);
                lbReservationsUsed = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_used);
                lbReservationsActive = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_active);
                lbReservationsPending = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_pending);
                lbReservationsTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_reservations_total);

                //  Money section
                lbMoneyTotal = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_total_value);
                lbMoneyToday = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_today);
                lbMoneyYesterday = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_yesterday);
                lbMoneyRefunded = rootFrame.FindViewById<TextView>(Resource.Id.lb_money_refunded);

                //
                swipeRefreshLayout = rootFrame.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                swipeRefreshLayout.SetColorSchemeColors(Views.AvatarDisplay.DefaultColors.Take(4).Select(x => x.ToArgb()).ToArray());

                swipeRefreshLayout.Refresh += async delegate
                {
                    //
                    if (isBusy)
                        return;

                    //
                    RestartUpdate();

                    //
                    using (Busy())
                    {
                        await LoadStatistics();
                    }

                };

                rootFrame.FindViewById<Button>(Resource.Id.btn_book_reservation).Click += delegate
                {
                    StartActivityForResult(new Intent(Activity, typeof(CreateReservationActivity)), CreateReservationLayout);
                };

                //  
                await LoadStatistics();

                updateTimer.Elapsed += delegate
               {
                   if (isBusy)
                       return;

                   Activity.RunOnUiThread(async delegate
                   {
                       using (Busy(false))
                           await LoadStatistics(true);
                   });

               };
            };

        }

        protected void RestartUpdate()
        {
            updateTimer.Stop();
            updateTimer.Start();
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            //  On activity result
            if (requestCode == CreateReservationLayout && resultCode == (int)Result.Ok)
            {
                //  Refresh
                OnRefresh();
            }
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

        public override void OnResume()
        {
            base.OnResume();

            //  Subscribe to events
            RealtimeNotifications.OnReservationPaid += OnReservationPaid;
            RealtimeNotifications.OnReservationRefunded += OnReservationRefunded;

            //
            updateTimer.Start();
        }

        public override void OnPause()
        {
            base.OnPause();

            //  Unsubscrive events
            RealtimeNotifications.OnReservationPaid -= OnReservationPaid;
            RealtimeNotifications.OnReservationRefunded -= OnReservationRefunded;

            //
            updateTimer.Stop();
        }

        private void OnReservationRefunded(object sender, long reservationId)
        {
            //  Refresh money to reflect
        }

        private void OnReservationPaid(object sender, long reservationId)
        {
            //  Refresh money to reflect
        }

        protected virtual async Task LoadStatistics(bool silent = false)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.UserDashboard);
            if (response.Successful)
            {
                //  Decode response
                dashboardModel = await response.GetDataAsync<UserDashboardModel>();

                //  
                BindDashboardValues();
            }
            else
            {
                if (!silent)
                    Toast.MakeText(Activity, response.GetErrorDescription(), ToastLength.Short).Show();
            }
        }

        private void Animate(TextView view, float value, string format = "{0:N0}")
        {
            float initial = view.Text == "" ? 0 : float.Parse(view.Text);

            //  No animation needed here
            if (initial == value)
                return;

            var anim = ValueAnimator.OfFloat(initial, value);
            anim.Start();
            anim.SetInterpolator(ValueInterpolator);
            anim.Update += (s, e) =>
            {
                view.Text = string.Format(format, ((float)e.Animation.AnimatedValue));
            };
        }

        private void UpdateReservationsSection()
        {
            var reservations = dashboardModel.Reservations;

            //
            lbReservationsToday.Text = reservations.Today.ToNumericStandard();
            lbReservationsYesterday.Text = reservations.Yesterday.ToNumericStandard();
            lbReservationsWeek.Text = reservations.Week.ToNumericStandard();
            lbReservationsUsed.Text = reservations.Used.ToNumericStandard();

            lbReservationsActive.Text = reservations.Active.ToNumericStandard();
            lbReservationsPending.Text = reservations.Pending.ToNumericStandard();
            lbReservationsTotal.Text = reservations.Total.ToNumericStandard();
        }

        private void UpdateMoneySection()
        {
            var money = dashboardModel.Money;

            lbMoneyTotal.Text = money.Total.ToNumericStandard(2);
            lbMoneyToday.Text = $"{money.Today.ToNumericStandard()} GHS";
            lbMoneyYesterday.Text = $"{money.Yesterday.ToNumericStandard()} GHS";

            //
            lbMoneyRefunded.Text = money.Refunded.ToNumericStandard();
        }

        public async void OnRefresh()
        {
            if (isBusy)
                return;

            //
            RestartUpdate();

            //
            using (Busy(false))
            {
                await LoadStatistics();
            }
        }

        protected void BindDashboardValues()
        {
            UpdateReservationsSection();
            UpdateMoneySection();
        }
    }
}