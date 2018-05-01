using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.OS;
using Android.Runtime;
using Android.Util;
using Android.Views;
using Android.Widget;
using BookingSystem.Android.Helpers;

namespace BookingSystem.Android.Views
{
    [Register("booking.system.DateTimeSelector")]
    public class DateTimeSelector : LinearLayout
    {
        private DateTime? selectedDate;
        private TextView lbTitle;
        private TextView lbSelectedDate;
        private ImageButton btnSelectDate;
        private string hint = "Tap to select date";

        public bool IncludeTime { get; set; } = true;

        public DateTimeSelector(Context context, IAttributeSet attrs) :
            base(context, attrs)
        {
            Initialize(attrs);
        }

        public DateTimeSelector(Context context, IAttributeSet attrs, int defStyle) :
            base(context, attrs, defStyle)
        {
            Initialize(attrs);
        }

        protected DateTimeSelector(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public DateTimeSelector(Context context) : base(context)
        {
            Initialize(null);
        }

        public DateTimeSelector(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize(null);
        }

        public string Title
        {
            get { return lbTitle.Text; }
            set { lbTitle.Text = value; }
        }

        public string Hint
        {
            get { return hint; }
            set
            {
                hint = value;
                UpdateLabel();
            }
        }

        public DateTime? Date
        {
            get { return selectedDate; }
            set
            {
                selectedDate = value;
                UpdateLabel();
            }
        }

        private void UpdateLabel()
        {
            if (selectedDate == null)
            {
                lbSelectedDate.Text = hint;
            }
            else
            {
                lbSelectedDate.Text = IncludeTime ? $"{selectedDate.Value.ToShortDateString()} {selectedDate.Value.ToShortTimeString()}" : selectedDate.Value.ToShortDateString();
            }
        }

        protected void OnSelectDate()
        {
            Context.SelectDate(IncludeTime, _date =>
            {
                Date = _date;
            }, selectedDate);
        }

        private void Initialize(IAttributeSet attrs)
        {
            Orientation = Orientation.Vertical;
            Inflate(Context, Resource.Layout.date_select_view_layout, this);

            //
            btnSelectDate = FindViewById<ImageButton>(Resource.Id.btn_select_date);
            btnSelectDate.Click += delegate
            {
                OnSelectDate();
            };

            lbTitle = FindViewById<TextView>(Resource.Id.lb_title);
            lbSelectedDate = FindViewById<TextView>(Resource.Id.lb_selected_date);

            lbSelectedDate.Click += delegate
            {
                OnSelectDate();
            };


            //
            if (attrs != null)
            {
                var typedArray = Context.ObtainStyledAttributes(attrs, Resource.Styleable.DateTimeSelector);
                Title = typedArray.GetString(Resource.Styleable.DateTimeSelector_title);
                Hint = typedArray.GetString(Resource.Styleable.DateTimeSelector_hint);

                var date = typedArray.GetString(Resource.Styleable.DateTimeSelector_date);
                if (DateTime.TryParse(date, out var result))
                {
                    Date = result;
                }
            }
        }
    }
}