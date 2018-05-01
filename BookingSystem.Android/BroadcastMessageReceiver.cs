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
    public class BroadcastMessageReceiver : BroadcastReceiver
    {
        public event EventHandler<Intent> BroadcastReceived;

        public override void OnReceive(Context context, Intent intent)
        {
            BroadcastReceived?.Invoke(this, intent);
        }
    }
}