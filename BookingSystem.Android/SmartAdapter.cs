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
using BookingSystem.Android.ViewHolders;

namespace BookingSystem.Android
{
    public class SmartAdapter<TItem> : BaseAdapter<TItem>
    {
        private IList<TItem> items = new List<TItem>();

        private IList<ViewBind> Bindings;

        public IList<TItem> Items
        {
            get { return items; }
            set
            {
                items = value;
                NotifyDataSetChanged();
            }
        }

        public override TItem this[int position]
        {
            get
            {
                return Items[position];
            }
        }

        public override int Count => Items.Count;

        public Context Context { get; }

        private int LayoutId;

        public SmartAdapter(Context context, int resourceLayoutId, IList<ViewBind> bindings)
        {
            Context = context;
            LayoutId = resourceLayoutId;
            Bindings = bindings;
        }

        public override long GetItemId(int position)
        {
            return position;
        }

        public override View GetView(int position, View convertView, ViewGroup parent)
        {
            ViewHolder<TItem> viewHolder = null;
            if (convertView == null)
            {
                convertView = LayoutInflater.FromContext(Context).Inflate(LayoutId, null);
                viewHolder = (ViewHolder<TItem>)Activator.CreateInstance(typeof(ViewHolder<TItem>), Items[position], Bindings);
                convertView.Tag = viewHolder;
            }
            else
            {
                viewHolder = (ViewHolder<TItem>)convertView.Tag;
                viewHolder.Item = Items[position];
            }

            viewHolder.Bind(convertView);

            return convertView;
        }
    }
}