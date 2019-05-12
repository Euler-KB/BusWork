using Android.OS;
using Android.Runtime;
using Android.Support.Design.Widget;
using Android.Support.V4.View;
using Android.Support.V4.Widget;
using Android.Views;
using Android.Widget;
using BookingSystem.Android.Factory;
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Pages;
using BookingSystem.Android.Views;
using System;
using System.Linq;
using SearchView = Android.Support.V7.Widget.SearchView;

namespace BookingSystem.Android
{
    public enum MenuFlag
    {
        /// <summary>
        /// Show search button
        /// </summary>
        Search = 1,

        /// <summary>
        /// Show refresh button
        /// </summary>
        Refresh = 2,

        /// <summary>
        /// Show add button
        /// </summary>
        AddButton = 4,

        /// <summary>
        /// The default page
        /// </summary>
        DefaultPage = 8
    }

    public class MenuLayoutBinding
    {
        public int MenuItemId { get; set; }

        public int Layout { get; set; }

        public string Title { get; set; }

        public object UserState { get; set; }

        public MenuFlag Flags { get; set; }

        public int? Icon { get; set; }

        public int ? ToolbarIcon { get; set; }

        public Type PageType { get; set; }
    }

    public abstract class BaseDrawerActivity : BaseActivity
    {
        private class SimpleFragmentView : global::Android.Support.V4.App.Fragment
        {
            private int LayoutId;

            public SimpleFragmentView(int layoutId)
            {
                this.LayoutId = layoutId;
            }

            public override View OnCreateView(LayoutInflater inflater, ViewGroup container, Bundle savedInstanceState)
            {
                return inflater.Inflate(LayoutId, container, false);
            }
        }

        protected DrawerLayout drawerLayout;
        protected NavigationView navigationView;
        protected FloatingActionButton fabAddItem;
        protected View navMenuHeader;
        private IMenuItem refreshMenuItem, searchBar;
        private SearchView searchView;


        //  Current view layout
        private MenuLayoutBinding currentBinding = null;
        private object currentPage = null;

        /// <summary>
        /// Gets the current page
        /// </summary>
        public object CurrentPage => currentPage;

        /// <summary>
        /// Invoked to add item to view
        /// </summary>
        public event EventHandler OnFabClicked;

        protected virtual MenuLayoutBinding[] GetMenuLayoutBindings()
        {
            return null;
        }

        public BaseDrawerActivity() : base(Resource.Layout.drawer_base_layout)
        {
            OnLoaded += delegate
            {
                //
                SupportActionBar.SetDisplayHomeAsUpEnabled(true);
                SupportActionBar.SetDisplayShowHomeEnabled(true);
                SupportActionBar.SetHomeButtonEnabled(true);

                //
                //Toolbar.NavigationClick += delegate
                //{
                //    DrawerOpen = true;
                //};

                //
                fabAddItem = FindViewById<FloatingActionButton>(Resource.Id.fab_add_item);

                //  Drawer Layout
                drawerLayout = FindViewById<DrawerLayout>(Resource.Id.drawer_main);
                SetupDrawerLayout();

                //  Navigation view
                navigationView = FindViewById<NavigationView>(Resource.Id.navigation_view_main);
                SetupNavigationView();

                //
                fabAddItem.Click += (s, e) =>
                {
                    //
                    (currentPage as IAddItemPage)?.OnAddItem();

                    //
                    OnFabClicked?.Invoke(this, e);
                };

                //
                var bindings = GetMenuLayoutBindings();
                if (bindings != null)
                {
                    //
                    var b = bindings.FirstOrDefault(x => x.Flags.HasFlag(MenuFlag.DefaultPage)) ?? bindings.FirstOrDefault();
                    if (b != null)
                    {
                        //  Set checked
                        navigationView.SetCheckedItem(b.MenuItemId);

                        //  Set content layout
                        SetContentLayout(b);
                    }
                }
            };
        }

        private void SetupDrawerLayout()
        {
            //  create drawer toggle or whatever

        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            //
            refreshMenuItem = menu.FindItem(Resource.Id.action_refresh);

            //
            searchBar = menu.FindItem(Resource.Id.search_bar);
            searchView = (SearchView)searchBar.ActionView;

            //
            searchView.QueryTextSubmit += (s, e) =>
            {
                e.Handled = true;
                OnSearchItem(e.Query);
            };

            searchView.QueryTextChange += (s, e) =>
            {
                e.Handled = true;
                OnSearchItem(e.NewText);
            };

            searchView.Close += (s, e) =>
            {
                e.Handled = false;
                (currentPage as ISearchPage)?.EndSearch();
            };

            if (currentBinding != null)
            {
                //  Apply binding visibility
                searchBar?.SetVisible(currentBinding.Flags.HasFlag(MenuFlag.Search));
                refreshMenuItem?.SetVisible(currentBinding.Flags.HasFlag(MenuFlag.Refresh));
            }

            return true;
        }

        protected void OnSearchItem(string query)
        {
            (currentPage as ISearchPage)?.Search(query);
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case global::Android.Resource.Id.Home:
                    DrawerOpen = true;
                    return true;
                case Resource.Id.action_refresh:
                    {
                        (currentPage as IRefreshPage)?.OnRefresh();
                        return true;
                    }

            }

            return base.OnOptionsItemSelected(item);
        }

        private void SetupNavigationView()
        {
            //
            navigationView.NavigationItemSelected += OnNavigationItemSelected;

            //  Inflate view
            navMenuHeader = navigationView.InflateHeaderView(GetHeaderLayout());

            //  update set to false because we're now setting it up
            OnSetupNavigationMenuHeader(navMenuHeader, update: false);
            navigationView.InflateMenu(GetNavigationMenu());
        }

