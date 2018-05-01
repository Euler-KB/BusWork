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

namespace BookingSystem.Android
{
    public class ClickListener : Java.Lang.Object, View.IOnClickListener
    {
        protected Action<View> _clickHandler;

        public ClickListener(Action<View> onClick)
        {
            _clickHandler = onClick;
        }

        public void OnClick(View v)
        {
            _clickHandler?.Invoke(v);
        }
    }
}