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

namespace BookingSystem.Android.API.Endpoints
{
    public static class WalletEndpoints
    {
        public static readonly string BaseUri = "api/wallet";

        public static IEndpoint GetMyWallets() => new ApiEndpoint(BaseUri, HttpMethod.Get);

        public static IEndpoint GetUserWallets(long id) => new ApiEndpoint($"{BaseUri}/user/{id}", HttpMethod.Get);

        public static IEndpoint GetWallet(long id) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Get);

        public static IEndpoint GetWalletSummary() => new ApiEndpoint($"{BaseUri}/wallet/summary", HttpMethod.Get);

        public static IEndpoint DeleteWallet(long id) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Delete);

        public static IEndpoint UpdateWallet(long id, EditWalletInfo walletInfo) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Put, walletInfo);

        public static IEndpoint CreateWallet(string provider, string value) => new ApiEndpoint(BaseUri, HttpMethod.Post, new CreateWalletInfo()
        {
            Provider = provider,
            Value = value
        });
    }
}