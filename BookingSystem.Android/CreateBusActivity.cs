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
using BookingSystem.API.Models.DTO;
using Newtonsoft.Json;
using BookingSystem.Android.Helpers;
using Android.Support.Design.Widget;
using BookingSystem.Android.Factory;
using Android.Graphics;
using System.IO;
using Android.Provider;
using BookingSystem.Android.Views;

namespace BookingSystem.Android
{
    [Activity(Label = "Create Bus")]
    public class CreateBusActivity : BaseActivity
    {
        public static event EventHandler<BusInfo> OnBusCreated;

        public const int ImagePickRequestCode = 0x45;
        private bool isEditing;
        private BusInfo bus;
        private AvatarDisplay busImageView;
        private InputHandler formInputHandler = new InputHandler();
        private Bitmap busImage;

        public CreateBusActivity() : base(Resource.Layout.create_bus_layout)
        {
            OnLoaded += delegate
            {
                AllowBackNavigation();

                var intent = Intent;

                if (intent.HasExtra("bus"))
                    bus = JsonConvert.DeserializeObject<BusInfo>(intent.GetStringExtra("bus"));

                isEditing = bus?.Id != null;

                formInputHandler.SetBindings(new InputBinding[]
                {
                    new InputBinding("Name",Resource.Id.tb_bus_name , true ,  InputTypes.Text)
                    {
                         OnBind = (view) => ((TextInputLayout)view).EditText.Text = bus?.Name
                    },
                    new InputBinding("Model",Resource.Id.tb_model, true ,  InputTypes.Text)
                    {
                         OnBind = (view) => ((TextInputLayout)view).EditText.Text = bus?.Model
                    },
                    new InputBinding("SeatStart",Resource.Id.tb_bus_seat_start, true , InputTypes.Number, min: 1)
                    {
                         OnBind = (view) => ((EditText)view).Text =  (bus?.Seats?.Split('-').First() ?? "1")
                    },
                    new InputBinding("SeatEnd",Resource.Id.tb_bus_seat_end, true, InputTypes.Number, min: 1)
                    {
                         OnBind = (view) => ((EditText)view).Text = (bus?.Seats?.Split('-').Last()  ?? "90")
                    },
                }, FindViewById<ViewGroup>(global::Android.Resource.Id.Content));

                //
                busImageView = FindViewById<AvatarDisplay>(Resource.Id.img_bus);
                busImageView.PlaceHolderImageSize = new global::Android.Util.Size(128, 128);
                busImageView.Shape = AvatarDisplay.AvatarShape.RoundedRect;
                busImageView.DrawShadow = false;

                busImageView.Click += (s, e) =>
                {
                    PickGalleryImage();
                };

                if (isEditing && bus.Photo != null)
                {
                    busImageView.SetPhoto(bus.Photo,thumbnail: false);
                }

                //
                var btnSubmit = FindViewById<Button>(Resource.Id.btn_submit);
                btnSubmit.Click += async delegate
                {
                    if (formInputHandler.ValidateInputs(true).Count == 0)
                    {
                        var inputs = formInputHandler.GetInputs();
                        var proxy = ProxyFactory.GetProxyInstace();
                        if (!isEditing)
                        {
                            //  Create a new bus
                            using (this.ShowProgress(null, "Creating bus, please hold on..."))
                            {
                                var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.Create(new CreateBusInfo()
                                {
                                    Name = inputs["Name"],
                                    Model = inputs["Model"],
                                    Seats = $"{inputs["SeatStart"]}-{inputs["SeatEnd"]}",
                                }));

                                if (response.Successful)
                                {
                                    SetResult(Result.Ok);

                                    var createdBus = await response.GetDataAsync<BusInfo>();

                                    //  Upload image if any
                                    if (busImage != null)
                                    {
                                        using (var ms = new MemoryStream())
                                        {
                                            busImage.Compress(Bitmap.CompressFormat.Png, 90, ms);
                                            ms.Flush();

                                            //
                                            ms.Seek(0, SeekOrigin.Begin);
                                            response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.UploadPhoto(createdBus.Id, ms, "image/png"));
                                        }

                                    }

                                    OnBusCreated?.Invoke(this, createdBus);
                                    Finish();
                                }
                                else
                                {
                                    Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                                }
                            }
                        }
                        else
                        {
                            //  Edit the bus
                            var editBusInfo = new EditBusInfo();
                            if (bus.Name != inputs["Name"])
                                editBusInfo.Name = inputs["Name"];
                            else if (bus.Model != inputs["Model"])
                                editBusInfo.Model = inputs["Model"];
                            else if (bus.Seats != $"{inputs["SeatStart"]}-{"SeatEnd"}")
                                editBusInfo.Seats = $"{inputs["SeatStart"]}-{"SeatEnd"}";

                            //
                            if (editBusInfo.AnyUpdate())
                            {
                                using (this.ShowProgress(null, "Save changes, please hold on..."))
                                {
                                    var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.Update(bus.Id, editBusInfo));
                                    if (response.Successful)
                                    {
                                        SetResult(Result.Ok);
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


                };

                //
                btnSubmit.Text = isEditing ? "Save changes" : "Add Bus";
                SupportActionBar.Title = isEditing ? "Edit Bus" : "Add Bus";
            };
        }

        private void PickGalleryImage()
        {
            Intent intent = new Intent(Intent.ActionPick, MediaStore.Images.Media.InternalContentUri);
            intent.SetType("image/*");
            intent.PutExtra("scale", true);
            intent.PutExtra("outputX", 240);
            intent.PutExtra("outputY", 240);
            intent.PutExtra("aspectX", 1);
            intent.PutExtra("aspectY", 1);
            intent.PutExtra("return-data", true);
            StartActivityForResult(intent, ImagePickRequestCode);
        }

        protected override async void OnActivityResult(int requestCode, [GeneratedEnum] Result resultCode, Intent data)
        {
            if (requestCode == ImagePickRequestCode && resultCode == Result.Ok)
            {
                try
                {
                    busImage = data.GetParcelableExtra("data") as Bitmap;

                    if (isEditing)
                    {
                        //  Update image
                        var proxy = ProxyFactory.GetProxyInstace();
                        using (var ms = new MemoryStream())
                        {
                            busImage.Compress(Bitmap.CompressFormat.Png, 90, ms);
                            ms.Flush();

                            //
                            ms.Seek(0, SeekOrigin.Begin);
                            var response = await proxy.ExecuteAsync(API.Endpoints.BusesEndpoints.UploadPhoto(bus.Id, ms, "image/png"));
                            if (response.Successful)
                            {
                                //
                                ms.Seek(0, SeekOrigin.Begin);
                                busImageView.Image = global::Android.Graphics.BitmapFactory.DecodeStream(ms);

                                //
                                Toast.MakeText(this, "Bus profile image updated!", ToastLength.Short).Show();
                            }
                            else
                            {
                                Toast.MakeText(this, response.GetErrorDescription(), ToastLength.Short).Show();
                            }
                        }
                    }
                    else
                    {
                        busImageView.Image = busImage;
                    }

                }
                catch(Exception)
                {
                    Toast.MakeText(this, "Invalid image selected", ToastLength.Short).Show();
                }
            }
        }

        public static void Navigate(Context context, BusInfo busInfo = null, int ? requestCode = null)
        {
            Intent intent = new Intent(context, typeof(CreateBusActivity));

            if (busInfo != null)
                intent.PutExtra("bus", JsonConvert.SerializeObject(busInfo));

            if (requestCode != null)
                ((Activity)context).StartActivityForResult(intent, requestCode.Value);
            else
                context.StartActivity(intent);
        }


    }
}