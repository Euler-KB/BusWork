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
using Android.Support.Design.Widget;
using BookingSystem.Android.Factory;
using BookingSystem.Android.Helpers;
using Android.Graphics;

namespace BookingSystem.Android
{
    [Activity(Label = "Login")]
    public class LoginActivity : AppCompatActivity
    {
        public enum ActivityCodes
        {
            Register = 0x2,
            Activate = 0x4
        };

        private TextInputLayout tbUsername;
        private TextInputLayout tbPassword;
        protected View rootView;

        protected override void OnCreate(Bundle savedInstanceState)
        {
            Window.AddFlags(WindowManagerFlags.DrawsSystemBarBackgrounds);
            base.OnCreate(savedInstanceState);

            // Create your application here
            SetContentView(Resource.Layout.login_layout);

            //
            Button btnLogin = FindViewById<Button>(Resource.Id.btn_login);
            btnLogin.Click += OnLogin;

            //
            tbUsername = FindViewById<TextInputLayout>(Resource.Id.tb_username);
            tbPassword = FindViewById<TextInputLayout>(Resource.Id.tb_password);
            rootView = FindViewById(Resource.Id.root_view);

            //
            FindViewById(Resource.Id.btn_register).Click += OnRegister;

            //
            FindViewById<TextView>(Resource.Id.btn_forgot_password).Click += OnForgotPassword;
        }

        private void OnForgotPassword(object sender, EventArgs e)
        {
            StartActivity(typeof(ForgotPasswordActivity));
        }

        private void OnRegister(object sender, EventArgs e)
        {
            StartActivityForResult(new Intent(this, typeof(RegisterActivity)), (int)ActivityCodes.Register);
        }

        private async void OnLogin(object sender, EventArgs e)
        {
            if (!ValidateInputs())
                return;

            //
            this.HideInputKeyboard();

            //
            string username = tbUsername.EditText.Text;
            string password = tbPassword.EditText.Text;

            //
            var proxy = ProxyFactory.GetProxyInstace();
            using (var dlg = this.ShowProgress(null, "Signing in, please hold on..."))
            {
                var result = await proxy.LoginAsync(username, password);
                if (result.RequireActivation)
                {
                    //
                    Intent intent = new Intent(this, typeof(ActivateAccountActivity));
                    intent.PutExtra("navigateHome", false);
                    intent.PutExtra("userToken", result.ActivationTicket);

                    //  Add credentials for automatic login
                    intent.PutExtra("username", username);
                    intent.PutExtra("password", password);

                    StartActivityForResult(intent, (int)ActivityCodes.Activate);
                }
                else
                {
                    if (result.Successful)
                    {
                        //
                        dlg.Dismiss();

                        //  Navigate user to home
                        NavigationHelper.NavigateUserHome(this, intent => intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.NewTask));
                        Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, result.Message, ToastLength.Short).Show();
                    }
                }
            }
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if(requestCode == (int)ActivityCodes.Activate && resultCode == Result.Ok)
            {
                Finish();
            }
            else if (requestCode == (int)ActivityCodes.Register && resultCode == Result.Ok)
            {
                if (!ProxyFactory.GetProxyInstace().IsAuthenticated)
                {
                    //  Go to Home
                    string username = data.GetStringExtra("username");
                    string password = data.GetStringExtra("password");

                    //
                    tbUsername.EditText.Text = username;
                    tbPassword.EditText.Text = password;

                    //
                    tbUsername.RequestFocus();
                    tbUsername.EditText.SelectAll();
                }
                else
                {
                    //  
                    Finish();
                }
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

        protected void ShowSnack(string message)
        {
            Snackbar.Make(rootView, message, Snackbar.LengthShort)
                .SetActionTextColor(Color.White)
                .Show();
        }

        protected bool ValidateInputs()
        {
            if (tbUsername.EditText.TrimInput().Length == 0)
            {
                ShowSnack("Please enter your username or email");
                tbUsername.RequestFocus();
                return false;
            }

            if (tbPassword.EditText.TrimInput().Length == 0)
            {
                ShowSnack("Enter your password to begin");
                tbPassword.RequestFocus();
                return false;
            }

            return true;
        }
    }
}