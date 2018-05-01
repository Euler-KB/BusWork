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
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Factory;

namespace BookingSystem.Android
{
    [Activity(Label = "Change Password")]
    public class ChangePasswordActivity : BaseActivity
    {
        private TextInputLayout tbOriginalPassword, tbPassword, tbConfirmPassword;

        private InputHandler formInputHandler;

        private InputBinding[] Bindings = new InputBinding[]
        {
            new InputBinding("OriginalPassword",  Resource.Id.tb_original_pwd , true , InputTypes.Text),
            new InputBinding("Password",  Resource.Id.tb_pwd)
            {
                Validation = new InputValidation()
                {
                    Required = true,
                    Min = 6
                }
            },
            new InputBinding("ConfirmPassword", Resource.Id.tb_pwd_confirm)
            {
                Validation = new InputValidation()
                {
                    Compare = Resource.Id.tb_pwd
                }
            }
        };

        public ChangePasswordActivity() : base(Resource.Layout.change_password_layout)
        {
            OnLoaded += delegate
            {
                //
                AllowBackNavigation();

                //
                formInputHandler = new InputHandler();
                formInputHandler.SetBindings(Bindings, FindViewById<ViewGroup>(global::Android.Resource.Id.Content));

                tbOriginalPassword = FindViewById<TextInputLayout>(Resource.Id.tb_original_pwd);
                tbPassword = FindViewById<TextInputLayout>(Resource.Id.tb_pwd);
                tbConfirmPassword = FindViewById<TextInputLayout>(Resource.Id.tb_pwd_confirm);

                FindViewById<Button>(Resource.Id.btn_submit).Click += OnChangePassword;
            };
        }

        private bool ValidateInputs()
        {
            return formInputHandler.ValidateInputs(true).Count == 0;
        }

        private async void OnChangePassword(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                var inputs = formInputHandler.GetInputs();
                using (var dlg = this.ShowProgress(null, "Processing, hold on..."))
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var result = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.ChangePassword(inputs["OriginalPassword"],
                        inputs["Password"]));

                    if (result.Successful)
                    {
                        dlg.Dismiss();

                        Finish();

                        Toast.MakeText(this, "Password changed successfully!", ToastLength.Short).Show();
                    }
                    else
                    {
                        Toast.MakeText(this, result.GetErrorDescription(), ToastLength.Short).Show();

                    }
                }
            }
        }
    }
}