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
using Android.Support.Design.Widget;
using Newtonsoft.Json;
using System.Reflection;
using Android.Text;

namespace BookingSystem.Android.Helpers
{
    public static class Extensions
    {
        public static bool AnyUpdate(this object obj)
        {
            foreach (var mem in obj.GetType().GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (mem.GetValue(obj) != null)
                    return true;
            }


            return true;
        }

        public static global::Android.Support.V7.App.AlertDialog.Builder SetHtml(this global::Android.Support.V7.App.AlertDialog.Builder builder, string html)
        {
            return builder.SetMessage(global::Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.N ? Html.FromHtml(html, FromHtmlOptions.ModeCompact) : Html.FromHtml(html));
        }

        public static void SetHtml(this TextView txtView, string html)
        {
            txtView.SetText(global::Android.OS.Build.VERSION.SdkInt > BuildVersionCodes.N ? Html.FromHtml(html, FromHtmlOptions.ModeCompact) : Html.FromHtml(html), TextView.BufferType.Spannable);
        }

        public static bool ContainsIgnoreCase(this string str, string keyword)
        {
            return str.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        public static bool IsValidString(this string str)
        {
            return str != null && str.Trim().Length > 0;
        }

        public static string TrimInput(this EditText editText)
        {
            return (editText.Text = editText.Text.Trim());
        }

        public static string ToNumericStandard(this double value, int decimalPlaces = 0)
        {
            return value.ToString($"N{decimalPlaces}");
        }

        public static string ToNumericStandard(this long value)
        {
            return value.ToString("N0");
        }

        public static bool SetError(this TextInputLayout textInputLayout, string error)
        {
            if (error == null)
            {
                textInputLayout.ErrorEnabled = false;
                textInputLayout.Error = null;
                return false;

            }
            else
            {
                textInputLayout.ErrorEnabled = true;
                textInputLayout.Error = error;
                return true;
            }
        }

        public static string GetHeader(this ApiResponse response, string key)
        {
            return response.ServerResponse?.Headers.GetValues(key).FirstOrDefault();
        }

        public static string GetResponseMessage(this ApiResponse response)
        {
            return JsonConvert.DeserializeAnonymousType(response.ServerResponse?.Content.ReadAsStringAsync().Result, new { Message = "" }).Message;
        }

        public static IEnumerable<KeyValuePair<string, string>> GetValidationErrors(this ApiResponse response)
        {
            return null;
        }

        public static string GetErrorDescription(this ApiResponse response)
        {
            if (response.Successful)
                return "Successful";
            else
            {

                if (response.ConnectionError)
                    return Resources.ERR_MSG_CONNECTION;

                if (response.Timeout)
                    return Resources.ERR_MSG_TIMEOUT;
                else
                {
                    if (response.BadRequest)
                        return response.GetResponseMessage();

                    return Resources.ERR_MSG_SERVER_ERROR;
                }
            }
        }
    }
}