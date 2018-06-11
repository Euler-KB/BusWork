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
using BookingSystem.Android.Factory;
using BookingSystem.API.Models;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Text;
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Views;

namespace BookingSystem.Android.ViewHolders
{
    public partial class ItemHolders
    {
        public static event EventHandler<BusInfo> OnBusRemoved;

        public static readonly IList<ViewBind> BusItemBindings = new List<ViewBind>()
        {
            new PropertyBind<AvatarDisplay,BusInfo>(Resource.Id.img_bus_icon,(view,bus) =>
            {
                view.Shape = AvatarDisplay.AvatarShape.RoundedRect;

                if(bus.Photo != null)
                    view.SetPhoto(bus.Photo);
                else
                    view.Image = null;

            }),
            new PropertyBind<TextView,BusInfo>(Resource.Id.lb_bus_name,(view,bus) => view.Text = bus.Name),
            new PropertyBind<TextView,BusInfo>(Resource.Id.lb_bus_model,(view,bus) => view.Text = bus.Model),
            new PropertyBind<TextView,BusInfo>(Resource.Id.lb_bus_seats,(view,bus) => view.Text = bus.Seats),
            new PropertyBind<View,BusInfo>(Resource.Id.btn_bus_info,(view,bus) =>
            {
                //  Replace OnClick listener to avoid repeated subscription
                view.SetOnClickListener(new ClickListener(delegate
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var popupMenu = new PopupMenu(view.Context , view ,  GravityFlags.Left);

                    //
                    popupMenu.Inflate(Resource.Menu.actions_bus_item);
                    popupMenu.MenuItemClick += (s,e) =>
                    {
                        switch (e.Item.ItemId)
                        {
                            case Resource.Id.action_show_routes:
                                BusRoutesActivity.Navigate(view.Context , bus , proxy.User.AccountType == AccountType.Administrator );
                                break;
                            case Resource.Id.action_add_route:
                                CreateRouteActivity.Navigate( view.Context , bus , null , false);
                                break;
                            case Resource.Id.action_edit:
                                CreateBusActivity.Navigate( view.Context , bus );
                                break;
                            case Resource.Id.action_delete:

                                new AlertDialog.Builder(view.Context)
                                .SetTitle("Delete Bus?")
                                .SetHtml($"Are you sure you want to delete the bus <b>{bus.Name}</b>")
                                .SetPositiveButton("Delete Bus", async delegate
                                {
                                    using(view.Context.ShowProgress(null,"Deleting bus, please hold on..."))
                                    {
                                        var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.Delete(bus.Id));
                                        if(response.Successful)
                                        {
                                            OnBusRemoved?.Invoke(BusItemBindings , bus);
                                        }
                                        else
                                        {
                                            Toast.MakeText(view.Context , response.GetErrorDescription() , ToastLength.Short).Show();
                                        }
                                    }
                                })
                                .SetNegativeButton("Cancel",delegate{})
                                .Show();


                                break;
                        }
                    };

                    var menu = popupMenu.Menu;
                    switch (proxy.User.AccountType)
                    {
                        case AccountType.Administrator:
                            break;
                        case AccountType.User:
                            menu.FindItem(Resource.Id.action_edit).SetVisible(false);
                            menu.FindItem(Resource.Id.action_delete).SetVisible(false);
                            menu.FindItem(Resource.Id.action_add_route).SetVisible(false);
                            break;
                    }

                    popupMenu.Show();

                }));
            }),
        };

    }
}