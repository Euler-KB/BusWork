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
using System.IO;
using Newtonsoft.Json;
using System.Collections.Concurrent;

namespace BookingSystem.Android
{
    public static class SettingsStore
    {
        public static readonly string SharedPreferenceName = "_user_data.pref";

        public static readonly string DefaultKey = "settings";

        static ConcurrentDictionary<string,string> valuesMap;

        static bool isLoaded;

        static bool isDirty;

        public static bool IsLoaded => isLoaded;

        public static void Load(bool deleteOnFail = true)
        {
            if (isLoaded)
                return;

            var editor = Application.Context.GetSharedPreferences(SharedPreferenceName,FileCreationMode.Private);

            if (editor.Contains(DefaultKey))
            {
                try
                {
                    var settingsPayload = editor.GetString(DefaultKey, null);
                    if (settingsPayload != null)
                    {
                        valuesMap = new ConcurrentDictionary<string, string>( JsonConvert.DeserializeObject<IDictionary<string, string>>(settingsPayload) );
                    }

                }
                catch (JsonSerializationException)
                {
                    //  Failed loading settings
                }

            }

            if (valuesMap == null)
            {
                valuesMap = new ConcurrentDictionary<string, string>();
            }

            isLoaded = true;
        }

        static void EnsureLoaded()
        {
            if (!isLoaded)
                throw new InvalidOperationException("Store not loaded yet!");
        }

        public static void Save()
        {
            EnsureLoaded();

            if (!isDirty)
                return;

            string payload = JsonConvert.SerializeObject(valuesMap);
            var editor = Application.Context.GetSharedPreferences(SharedPreferenceName, FileCreationMode.Private).Edit();
            editor.PutString(DefaultKey, payload);
            editor.Apply();

            isDirty = false;
        }

        public static void SetAs<T>(string key, T value) where T : class
        {
            Set(key, JsonConvert.SerializeObject(value));
        }

        public static void Set(string key, string value)
        {
            EnsureLoaded();
            valuesMap[key] = value;
            isDirty = true;
        }

        public static void Remove(string key)
        {
            EnsureLoaded();
            valuesMap.TryRemove(key,out _);
            isDirty = true;
        }

        public static T GetAs<T>(string key)
        {
            EnsureLoaded();
            return JsonConvert.DeserializeObject<T>(valuesMap[key]);
        }

        public static bool Contains(string key)
        {
            EnsureLoaded();
            return valuesMap.ContainsKey(key);
        }

        public static void Clear()
        {
            EnsureLoaded();
            valuesMap.Clear();
            isDirty = true;
        }

        public static string Get(string key)
        {
            EnsureLoaded();
            return valuesMap[key];
        }
    }
}