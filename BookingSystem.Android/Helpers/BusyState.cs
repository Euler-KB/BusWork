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

namespace BookingSystem.Android.Helpers
{
    public static class BusyState
    {
        private class DisposableAction : IDisposable
        {
            Action begin, exit;

            public DisposableAction(Action begin, Action exit = null)
            {
                this.begin = begin;
                this.exit = exit;

                //
                begin?.Invoke();
            }

            public void Dispose()
            {
                exit?.Invoke();
            }
        }

        public static IDisposable Begin(Action begin  , Action exit)
        {
            return new DisposableAction(begin,exit);
        }
    }
}