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
using System.IO;

namespace BookingSystem.Android.API.Endpoints
{
    public static class BusesEndpoints
    {
        public static readonly string BaseUri = "api/buses";

        public static IEndpoint GetAll() => new ApiEndpoint(BaseUri)
        {
            Method = HttpMethod.Get,
            RequireAuth = true
        };

        public static IEndpoint Get(long id) => new ApiEndpoint($"{BaseUri}/{id}")
        {
            Method = HttpMethod.Get,
            RequireAuth = true
        };

        public static IEndpoint Delete(long id) => new ApiEndpoint($"{BaseUri}/{id}")
        {
            Method = HttpMethod.Delete,
            RequireAuth = true
        };

        public static IEndpoint Create(CreateBusInfo model) => new ApiEndpoint(BaseUri, HttpMethod.Post, model);

        public static IEndpoint UploadPhoto(long id, Stream stream, string mimeType) => new ApiEndpoint($"{BaseUri}/{id}/photo", HttpMethod.Post)
            .SetContent(stream, mimeType);

        public static IEndpoint RemovePhoto(long id) => new ApiEndpoint($"{BaseUri}/{id}/photo", HttpMethod.Delete);

        public static IEndpoint Update(long id, EditBusInfo model) => new ApiEndpoint($"{BaseUri}/{id}", HttpMethod.Put, model);
    }
}