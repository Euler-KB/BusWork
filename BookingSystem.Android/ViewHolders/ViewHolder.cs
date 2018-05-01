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

namespace BookingSystem.Android.ViewHolders
{
    public interface IViewHolder
    {
        void Bind(View view);
    }

    public class ViewBind
    {
        public int Resource { get; set; }

        public Delegate OnBind { get; set; }
    }

    public class PropertyBind<TView, TItem> : ViewBind where TView : View
    {
        public new Action<TView, TItem> OnBind
        {
            get
            {
                return (Action<TView, TItem>)base.OnBind;
            }

            set
            {
                base.OnBind = value;
            }
        }

        public PropertyBind() { }

        public PropertyBind(int resource, Action<TView, TItem> onBind)
        {
            Resource = resource;
            OnBind = onBind;
        }
    }

    public class ViewHolder<TItem> : Java.Lang.Object, IViewHolder
    {
        protected View View { get; set; }

        public TItem Item { get; set; }

        public IList<ViewBind> Bindings { get; }

        public ViewHolder(TItem item)
        {
            Item = item;
        }

        public ViewHolder(TItem item, IList<ViewBind> bindings)
        {
            Item = item;
            Bindings = bindings;
        }

        protected IDictionary<int, View> viewCache = new Dictionary<int, View>();

        private bool isLoaded;

        protected T GetView<T>(int id) where T : View
        {
            return View.FindViewById<T>(id);
        }

        protected IList<ViewBind> GetBindings()
        {
            return Bindings;
        }

        public virtual void Bind(View view)
        {
            View = view;

            //
            var bindings = GetBindings();
            if (!isLoaded)
            {
                foreach (var item in bindings)
                    viewCache[item.Resource] = view.FindViewById(item.Resource);

                isLoaded = true;
            }

            foreach (var bind in bindings)
            {
                try
                {
                    bind.OnBind.DynamicInvoke(viewCache[bind.Resource], Item);
                }
                catch
                {

                }
            }
        }
    }

}