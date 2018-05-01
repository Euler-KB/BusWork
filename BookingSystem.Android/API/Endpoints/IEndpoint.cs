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
using System.Threading.Tasks;
using System.Net.Http;

namespace BookingSystem.Android.API.Endpoints
{
    public interface IEndpoint
    {
        /// <summary>
        /// The uri for the endpoint
        /// </summary>
        Uri Uri { get; }

        /// <summary>
        /// Ensures whether the user is authenticated
        /// </summary>
        bool RequireAuth { get; }

        /// <summary>
        /// Generates a request message to be sent to server directly
        /// </summary>
        /// <returns></returns>
        Task<HttpRequestMessage> GetRequstAsync(Uri baseUri);
    }
}