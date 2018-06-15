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
using Android.Support.Design.Widget;
using BookingSystem.API.Models.DTO;
using Android.Support.V4.Widget;
using System.Threading.Tasks;
using BookingSystem.Android.Helpers;
using Newtonsoft.Json;
using BookingSystem.Android.Factory;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace BookingSystem.Android
{
    [Activity(Label = "Bus Routes")]
    public class BusRoutesActivity : BaseActivity
    {
        class FilterInfo
        {
            public string From { get; set; }

            public string Destination { get; set; }

            public DateTime? StartDate { get; set; }

            public DateTime? EndDate { get; set; }

        }

        private ListView listView;
        private SmartAdapter<RouteInfo> routesAdapter;
        private IList<RouteInfo> routes;

        private FloatingActionButton fabAddRoute;
        private BusInfo bus;
        private bool isBusy;
        private SwipeRefreshLayout swipeRefreshLayout;
        private FilterInfo currentFilter = new FilterInfo();

        public static void Navigate(Context context, BusInfo busInfo, bool allowAdd)
        {
            Intent intent = new Intent(context, typeof(BusRoutesActivity));
            intent.PutExtra("bus", JsonConvert.SerializeObject(busInfo));
            intent.PutExtra("allow_add", allowAdd);
            context.StartActivity(intent);
        }


        protected override int? GetMenuResource() => Resource.Menu.actions_bus_routes;

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_filter:
                    {
                        ShowFilterDialog(filter =>
                        {
                            currentFilter = filter;

                            var items = FilterRoutes(routes).ToList();
                            if (items.Count > 0)
                            {
                                if (listView.Adapter is SmartAdapter<RouteInfo>)
                                    routesAdapter.Items = items;
                                else
                                {
                                    listView.Adapter = (routesAdapter = new SmartAdapter<RouteInfo>(this, Resource.Layout.route_dropdown_item, ViewHolders.ItemHolders.RouteItemBindings)
                                    {
                                        Items = items
                                    });
                                }

                            }
                            else
                            {
                                listView.Adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleListItem1, new string[] { "No routes match selected filters!" });
                            }

                        });
                    }
                    break;
            }
            return base.OnOptionsItemSelected(item);
        }

        void ShowFilterDialog(Action<FilterInfo> onSave = null)
        {
            var view = LayoutInflater.Inflate(Resource.Layout.routes_filter_layout, null);

            TextInputLayout tbFrom = view.FindViewById<TextInputLayout>(Resource.Id.tb_from);
            TextInputLayout tbDestination = view.FindViewById<TextInputLayout>(Resource.Id.tb_destination);
            Views.DateTimeSelector startDatePicker = view.FindViewById<Views.DateTimeSelector>(Resource.Id.start_date_picker);
            Views.DateTimeSelector endDatePicker = view.FindViewById<Views.DateTimeSelector>(Resource.Id.end_date_picker);

            //  Dont allow selection of time
            startDatePicker.IncludeTime = endDatePicker.IncludeTime = false;

            //  Update stuffs
            if (currentFilter.From.IsValidString())
                tbFrom.EditText.Text = currentFilter.From;

            if (currentFilter.Destination.IsValidString())
                tbDestination.EditText.Text = currentFilter.Destination;

            if (currentFilter.StartDate != null)
                startDatePicker.Date = currentFilter.StartDate;

            if (currentFilter.EndDate != null)
                endDatePicker.Date = currentFilter.EndDate;


            new AlertDialog.Builder(this)
                .SetTitle("Configure Filters")
                .SetView(view)
                .SetPositiveButton("Save", delegate
                {
                    var fInfo = new FilterInfo()
                    {
                        From = tbFrom.EditText.TrimInput(),
                        Destination = tbDestination.EditText.TrimInput(),
                        EndDate = endDatePicker.Date,
                        StartDate = startDatePicker.Date
                    };

                    //  Save info
                    onSave(fInfo);


                }).SetNegativeButton("Cancel", delegate { })
                .Show();
        }

        public BusRoutesActivity() : base(Resource.Layout.bus_routes_layout)
        {
            OnLoaded += async delegate
            {
                AllowBackNavigation();

                var intent = Intent;
                bus = JsonConvert.DeserializeObject<BusInfo>(intent.GetStringExtra("bus"));
                SupportActionBar.Title = $"Routes - ({bus.Name})";

                //
                swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                swipeRefreshLayout.SetDefaultColorScheme();
                swipeRefreshLayout.Refresh += async (s, e) =>
                {
                    if (isBusy)
                        return;

                    using (Busy(true))
                    {
                        await LoadRoutes();
                    }

                };

                //
                listView = FindViewById<ListView>(Resource.Id.routes_list_view);


                fabAddRoute = FindViewById<FloatingActionButton>(Resource.Id.fab_add_route);

                if (intent.GetBooleanExtra("allow_add", false))
                {
                    fabAddRoute.Click += OnAddRoute;
                }
                else
                {
                    //  Hide add button
                    fabAddRoute.Visibility = ViewStates.Gone;
                }

                //
                routesAdapter = new SmartAdapter<RouteInfo>(this, Resource.Layout.route_dropdown_item, ViewHolders.ItemHolders.RouteItemBindings);
                listView.Adapter = routesAdapter;

                //
                await LoadRoutes();
            };

        }

        private void OnAddRoute(object sender, EventArgs e)
        {
            CreateRouteActivity.Navigate(this, bus, requestCode: 0x500);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == 0x500 && resultCode == Result.Ok)
            {
                await LoadRoutes();
            }
        }

        IEnumerable<RouteInfo> FilterRoutes(IList<RouteInfo> routes)
        {
            IEnumerable<RouteInfo> result = routes;
            if (currentFilter.From.IsValidString())
                result = result.Where(x => x.From.ContainsIgnoreCase(currentFilter.From));

            if (currentFilter.Destination.IsValidString())
                result = result.Where(x => x.Destination.ContainsIgnoreCase(currentFilter.Destination));

            if (currentFilter.StartDate != null)
                result = result.Where(x => x.DepartureTime > currentFilter.StartDate);

            if (currentFilter.EndDate != null)
                result = result.Where(x => x.ArrivalTime < currentFilter.EndDate);

            return result;
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

        protected async Task LoadRoutes()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.GetForBus(bus.Id));
            if (response.Successful)
            {
                routes = await response.GetDataAsync<IList<RouteInfo>>();
                int actualCount = routes.Count;

                var items = FilterRoutes(routes).ToList();
                routesAdapter.Items = items;

                int filteredCount = items.Count;

                listView.ItemClick -= OnItemClicked;
                if (actualCount == 0)
                {
                    listView.Adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleListItem1, new string[] { "No routes available for this bus" });
                }
                else if (filteredCount == 0)
                {
                    listView.Adapter = new ArrayAdapter<string>(this, global::Android.Resource.Layout.SimpleListItem1, new string[] { "No routes match configured filters!" });
                }
                else
                {
                    listView.Adapter = routesAdapter;
                    listView.ItemClick += OnItemClicked;
                }

            }
            else
            {
                Toast.MakeText(this, response.GetResponseMessage(), ToastLength.Short).Show();
            }
        }

        void OnItemClicked(object sender, AdapterView.ItemClickEventArgs e)
        {
            if (ProxyFactory.GetProxyInstace().User.AccountType == BookingSystem.API.Models.AccountType.User)
            {
                Intent intent = new Intent(this, typeof(CreateReservationActivity));
                intent.PutExtra("route", routes[e.Position].Id);
                intent.PutExtra("bus", bus.Id);
                StartActivity(intent);
            }
        }
    }
}