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

namespace BookingSystem.Android.Pages
{
    public class BasePage : global::Android.Support.V4.App.Fragment
    {
        public int LayoutId { get; set; }

        public object UserState { get; set; }

        public event EventHandler<View> OnLoaded;

        public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
        {
            //  Load view
            var view = inflater.Inflate(LayoutId, container, false) as ViewGroup;

            //  Initialize view
            OnLoaded?.Invoke(this, view);

            return view;
        }

    }
}