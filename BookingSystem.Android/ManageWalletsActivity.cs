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
using Android.Support.Design.Widget;
using Android.Support.V4.Widget;
using BookingSystem.Android.Helpers;
using BookingSystem.API.Models.DTO;
using System.Threading.Tasks;
using BookingSystem.Android.Factory;
using Android.Text;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.V4.View;
using BookingSystem.Android.Views;

namespace BookingSystem.Android
{
    [Activity(Label = "Manage Wallets")]
    public class ManageWalletsActivity : BaseActivity
    {
        public static readonly IDictionary<string, string> NetworkProviders = new Dictionary<string, string>()
        {
            { "MTN" ,  "MTN Mobile Money" },
            { "Tigo" ,  "Tigo Cash" },
            { "Airtel" , "Airtel Money" },
            { "Vodafone", "Vodafone Cash" }
        };

        private SwipeRefreshLayout swipeRefreshLayout;
        private SmartAdapter<WalletInfo> itemsAdapter;
        private IList<WalletInfo> wallets;

        private ViewGroup emptyFrame;
        private bool isBusy;
        private bool createOnly;
        private ListView listView;
        private string searchQuery;

        public ManageWalletsActivity() : base(Resource.Layout.manage_wallet_layout)
        {
            OnLoaded += async delegate
            {
                //
                AllowBackNavigation();

                createOnly = Intent.GetBooleanExtra("createOnly", false);

                //
                FindViewById<FloatingActionButton>(Resource.Id.fab_add_wallet).Click += OnAddWallet;
                listView = FindViewById<ListView>(Resource.Id.wallets_listview);
                emptyFrame = FindViewById<ViewGroup>(Resource.Id.empty_frame);
                //
                itemsAdapter = new SmartAdapter<WalletInfo>(this, Resource.Layout.user_wallet_item_layout, ViewHolders.ItemHolders.WalletItemBindings);
                listView.Adapter = itemsAdapter;

                //
                swipeRefreshLayout = FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                swipeRefreshLayout.SetColorSchemeColors(AvatarDisplay.DefaultColors.Take(4).Select(t => t.ToArgb()).ToArray());
                swipeRefreshLayout.Refresh += async delegate
                {
                    await RefreshView();
                };


                //
                AddWalletDialogFragment.OnCreated += OnWalletCreated;
                AddWalletDialogFragment.OnUpdated += OnWalletUpdate;
                ViewHolders.ItemHolders.OnWalletRemoved += OnWalletRemoved;

                await RefreshView(false, true);
            };

        }


        async Task RefreshView(bool indicator = true, bool interrogate = false)
        {
            if (isBusy)
                return;

            using (Busy())
            {
                await LoadWalletsAsync(interrogate);
            }
        }

        public void NotifyDataChanged()
        {
            itemsAdapter?.NotifyDataSetChanged();
        }


        private async void OnWalletRemoved(object sender, WalletInfo e)
        {
            await RefreshView(false, false);
        }

        protected async void OnWalletCreated(object sender, WalletInfo wallet)
        {

            await RefreshView(false, false).ContinueWith(t =>
            {
                if (wallets.Count == 1)
                {
                    RunOnUiThread(async delegate
                    {
                        if (await this.ShowConfirm("Configure Wallet", $"Do you want to make the wallet <b>{wallet.Provider}</b> - <b>{wallet.Value}</b> your default wallet?", "Yes", "No") == true)
                        {
                            UserPreferences.Default.PrimaryWalletId = wallet.Id;
                            itemsAdapter.NotifyDataSetChanged();
                        }

                        if (createOnly)
                        {
                            SetResult(Result.Ok);
                            Finish();
                        }
                    });

                }

            });



        }

        protected async void OnWalletUpdate(object sender, WalletInfo wallet)
        {
            await RefreshView(false, false);
        }

        protected override void OnDestroy()
        {
            AddWalletDialogFragment.OnCreated -= OnWalletCreated;
            AddWalletDialogFragment.OnUpdated -= OnWalletUpdate;
            ViewHolders.ItemHolders.OnWalletRemoved -= OnWalletRemoved;
            base.OnDestroy();
        }

