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
using BookingSystem.Android.Helpers;
using System.Threading.Tasks;
using BookingSystem.Android.Factory;
using BookingSystem.Android.API;
using System.Timers;
using BookingSystem.API.Models.DTO;
using BookingSystem.API.Models;

namespace BookingSystem.Android.Pages
{
    public class ReservationsPage : ContentPage, IAddItemPage, ISearchPage
    {
        class FilterInfo
        {
            public string From { get; set; }

            public string Destination { get; set; }

            public string Search { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }
        }

        public const int UpdateInterval = 980;

        private SmartAdapter<ReservationInfo> itemsAdapter;
        private IList<ReservationInfo> reservations;
        private FilterInfo currentFilter = new FilterInfo();

        private Timer timer = new Timer(UpdateInterval);

        protected override int ContentResourceId => Resource.Id.reservations_list_view;

        protected override async void Initialize()
        {
            //
            NoItemsView.Message = "No reservation tickets booked yet!";
            NoItemsView.SubMessage = "To book reservation, tap on the floating button below";


            //
            var itemsView = GetContentView<ListView>();
            itemsAdapter = new SmartAdapter<ReservationInfo>(Activity, Resource.Layout.ticket_small_layout,
                ViewHolders.ItemHolders.ReservationSmallBindings);

            itemsView.Adapter = itemsAdapter;
            itemsView.ItemClick += (s, e) =>
            {
                ReservationInfoActivity.Navigate(Activity, itemsAdapter[e.Position]);
            };

            if (HasData)
            {
                reservations = GetData<IList<ReservationInfo>>();
                itemsAdapter.Items = reservations;
            }

            timer.Elapsed += async delegate
            {
                var reservations = itemsAdapter.Items;
                if (reservations?.Count > 0)
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.GetReservationCategory(reservations.Select(x => x.Id).ToArray()));
                    if (response.Successful)
                    {
                        var data = await response.GetDataAsync<IList<ReservationCategoryBinding>>();
                        foreach (var item in data)
                        {
                            var reservation = reservations.FirstOrDefault(x => x.Id == item.ReservationId);
                            if (reservation != null)
                                reservation.Category = item.Category;

                        }

                        //
                        SetLoaded(data);

                        //  Re-render
                        itemsAdapter.NotifyDataSetChanged();
                    }
                    else
                    {
                        LogHelpers.Write(nameof(ReservationsPage), $"Failed refreshing reservations: {response.GetErrorDescription()}");
                    }
                }

            };

            //
            await LoadReservations();
        }


        public void OnAddItem()
        {
            //  Book reservation here
            StartActivityForResult(new Intent(Activity, typeof(CreateReservationActivity)), 0x20);
        }

        public override async void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == 0x20 && resultCode == (int)Result.Ok)
            {
                //  Refresh view
                await LoadReservations();
            }
        }

        public override void OnResume()
        {
            timer.Start();
            base.OnResume();
        }

        public override void OnPause()
        {
            timer.Stop();
            base.OnPause();
        }

        protected override Task OnRefreshViewAsync() => LoadReservations();

        protected IEnumerable<ReservationInfo> FilterReservations(IEnumerable<ReservationInfo> rsv)
        {
            if (currentFilter.Search.IsValidString())
                rsv = rsv.Where(x => (x.UserFullName.IsValidString() ? x.UserFullName.ContainsIgnoreCase(currentFilter.Search) : true) &&
                x.ReferenceNo.Contains(currentFilter.Search));

            if (currentFilter.From.IsValidString())
                rsv = rsv.Where(x => x.Route.From.ContainsIgnoreCase(currentFilter.From));

            if (currentFilter.Destination.IsValidString())
                rsv = rsv.Where(x => x.Route.Destination.ContainsIgnoreCase(currentFilter.Destination));

            if (currentFilter.StartDate != null)
                rsv = rsv.Where(x => x.Route.DepartureTime > currentFilter.StartDate);

            if (currentFilter.EndDate != null)
                rsv = rsv.Where(x => x.Route.ArrivalTime < currentFilter.EndDate);

            return rsv;
        }

        public async Task LoadReservations()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            ApiResponse response = null;
            switch (proxy.User.AccountType)
            {
                case AccountType.User:
                    response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.GetMyReservations());
                    break;
                case AccountType.Administrator:
                    response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.GetAllReservations());
                    break;
            }

            if (response.Successful)
            {
                //
                reservations = await response.GetDataAsync<IList<ReservationInfo>>();
                itemsAdapter.Items = FilterReservations(reservations).ToList();

                //  Mark loaded
                SetLoaded(reservations);
            }
            else
            {
                if (IsLoaded)
                    Toast.MakeText(Activity, response.GetErrorDescription(), ToastLength.Short).Show();
                else
                    UpdateResponse(response);
            }
        }


        private async Task UpdateSearch()
        {
            if (reservations == null)
            {
                await LoadReservations();
            }
            else
            {
                this.itemsAdapter.Items = FilterReservations(reservations).ToList();
            }
        }

        public async void Search(string query)
        {
            currentFilter.Search = query;
            await UpdateSearch();
        }

        public async void EndSearch()
        {
            currentFilter.Search = null;
            await UpdateSearch();
        }
    }
}