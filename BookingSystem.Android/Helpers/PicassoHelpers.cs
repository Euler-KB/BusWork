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
using BookingSystem.API.Models.DTO;
using Square.Picasso;
using BookingSystem.Android.Factory;

namespace BookingSystem.Android.Helpers
{
    public static class PicassoHelpers
    {
        public static void InvalidateMedia(MediaInfo media)
        {
            var proxy = ProxyFactory.GetProxyInstace();

            var picasso = Picasso.With(Application.Context);

            if (media.ThumbnailUri != null)
                picasso.Invalidate(proxy.GetUri(media.ThumbnailUri));

            picasso.Invalidate(proxy.GetUri(media.Uri));
        }

        public static Task ClearCaches()
        {
            return Task.Run(delegate
            {
                //  Clear image cache

            });
        }
    }
}