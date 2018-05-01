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
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android.ViewHolders
{
    public partial class ItemHolders
    {
        public static readonly IList<ViewBind> RouteItemBindings = new List<ViewBind>()
        {
            new PropertyBind<TextView,RouteInfo>(Resource.Id.lb_from,(view,route) => view.Text = route.From),
            new PropertyBind<TextView,RouteInfo>(Resource.Id.lb_to,(view,route) => view.Text = route.Destination),
            new PropertyBind<TextView,RouteInfo>(Resource.Id.lb_departure_time,(view,route) => view.Text = route.DepartureTime.ToShortDateString()),
            new PropertyBind<TextView,RouteInfo>(Resource.Id.lb_arrival_time,(view,route) => view.Text = route.ArrivalTime.ToShortDateString()),
            new PropertyBind<TextView,RouteInfo>(Resource.Id.lb_duration,(view,route) => view.Text = DateHelper.FormatDifference(route.DepartureTime,route.ArrivalTime)),
        };

    }
}