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
using System.Diagnostics;
using System.Threading.Tasks;
using System.Threading;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace BookingSystem.Android.Helpers
{
    public static class DialogHelpers
    {
        public class DialogDisposable : IDisposable
        {
            ProgressDialog dlg;

            public ProgressDialog Dialog => dlg;

            public DialogDisposable(Func<ProgressDialog> begin)
            {
                dlg = begin();
            }

            public void Dismiss()
            {
                dlg?.Dismiss();
            }

            public void Dispose()
            {
                Dismiss();
            }
        }

        public static DialogDisposable ShowProgress(this Context context, string title, string message, bool indeterminate = true)
        {
            return new DialogDisposable(() => ProgressDialog.Show(context, title, message, indeterminate));
        }

        public static Task<bool?> ShowConfirm(this Context context, string title, string message, string positiveBtn = "OK", string negativeBtn = "Cancel")
        {
            ManualResetEvent handle = new ManualResetEvent(false);

            bool? result = null;

            var builder = new AlertDialog.Builder(context);

            if (title != null)
                builder.SetTitle(title);

            if (message != null)
            {
                if (message.Contains("<") || message.Contains(">"))
                    builder.SetHtml(message);
                else
                    builder.SetMessage(message);
            }

            if (positiveBtn != null)
                builder.SetPositiveButton(positiveBtn, delegate { result = true; });

            if (negativeBtn != null)
                builder.SetNegativeButton(negativeBtn, delegate { result = false; });

            var dlg = builder.Create();
            dlg.Show();

            dlg.DismissEvent += delegate
            {
                handle.Set();
            };

            return Task.Run(() =>
            {
                handle.WaitOne();
                return result;
            });
        }

        public static Task<bool?> ShowConfirm(this Context context, string message, string positiveBtn = "OK", string negativeBtn = "Cancel")
        {
            return ShowConfirm(context, null, message, positiveBtn, negativeBtn);
        }

        [Conditional("DEBUG")]
        public static void ShowDebugMessage(string title, string message)
        {
            var context = CustomApplication.CurrentActivity ?? Application.Context;
            if (context != null)
            {
                //  Print to output window
                System.Diagnostics.Debug.WriteLine(message);

                new global::Android.Support.V7.App.AlertDialog.Builder(context)
                    .SetTitle(title)
                    .SetMessage(message)
                    .SetPositiveButton("OK", delegate { })
                    .Show();
            }
        }
    }
}