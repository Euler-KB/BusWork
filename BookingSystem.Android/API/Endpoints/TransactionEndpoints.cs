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
using BookingSystem.API.Models;
using System.Net.Http;

namespace BookingSystem.Android.API.Endpoints
{
    public static class TransactionEndpoints
    {
        public static readonly string BaseUri = "api/transactions";

        public static IEndpoint GetTranasctions(TransactionType[] types, TransactionStatus[] statuses) => new ApiEndpoint(BaseUri, HttpMethod.Get);

        public static IEndpoint GetForWallet(long walletId) => new ApiEndpoint($"{BaseUri}/{walletId}/wallet", HttpMethod.Get);

        public static IEndpoint GetForReservation(long reservationId) => new ApiEndpoint($"{BaseUri}/{reservationId}/reservation", HttpMethod.Get);

        public static IEndpoint GetSummary(QueryOptions queryOptions = null) => new ApiEndpoint($"{BaseUri}/summary", HttpMethod.Get).SetQueryOptions(queryOptions);

    }
}