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
using AlertDialog = Android.Support.V7.App.AlertDialog;
using BookingSystem.Android.Helpers;
using BookingSystem.Android.Factory;
using Android.Support.Design.Widget;
using System.Threading.Tasks;
using System.IO;
using Android.Graphics;
using Android.Provider;
using Square.Picasso;
using BookingSystem.Android.Views;

namespace BookingSystem.Android
{
    [Activity(Label = "User Profile")]
    public class UserProfileActivity : BaseActivity
    {
        public const int ImagePickRequestCode = 0x45;

        private TextView lbUsername, lbFullname, lbEmail, lbPhone;
        private Views.AvatarDisplay profileImageView;

        protected bool IsAdmin
        {
            get
            {
                return ProxyFactory.GetProxyInstace().User.AccountType == BookingSystem.API.Models.AccountType.Administrator;
            }
        }

        public UserProfileActivity() : base(Resource.Layout.user_profile_layout)
        {
            OnLoaded += async delegate
            {
                //
                AllowBackNavigation();

                //
                lbUsername = FindViewById<TextView>(Resource.Id.lb_username);
                lbFullname = FindViewById<TextView>(Resource.Id.lb_fullname);
                lbEmail = FindViewById<TextView>(Resource.Id.lb_email);
                lbPhone = FindViewById<TextView>(Resource.Id.lb_phone);

                //
                FindViewById(Resource.Id.btn_edit_username).Click += delegate
                {
                    OnChangeUsername();
                };

                FindViewById(Resource.Id.btn_edit_fullname).Click += delegate
                {
                    OnChangeFullName();
                };

                FindViewById(Resource.Id.btn_edit_email).Click += delegate
                {
                    OnChangeEmail();
                };

                FindViewById(Resource.Id.btn_edit_phone).Click += delegate
                {
                    OnChangePhone();
                };

                var btnManageWallet = FindViewById<Button>(Resource.Id.btn_manage_wallet);
                btnManageWallet.Click += delegate
                {
                    OnManageWallets();
                };

                //  Bind user profile user here
                profileImageView = FindViewById<AvatarDisplay>(Resource.Id.profile_img_placeholder);

                //  Hide for admin
                if (IsAdmin)
                    btnManageWallet.Visibility = ViewStates.Gone;

                //
                UpdateProfileImage();
                profileImageView.Click += (s, e) =>
                {
                    //  Update profile image
                    Intent intent = new Intent(Intent.ActionPick, MediaStore.Images.Media.InternalContentUri);
                    intent.SetType("image/*");
                    intent.PutExtra("crop", "true");
                    intent.PutExtra("scale", true);
                    intent.PutExtra("outputX", 240);
                    intent.PutExtra("outputY", 240);
                    intent.PutExtra("aspectX", 1);
                    intent.PutExtra("aspectY", 1);
                    intent.PutExtra("return-data", true);
                    StartActivityForResult(intent, ImagePickRequestCode);
                };

                var proxy = ProxyFactory.GetProxyInstace();
                await proxy.UpdateUserInfoAsync();
            };
        }

        protected void OnManageWallets()
        {
            StartActivity(typeof(ManageWalletsActivity));
        }

        protected void UpdateProfileImage()
        {
            var proxy = ProxyFactory.GetProxyInstace();
            profileImageView.Name = proxy.User.FullName;

            if (proxy.User.ProfileImage != null)
            {
                profileImageView.SetPhoto(proxy.User.ProfileImage, false);
            }
        }

