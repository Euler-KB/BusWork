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

namespace BookingSystem.Android
{
    [Activity(Label = "Forgot Password")]
    public class ForgotPasswordActivity : BaseActivity
    {
        public const int CompletePasswordActivityCode = 0x2;

        private EditText tbEmail;

        public ForgotPasswordActivity() : base(Resource.Layout.forgot_password)
        {
            OnLoaded += delegate
            {
                tbEmail = FindViewById<EditText>(Resource.Id.tb_email);

                FindViewById<Button>(Resource.Id.btn_reset_password).Click += OnResetPasswordAsync;
                FindViewById(Resource.Id.btn_nav_home).Click += OnNavigateHome;
            };
        }

        private void OnNavigateHome(object sender, EventArgs e)
        {
            Finish();
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (resultCode == Result.Ok && requestCode == CompletePasswordActivityCode)
            {
                Toast.MakeText(this, "Password reset complete", ToastLength.Long).Show();
                Finish();
                return;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        private async void OnResetPasswordAsync(object sender, EventArgs e)
        {
            string email = tbEmail.TrimInput();
            using (var dlg = this.ShowProgress(null, "Processing, please hold on..."))
            {
                var proxy = ProxyFactory.GetProxyInstace();
                var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.ForgotPassword(email));
                if (response.Successful)
                {
                    //  Save user identity
                    dlg.Dismiss();

                    //  Get body with reset token
                    var body = await response.GetDataAsAnonymous(new { ResetToken = "" });

                    //  
                    Intent intent = new Intent(this, typeof(CompletePasswordResetActivity));
                    intent.PutExtra("resetToken", body.ResetToken);
                    StartActivityForResult(intent, CompletePasswordActivityCode);
                }
                else
                {
                    string message = "";
                    if (response.HasStatus(System.Net.HttpStatusCode.Forbidden))
                    {
                        message = response.GetResponseMessage();
                    }
                    else
                    {
                        message = response.GetErrorDescription();
                    }

                    Toast.MakeText(this, message, ToastLength.Short).Show();
                }
            }

        }
    }
}