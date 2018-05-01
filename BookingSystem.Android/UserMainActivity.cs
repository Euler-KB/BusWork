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
    [Activity(Label = "@string/AppName")]
    public class UserMainActivity : BaseDrawerActivity
    {
        private readonly MenuLayoutBinding[] Bindings = new MenuLayoutBinding[]
        {
            new MenuLayoutBinding()
            {
                Title = Application.Context.Resources.GetString(Resource.String.AppName),
                Layout = Resource.Layout.user_home_layout,
                MenuItemId = Resource.Id.nav_action_home,
                PageType = typeof(Pages.UserHomePage),
                Icon = Resource.Drawable.ic_home_black_18dp,
                ToolbarIcon = Resource.Drawable.ic_home_white_18dp,
                Flags = MenuFlag.DefaultPage | MenuFlag.Refresh,
            },

            new MenuLayoutBinding()
            {
                Title = "Buses",
                Layout = Resource.Layout.buses_layout,
                MenuItemId = Resource.Id.nav_action_buses,
                PageType = typeof(Pages.BusesPage),
                Icon = Resource.Drawable.ic_bus_black_18dp,
                ToolbarIcon = Resource.Drawable.ic_bus_white_18dp,
                Flags = MenuFlag.Search
            },

            new MenuLayoutBinding()
            {
                Title = "Reservations",
                Layout = Resource.Layout.reservations_layout,
                PageType = typeof(Pages.ReservationsPage),
                MenuItemId = Resource.Id.nav_action_reservations,
                Icon = Resource.Drawable.ic_book_black_18dp,
                ToolbarIcon = Resource.Drawable.ic_book_white_18dp,
                Flags = MenuFlag.AddButton | MenuFlag.Search
            }

        };

        public UserMainActivity()
        {

        }

        protected override int GetHeaderLayout() => Resource.Layout.nav_user_info;

        protected override int GetNavigationMenu() => Resource.Menu.navigation_menu_user;

        protected override int? GetMenuResource() => Resource.Menu.actions_main;

        protected override MenuLayoutBinding[] GetMenuLayoutBindings() => Bindings;

        protected override bool OnNavigationItemClicked(IMenuItem menu)
        {
            if (menu.ItemId == Resource.Id.nav_signout)
            {
                ConfirmSignout();
                return true;
            }

            //  Wasn't handled
            return false;
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_user_profile:
                    StartActivity(new Intent(this, typeof(UserProfileActivity)));
                    return true;
                case Resource.Id.action_about:
                    StartActivity(new Intent(this, typeof(AboutActivity)));
                    return true;
                case Resource.Id.action_signout:
                    ConfirmSignout();
                    return true;
            }

            return base.OnOptionsItemSelected(item);
        }
    }
}