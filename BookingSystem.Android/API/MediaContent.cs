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
using System.IO;

namespace BookingSystem.Android.API
{
    public class MediaContent : StreamContent
    {
        public MediaContent(string mimeType, Stream content) : base(content)
        {
            Headers.ContentType = new System.Net.Http.Headers.MediaTypeHeaderValue(mimeType);
        }
    }
}