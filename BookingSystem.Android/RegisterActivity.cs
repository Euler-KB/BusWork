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
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Factory;
using Newtonsoft.Json;
using BookingSystem.API.Models.DTO;

namespace BookingSystem.Android
{
    [Activity(Label = "Register")]
    public class RegisterActivity : BaseActivity
    {
        public const int ActivateAccountRequestCode = 0x2;

        private InputHandler formInputHandler;
        private IDictionary<string, string> registerFormInputs;

        public RegisterActivity() : base(Resource.Layout.register_layout)
        {
            OnLoaded += delegate
            {
                //  Setup form input handler
                formInputHandler = new InputHandler();

                //  
                SetupInputs();

                //  
                FindViewById<FloatingActionButton>(Resource.Id.fab_register).Click += OnRegisterUser;
            };

        }

        private InputBinding[] Bindings = new InputBinding[]
        {
            new InputBinding("Username",Resource.Id.tb_username,true, InputTypes.Username),
            new InputBinding("FirstName",Resource.Id.tb_firstname,true , InputTypes.Text),
            new InputBinding("LastName",Resource.Id.tb_lastname , true , InputTypes.Text),
            new InputBinding("Email",Resource.Id.tb_email , true , InputTypes.Email),
            new InputBinding("Phone",Resource.Id.tb_phone , true , InputTypes.Phone),
            new InputBinding("Password",Resource.Id.tb_password , true , min: 6, max: 32),
            new InputBinding("ConfirmPassword",Resource.Id.tb_confirm_password)
            {
                Validation = new InputValidation()
                {
                    Compare = Resource.Id.tb_password,
                }
            },
        };

        private async void OnRegisterUser(object sender, EventArgs e)
        {
            var errors = formInputHandler.ValidateInputs(updateInputError: true);
            if (errors.Count == 0)
            {
                registerFormInputs = formInputHandler.GetInputs();

                using (var dlg = this.ShowProgress(null, "Processing registration, please hold on..."))
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var results = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.Register(BookingSystem.API.Models.AccountType.User, new BookingSystem.API.Models.RegisterModel()
                    {
                        Email = registerFormInputs["Email"],
                        FirstName = registerFormInputs["FirstName"],
                        LastName = registerFormInputs["LastName"],
                        Password = registerFormInputs["Password"],
                        Phone = registerFormInputs["Phone"],
                        Username = registerFormInputs["Username"],
                    }));

                    if (results.Successful)
                    {
                        //
                        dlg.Dismiss();

                        //  Get user token
                        string activationTicket = results.GetHeader("UserToken");

                        //  Activate account
                        var intent = new Intent(this, typeof(ActivateAccountActivity));
                        intent.SetFlags(ActivityFlags.ClearTask | ActivityFlags.SingleTop);
                        intent.PutExtra("userToken", activationTicket);
                        intent.PutExtra("user", JsonConvert.SerializeObject(await results.GetDataAsync<UserInfo>()));
                        intent.PutExtra("password", registerFormInputs["Password"]);

                        //  Show toast
                        Toast.MakeText(this, "Account registered successfully. Activate your account to begin!", ToastLength.Short).Show();

                        //  
                        StartActivityForResult(intent, ActivateAccountRequestCode);
                    }
                    else
                    {
                        Toast.MakeText(this, results.GetErrorDescription(), ToastLength.Short).Show();
                    }
                }
            }

        }

        private void ShowSnack(string message)
        {
            //  Show error
            Snackbar.Make(FindViewById(Resource.Id.root_view), message, Snackbar.LengthShort).Show();
        }

        protected void SetupInputs()
        {
            formInputHandler.SetBindings(Bindings, FindViewById<ViewGroup>(Resource.Id.reg_input_container));
            formInputHandler.SetCustomError("ConfirmPassword", "compare", "Passwords do not match");

            formInputHandler.SetCustomError("Password", "min", "Password should not be less than 6 characters");
            formInputHandler.SetCustomError("Password", "max", "Password cannot be greater than 32 characters");
        }

        protected override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == ActivateAccountRequestCode)
            {
                var intent = new Intent();
                if (registerFormInputs != null)
                {
                    intent.PutExtra("username", this.registerFormInputs["Username"]);
                    intent.PutExtra("password", this.registerFormInputs["Password"]);
                }

                SetResult(Result.Ok, intent);
                Finish();
                return;
            }

            base.OnActivityResult(requestCode, resultCode, data);
        }

    }
}