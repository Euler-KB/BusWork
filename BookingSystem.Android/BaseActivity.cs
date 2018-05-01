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
using Android.Support.V7.App;
using Toolbar = Android.Support.V7.Widget.Toolbar;

namespace BookingSystem.Android
{
    public class BaseActivity : AppCompatActivity
    {
        int layoutId;

        private Toolbar supportToolbar;

        protected Toolbar Toolbar => supportToolbar;

        public event EventHandler<Bundle> OnLoaded;

        public BaseActivity(int layoutId)
        {
            this.layoutId = layoutId;
        }

        public void AllowBackNavigation()
        {
            var actionBar = SupportActionBar;
            if (actionBar != null)
            {
                actionBar.SetDefaultDisplayHomeAsUpEnabled(true);
                actionBar.SetDisplayShowHomeEnabled(true);
                actionBar.SetDisplayHomeAsUpEnabled(true);
            }
        }

        protected override void OnCreate(Bundle savedInstanceState)
        {
            //
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);

            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(layoutId);

            //  Get support toolbar
            supportToolbar = FindViewById<Toolbar>(Resource.Id.support_toolbar);
            if (supportToolbar != null)
            {
                SetSupportActionBar(supportToolbar);
            }

       

            //
            OnLoaded?.Invoke(this, savedInstanceState);
        }

        protected virtual int? GetMenuResource()
        {
            return null;
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            var menuId = GetMenuResource();
            if (menuId != null)
            {
                MenuInflater.Inflate(menuId.Value, menu);
            }

            return base.OnCreateOptionsMenu(menu);
        }

        protected virtual bool CanGoBack()
        {
            return true;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            if (item.ItemId == global::Android.Resource.Id.Home)
            {
                Finish();
                return true;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {

            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down)
            {
                if (!CanGoBack())
                    return true;

            }

            return base.OnKeyDown(keyCode, e);
        }
    }
}