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
using BookingSystem.Android.API;
using Android.Text;

namespace BookingSystem.Android
{
    [Activity(Label = "Confirm")]
    public class EnterConfirmCodeActivity : BaseActivity
    {
        public const string Email = "email";
        public const string Phone = "phone";
        public const string OperationKey = "_operation";
        public const string InputKey = "_input";

        private ProgressBar pgbBusy;
        private TextInputLayout tbConfirmCode;
        private Button btnConfirm;
        private string operation;
        private bool isBusy;
        private string currentInput;

        public EnterConfirmCodeActivity() : base(Resource.Layout.input_confirm_code_layout)
        {
            OnLoaded += delegate
            {
                //
                AllowBackNavigation();

                //
                currentInput = Intent.GetStringExtra(InputKey);
                operation = Intent.GetStringExtra(OperationKey) ?? Phone;

                TextView lbHint = FindViewById<TextView>(Resource.Id.lb_hint);
                switch (operation)
                {
                    case Phone:
                        SupportActionBar.Title = "Confirm Phone";
                        lbHint.SetHtml(string.Format(BaseContext.GetString(Resource.String.PhoneConfirmMsg), currentInput));
                        break;
                    case Email:
                        SupportActionBar.Title = "Confirm Email";
                        lbHint.SetHtml(string.Format(BaseContext.GetString(Resource.String.EmailConfirmMsg), currentInput));
                        break;
                }

                //
                pgbBusy = FindViewById<ProgressBar>(Resource.Id.pgb_busy);
                pgbBusy.Visibility = ViewStates.Gone;

                tbConfirmCode = FindViewById<TextInputLayout>(Resource.Id.tb_confirm_txt);
                (btnConfirm = FindViewById<Button>(Resource.Id.btn_submit)).Click += OnConfirm;
            };
        }

        public static void Show(Context context, string operation, string newInput, Action<Intent> prepareIntent = null)
        {
            //
            Intent intent = new Intent(context, typeof(EnterConfirmCodeActivity));
            intent.PutExtra(OperationKey, operation);
            intent.PutExtra(InputKey, newInput);
            prepareIntent?.Invoke(intent);

            //
            context.StartActivity(intent);
        }

        IDisposable Busy()
        {
            return BusyState.Begin(delegate
            {
                isBusy = true;
                pgbBusy.Visibility = ViewStates.Visible;
                tbConfirmCode.Enabled = false;
                btnConfirm.Enabled = false;

            }, delegate
            {
                isBusy = false;
                pgbBusy.Visibility = ViewStates.Gone;
                tbConfirmCode.Enabled = true;
                btnConfirm.Enabled = true;
            });
        }

        protected override bool CanGoBack() => !isBusy;

        private bool ValidateInputs()
        {
            return !tbConfirmCode.SetError(tbConfirmCode.EditText.TrimInput().Length != 6 ? "Enter a valid confirmation code!" : null);
        }

        private void ToastSuccess()
        {
            string msg = "";
            switch (operation)
            {
                case Phone:
                    msg = "Phone No. changed successfully";
                    break;
                case Email:
                    msg = "Email change successfully";
                    break;
            }

            Toast.MakeText(this, msg, ToastLength.Long).Show();
        }

        private async void OnConfirm(object sender, EventArgs e)
        {
            if (ValidateInputs())
            {
                string confirmCode = tbConfirmCode.EditText.Text;
                using (this.ShowProgress(null, "Verying code, please hold on..."))
                using (Busy())
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    ApiResponse response = null;
                    switch (operation)
                    {
                        case Phone:
                            response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.CompletePhoneChange(confirmCode, currentInput));
                            break;
                        case Email:
                            response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.CompleteChangeEmail(confirmCode, currentInput));
                            break;
                    }

                    if (response.Successful)
                    {
                        //  Yay!
                        ToastSuccess();

                        //  Update user info
                        await proxy.UpdateUserInfoAsync();

                        SetResult(Result.Ok);

                        //
                        Finish();
                    }
                    else
                    {
                        Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                    }
                }
            }
        }
    }
}