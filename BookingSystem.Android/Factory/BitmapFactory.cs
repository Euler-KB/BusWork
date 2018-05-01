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
using Android.Graphics;
using System.Threading.Tasks;
using System.IO;
using BookingSystem.Android.API;
using BookingSystem.API.Models.DTO;
using Square.Picasso;

namespace BookingSystem.Android.Factory
{
    public static class BitmapFactory
    {
        public static void LoadIntoImageView(MediaInfo media, ImageView imageView, bool thumbnail = true)
        {
            Picasso.With(CustomApplication.CurrentActivity ?? Application.Context)
                          .Load(global::Android.Net.Uri.Parse(ProxyFactory.GetProxyInstace().GetUri(thumbnail ? media.Uri : media.ThumbnailUri)))
                          .Fit()
                          .Into(imageView);
        }

    }
}