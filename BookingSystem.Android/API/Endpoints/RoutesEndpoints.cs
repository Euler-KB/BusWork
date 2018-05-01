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
using System.Net.Http;
using BookingSystem.API.Models.DTO;
using BookingSystem.API.Models;

namespace BookingSystem.Android.API.Endpoints
{
    public static class RoutesEndpoints
    {
        public static string BaseUri = "api/routes";

        public static IEndpoint GetForBus(long id) => new ApiEndpoint($"{BaseUri}/bus/{id}", HttpMethod.Get);

        public static IEndpoint Get(long id) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Get);

        public static IEndpoint GetRouteSummary(long id) => new ApiEndpoint($"{BaseUri}/{id}/summary", HttpMethod.Get);

        public static IEndpoint GetAvailableSeats(long routeId) => new ApiEndpoint($"{BaseUri}/{routeId}/available/seats", HttpMethod.Get);

        public static IEndpoint CreateRoute(long busId, CreateRouteInfo routeInfo) => new ApiEndpoint($"{BaseUri}/bus/{busId}", HttpMethod.Post, routeInfo);

        public static IEndpoint UpdateRoute(long id, EditRouteInfo routeInfo) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Put, routeInfo);

        public static IEndpoint DeleteRoute(long id) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Delete);
    }
}