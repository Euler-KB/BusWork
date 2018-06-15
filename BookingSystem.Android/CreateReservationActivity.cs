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
using System.Threading.Tasks;
using BookingSystem.API.Models.DTO;
using BookingSystem.Android.Factory;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.Design.Widget;
using Java.Lang;
using Newtonsoft.Json;

namespace BookingSystem.Android
{
    [Activity(Label = "Book Reservation")]
    public class CreateReservationActivity : BaseActivity
    {
        public static event EventHandler<ReservationInfo> OnReservationCreate;

        public const int CreateWalletRequestCode = 0x54;

        private Spinner busesSpinner;
        private Spinner routesSpinner;
        private Views.MultiSelectSpinner seatsSpinner;
        private TextView lbEstimatedCost;
        private TextInputLayout tbPickupLocation;

        #region Payment

        private string vodafoneToken = null;

        #endregion

        //  Adapters
        class SpinnerAdapter<T> : BaseAdapter<T>
        {
            private Func<T, View, ViewGroup, View> getViewFunc, getDropDownViewFunc;

            private IList<T> items;

            public string EmptyMessage { get; set; } = "There are no items yet!";

            private bool isEmpty = true;

            public Context Context { get; }

            public SpinnerAdapter(Context context, IList<T> items, Func<T, View, ViewGroup, View> getViewFunc, Func<T, View, ViewGroup, View> getDropDownView)
            {
                Context = context;
                this.getViewFunc = getViewFunc;
                this.items = items;
                this.getDropDownViewFunc = getDropDownView;
            }


            public override int Count => isEmpty ? 1 : Items.Count;

            public void SetEmpty()
            {
                if (!isEmpty)
                {
                    isEmpty = true;
                    NotifyDataSetChanged();
                }
            }

            public bool Empty
            {
                get
                {
                    return isEmpty;
                }
            }

            public IList<T> Items
            {
                get { return items; }
                set
                {
                    items = value;
                    isEmpty = items.Count == 0;
                    NotifyDataSetChanged();
                }
            }

            public override T this[int position] => isEmpty ? default(T) : items[position];

            public override View GetView(int position, View convertView, ViewGroup parent)
            {
                var item = this[position];
                if (isEmpty || item == null)
                {
                    if (convertView == null)
                        convertView = ((Activity)Context).LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleListItem1, null);

                    convertView.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = EmptyMessage;
                    return convertView;
                }

                return getViewFunc(item, convertView, parent);
            }

            public override View GetDropDownView(int position, View convertView, ViewGroup parent)
            {
                var item = this[position];
                if (isEmpty || item == null)
                {
                    if (convertView == null)
                        convertView = ((Activity)Context).LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleDropDownItem1Line, null);

                    var view = convertView.FindViewById<TextView>(global::Android.Resource.Id.Text1);
                    if (view != null)
                        view.Text = EmptyMessage;

                    return convertView;
                }

                return getDropDownViewFunc(item, convertView, parent);
            }

