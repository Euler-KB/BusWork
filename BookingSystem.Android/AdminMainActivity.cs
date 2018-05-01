using Android.App;
using Android.Widget;
using Android.OS;
using Android.Views;
using System;

namespace BookingSystem.Android
{
    [Activity(Label = "@string/AppName", Icon = "@drawable/icon")]
    public class AdminMainActivity : BaseDrawerActivity
    {
        static MenuLayoutBinding[] Bindings = new MenuLayoutBinding[]
        {
           new MenuLayoutBinding()
            {
                Title = Application.Context.Resources.GetString(Resource.String.AppName),
                Layout = Resource.Layout.admin_home_layout,
                MenuItemId = Resource.Id.nav_action_home,
                PageType = typeof(Pages.AdminHomePage),
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
                Flags = MenuFlag.Search | MenuFlag.AddButton
            },

            new MenuLayoutBinding()
            {
                Title = "Reservations",
                Layout = Resource.Layout.reservations_layout,
                PageType = typeof(Pages.ReservationsPage),
                MenuItemId = Resource.Id.nav_action_reservations,
                Icon = Resource.Drawable.ic_book_black_18dp,
                ToolbarIcon = Resource.Drawable.ic_book_white_18dp,
                Flags = MenuFlag.Search
            },

            new MenuLayoutBinding()
            {
                Title = "Feedback(s)",
                Layout = Resource.Layout.feedbacks_layout,
                MenuItemId = Resource.Id.nav_action_feedback,
                PageType = typeof(Pages.FeedbackPage),
                Icon = Resource.Drawable.ic_feedback_black_24dp,
                ToolbarIcon = Resource.Drawable.ic_feedback_white_24dp,
                Flags = MenuFlag.Refresh
            }
        };

        public AdminMainActivity()
        {

        }

        protected override int GetHeaderLayout() => Resource.Layout.nav_user_info;

        protected override int GetNavigationMenu() => Resource.Menu.navigation_menu_admin;

        protected override int? GetMenuResource() => Resource.Menu.actions_main;

        /// <summary>
        /// Gets binding for menu items
        /// </summary>
        /// <returns></returns>
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
                    StartActivity(typeof(UserProfileActivity));
                    break;
                case Resource.Id.action_about:
                    StartActivity(typeof(AboutActivity));
                    break;
                case Resource.Id.action_signout:
                    ConfirmSignout();
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

    }
}