        protected async override void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == ImagePickRequestCode && resultCode == Result.Ok)
            {
                //
                using (this.ShowProgress(null, "Updating profile image..."))
                {

                    Bitmap image = data.GetParcelableExtra("data") as Bitmap;
                    var proxy = ProxyFactory.GetProxyInstace();
                    using (var ms = new MemoryStream())
                    {
                        image.Compress(Bitmap.CompressFormat.Png, 90, ms);
                        ms.Flush();

                        ms.Seek(0, SeekOrigin.Begin);

                        var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.UploadProfileImage(ms, "image/png"));
                        if (response.Successful)
                        {
                            profileImageView.Image = image;

                            //
                            await proxy.UpdateUserInfoAsync().ContinueWith(t =>
                            {
                                PicassoHelpers.InvalidateMedia(t.Result.ProfileImage);
                            });

                            //
                            Toast.MakeText(this, "Profile image updated successfully!", ToastLength.Short).Show();
                        }
                        else
                        {
                            Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                        }
                    }
                }

            }

        }

        protected override void OnResume()
        {
            //  Setup user profile
            SetupUserProfile();

            //  Update profile image
            UpdateProfileImage();

            ProxyFactory.GetProxyInstace().OnUserUpdated += OnUserProfileUpdated;

            base.OnResume();
        }

        protected override void OnPause()
        {
            ProxyFactory.GetProxyInstace().OnUserUpdated -= OnUserProfileUpdated;
            base.OnPause();
        }

        private void OnUserProfileUpdated(object sender, API.AuthenticationInfo e)
        {
            SetupUserProfile();
        }

        private void SetupUserProfile()
        {
            var user = ProxyFactory.GetProxyInstace().User;
            if (user != null)
            {
                lbUsername.Text = user.Username;
                lbFullname.Text = user.FullName;
                lbEmail.Text = user.Email;
                lbPhone.Text = user.Phone;
            }
        }

        private void OnChangeEmail()
        {
            var contentView = LayoutInflater.Inflate(Resource.Layout.change_input_layout, null);
            var tbInput = contentView.FindViewById<TextInputLayout>(Resource.Id.tb_change_input);
            tbInput.Hint = "Email";

            //
            var dlg = new AlertDialog.Builder(this)
                            .SetTitle("Change Email")
                            .SetView(contentView)
                            .Show();
            //
            contentView.FindViewById<Button>(Resource.Id.btn_submit).Click += async delegate
            {
                string email = tbInput.EditText.TrimInput();

                if (tbInput.SetError(InputHandler.IsValidEmail(email) ? null : $"'{email}' isn't a valid email address"))
                    return;

                //
                using (var pg = this.ShowProgress(null, "Processing, please hold..."))
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.BeginChangeEmail(email));

                    //
                    dlg.Dismiss();
                    if (response.Successful)
                    {
                        pg.Dismiss();
                        EnterConfirmCodeActivity.Show(this, EnterConfirmCodeActivity.Email, email);
                    }
                    else
                    {
                        Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                    }
                }
            };

        }

        private void OnChangeUsername()
        {
            var view = LayoutInflater.Inflate(Resource.Layout.change_username_layout, null);
            var dlg = new AlertDialog.Builder(this)
                .SetTitle("Change Username")
                .SetView(view)
                .Show();

            var formInputHandler = new InputHandler();
            formInputHandler.SetBindings(new InputBinding[]
            {
                new InputBinding("Username",Resource.Id.tb_username,true , InputTypes.Username)
            }, view as ViewGroup);

            var tbUsername = view.FindViewById<TextInputLayout>(Resource.Id.tb_username);
            view.FindViewById<Button>(Resource.Id.btn_submit).Click += async delegate
            {
                var errors = formInputHandler.ValidateInputs(true);
                if (errors.Count == 0)
                {
                    string username = tbUsername.EditText.TrimInput();
                    var proxy = ProxyFactory.GetProxyInstace();
                    using (this.ShowProgress(null, "Applying changes..."))
                    {
                        var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.Update(new BookingSystem.API.Models.DTO.UpdateUserInfo() { Username = username }));
                        if (response.Successful)
                        {
                            //  Update current user info
                            await proxy.UpdateUserInfoAsync();

                            //
                            dlg.Dismiss();

                            Toast.MakeText(this, "Username changed successfully!", ToastLength.Long).Show();
                        }
                        else
                        {
                            Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                        }

                    }
                }

            };
        }

        private void OnChangeFullName()
        {
            var view = LayoutInflater.Inflate(Resource.Layout.change_fullname_layout, null);

            //
            var dlg = new AlertDialog.Builder(this)
                .SetTitle("Change Name")
                .SetView(view)
                .Show();

            InputHandler formHandler = new InputHandler();
            formHandler.SetBindings(new InputBinding[]
            {
                new InputBinding("FirstName",Resource.Id.tb_firstname,true , InputTypes.Text),
                new InputBinding("LastName", Resource.Id.tb_lastname,true , InputTypes.Text)
            }, view as ViewGroup);

            formHandler.SetCustomError("FirstName", "required", "Please enter your first name");
            formHandler.SetCustomError("LastName", "required", "Last name is required");

            view.FindViewById<Button>(Resource.Id.btn_submit).Click += async delegate
            {
                if (formHandler.ValidateInputs(true).Count == 0)
                {
                    var inputs = formHandler.GetInputs();
                    using (this.ShowProgress(null, "Applying changes..."))
                    {
                        var proxy = ProxyFactory.GetProxyInstace();
                        var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.Update(new BookingSystem.API.Models.DTO.UpdateUserInfo()
                        {
                            FullName = $"{inputs["FirstName"]} {inputs["LastName"]}"
                        }));

                        if (response.Successful)
                        {
                            //  Update current user info
                            await proxy.UpdateUserInfoAsync();

                            //
                            dlg.Dismiss();

                            Toast.MakeText(this, "Name changed successfully!", ToastLength.Long).Show();

                        }
                        else
                        {
                            Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                        }

                    }
                }

            };

        }

        private void OnChangePhone()
        {
            var contentView = LayoutInflater.Inflate(Resource.Layout.change_input_layout, null);
            var tbInput = contentView.FindViewById<TextInputLayout>(Resource.Id.tb_change_input);
            tbInput.Hint = "Phone";

            //
            var dlg = new AlertDialog.Builder(this)
                            .SetTitle("Change Phone")
                            .SetView(contentView)
                            .Show();

            //
            contentView.FindViewById<Button>(Resource.Id.btn_submit).Click += async delegate
            {
                string phone = tbInput.EditText.TrimInput();
                if (tbInput.SetError(InputHandler.IsValidPhone(phone) ? null : "Invalid phone number!"))
                    return;

                using (var pg = this.ShowProgress(null, "Processing, please hold on...."))
                {
                    var proxy = ProxyFactory.GetProxyInstace();
                    var response = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.BeginChangeEmail(phone));
                    //
                    dlg.Dismiss();

                    if (response.Successful)
                    {
                        EnterConfirmCodeActivity.Show(this, EnterConfirmCodeActivity.Phone, phone);
                    }
                    else
                    {
                        Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                    }
                }
            };
        }

        public override bool OnOptionsItemSelected(IMenuItem item)
        {
            switch (item.ItemId)
            {
                case Resource.Id.action_change_username:
                    {
                        OnChangeUsername();
                    }
                    break;
                case Resource.Id.action_change_fullname:
                    {
                        OnChangeFullName();
                    }
                    break;
                case Resource.Id.action_change_email:
                    {
                        OnChangeEmail();
                    }
                    break;
                case Resource.Id.action_change_phone:
                    {
                        OnChangePhone();
                    }
                    break;
                case Resource.Id.action_change_password:
                    {
                        StartActivity(typeof(ChangePasswordActivity));
                    }
                    break;

                case Resource.Id.action_manage_wallets:
                    {
                        OnManageWallets();
                    }
                    break;
                case Resource.Id.action_delete_account:
                    {
                        //
                        var contentView = LayoutInflater.Inflate(Resource.Layout.delete_account_layout, null);
                        var dlg = new AlertDialog.Builder(this)
                            .SetTitle("Delete Account")
                            .SetView(contentView)
                            .Show();

                        contentView.FindViewById<Button>(Resource.Id.btn_delete_account).Click += async delegate
                        {
                            //  Delete account here
                            using (this.ShowProgress(null, "Working on it..."))
                            {
                                var proxy = ProxyFactory.GetProxyInstace();
                                var result = await proxy.ExecuteAsync(API.Endpoints.AccountEndpoints.DeleteAccount());
                                if (result.Successful)
                                {

                                    //  Reset preferences
                                    await proxy.SignOut();

                                }
                                else
                                {
                                    //  
                                    dlg.Dismiss();

                                    Toast.MakeText(this, result.GetErrorDescription(), ToastLength.Short).Show();
                                }
                            }
                        };
                    }
                    break;
            }

            return base.OnOptionsItemSelected(item);
        }

        public override bool OnCreateOptionsMenu(IMenu menu)
        {
            MenuInflater.Inflate(Resource.Menu.user_profile_actions, menu);

            if (IsAdmin)
            {
                menu.FindItem(Resource.Id.action_manage_wallets).SetVisible(false);
            }

            return true;
        }
    }
}