            public override long GetItemId(int position)
            {
                return position;
            }

        }

        private SpinnerAdapter<BusInfo> busAdapter;
        private SpinnerAdapter<RouteInfo> routeAdapter;
        private IList<KeyValuePair<string, bool>> availableSeats;
        private bool isBusy;

        //
        private long? targetBusId;
        private long? targetRouteId;
        private bool isTargetApplied;

        private async void OnBusSelected(int position)
        {
            var item = busAdapter[position];
            if (item != null)
            {
                await LoadRoutes(item.Id, delegate
                 {
                     if (!isTargetApplied && targetRouteId != null)
                     {
                         for (int i = 0; i < routeAdapter.Items.Count; i++)
                         {
                             if (routeAdapter.Items[i].Id == targetRouteId)
                             {
                                 if (routesSpinner.SelectedItemPosition == i)
                                     OnRouteSelected(i);
                                 else
                                     routesSpinner.SetSelection(i);

                                 break;
                             }


                         }

                         isTargetApplied = true;

                     }

                 });
            }
        }

        private async void OnRouteSelected(int position)
        {
            var item = routeAdapter[position];
            if (item != null)
            {
                var proxy = ProxyFactory.GetProxyInstace();
                var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.GetAvailableSeats(item.Id));
                if (response.Successful)
                {
                    var data = await response.GetDataAsync<int[]>();
                    this.availableSeats = data.Select(x => new KeyValuePair<string, bool>(x.ToString(), false)).ToArray();
                    seatsSpinner.SetItems(availableSeats);
                }
                else
                {
                    Toast.MakeText(this, "Failed loading available seats!", ToastLength.Short).Show();
                }
            }
        }

        public CreateReservationActivity() : base(Resource.Layout.create_reservation_layout)
        {
            OnLoaded += delegate
            {
                AllowBackNavigation();

                var intent = Intent;
                if (intent.HasExtra("bus"))
                    targetBusId = intent.GetLongExtra("bus", -1);

                if (intent.HasExtra("route"))
                    targetRouteId = intent.GetLongExtra("route", -1);

                busesSpinner = FindViewById<Spinner>(Resource.Id.buses_spinner);
                routesSpinner = FindViewById<Spinner>(Resource.Id.routes_spinner);
                lbEstimatedCost = FindViewById<TextView>(Resource.Id.lb_estimated_cost);
                tbPickupLocation = FindViewById<TextInputLayout>(Resource.Id.tb_pickup_location);

                //
                busesSpinner.ItemSelected += (s, e) =>
                {
                    //  Refresh the routes for the currently select buse
                    OnBusSelected(e.Position);

                };

                //
                seatsSpinner = FindViewById<Views.MultiSelectSpinner>(Resource.Id.seats_spinner);
                seatsSpinner.Title = "Select seats to book";

                seatsSpinner.OnSelected += (s, selection) =>
                {
                    if (availableSeats != null)
                    {
                        for (var i = 0; i < availableSeats.Count; i++)
                        {
                            var k = availableSeats[i];
                            availableSeats[i] = new KeyValuePair<string, bool>(k.Key, selection[i]);
                        }

                        //  Update cost
                        var route = routeAdapter.IsEmpty ? null : routeAdapter.Items[routesSpinner.SelectedItemPosition];
                        if (route != null)
                            lbEstimatedCost.Text = (selection.Count(x => x) * route.Cost).ToNumericStandard(2);

                    }
                };

                //
                busAdapter = new SpinnerAdapter<BusInfo>(this, new List<BusInfo>(),
                    (item, view, parent) =>
                    {
                        if (view == null)
                            view = LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleListItem1, null);

                        view.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = item.Name;
                        return view;
                    },
                    (item, view, parent) =>
                    {
                        if (view == null)
                            view = LayoutInflater.Inflate(Resource.Layout.bus_dropdown_item, null);

                        view.FindViewById<TextView>(Resource.Id.lb_bus_name).Text = item.Name;
                        view.FindViewById<TextView>(Resource.Id.lb_bus_model).Text = item.Model;
                        view.FindViewById<TextView>(Resource.Id.lb_bus_seats).Text = item.Seats;

                        return view;
                    })
                {
                    EmptyMessage = "No buses available yet!"
                };

                //
                routeAdapter = new SpinnerAdapter<RouteInfo>(this, new List<RouteInfo>(),
                    (item, view, parent) =>
                    {
                        if (view == null)
                            view = LayoutInflater.Inflate(global::Android.Resource.Layout.SimpleListItem1, null);

                        view.FindViewById<TextView>(global::Android.Resource.Id.Text1).Text = $"From: {item.From} , To: {item.Destination} - {DateHelper.FormatDifference(item.DepartureTime, item.ArrivalTime)}";
                        return view;
                    },
                    (item, view, parent) =>
                    {
                        if (view == null)
                            view = LayoutInflater.Inflate(Resource.Layout.route_dropdown_item, null);

                        //
                        view.FindViewById<TextView>(Resource.Id.lb_from).Text = item.From;
                        view.FindViewById<TextView>(Resource.Id.lb_to).Text = item.Destination;
                        view.FindViewById<TextView>(Resource.Id.lb_departure_time).Text = item.DepartureTime.ToShortDateString();
                        view.FindViewById<TextView>(Resource.Id.lb_arrival_time).Text = item.ArrivalTime.ToShortDateString();
                        view.FindViewById<TextView>(Resource.Id.lb_duration).Text = DateHelper.FormatDifference(item.DepartureTime, item.ArrivalTime);

                        return view;
                    })
                {
                    EmptyMessage = "No routes available"
                };


                //
                busesSpinner.Adapter = busAdapter;
                routesSpinner.Adapter = routeAdapter;

                //
                routesSpinner.ItemSelected += (s, e) =>
                {
                    OnRouteSelected(e.Position);
                };

                //
                FindViewById<Button>(Resource.Id.btn_submit).Click += async delegate
                {
                    await OnCreateReservation();
                };

            };
        }

        protected async override void OnResume()
        {
            base.OnResume();

            if (!isTargetApplied && (targetBusId != null && targetRouteId != null))
            {
                await LoadBusesAsync(delegate
                {
                    for (int i = 0; i < busAdapter.Items.Count; i++)
                    {
                        var bus = busAdapter.Items[i];
                        if (bus.Id == targetBusId)
                        {
                            if (busesSpinner.SelectedItemPosition == i)
                            {
                                OnBusSelected(i);
                            }
                            else
                            {
                                busesSpinner.SetSelection(i);
                            }

                            break;
                        }
                    }
                });

            }
            else
            {

                //
                await LoadBusesAsync().ContinueWith(async t =>
                {
                    if (t.Result)
                    {
                        var item = busAdapter.Empty ? null : busAdapter[0];
                        if (item != null)
                            await LoadRoutes(item.Id);
                    }


                    //  
                });
            }
        }

        protected IDisposable Busy()
        {
            return BusyState.Begin(delegate
            {
                isBusy = true;
            },
            delegate
            {
                isBusy = false;
            });
        }

        protected override bool CanGoBack() => !isBusy;

        protected async Task<bool> LoadSeats()
        {
            RouteInfo route = routesSpinner.SelectedItemPosition >= 0 ? routeAdapter[routesSpinner.SelectedItemPosition] : null;
            if (route == null)
                return false;

            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.GetAvailableSeats(route.Id));
            if (response.Successful)
            {
                RunOnUiThread(async () =>
                {
                    var data = await response.GetDataAsync<int[]>();
                    this.availableSeats = data.Select(x => new KeyValuePair<string, bool>(x.ToString(), false)).ToArray();
                    seatsSpinner.SetItems(availableSeats);
                });

                return true;
            }
            else
            {
                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
            }

            return false;
        }

        protected async Task<bool> LoadBusesAsync(Action<IList<BusInfo>> load = null)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.GetAll());
            if (response.Successful)
            {
                RunOnUiThread(async () =>
                {
                    var items = await response.GetDataAsync<IList<BusInfo>>();
                    busAdapter.Items = items;

                    //
                    load?.Invoke(items);
                });

                return true;
            }
            else
            {
                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
            }

            return false;
        }

        protected async Task OnCreateReservation()
        {
            if (Validate())
            {
                //  
                var proxy = ProxyFactory.GetProxyInstace();
                WalletInfo wallet = null;

                // Get user wallets
                API.ApiResponse response = await proxy.ExecuteAsync(API.Endpoints.WalletEndpoints.GetMyWallets());
                if (response.Successful)
                {
                    var wallets = await response.GetDataAsync<IList<WalletInfo>>();

                    if (wallets.Count == 0)
                    {
                        Intent intent = new Intent(this, typeof(ManageWalletsActivity));
                        intent.PutExtra("createOnly", true);
                        StartActivityForResult(intent, CreateWalletRequestCode);
                    }
                    else
                    {
                        //  User has a default wallet
                        var userWallet = UserPreferences.Default.PrimaryWalletId;
                        if (userWallet != null)
                        {
                            wallet = wallets.FirstOrDefault(x => x.Id == userWallet);
                            await BookReservation();
                        }
                        else
                        {
                            //  Select wallet for payment
                            Dialog dlg = null;
                            dlg = new AlertDialog.Builder(this)
                                .SetTitle("Choose Payment Wallet")
                                .SetNegativeButton("Cancel", delegate { })
                                .SetItems(wallets.Select(t => $"{t.Provider}{System.Environment.NewLine}{t.Value}").ToArray(), new EventHandler<DialogClickEventArgs>(async (s, e) =>
                                {
                                    wallet = wallets[e.Which];
                                    dlg.Dismiss();

                                    //
                                    await BookReservation();
                                }))
                                .Show();
                        }

                        async Task BookReservation()
                        {
                            RouteInfo route = routeAdapter.Empty ? null : routeAdapter.Items[routesSpinner.SelectedItemPosition];
                            string seats = availableSeats != null ? string.Join(",", availableSeats.Where(x => x.Value).Select(t => t.Key)) : null;
                            string pickupLocation = tbPickupLocation.EditText.TrimInput();
                            var costInfo = await CalculateCostAsync(route.Id, wallet.Id, seats);

                            if (costInfo == null)
                            {
                                this.ToastMessage("Operation failed. Try again!");
                                return;
                            }

                            var sb = new System.Text.StringBuilder();
                            sb.AppendLine($"Are you sure you want to book the reservation from <b>{route.From}</b> to <b>{route.Destination}</b><br/>");
                            sb.AppendLine($"Duration: {DateHelper.FormatDifference(route.DepartureTime, route.ArrivalTime)}<br/>");
                            sb.AppendLine();

                            //
                            sb.AppendLine($"Booking Cost: <b>GHS {costInfo.ReservationCost}</b><br/>");
                            sb.AppendLine($"Additional Charges: <b> GHS {costInfo.Charges}</b><br/>");

                            if (await this.ShowConfirm("Book Reservation", sb.ToString(), "Book", "Cancel") == true)
                            {
                                using (Busy())
                                using (this.ShowProgress(null, "Booking reservation, please hold on..."))
                                {
                                    response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.CreateReservation(new CreateReservationInfo()
                                    {
                                        RouteId = route.Id,
                                        PickupLocation = pickupLocation,
                                        Seats = seats,
                                        AdditionalToken = vodafoneToken,
                                        WalletId = wallet.Id
                                    }));

                                    if (response.Successful)
                                    {
                                        var reservation = await response.GetDataAsync<ReservationInfo>();
                                        response = await proxy.ExecuteAsync(API.Endpoints.TransactionEndpoints.GetForReservation(reservation.Id));
                                        if (response.Successful)
                                        {
                                            var txn = await response.GetDataAsync<IEnumerable<TransactionInfo>>();
                                            var successfulCharge = txn.Where(x => x.Type == BookingSystem.API.Models.TransactionType.Charge && x.Status == BookingSystem.API.Models.TransactionStatus.Successful).ToList();
                                            if (successfulCharge.Count > 0)
                                            {
                                                var fTxn = successfulCharge.First();
                                                Toast.MakeText(this, "Payment has been made successfully!", ToastLength.Short).Show();

                                                //  Show payment confirmation
                                                //SlydePayOrderProcessing.Show(this, fTxn.RefExternal, reservation.Id, SlydePayOrderProcessing.SlydePayPaymentOrderCode);
                                            }
                                            else
                                            {
                                                Toast.MakeText(this, "Failed initiating payment processor", ToastLength.Short).Show();
                                            }
                                        }

                                        //
                                        OnReservationCreate?.Invoke(this, reservation);

                                        //  Other's rely on this to refresh
                                        SetResult(Result.Ok);

                                        //
                                        Finish();
                                    }
                                    else
                                    {
                                        this.ToastMessage(response);
                                    }
                                }

                            }
                        }

                    }
                }
                else
                {
                    this.ToastMessage(response);
                }

            }

        }

        protected override void OnSaveInstanceState(Bundle outState)
        {
            seatsSpinner.SaveToBundle("SelectedSeats", outState);
            base.OnSaveInstanceState(outState);
        }

        protected override void OnRestoreInstanceState(Bundle savedInstanceState)
        {
            seatsSpinner.RestoreFromBundle("SelectedSeats", savedInstanceState);
            base.OnRestoreInstanceState(savedInstanceState);
        }

        private async Task<bool> LoadRoutes(long busId , Action<List<RouteInfo>> load = null)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.GetForBus(busId));
            if (response.Successful)
            {
                //  Update adapter
                RunOnUiThread(async () =>
                {
                    var routes = await response.GetDataAsync<List<RouteInfo>>();
                    routeAdapter.Items = routes;

                    //
                    load?.Invoke(routes);

                    await LoadSeats();

                });

                return true;

            }
            else
            {
                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
            }

            return false;
        }

        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == CreateWalletRequestCode && resultCode == Result.Ok)
            {
                //  We have all inputs setup
                await OnCreateReservation();
            }
            else if (requestCode == SlydePayOrderProcessing.SlydePayPaymentOrderCode)
            {
                var status = (SlydePayOrderProcessing.PaymentStatus)Intent.GetIntExtra("Status", -1);
                switch (status)
                {
                    case SlydePayOrderProcessing.PaymentStatus.Successful:
                        Toast.MakeText(this, "Payment was successfully made!", ToastLength.Long).Show();
                        break;
                    case SlydePayOrderProcessing.PaymentStatus.Failed:
                        Toast.MakeText(this, "Payment for reservation failed!", ToastLength.Short).Show();
                        break;
                }
            }
        }

        protected async Task<ReservationCostInfo> CalculateCostAsync(long routeId, long walletId, string seats)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.ReservationsEndpoints.CalculateCost(routeId, walletId, seats));
            if (response.Successful)
            {
                return await response.GetDataAsync<ReservationCostInfo>();
            }

            return null;
        }

        private bool Validate()
        {
            if (busesSpinner.SelectedItemPosition < 0)
            {
                Toast.MakeText(this, "Please select a bus to begin", ToastLength.Short).Show();
                return false;
            }

            if (routesSpinner.SelectedItemPosition < 0)
            {
                Toast.MakeText(this, "No route selected!", ToastLength.Short).Show();
                return false;
            }

            if (availableSeats?.Any(x => x.Value) == false)
            {
                this.ToastMessage("Please select the seats you wish to book!");
                return false;
            }

            return true;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.actions_create_reservation, menu);
            return true;
        }

        async void OnCreateReservationMenuOption()
        {
            await OnCreateReservation();
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_reset:
                    break;
                case Resource.Id.action_create:
                    OnCreateReservationMenuOption();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}