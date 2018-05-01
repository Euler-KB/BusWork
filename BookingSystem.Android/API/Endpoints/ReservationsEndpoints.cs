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
using System.Net.Http;
using BookingSystem.API.Models;

namespace BookingSystem.Android.API.Endpoints
{
    public static class ReservationsEndpoints
    {
        public static readonly string BaseUri = "api/reservations";

        public static IEndpoint GetAllReservations(QueryOptions options = null) => new ApiEndpoint($"{BaseUri}/all", HttpMethod.Get).SetQueryOptions(options);

        public static IEndpoint GetMyReservations() => new ApiEndpoint(BaseUri, HttpMethod.Get);

        public static IEndpoint GetReservationCategory(long[] reservationIds)
        {
            var endpoint = new ApiEndpoint($"{BaseUri}/category");
            foreach (var id in reservationIds)
                endpoint.AddQuery("Id", id);

            return endpoint;
        }

        public static IEndpoint CalculateCost(long routeId, long walletId, string seats) => new ApiEndpoint($"{BaseUri}/cost/{routeId}/{walletId}",HttpMethod.Get)
                                                                                                        .AddQuery("Seats", seats);

        public static IEndpoint GetUser(long reservationId) => new ApiEndpoint($"{BaseUri}/{reservationId}/user");

        public static IEndpoint GetForUser(string id) => new ApiEndpoint($"{BaseUri}/admin/{id}/user", HttpMethod.Get);

        public static IEndpoint GetForBus(long id) => new ApiEndpoint($"{BaseUri}/{id}/bus", HttpMethod.Get);

        public static IEndpoint Charge(long id, PaymentDetails payDetails) => new ApiEndpoint($"{BaseUri}/{id}/charge", HttpMethod.Post, payDetails , true);

        public static IEndpoint CreateReservation(CreateReservationInfo reservation) => new ApiEndpoint(BaseUri, HttpMethod.Post, reservation);

        public static IEndpoint CancelReservation(long id) => new ApiEndpoint($"{BaseUri}/cancel/{id}", HttpMethod.Put);


    }
}