        protected IEnumerable<WalletInfo> FilterWallets(IEnumerable<WalletInfo> wallets)
        {
            return searchQuery.IsValidString() ? wallets.Where(x => x.Provider.ContainsIgnoreCase(searchQuery) || x.Value.ContainsIgnoreCase(searchQuery)) : wallets;
        }

        protected override int? GetMenuResource() => Resource.Menu.actions_manage_wallet;

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            base.OnCreateOptionsMenu(menu);

            var searchView = (global::Android.Support.V7.Widget.SearchView)MenuItemCompat.GetActionView(menu.FindItem(Resource.Id.search_bar));
            searchView.QueryTextChange += (s, e) =>
            {
                e.Handled = true;
                searchQuery = e.NewText;
                itemsAdapter.Items = FilterWallets(wallets).ToList();
            };

            searchView.QueryTextSubmit += (s, e) =>
            {
                e.Handled = true;
                searchQuery = e.Query;
                itemsAdapter.Items = FilterWallets(wallets).ToList();
            };

            searchView.Close += (s, e) =>
            {
                e.Handled = false;
                searchQuery = null;
                itemsAdapter.Items = FilterWallets(wallets).ToList();
            };


            return true;
        }

        protected IDisposable Busy(bool indicator = true)
        {
            return BusyState.Begin(delegate
            {
                isBusy = true;

                if (indicator)
                    swipeRefreshLayout.Refreshing = true;

            },
            delegate
            {
                isBusy = false;
                if (indicator)
                    swipeRefreshLayout.Refreshing = false;
            });
        }

        protected async Task LoadWalletsAsync(bool allowInterogation = false)
        {
            var proxy = ProxyFactory.GetProxyInstace();
            var response = await proxy.ExecuteAsync(API.Endpoints.WalletEndpoints.GetMyWallets());

            if (CustomApplication.CurrentActivity != this)
                return;

            if (response.Successful)
            {
                wallets = await response.GetDataAsync<IList<WalletInfo>>();
                itemsAdapter.Items = wallets;

                //
                if (wallets.Count == 0)
                {
                    emptyFrame.Visibility = ViewStates.Visible;
                    listView.Visibility = ViewStates.Gone;
                }
                else
                {
                    emptyFrame.Visibility = ViewStates.Gone;
                    listView.Visibility = ViewStates.Visible;
                }

                //
                if (wallets.Count == 0 && allowInterogation)
                {
                    new AlertDialog.Builder(this)
                        .SetTitle("Setup Wallet")
                        .SetHtml($"Hi <b>{proxy.User.FullName.Split(' ').First()}</b>, Seems you havn't created any wallet yet. Do you want to create a new wallet with your phone number <b>{proxy.User.Phone}</b>?")
                        .SetPositiveButton("Create Wallet", delegate
                        {
                            //
                            string phone = proxy.User.Phone;
                            string provider = null;

                            if (phone.StartsWith("020") || phone.StartsWith("050"))
                                provider = NetworkProviders["Vodafone"];

                            else if (phone.StartsWith("054") || phone.StartsWith("024"))
                                provider = NetworkProviders["MTN"];

                            else if (phone.StartsWith("027") || phone.StartsWith("057"))
                                provider = NetworkProviders["Tigo"];

                            else if (phone.StartsWith("023"))
                                provider = NetworkProviders["Airtel"];

                            AddWalletDialogFragment.NewInstance(NetworkProviders.Values.ToArray(), new WalletInfo()
                            {
                                Provider = provider,
                                Value = phone
                            }, false).Show(FragmentManager, "add-wallet");
                        })
                        .SetNegativeButton("Nope Thanks", delegate
                        {

                            if (createOnly)
                            {
                                SetResult(Result.Canceled);
                                Finish();
                            }

                        })
                        .Show();
                }
            }
            else
            {
                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
            }
        }

        protected async override void OnResume()
        {
            base.OnResume();

            //
            if (!isBusy)
            {
                using (Busy(false))
                    await LoadWalletsAsync();
            }
        }

        protected override void OnPause()
        {
            base.OnPause();
        }

        private void OnAddWallet(object sender, EventArgs e)
        {
            AddWalletDialogFragment.NewInstance(NetworkProviders.Values.ToArray()).Show(FragmentManager, "add-wallet");
        }

    }
}