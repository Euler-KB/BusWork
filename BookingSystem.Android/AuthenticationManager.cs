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

namespace BookingSystem.Android
{
    /// <summary>
    /// Manages authentication for users
    /// </summary>
    public static class AuthenticationManager
    {
        public const string AuthSettingKey = "authInfo";

        public static bool HasSession
        {
            get
            {
                return SettingsStore.Contains(AuthSettingKey);
            }
        }

        public static AuthenticationInfo CurrentSession
        {
            get
            {
                return SettingsStore.GetAs<AuthenticationInfo>(AuthSettingKey);
            }

            set
            {
                SettingsStore.SetAs(AuthSettingKey, value);
            }
        }

        public static void Destroy()
        {
            SettingsStore.Remove(AuthSettingKey);
        }
    }
}