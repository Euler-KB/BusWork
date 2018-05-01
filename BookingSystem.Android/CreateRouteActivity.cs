using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BookingSystem.API.Models.DTO;
using Newtonsoft.Json;
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Factory;
using BookingSystem.Android.Views;
using Android.Support.Design.Widget;

namespace BookingSystem.Android
{
    [Activity(Label = "Add Route")]
    public class CreateRouteActivity : BaseActivity
    {
        RouteInfo routeInfo;

        private BusInfo bus;
        private DateTimeSelector departureTimeSelector;
        private DateTimeSelector arrivalTimeSelector;


        public CreateRouteActivity() : base(Resource.Layout.create_route_layout)
        {
            OnLoaded += delegate
            {
                AllowBackNavigation();

                var intent = Intent;

                departureTimeSelector = FindViewById<DateTimeSelector>(Resource.Id.depature_date_picker);
                arrivalTimeSelector = FindViewById<DateTimeSelector>(Resource.Id.arrival_date_picker);

                if (intent.HasExtra("route"))
                {
                    routeInfo = JsonConvert.DeserializeObject<RouteInfo>(intent.GetStringExtra("route"));
                    departureTimeSelector.Date = routeInfo.DepartureTime;
                    arrivalTimeSelector.Date = routeInfo.ArrivalTime;
                }

                bus = JsonConvert.DeserializeObject<BusInfo>(intent.GetStringExtra("bus"));

                //  Are we editing the route
                var isEdit = intent.GetBooleanExtra("edit", false);


                var btnSubmit = FindViewById<Button>(Resource.Id.btn_submit);
                btnSubmit.Text = isEdit ? "Save Changes" : "Add Route";
                SupportActionBar.Title = isEdit ? "Edit Route" : "Add Route";

                var handler = new InputHandler();
                handler.SetBindings(new InputBinding[]
                {
                    new InputBinding("From",Resource.Id.tb_from , true)
                    {
                        OnBind = (view) => ((TextInputLayout)view).EditText.Text = routeInfo?.From
                    },
                    new InputBinding("Destination",Resource.Id.tb_destination, true)
                    {
                        OnBind = (view) => ((TextInputLayout)view).EditText.Text = routeInfo?.Destination
                    },
                    new InputBinding("Cost",Resource.Id.tb_cost, true , min: 1 , max: 999999 , compare: null )
                    {
                        OnBind = (view) => ((TextInputLayout)view).EditText.Text = routeInfo?.Cost.ToString()
                    },
                }, FindViewById<ViewGroup>(global::Android.Resource.Id.Content));

                bool ValidateInputs()
                {
                    if (handler.ValidateInputs(true).Count != 0)
                    {
                        return false;
                    }

                    //  Validate departure and arrival dates
                    if (departureTimeSelector.Date == null)
                    {
                        Toast.MakeText(this, "Please select departure time!", ToastLength.Short).Show();
                        return false;
                    }

                    if (arrivalTimeSelector.Date == null)
                    {
                        Toast.MakeText(this, "Please select arrival time!", ToastLength.Short).Show();
                        return false;
                    }

                    if (departureTimeSelector.Date >= arrivalTimeSelector.Date)
                    {
                        Toast.MakeText(this, "Departure time cannot be greater or the same as the arrival time!", ToastLength.Short).Show();
                        return false;
                    }

                    return true;
                }

                btnSubmit.Click += async (s, e) =>
                {
                    if (ValidateInputs())
                    {
                        var proxy = ProxyFactory.GetProxyInstace();
                        var formInputs = handler.GetInputs();
                        if (isEdit)
                        {
                            //  Update
                            var editRoute = new EditRouteInfo();

                            if (routeInfo.From != formInputs["From"])
                                editRoute.From = formInputs["From"];

                            if (routeInfo.Destination != formInputs["Destination"])
                                editRoute.Destination = formInputs["Destination"];

                            if (double.TryParse(formInputs["Cost"], out var cost) && routeInfo.Cost != cost)
                            {
                                editRoute.Cost = cost;
                            }

                            if (departureTimeSelector.Date != routeInfo.DepartureTime)
                                editRoute.DepartureTime = departureTimeSelector.Date;

                            if (arrivalTimeSelector.Date != routeInfo.ArrivalTime)
                                editRoute.ArrivalTime = arrivalTimeSelector.Date;

                            if (editRoute.AnyUpdate())
                            {
                                var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.UpdateRoute(routeInfo.Id, editRoute));
                                if (response.Successful)
                                {
                                    var bundle = new Intent();
                                    bundle.PutExtra("action", "edit");
                                    SetResult(Result.Ok, bundle);
                                    Finish();
                                }
                                else
                                {
                                    Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                                }
                            }
                        }
                        else
                        {
                            //  Add new route
                            var response = await proxy.ExecuteAsync(API.Endpoints.RoutesEndpoints.CreateRoute(bus.Id, new CreateRouteInfo()
                            {
                                From = formInputs["From"],
                                Destination = formInputs["Destination"],
                                Cost = double.Parse(formInputs["Cost"]),
                                DepartureTime = departureTimeSelector.Date.Value,
                                ArrivalTime = arrivalTimeSelector.Date.Value,
                                Comments = ""
                            }));

                            if (response.Successful)
                            {
                                var bundle = new Intent();
                                bundle.PutExtra("action", "create");
                                SetResult(Result.Ok, bundle);
                                Finish();
                            }
                            else
                            {
                                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                            }

                        }

                    }
                };

            };
        }

        public static void Navigate(Context context, BusInfo bus, RouteInfo route = null, bool isEdit = false, int? requestCode = null)
        {
            Intent intent = new Intent(context, typeof(CreateRouteActivity));

            intent.PutExtra("bus", JsonConvert.SerializeObject(bus));
            intent.PutExtra("edit", isEdit);

            if (route != null)
                intent.PutExtra("route", JsonConvert.SerializeObject(route));

            if (requestCode == null)
                context.StartActivity(intent);
            else
                ((Activity)context).StartActivityForResult(intent, requestCode.Value);
        }


    }
}