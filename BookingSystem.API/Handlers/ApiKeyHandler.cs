using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Net;
using System.Net.Http;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using System.Web;

namespace BookingSystem.API.Handlers
{
    public class ApiKeyHandler : DelegatingHandler
    {
        static readonly string apiKey;

        static ApiKeyHandler()
        {
            apiKey = ConfigurationManager.AppSettings["API_KEY"];
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken)
        {
            if (!request.Headers.Contains("X-Api-Key") || request.Headers.GetValues("X-Api-Key").FirstOrDefault() != apiKey)
            {
                return request.CreateErrorResponse(HttpStatusCode.Unauthorized, "Invalid api-key!");
            }

            return await base.SendAsync(request, cancellationToken);
        }

    }
}