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
using BookingSystem.Android.Factory;
using System.Threading.Tasks;
using System.Timers;
using Newtonsoft.Json;

namespace BookingSystem.Android
{
    public static class UserPreferences
    {
        public static class Keys
        {
            public const string WalletId = "_defWalletId";
        }

        public static class Default
        {

            public static long? PrimaryWalletId
            {
                get
                {
                    if (Contains(Keys.WalletId))
                    {
                        return GetLong(Keys.WalletId);
                    }

                    return null;
                }

                set
                {
                    Set(Keys.WalletId, value.Value);
                }
            }

        }


        static IDictionary<string, string> _preferences;
        static bool isLoaded;
        static Task<API.ApiResponse> loadTask;

        public static async Task Load()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            loadTask = proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.GetPreferences());

            var response = await loadTask;
            if (response.Successful)
            {
                _preferences = await response.GetDataAsync<IDictionary<string, string>>();
                isLoaded = true;
            }
            else
            {
                //  Fallback to local storage
                if (_preferences == null)
                    _preferences = new Dictionary<string, string>();
            }
        }

        static void EnsureLoaded()
        {
            if (!isLoaded)
            {
                if (loadTask != null)
                {

                    if (loadTask.Status == TaskStatus.Running || loadTask.Status == TaskStatus.WaitingToRun)
                        loadTask.Wait();

                }
                else
                {
                    Load().Wait();
                }
            }
        }

        public static int GetInt(string key, Func<int> getDefault = null)
        {

            EnsureLoaded();

            if (_preferences.ContainsKey(key))
                return int.Parse(_preferences[key]);

            return getDefault();
        }

        public static bool GetBool(string key, Func<bool> getDefault = null)
        {
            EnsureLoaded();

            if (_preferences.ContainsKey(key))
                return bool.Parse(_preferences[key]);

            return getDefault();
        }

        public static long GetLong(string key, Func<long> getDefault = null)
        {
            EnsureLoaded();

            if (_preferences.ContainsKey(key))
                return long.Parse(_preferences[key]);

            return getDefault();
        }

        public static string GetString(string key, Func<string> getDefault = null)
        {
            EnsureLoaded();

            if (_preferences.ContainsKey(key))
                return _preferences[key];

            return getDefault();
        }

        public static bool Contains(string key)
        {
            EnsureLoaded();
            return _preferences.ContainsKey(key);
        }

        public static T GetObject<T>(string key, Func<T> getDefault = null)
        {
            EnsureLoaded();
            if (_preferences.ContainsKey(key))
                return JsonConvert.DeserializeObject<T>(_preferences[key]);

            return getDefault();
        }

        public static async Task<bool> Set(string key, string value)
        {
            EnsureLoaded();

            var proxy = ProxyFactory.GetProxyInstace();
            _preferences[key] = value;
            return (await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.SetPreference(key, value))).Successful;
        }

        public static Task<bool> Set(string key, int value)
        {
            return Set(key, value.ToString());
        }

        public static Task<bool> Set<T>(string key, T value)
        {
            return Set(key, JsonConvert.SerializeObject(value));
        }

        public static Task<bool> Set(string key, bool value)
        {
            return Set(key, value.ToString());
        }

        public static Task<bool> Set(string key, long value)
        {
            return Set(key, value.ToString());
        }

    }
}