        protected virtual void OnSetupNavigationMenuHeader(View view, bool update)
        {
            var proxy = ProxyFactory.GetProxyInstace(ensureAuthenticated: true);
            var user = proxy.User;

            //  Setup greetings
            var lbMessage = view.FindViewById<TextView>(Resource.Id.lb_msg);
            lbMessage.Text = $"Hello, {user.FullName.Split(' ').FirstOrDefault()}";

            //  Setup profile image
            var profileImage = view.FindViewById<AvatarDisplay>(Resource.Id.user_image_frame);

            if (update)
            {
                profileImage.Name = user.FullName;

                if (user.ProfileImage != null)
                    profileImage.SetPhoto(user.ProfileImage);
            }
            else
            {
                //  Load bitmap in background
                profileImage.Name = user.FullName;

                if (user.ProfileImage != null)
                    profileImage.SetPhoto(user.ProfileImage);
            }
        }

        protected override void OnResume()
        {
            base.OnResume();

            //  Update navigation menu header
            OnSetupNavigationMenuHeader(navMenuHeader, true);

            //  Wire event
            ProxyFactory.GetProxyInstace().OnUserUpdated += OnUserProfileUpdated;
        }

        protected override void OnPause()
        {
            base.OnPause();

            //  Unwire event
            ProxyFactory.GetProxyInstace().OnUserUpdated -= OnUserProfileUpdated;
        }

        private void OnUserProfileUpdated(object sender, API.AuthenticationInfo e)
        {
            //  Update
            OnSetupNavigationMenuHeader(navMenuHeader, true);
        }

        protected void ConfirmSignout()
        {
            new global::Android.Support.V7.App.AlertDialog.Builder(this)
                  .SetTitle("Sign Out?")
                  .SetMessage("Are you sure you want to sign out?")
                  .SetPositiveButton("Yes, Signout", delegate
                  {
                      SignOut();
                  })
                  .SetNegativeButton("Cancel", delegate { })
                  .Show();

        }

        protected async void SignOut()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            await proxy.SignOut();
        }

        private void OnNavigationItemSelected(object sender, NavigationView.NavigationItemSelectedEventArgs e)
        {
            //  Not handled
            if (!OnNavigationItemClicked(e.MenuItem))
            {
                var bindings = GetMenuLayoutBindings();
                if (bindings != null)
                {
                    var bind = bindings.FirstOrDefault(x => x.MenuItemId == e.MenuItem.ItemId);
                    if (bind != null)
                    {
                        //  Navigate to page
                        SetContentLayout(bind);
                    }
                }
            }

            DrawerOpen = false;
        }


        /// <summary>
        /// Gets the layout for the header
        /// </summary>
        /// <returns></returns>
        protected abstract int GetHeaderLayout();

        /// <summary>
        /// Gets the layout for the menu
        /// </summary>
        /// <returns></returns>
        protected abstract int GetNavigationMenu();

        /// <summary>
        /// Called when a navigation item is clicked
        /// </summary>
        protected virtual bool OnNavigationItemClicked(IMenuItem menu)
        {
            //  Not handled
            return false;
        }

        public override bool OnKeyDown([GeneratedEnum] Keycode keyCode, KeyEvent e)
        {
            if (keyCode == Keycode.Back && e.Action == KeyEventActions.Down && DrawerOpen)
            {
                DrawerOpen = false;
                return true;
            }

            //  Just regular back action
            if (keyCode == Keycode.Back)
            {
                //  Hide activity
                this.HideActivity();
                return true;

            }

            return base.OnKeyDown(keyCode, e);
        }

        public bool ShowAddButton
        {
            get
            {
                return fabAddItem.Visibility == ViewStates.Visible;
            }

            set
            {
                if (value)
                {
                    fabAddItem.Visibility = ViewStates.Visible;
                }
                else
                {
                    fabAddItem.Visibility = ViewStates.Gone;
                }
            }
        }

        /// <summary>
        /// Sets the main layout view
        /// </summary>
        protected void SetContentLayout(MenuLayoutBinding binding)
        {
            if (currentBinding == binding)
                return;

            //
            if(currentPage is BasePage page)
            {
                page.OnLeavePage();
            }

            //
            var basePage = Activator.CreateInstance(binding.PageType) as BasePage;
            if (basePage != null)
            {
                basePage.LayoutId = binding.Layout;
                basePage.UserState = binding.UserState;
            }

            try
            {
                SupportFragmentManager.BeginTransaction()
                      .Replace(Resource.Id.content_frame, basePage)
                      .Commit();
            }
            catch(Exception ex)
            {
                LogHelpers.Write($"Failed replacing fragment view" + System.Environment.NewLine + ex.ToString());
            }
          

            //
            SupportActionBar.Title = binding.Title;

            //
            SupportActionBar.SetHomeAsUpIndicator(binding.ToolbarIcon ?? binding.Icon ?? Resource.Drawable.ic_bus_black_18dp);

            //
            searchBar?.SetVisible(binding.Flags.HasFlag(MenuFlag.Search));
            refreshMenuItem?.SetVisible(binding.Flags.HasFlag(MenuFlag.Refresh));

            //
            currentBinding = binding;
            currentPage = basePage;

            //
            ShowAddButton = binding.Flags.HasFlag(MenuFlag.AddButton);
        }

        public bool DrawerOpen
        {
            get
            {
                return drawerLayout.IsDrawerOpen(GravityCompat.End);
            }
            set
            {
                if (value)
                {
                    drawerLayout.OpenDrawer(GravityCompat.Start);
                }
                else
                {
                    drawerLayout.CloseDrawers();
                }
            }
        }



    }
}