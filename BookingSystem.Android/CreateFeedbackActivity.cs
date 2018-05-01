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
    [Activity(Label = "Send Feedback")]
    public class CreateFeedbackActivity : BaseActivity
    {
        private EditText tbFeedbackMessage;

        public CreateFeedbackActivity() : base(Resource.Layout.create_feedback_layout)
        {
            OnLoaded += delegate
            {
                //  
                AllowBackNavigation();

                tbFeedbackMessage = FindViewById<EditText>(Resource.Id.tb_feedback_message);
                FindViewById<Button>(Resource.Id.btn_submit).Click += OnSendFeedback;
            };
        }

        private async void OnSendFeedback(object sender, EventArgs e)
        {
            string message = tbFeedbackMessage.TrimInput();
            if (message.Length == 0)
            {
                Toast.MakeText(this, "Please enter your message", ToastLength.Short).Show();
            }
            else
            {
                using (var dlg = this.ShowProgress(null, "Sending feedback..."))
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var result = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.CreateFeedback(message));
                    if (result.Successful)
                    {
                        //
                        dlg.Dismiss();

                        //
                        Toast.MakeText(this, "Feedback sent successfully", ToastLength.Short).Show();

                        //  Finish current activity
                        Finish();

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