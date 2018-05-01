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
using Android.Support.V7.Widget;
using Android.Content.Res;
using AlertDialog = Android.Support.V7.App.AlertDialog;

namespace BookingSystem.Android.Views
{
    [Register("booking.system.MultiSelectSpinner")]
    public class MultiSelectSpinner : AppCompatSpinner
    {
        private IList<string> items = new List<string>();
        private bool[] selected = new bool[0];
        private string defaultText = "Select Items";
        private string spinnerTitle = "Select ";

        public event EventHandler<bool[]> OnSelected;

        public string Title
        {
            get { return spinnerTitle; }
            set { spinnerTitle = value; }
        }

        public string DefaultText
        {
            get { return defaultText; }
            set { defaultText = value; }
        }

        public MultiSelectSpinner(Context context) : base(context)
        {
            Initialize(null);
        }

        public MultiSelectSpinner(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(attrs);
        }

        public MultiSelectSpinner(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(attrs);
        }

        public MultiSelectSpinner(Context context, IAttributeSet attrs, int defStyleAttr, int mode, global::Android.Content.Res.Resources.Theme popupTheme) : base(context, attrs, defStyleAttr, mode, popupTheme)
        {
            Initialize(attrs);
        }

        public MultiSelectSpinner(Context context, IAttributeSet attrs, int defStyleAttr, int mode) : base(context, attrs, defStyleAttr, mode)
        {
            Initialize(attrs);
        }

        public MultiSelectSpinner(Context context, int mode) : base(context, mode)
        {
            Initialize(null);
        }

        public void OnClick(DialogInterface dialog, int which, bool isChecked)
        {
            selected[which] = isChecked;
        }

        protected void Initialize(IAttributeSet attrs)
        {
            if (attrs != null)
            {
                TypedArray a = Context.ObtainStyledAttributes(attrs, Resource.Styleable.MultiSpinnerSearch);

                if (a.HasValue(Resource.Styleable.MultiSpinnerSearch_hintText))
                    spinnerTitle = a.GetString(Resource.Styleable.MultiSpinnerSearch_hintText);

                if (a.HasValue(Resource.Styleable.MultiSpinnerSearch_emptyText))
                    defaultText = a.GetString(Resource.Styleable.MultiSpinnerSearch_emptyText);


                a.Recycle();
            }

            Adapter = new ArrayAdapter<string>(Context, Resource.Layout.spinner_text_view, new string[] { Label });
        }

        private string Label
        {
            get
            {
                if (items.Count == 0)
                    return DefaultText;

                List<string> selectedItems = new List<string>();
                for (int i = 0; i < items.Count; i++)
                {
                    if (selected[i])
                        selectedItems.Add(items[i]);
                }


                string label = "";
                if (selectedItems.Count > 0)
                {
                    label = string.Join(",", selectedItems);
                }
                else
                {
                    label = spinnerTitle;
                }

                return label;
            }
        }

        public void OnCancel()
        {
            Adapter = new ArrayAdapter<string>(Context, Resource.Layout.spinner_text_view, new string[] { Label });
            if (selected.Length > 0)
            {
                OnSelected?.Invoke(this, selected);
            }

        }

        public override bool PerformClick()
        {
            if (items?.Count > 0)
            {

                var dlg = new AlertDialog.Builder(Context)
                    .SetTitle(spinnerTitle)
                    .SetMultiChoiceItems(items.ToArray(), selected, new EventHandler<DialogMultiChoiceClickEventArgs>((s,e) =>
                    {
                        selected[e.Which] = e.IsChecked;
                    }))
                    .SetPositiveButton(global::Android.Resource.String.Ok, delegate {
                    })
                    .Create();

                dlg.DismissEvent += (s, e) =>
                {
                    OnCancel();
                };

                dlg.Show();
            }

            return true;
        }


        /**
         * Sets items to this spinner.
         *
         * @param items    A TreeMap where the keys are the values to display in the spinner
         *                 and the value the initial selected state of the key.
         */
        public void SetItems(IList<KeyValuePair<string, bool>> items)
        {
            this.items = items.Select(x => x.Key).ToArray();
            var values = items.Select(x => x.Value).ToArray();
            selected = new bool[values.Length];
            for (int i = 0; i < items.Count; i++)
            {
                selected[i] = values[i];
            }

            // all text on the spinner
            Adapter = new ArrayAdapter<string>(Context, Resource.Layout.spinner_text_view, new string[] { Label });

            // Set Spinner Text
            OnCancel();
        }

    }

}