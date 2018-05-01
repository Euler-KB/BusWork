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
using BookingSystem.API.Models.DTO;

namespace BookingSystem.Android.Pages
{
    public class BusesPage : ContentPage, ISearchPage, IAddItemPage
    {
        private ListView listView;

        private SmartAdapter<BusInfo> busAdapter;
        private IList<BusInfo> buses;

        private string searchQuery;

        protected override int ContentResourceId => Resource.Id.buses_list_view;

        private void UpdateEmptyLayout()
        {
            NoItemsView.Message = "No buses available";

            switch (ProxyFactory.GetProxyInstace().User.AccountType)
            {
                case BookingSystem.API.Models.AccountType.Administrator:
                    {
                        if(buses == null || buses.Count == 0)
                        {
                            NoItemsView.SubMessage = "To add a new bus, tap the floating button below";
                        }
                        else if(buses?.Count != 0 && busAdapter?.Items.Count == 0)
                        {
                            NoItemsView.SubMessage = "No buses match search keyword or filters!";
                        }

                    }
                    break;
                case BookingSystem.API.Models.AccountType.User:
                    {
                        if (buses == null || buses.Count == 0)
                        {
                            NoItemsView.SubMessage = "Pull to refresh view and see newly created buses";
                        }
                        else if (buses?.Count != 0 && busAdapter?.Items.Count == 0)
                        {
                            NoItemsView.SubMessage = "No buses match the search or filters!";
                        }

                    }
                    break;
            }

        }

        protected override async void Initialize()
        {
            //  Get content view
            listView = GetContentView<ListView>();
            listView.ItemClick += (s, e) =>
            {
                var proxy = ProxyFactory.GetProxyInstace();
                BusRoutesActivity.Navigate(Activity, buses[e.Position], proxy.User.AccountType == BookingSystem.API.Models.AccountType.Administrator);
            };

            //  Create adapter
            busAdapter = new SmartAdapter<BusInfo>(Activity, Resource.Layout.bus_item_layout, ViewHolders.ItemHolders.BusItemBindings);

            //  
            listView.Adapter = busAdapter;

            if (HasData)
            {
                buses = GetData<IList<BusInfo>>();
                busAdapter.Items = buses;
            }

            //
            UpdateEmptyLayout();

            //  Refresh view anyway
            await LoadBusesAsync();
        }

        protected IEnumerable<BusInfo> FilterBuses(IEnumerable<BusInfo> buses, string query)
        {
            return query.IsValidString() ? buses.Where(x => x.Name.StartsWith(query, StringComparison.OrdinalIgnoreCase) || x.Model.StartsWith(query, StringComparison.OrdinalIgnoreCase)) : buses;
        }

        protected override Task OnRefreshViewAsync() => LoadBusesAsync();

        public async void Refresh()
        {
            await LoadBusesAsync();
        }

        protected async Task LoadBusesAsync()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.GetAll());
            if (response.Successful)
            {
                buses = await response.GetDataAsync<IList<BusInfo>>();
                busAdapter.Items = FilterBuses(buses, searchQuery).ToList();

                //
                UpdateEmptyLayout();

                //  Mark view loaded
                SetLoaded(buses);
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
            if (buses != null)
            {
                var items = FilterBuses(buses, searchQuery).ToList();
                busAdapter.Items = items;

                if (items.Count == 0)
                {
                    UpdateEmptyLayout();
                    ContentType = PageContentType.Empty;
                }
                else
                {
                    ContentType = PageContentType.Content;
                }
            }
            else
            {
                await LoadBusesAsync();
            }
        }

        public async void Search(string query)
        {
            this.searchQuery = query;
            await UpdateSearch();
        }

        public async void EndSearch()
        {
            this.searchQuery = null;
            await UpdateSearch();
        }

        public void OnAddItem()
        {
            CreateBusActivity.Navigate(Activity, requestCode: 0x500);
        }

        public override void OnActivityResult(int requestCode, int resultCode, Intent data)
        {
            if (requestCode == 0x500 && resultCode == (int)Result.Ok)
            {
                Refresh();
            }
        }
    }
}