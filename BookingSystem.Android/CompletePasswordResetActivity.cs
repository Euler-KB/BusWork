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
using BookingSystem.Android.Helpers;
using Android.Support.Design.Widget;
using BookingSystem.Android.Factory;

namespace BookingSystem.Android
{
    [Activity(Label = "Reset Password")]
    public class CompletePasswordResetActivity : BaseActivity
    {
        private EditText tbConfirmCode;
        private ViewGroup inputPwdFrame;
        private ProgressBar pgBusy;
        private bool isBusy;
        private string resetBearerToken;

        //
        private EditText tbPwd;
        private Button btnReset;
        private EditText tbPwdConfirm;

        private void UpdateResetButtonState()
        {
            btnReset.Enabled = tbConfirmCode.Text.Length >= 6 && tbPwd.Text.Length >= 6 && tbPwdConfirm.Text == tbPwd.Text;
        }

        public CompletePasswordResetActivity() : base(Resource.Layout.confirm_password_reset_layout)
        {
            OnLoaded += delegate
            {
                resetBearerToken = Intent.GetStringExtra("resetToken");

                tbConfirmCode = FindViewById<EditText>(Resource.Id.tb_confirm_txt);
                tbConfirmCode.TextChanged += (s, e) =>
                {
                    inputPwdFrame.Visibility = tbConfirmCode.Text.Length < 6 ? ViewStates.Gone : ViewStates.Visible;
                    UpdateResetButtonState();
                };

                //
                (btnReset = FindViewById<Button>(Resource.Id.btn_reset_password)).Click += OnResetPassword;
                inputPwdFrame = FindViewById<ViewGroup>(Resource.Id.password_input_frame);
                pgBusy = FindViewById<ProgressBar>(Resource.Id.pgb_busy);

                //  Show when code is entered
                inputPwdFrame.Visibility = ViewStates.Gone;
                pgBusy.Visibility = ViewStates.Gone;

                //
                tbPwd = FindViewById<EditText>(Resource.Id.tb_pwd);
                tbPwdConfirm = FindViewById<EditText>(Resource.Id.tb_pwd_confirm);

                tbPwd.TextChanged += (s, e) =>
                {
                    UpdateResetButtonState();
                };

                tbPwdConfirm.TextChanged += (s, e) =>
                {
                    UpdateResetButtonState();
                };
            };

        }

        IDisposable Busy()
        {
            return BusyState.Begin(delegate
            {
                isBusy = true;
                pgBusy.Visibility = ViewStates.Visible;
                btnReset.Enabled = false;
            },
            delegate
            {
                pgBusy.Visibility = ViewStates.Gone;
                UpdateResetButtonState();
                isBusy = false;
            });
        }

        protected override bool CanGoBack() => !isBusy;

        private void ShowMessage(string message)
        {
            Toast.MakeText(this, message, ToastLength.Short).Show();
        }

        private async void OnResetPassword(object sender, EventArgs e)
        {
            string confirmCode = tbConfirmCode.TrimInput();
            if (confirmCode.Length < 6)
            {
                //  Enter cofirm code
                tbConfirmCode.RequestFocus();
                tbConfirmCode.SelectAll();

                //
                ShowMessage("Please enter a valid reset code");
                return;
            }

            string passwordNew = tbPwd.Text;
            string passwordConfirm = tbPwdConfirm.Text;

            if (passwordNew.Length < 6)
            {
                tbPwd.RequestFocus();
                tbPwd.SelectAll();
                return;
            }

            if (passwordConfirm != passwordNew)
            {
                tbPwd.RequestFocus();
                tbPwd.SelectAll();
                return;
            }

            //  Validate code
            using (Busy())
            {
                var proxy = ProxyFactory.GetProxyInstace();
                var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.ResetPassword(confirmCode,resetBearerToken, passwordNew));
                if (response.Successful)
                {
                    SetResult(Result.Ok);
                    Finish();
                }
                else
                {
                    ShowMessage(response.GetErrorDescription());
                }
            }

        }
    }
}