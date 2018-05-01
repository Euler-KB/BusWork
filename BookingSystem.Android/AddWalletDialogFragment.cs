using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using AlertDialog = Android.Support.V7.App.AlertDialog;
using Android.Support.Design.Widget;
using BookingSystem.Android.Factory;
using BookingSystem.API.Models.DTO;
using Newtonsoft.Json;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android
{
    public class AddWalletDialogFragment : DialogFragment
    {
        private WalletInfo walletInfo;
        private Spinner networkProviderSpinner;
        private TextInputLayout tbPhoneNo;
        private IList<string> networkProvider;
        private bool isEdit;

        public static event EventHandler<WalletInfo> OnCreated;

        public static event EventHandler<WalletInfo> OnUpdated;

        public static AddWalletDialogFragment NewInstance(string[] networkProviders, WalletInfo wallet = null, bool edit = false)
        {
            AddWalletDialogFragment fragment = new AddWalletDialogFragment();
            Bundle bundle = new Bundle();
            bundle.PutStringArray("networks", networkProviders);
            bundle.PutBoolean("edit", edit);
            if (wallet != null)
                bundle.PutString("wallet", JsonConvert.SerializeObject(wallet));

            fragment.Arguments = bundle;
            return fragment;
        }

        public override void OnResume()
        {
            (Dialog as AlertDialog)?.GetButton((int)DialogButtonType.Positive).SetOnClickListener(new ClickListener(async delegate
            {
                string provider = networkProvider[networkProviderSpinner.SelectedItemPosition];
                string value = tbPhoneNo.EditText.Text;

                //  Validate phone no
                if (tbPhoneNo.SetError(InputHandler.IsValidPhone(value) ? null : "Invalid phone number"))
                    return;

                //
                var proxy = ProxyFactory.GetProxyInstace();

                if (isEdit)
                {
                    var editInfo = new EditWalletInfo();

                    if (editInfo.Value != value)
                        editInfo.Value = value;

                    if (editInfo.Provider != provider)
                        editInfo.Provider = provider;

                    if (editInfo.AnyUpdate())
                    {
                        using (Activity.ShowProgress(null, "Saving changes, please hold on..."))
                        {
                            var response = await proxy.ExecuteAsync(API.Endpoints.WalletEndpoints.UpdateWallet(walletInfo.Id, editInfo));
                            if (response.Successful)
                            {
                                //
                                walletInfo.Provider = provider;
                                walletInfo.Value = value;

                                //
                                OnUpdated?.Invoke(this, walletInfo);

                                //
                                Dialog.Dismiss();
                            }
                            else
                            {
                                Toast.MakeText(Activity, response.GetErrorDescription(), ToastLength.Short).Show();
                            }
                        }
                    }

                }
                else
                {
                    using (Activity.ShowProgress(null, "Add wallet, please hold on..."))
                    {
                        var response = await proxy.ExecuteAsync(API.Endpoints.WalletEndpoints.CreateWallet(provider, value));
                        if (response.Successful)
                        {
                            OnCreated?.Invoke(this, await response.GetDataAsync<WalletInfo>());

                            //
                            Dialog.Dismiss();
                        }
                        else
                        {
                            Toast.MakeText(Activity, response.GetErrorDescription(), ToastLength.Short).Show();
                        }
                    }
                }
            }));

            base.OnResume();
        }

        public override Dialog OnCreateDialog(Bundle savedInstanceState)
        {
            if (Arguments.ContainsKey("wallet"))
                walletInfo = JsonConvert.DeserializeObject<WalletInfo>(Arguments.GetString("wallet"));

            isEdit = Arguments.GetBoolean("edit");

            //
            networkProvider = Arguments.GetStringArray("networks").ToList();

            var view = Activity.LayoutInflater.Inflate(Resource.Layout.add_wallet_layout, null);
            var dlg = new AlertDialog.Builder(Activity)
                .SetTitle(isEdit ? "Edit Wallet" : "Add Wallet")
                .SetView(view)
                .SetPositiveButton(isEdit ? "Save Changes" : "Add", delegate { })
                .SetNegativeButton("Cancel", delegate { })
                .Create();

            networkProviderSpinner = view.FindViewById<Spinner>(Resource.Id.network_operators_spinner);
            tbPhoneNo = view.FindViewById<TextInputLayout>(Resource.Id.tb_phone);

            //  Assign network providers
            networkProviderSpinner.Adapter = new ArrayAdapter<string>(Activity, global::Android.Resource.Layout.SimpleSpinnerDropDownItem, networkProvider);

            if (walletInfo?.Provider != null)
            {
                var index = networkProvider.IndexOf(walletInfo.Provider);
                if (index >= 0)
                    networkProviderSpinner.SetSelection(index);
            }

            //
            if (walletInfo?.Value != null)
                tbPhoneNo.EditText.Text = walletInfo.Value;

            return dlg;
        }

    }
}