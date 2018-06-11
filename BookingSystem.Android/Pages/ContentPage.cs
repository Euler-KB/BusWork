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
using BookingSystem.Android.Helpers;
using Android.Support.V4.Widget;
using System.Threading.Tasks;
using System.Collections;
using BookingSystem.Android.API;

namespace BookingSystem.Android.Pages
{
    public enum PageContentType
    {
        Content,
        ConnectionError,
        Empty
    }

    public class ContentPage : BasePage
    {
        private bool isBusy;

        private SwipeRefreshLayout swipeRefreshLayout;

        #region Page Content Switching

        private PageContentType _pageContentType = PageContentType.Content;

        protected PageContentType ContentType
        {
            get
            {
                return _pageContentType;
            }

            set
            {
                if (_pageContentType != value)
                {
                    SetPageContent(value);
                    _pageContentType = value;
                }
            }
        }

        private View _contentView;

        protected ConnectionErrorContent ConnectionErrorView { get; private set; }

        protected EmptyViewContent NoItemsView { get; private set; }

        protected virtual int ContentResourceId => 0;

        protected virtual int ConnectionErrorResourceId => Resource.Id.connection_error_frame;

        protected virtual int EmptyResourceId => Resource.Id.empty_frame;

        private void SetPageContent(PageContentType content)
        {
            switch (content)
            {
                case PageContentType.ConnectionError:

                    if (ConnectionErrorView != null)
                        ConnectionErrorView.View.Visibility = ViewStates.Visible;

                    if (_contentView != null)
                        _contentView.Visibility = ViewStates.Gone;

                    if (NoItemsView != null)
                        NoItemsView.View.Visibility = ViewStates.Gone;

                    break;
                case PageContentType.Content:

                    if (ConnectionErrorView != null)
                        ConnectionErrorView.View.Visibility = ViewStates.Gone;

                    if (_contentView != null)
                        _contentView.Visibility = ViewStates.Visible;

                    if (NoItemsView != null)
                        NoItemsView.View.Visibility = ViewStates.Gone;

                    break;
                case PageContentType.Empty:

                    if (ConnectionErrorView != null)
                        ConnectionErrorView.View.Visibility = ViewStates.Gone;

                    if (_contentView != null)
                        _contentView.Visibility = ViewStates.Gone;

                    if (NoItemsView != null)
                        NoItemsView.View.Visibility = ViewStates.Visible;

                    break;
            }
        }

        protected T GetContentView<T>() where T : View
        {
            return _contentView as T;
        }

        #endregion


        public ContentPage()
        {
            OnLoaded += (s, view) =>
            {
                swipeRefreshLayout = view.FindViewById<SwipeRefreshLayout>(Resource.Id.swipe_refresh_root);
                if (swipeRefreshLayout != null)
                {
                    swipeRefreshLayout.Refresh += async delegate
                    {
                        if (isBusy)
                            return;

                        using (Busy(true))
                        {
                            await OnRefreshViewAsync();
                        }
                    };

                    swipeRefreshLayout.SetColorSchemeColors(Views.AvatarDisplay.DefaultColors.Take(4).Select(x => x.ToArgb()).ToArray());
                }

                //
                _contentView = view.FindViewById(ContentResourceId);

                //
                NoItemsView = new EmptyViewContent(view.FindViewById(EmptyResourceId));
                ConnectionErrorView = new ConnectionErrorContent(view.FindViewById(ConnectionErrorResourceId));

                //
                isLoaded = HasData;

                //  Initialize logic here...
                Initialize();
            };
        }

        private bool isLoaded;

        protected bool HasData => PageDataCache.HasData(GetType());

        protected T GetData<T>() where T : class
        {
            if (!HasData)
                throw new InvalidOperationException("Page state not available!");

            //
            return PageDataCache.Get<T>(GetType());

        }

        protected void SetLoaded(object data)
        {
            isLoaded = true;

            if (data == null || (data as IList)?.Count == 0)
                ContentType = PageContentType.Empty;
            else
                ContentType = PageContentType.Content;

            //  Keep page data
            PageDataCache.Save(GetType(), data);
        }

        protected void ClearLoaded()
        {
            isLoaded = false;
            PageDataCache.ClearData(GetType());
        }

        protected void UpdateResponse(ApiResponse response)
        {
            if (response.ConnectionError)
                ContentType = PageContentType.ConnectionError;
        }

        protected bool IsLoaded => isLoaded;

        protected IDisposable Busy(bool indicator = true)
        {
            return BusyState.Begin(() =>
            {
                isBusy = true;

                if (indicator && swipeRefreshLayout != null)
                    swipeRefreshLayout.Refreshing = true;
            },
            () =>
            {
                isBusy = false;

                if (indicator && swipeRefreshLayout != null)
                    swipeRefreshLayout.Refreshing = false;

            });
        }

        public virtual Task OnRefreshViewAsync()
        {
            return Task.FromResult(0);
        }

        protected virtual void Initialize()
        {

        }

    }
}