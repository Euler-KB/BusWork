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

namespace BookingSystem.Android.ViewHolders
{
    public partial class ItemHolders
    {
        public static readonly IList<ViewBind> FeedBackItemBindings = new List<ViewBind>()
        {
            new PropertyBind<TextView,FeedbackInfoEx>(Resource.Id.lb_feedback_message , (view,feedback) => view.Text = feedback.Message),
            new PropertyBind<TextView,FeedbackInfoEx>(Resource.Id.lb_date , (view,feedback) => view.Text = view.Text = feedback.DateCreated.ToLongDateString() ),
            new PropertyBind<TextView,FeedbackInfoEx>(Resource.Id.lb_user_name , (view,feedback) =>   view.Text = view.Text = feedback.User.FullName ),
        };
    }
}