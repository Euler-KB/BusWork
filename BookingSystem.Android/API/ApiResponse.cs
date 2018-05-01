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
using System.Threading.Tasks;
using System.Net;
using Newtonsoft.Json;
using BookingSystem.Android.API.Endpoints;

namespace BookingSystem.Android.API
{
    public class ApiResponse
    {
        public HttpResponseMessage ServerResponse { get; }

        public HttpRequestException Exception { get; }

        public ApiResponse(HttpRequestException exception, IEndpoint endpoint)
        {
            Exception = exception;
            Endpoint = endpoint;
        }

        public ApiResponse(HttpResponseMessage response, IEndpoint endpoint)
        {
            ServerResponse = response;
            Endpoint = endpoint;
        }

        public HttpStatusCode StatusCode
        {
            get
            {
                if (ServerResponse == null)
                {
                    throw new InvalidOperationException("Cannot get status code. Server response is null");
                }

                return ServerResponse.StatusCode;
            }

        }

        public IEndpoint Endpoint { get; }

        public bool NotFound
        {
            get
            {
                return ServerResponse?.StatusCode == HttpStatusCode.NotFound;
            }
        }

        public bool BadRequest
        {
            get
            {
                return ServerResponse?.StatusCode == HttpStatusCode.BadRequest;
            }
        }

        public bool Successful
        {
            get
            {
                return ServerResponse?.IsSuccessStatusCode == true;
            }
        }

        public bool IsCancelled
        {
            get
            {
                return InnerWebException?.Status == WebExceptionStatus.RequestCanceled;
            }
        }

        public WebException InnerWebException
        {
            get
            {
                return Exception?.InnerException as WebException;
            }
        }

        public bool Timeout
        {
            get
            {
                return InnerWebException?.Status == WebExceptionStatus.Timeout;
            }
        }

        public bool HasStatus(HttpStatusCode status)
        {
            return ServerResponse != null && ServerResponse.StatusCode == status;
        }


        public bool ConnectionError
        {
            get
            {
                var ex = InnerWebException;
                if (ex != null)
                {
                    switch (ex.Status)
                    {
                        case WebExceptionStatus.ConnectFailure:
                        case WebExceptionStatus.ConnectionClosed:
                        case WebExceptionStatus.ReceiveFailure:
                        case WebExceptionStatus.SecureChannelFailure:
                        case WebExceptionStatus.TrustFailure:
                        case WebExceptionStatus.SendFailure:
                        case WebExceptionStatus.Timeout:
                            return ServerResponse == null;
                    }
                }

                return false;
            }
        }

        public async Task<T> GetDataAsync<T>()
        {
            var content = await ServerResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeObject<T>(content);
        }

        public async Task<T> GetDataAsAnonymous<T>(T anonymousObject)
        {
            var content = await ServerResponse.Content.ReadAsStringAsync();
            return JsonConvert.DeserializeAnonymousType<T>(content, anonymousObject);
        }

    }
}