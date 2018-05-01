using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Collections.Specialized;
using System.IO;
using System.Runtime.Serialization;
using Newtonsoft.Json;
using BookingSystem.API.Models;

namespace BookingSystem.Android.API.Endpoints
{

    public class ApiEndpoint : IEndpoint
    {
        private string endpointAddress;

        private NameValueCollection queryValues = new NameValueCollection();

        private HttpRequestMessage request;

        public HttpMethod Method
        {
            get { return request.Method; }
            set { request.Method = value; }
        }

        public ApiEndpoint(string endpointAddress)
        {
            this.endpointAddress = endpointAddress;
            request = new HttpRequestMessage();
        }

        public ApiEndpoint(string endpointAddress, HttpMethod method, object jsonContent = null, bool requireAuthentication = true) : this(endpointAddress)
        {
            request.Method = method;

            if (jsonContent != null)
                SetJsonContent(jsonContent);

            RequireAuth = requireAuthentication;
        }

        public string RawAddress => endpointAddress;

        public ApiEndpoint AddHeader(string key, object value)
        {
            if (value != null)
            {
                request.Headers.Add(key, value.ToString());
            }
            return this;
        }

        public ApiEndpoint AddQuery(string key, object value)
        {
            if (value != null)
            {
                queryValues.Add(key, global::System.Uri.EscapeDataString(value.ToString()));
            }
            return this;
        }

        public ApiEndpoint SetQueryOptions(QueryOptions options)
        {
            if (options != null)
            {
                if (options.SearchKeyword != null)
                    AddQuery("SearchKeyword", options.SearchKeyword);

                if (options.FromRange != null)
                    AddQuery("FromRange", options.FromRange.Value);

                if (options.ToRange != null)
                    AddQuery("ToRange", options.ToRange.Value);

                if (options.Limit != null)
                    AddQuery("Limit", options.Limit);

                if (options.Offset != null)
                    AddQuery("Offset", options.Offset);

                if (options.CreatedAfter != null)
                    AddQuery("CreatedAfter", options.CreatedAfter);

                if (options.CreatedBefore != null)
                    AddQuery("CreatedBefore", options.CreatedBefore);

                if (options.SearchFields != null)
                    AddQuery("SearchFields", options.SearchFields);
            }

            return this;
        }

        public ApiEndpoint SetContent(Stream stream)
        {
            request.Content = new StreamContent(stream);
            return this;
        }

        public ApiEndpoint SetContent(Stream stream, string mimeType)
        {
            request.Content = new MediaContent(mimeType, stream);
            return this;
        }

        public ApiEndpoint SetContent(HttpContent content)
        {
            request.Content = content;
            return this;
        }


        public ApiEndpoint SetJsonContent<T>(T content)
        {
            request.Content = new StringContent(JsonConvert.SerializeObject(content), Encoding.UTF8, "application/json");
            return this;
        }


        public Uri Uri
        {
            get
            {
                //  build uri with 
                UriBuilder builder = new UriBuilder($"{Resources.APIBaseAddress}/{endpointAddress}");
                string query = "";
                foreach (string key in queryValues.Keys)
                {
                    query += $"&{key}={queryValues[key]}";
                }

                builder.Query = query.Length > 0 ? query.Substring(1) : "";
                return builder.Uri;

            }
        }

        public bool RequireAuth { get; set; }

        public Task<HttpRequestMessage> GetRequstAsync(Uri baseUri)
        {
            request.RequestUri = Uri;
            return Task.FromResult(request);

        }
    }
}