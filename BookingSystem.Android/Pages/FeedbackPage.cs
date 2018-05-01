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
using BookingSystem.Android.Helpers;
using Android.Support.V4.Widget;
using BookingSystem.API.Models.DTO;

namespace BookingSystem.Android.Pages
{
    public class FeedbackPage : BasePage
    {
        private bool isBusy;
        private SwipeRefreshLayout swipeRefreshLayout;
        private ListView itemsView;
        private SmartAdapter<FeedbackInfoEx> itemsAdapter;

        public FeedbackPage()
        {
            OnLoaded += (s, view) =>
            {
                swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                swipeRefreshLayout.SetDefaultColorScheme();
                swipeRefreshLayout.Refresh += async delegate
                {
                    if (isBusy)
                        return;

                    using (Busy())
                    {
                        await LoadFeedbacksAsync();
                    }
                };

                //
                itemsView = view.FindViewById<ListView>(Resource.Id.feedback_listview);
                itemsAdapter = new SmartAdapter<FeedbackInfoEx>(Activity, Resource.Layout.user_feedback_layout, ViewHolders.ItemHolders.FeedBackItemBindings);
                itemsView.Adapter = itemsAdapter;
            };
        }


        public override async void OnResume()
        {
            base.OnResume();

            //
            await LoadFeedbacksAsync();
        }

        protected async Task LoadFeedbacksAsync()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.GetAllFeedbacks());
            if (response.Successful)
            {
                var items = await response.GetDataAsync<IList<FeedbackInfoEx>>();
                itemsAdapter.Items = items;
            }
            else
            {
                Toast.MakeText(Activity, response.GetErrorDescription(), ToastLength.Short).Show();
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
                if (indicator)
                    swipeRefreshLayout.Refreshing = false;
            });
        }
    }
}