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
using BookingSystem.API.Models.DTO;
using BookingSystem.Android.Helpers;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using BookingSystem.Android.Factory;
using Square.Picasso;

namespace BookingSystem.Android.ViewHolders
{
    public partial class ItemHolders
    {
        public static readonly IList<ViewBind> WalletItemBindings = new List<ViewBind>()
        {
            new PropertyBind<ImageView,WalletInfo>(Resource.Id.wallet_provider_img, (view,wallet) =>
            {
                int resourceId = 0;
                if(wallet.Provider.ContainsIgnoreCase("MTN"))
                {
                    resourceId =  Resource.Drawable.mtn_mm;
                }
                else if(wallet.Provider.ContainsIgnoreCase("TIGO"))
                {
                    resourceId = Resource.Drawable.tigo_mm;
                }
                else if(wallet.Provider.ContainsIgnoreCase("VODAFONE"))
                {
                    resourceId = Resource.Drawable.vodafone_mm;
                }
                else if(wallet.Provider.ContainsIgnoreCase("AIRTEL"))
                {
                    resourceId = Resource.Drawable.airtel_mm;
                }

                view.SetImageResource(resourceId);
            }),
            new PropertyBind<ImageView, WalletInfo>(Resource.Id.img_check_default, (view,wallet) => view.Visibility = UserPreferences.Default.PrimaryWalletId  == wallet.Id ? ViewStates.Visible : ViewStates.Gone),
            new PropertyBind<TextView, WalletInfo>(Resource.Id.lb_wallet_provider_name, (view,wallet) => view.Text = wallet.Provider ),
            new PropertyBind<TextView, WalletInfo>(Resource.Id.lb_wallet_value, (view,wallet) => view.Text = wallet.Value ),
            new PropertyBind<ImageView, WalletInfo>(Resource.Id.btn_wallet_more, (view,wallet) =>
            {
                var context = view.Context;
                view.SetOnClickListener(new ClickListener(_ =>
                {
                    var popupMenu = new PopupMenu(context, view);
                    popupMenu.Inflate(Resource.Menu.actions_wallet_item);
                    if(wallet.Id == UserPreferences.Default.PrimaryWalletId)
                    {
                        popupMenu.Menu.FindItem(Resource.Id.action_set_default).SetEnabled(false);
                    }

                    popupMenu.MenuItemClick += (s,e) =>
                    {
                        switch (e.Item.ItemId)
                        {
                            case Resource.Id.action_set_default:
                                {
                                    UserPreferences.Default.PrimaryWalletId = wallet.Id;
                                    (CustomApplication.CurrentActivity as ManageWalletsActivity)?.NotifyDataChanged();
                                }
                                break;
                            case Resource.Id.action_edit:
                                AddWalletDialogFragment.NewInstance(ManageWalletsActivity.NetworkProviders.Values.ToArray() ,wallet,true).Show( CustomApplication.CurrentActivity?.FragmentManager , "edit-wallet");
                                break;
                                case Resource.Id.action_delete:

                                    new AlertDialog.Builder(context)
                                            .SetTitle("Delete Wallet")
                                            .SetMessage("Are you sure you want to delete wallet?")
                                            .SetPositiveButton("Yes", async delegate
                                            {

                                                using(context.ShowProgress(null,"Removing wallet, please hold on..."))
                                                {
                                                    var proxy = ProxyFactory.GetProxyInstace();
                                                    var response = await proxy.ExecuteAsync(API.Endpoints.WalletEndpoints.DeleteWallet(wallet.Id));
                                                    if (response.Successful)
                                                    {
                                                        Toast.MakeText(context,"Wallet removed successfully!" , ToastLength.Short).Show();
                                                    }
                                                    else
                                                    {
                                                        Toast.MakeText(context,response.GetErrorDescription() , ToastLength.Short).Show();
                                                    }
                                                }

                                            })
                                            .SetNegativeButton("No",delegate{ })
                                            .Show();
                                break;
                        }
                    };
                    popupMenu.Show();
                }));
            })
        };

    }
}