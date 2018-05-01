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
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Factory;
using System.Threading.Tasks;
using BookingSystem.API.Models.DTO;
using Newtonsoft.Json;

namespace BookingSystem.Android
{
    [Activity(Label = "Activate Account")]
    public class ActivateAccountActivity : BaseActivity
    {
        private EditText tbConfirmText;
        private string activationTicket;
        private bool autoLogin;

        //  Login credentials
        private UserInfo userInfo;
        private string userPassword;


        public ActivateAccountActivity() : base(Resource.Layout.activate_account_layout)
        {
            OnLoaded += (s, bundle) =>
            {
                tbConfirmText = FindViewById<EditText>(Resource.Id.tb_confirm_txt);
                FindViewById<Button>(Resource.Id.btn_activate_account).Click += OnActivate;

                //
                var intent = Intent;

                if (intent.HasExtra("user"))
                    userInfo = JsonConvert.DeserializeObject<UserInfo>(intent.GetStringExtra("user"));

                if(intent.HasExtra("username") && userInfo == null)
                {
                    userInfo = new UserInfo()
                    {
                        Username = intent.GetStringExtra("username")
                    };
                }

                if (intent.HasExtra("password"))
                    userPassword = intent.GetStringExtra("password");

                activationTicket = intent.GetStringExtra("userToken");
                autoLogin = intent.GetBooleanExtra("autoLogin", true);
            };
        }

        private async Task<bool> SendActivationTokenAsync()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            return (await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.BeginActivation(activationTicket))).Successful;
        }

        private async void OnActivate(object sender, EventArgs e)
        {
            string activationCode = tbConfirmText.TrimInput();
            using (var dlg = this.ShowProgress(null, "Activating account..."))
            {
                var proxy = ProxyFactory.GetProxyInstace();
                var result = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.ActivateAccount(activationCode, activationTicket));
                if (result.Successful)
                {

                    //  Auto login
                    if (autoLogin)
                    {
                        dlg.Dialog.SetMessage("Setting up your account...");

                        var loginResult = await proxy.LoginAsync(userInfo.Username, userPassword);
                        if (result.Successful)
                        {
                            NavigationHelper.NavigateUserHome(this, intent => intent.SetFlags(ActivityFlags.SingleTop | ActivityFlags.ClearTask));
                            Toast.MakeText(this, "Welcome!", ToastLength.Short).Show();

                            //  Set result to ok
                            SetResult(Result.Ok);
                        }
                        else
                        {
                            Toast.MakeText(this, loginResult.Message, ToastLength.Short).Show();
                        }
                    }

                    Finish();
                }
                else
                {
                    string message = string.Empty;
                    if (result.HasStatus(System.Net.HttpStatusCode.Forbidden))
                    {
                        message = Android.Resources.ERR_MSG_INVALID_ACTIVATION_CODE;
                        tbConfirmText.RequestFocus();
                        tbConfirmText.SelectAll();
                    }
                    else
                    {
                        message = result.GetErrorDescription();
                    }

                    //  Toast message
                    Toast.MakeText(this, message, ToastLength.Short).Show();

                }

            }
        }
    }
}