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
using BookingSystem.Android.API;

namespace BookingSystem.Android.Factory
{
    public static class ProxyFactory
    {
        /// <summary>
        /// The current proxy instance
        /// </summary>
        static ServiceProxy _proxyInstance;

        public static ServiceProxy GetProxyInstace(bool ensureAuthenticated = false)
        {
            if(_proxyInstance == null)
            {
                //  Instantiate a new proxy here
                _proxyInstance = new ServiceProxy();

            }

            if (ensureAuthenticated)
            {
                _proxyInstance.EnsureEnthenticated();
            }

            return _proxyInstance;
        }

    }
}