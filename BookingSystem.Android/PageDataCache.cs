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

namespace BookingSystem.Android
{
    /// <summary>
    /// A wrapper for storing page data using SettingsStore Manager internally
    /// </summary>
    public static class PageDataCache
    {
        public static void Save<T>(Type pageType, T data) where T : class
        {
            Task.Factory.StartNew(delegate
            {
                SettingsStore.SetAs(pageType.ToString(), data);
            });
        }

        public static bool HasData(Type pageType)
        {
            return SettingsStore.Contains(pageType.ToString());
        }

        public static void ClearData(Type pageType)
        {
            SettingsStore.Remove(pageType.ToString());
        }

        public static T Get<T>(Type pageType) where T : class
        {
            return SettingsStore.GetAs<T>(pageType.ToString());
        }
    }
}