using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Android.App;
using Android.Content;
using Android.Graphics;
using Android.OS;
using Android.Runtime;
using Android.Views;
using Android.Widget;
using System.Threading.Tasks;
using BookingSystem.Android.Factory;
using BookingSystem.API.Models.DTO;
using Square.Picasso;
using Android.Util;

namespace BookingSystem.Android.Views
{
    [Register("booking.system.AvatarDisplay")]
    public class AvatarDisplay : View
    {
        public enum AvatarShape
        {
            Circle,
            RoundedRect
        }

        public static readonly Color OverlayColor;

        static AvatarDisplay()
        {
            //  
            OverlayColor = Color.LightGray;
            OverlayColor.A = 50;

        }

        public static Color[] DefaultColors = new Color[]
        {
            Color.ParseColor("#d73d32"),
            Color.ParseColor( "#7e3794" ),
            Color.ParseColor( "#4285f4" ),
            Color.ParseColor( "#67ae3f" ),
            Color.ParseColor( "#d61a7f" ),
            Color.ParseColor( "#ff4080")
        };

        private Bitmap image;
        private Bitmap placeHolderImage;

        private bool _fetchImage;

        public Bitmap Image
        {
            get { return image; }
            set
            {
                image = value;
                _fetchImage = false;
                Invalidate();
            }
        }

        public Bitmap PlaceHolderImage
        {
            get { return placeHolderImage; }
            set
            {
                if (placeHolderImage != value)
                {
                    placeHolderImage = value;
                    Invalidate();
                }
            }
        }

        private AvatarShape shape = AvatarShape.Circle;
        public AvatarShape Shape
        {
            get { return shape; }
            set
            {
                if (shape != value)
                {
                    shape = value;
                    Invalidate();
                }
            }
        }

        private Size placeHolderImageSize;
        public Size PlaceHolderImageSize
        {
            get { return placeHolderImageSize; }
            set
            {
                if (value != placeHolderImageSize)
                {
                    placeHolderImageSize = value;
                    Invalidate();
                }
            }
        }

        private Size imageSize;
        public Size ImageSize
        {
            get { return imageSize; }
            set
            {
                if (value != imageSize)
                {
                    imageSize = value;
                    Invalidate();
                }
            }
        }

        public async void SetPhoto(MediaInfo profile, bool thumbnail = true)
        {
            var bitmap = await Task.Run(() =>
            {
                _fetchImage = true;
                try
                {
                    var uri = global::Android.Net.Uri.Parse(ProxyFactory.GetProxyInstace().GetUri(thumbnail ? profile.ThumbnailUri : profile.Uri));
                    return Picasso.With(Context)
                                        .Load(uri)
                                        .Get();
                }
                catch
                {
                    return null;
                }
            });

            if (bitmap != null)
            {
                this.image?.Recycle();
                Image = bitmap;
            }
        }

        private string name;
        public string Name
        {
            get { return name; }
            set
            {
                if (value != name)
                {
                    name = value;
                    Invalidate();
                }
            }
        }

        private Color borderColor = Color.White;
        public Color BorderColor
        {
            get { return borderColor; }
            set
            {
                if (borderColor != value)
                {
                    borderColor = value;
                    Invalidate();
                }
            }
        }

        private Color backgroundColor = Color.Transparent;
        public Color BackgroundColor
        {
            get { return backgroundColor; }
            set
            {
                if (value != backgroundColor)
                {
                    backgroundColor = value;
                    Invalidate();
                }
            }
        }

        private float borderRadius = 8;
        public float BorderRadius
        {
            get { return borderRadius; }
            set
            {
                if (borderRadius != value)
                {
                    borderRadius = value;
                    Invalidate();
                }
            }
        }

        private bool enableShadow;
        public bool DrawShadow
        {
            get { return enableShadow; }
            set
            {
                if (value != enableShadow)
                {
                    if (value)
                    {
                        SetLayerType(LayerType.Software, null);
                    }
                    else
                    {
                        SetLayerType(LayerType.Hardware, null);
                    }

                    enableShadow = value;
                    Invalidate();
                }
            }
        }

        private float shadowRadius = 10;
        public float ShadowRadius
        {
            get { return shadowRadius; }
            set
            {
                if (shadowRadius != value)
                {
                    shadowRadius = value;
                    Invalidate();
                }
            }
        }

        private float shadowOffsetX = 0;
        public float ShadowOffsetX
        {
            get { return shadowOffsetX; }
            set
            {
                if (shadowOffsetX != value)
                {
                    shadowOffsetX = value;
                    Invalidate();
                }
            }
        }

        private float shadowOffsetY = 0;
        public float ShadowOffsetY
        {
            get { return shadowOffsetY; }
            set
            {
                if (shadowOffsetY != value)
                {
                    shadowOffsetY = value;
                    Invalidate();
                }
            }
        }

        private Color shadowColor = Color.Black;
        public Color ShadowColor
        {
            get { return shadowColor; }
            set
            {
                if (shadowColor != value)
                {
                    shadowColor = value;
                    Invalidate();
                }
            }
        }

        private int renderSize = 122;
        public int RenderSize
        {
            get { return renderSize; }
            set
            {
                if (renderSize != value)
                {
                    renderSize = value;
                    Invalidate();
                }
            }
        }

        public AvatarDisplay(Context context) : base(context)
        {
            Initialize(null);
        }

        public AvatarDisplay(Context context, IAttributeSet attrs, int defStyleAttr) : base(context, attrs, defStyleAttr)
        {
            Initialize(attrs);
        }

        protected AvatarDisplay(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {

        }

        public AvatarDisplay(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Initialize(attrs);
        }

        public AvatarDisplay(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes) : base(context, attrs, defStyleAttr, defStyleRes)
        {
            Initialize(attrs);
        }

        private void Initialize(IAttributeSet attrs)
        {
            if (attrs != null)
            {
                var style = Context.ObtainStyledAttributes(attrs, Resource.Styleable.AvatarDisplay);
                var placeHolder = style.GetResourceId(Resource.Styleable.AvatarDisplay_placeholder, -1);
                if (placeHolder != -1)
                    placeHolderImage = global::Android.Graphics.BitmapFactory.DecodeResource(Context.Resources, placeHolder);

                var src = style.GetResourceId(Resource.Styleable.AvatarDisplay_src, -1);
                if (src != -1)
                    image = global::Android.Graphics.BitmapFactory.DecodeResource(Context.Resources, src);

                renderSize = (int)style.GetDimension(Resource.Styleable.AvatarDisplay_viewSize, 122);
                borderColor = style.GetColor(Resource.Styleable.AvatarDisplay_borderColor, Color.White.ToArgb());
                borderRadius = style.GetDimension(Resource.Styleable.AvatarDisplay_borderRadius, 8);
                enableShadow = style.GetBoolean(Resource.Styleable.AvatarDisplay_shadow, true);
                shadowRadius = style.GetDimension(Resource.Styleable.AvatarDisplay_shadowRadius, 10);
                shadowOffsetX = style.GetDimension(Resource.Styleable.AvatarDisplay_shadowOffsetX, 0);
                shadowOffsetY = style.GetDimension(Resource.Styleable.AvatarDisplay_shadowOffsetY, 0);
                shadowColor = style.GetColor(Resource.Styleable.AvatarDisplay_shadowColor, Color.Black.ToArgb());
                backgroundColor = style.GetColor(Resource.Styleable.AvatarDisplay_backgroundColor, Color.White);
            }


            if (enableShadow && LayerType != LayerType.Software)
            {
                //  Ensure layer type is 
                SetLayerType(LayerType.Software, null);
            }
        }

        public static string GetAvatarName(string name)
        {
            return string.Concat(name.Replace("  ", " ").Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries).Take(2).Select(x => x[0])).ToUpper();
        }

        public static Color GetColor(string name)
        {
            return DefaultColors[name.Select(x => (int)x).Aggregate((a, b) => (a + b)) % DefaultColors.Length];
        }

        protected virtual string AvatarName
        {
            get
            {
                return GetAvatarName(Name);
            }
        }

        protected virtual Color GetAvatarBackgroundColor(string avatarName)
        {
            return GetColor(avatarName);
        }

        protected override void OnDraw(Canvas canvas)
        {
            float centerX = Width / 2F;
            float centerY = Height / 2F;
            float uniformWidth = Math.Min(Math.Min(centerX, centerY), RenderSize);

            //
            Path pathClip = new Path();
            switch (shape)
            {
                case AvatarShape.Circle:
                    pathClip.AddCircle(centerX, centerY, uniformWidth - (BorderRadius * 2), Path.Direction.Cw);
                    break;
                case AvatarShape.RoundedRect:
                    pathClip.AddRoundRect(0, 0, Width, Height, 5, 5, Path.Direction.Ccw);
                    break;
            }

            canvas.Save(SaveFlags.Clip);

            canvas.ClipPath(pathClip);

            //
            canvas.DrawColor(backgroundColor);

            if (_fetchImage && placeHolderImage == null)
            {
                canvas.DrawColor(Color.LightGray);
            }
            else
            {
                if (image != null)
                {
                    float subX = imageSize != null ? imageSize.Width / 2F : image.Width / 2F;
                    float subY = imageSize != null ? imageSize.Height / 2F : image.Height / 2F;
                    canvas.DrawBitmap(image,
                        new Rect(0, 0, image.Width, image.Height),
                        new Rect((int)(centerX - subX),
                        (int)(centerY - subY),
                        (int)(centerX - subX + subX * 2F),
                        (int)(centerY - subY + subY * 2F)),
                        null);
                }
                else if (placeHolderImage != null)
                {
                    float subX = placeHolderImageSize != null ? placeHolderImageSize.Width / 2F : placeHolderImage.Width / 2F;
                    float subY = placeHolderImageSize != null ? placeHolderImageSize.Height / 2F : placeHolderImage.Height / 2F;
                    canvas.DrawBitmap(placeHolderImage, new Rect(0, 0, placeHolderImage.Width, placeHolderImage.Height), new Rect((int)(centerX - subX), (int)(centerY - subY),
                        (int)((centerX - subX) + subX * 2F),
                        (int)((centerY - subY) + subY * 2F)),
                        null);

                    if (_fetchImage)
                    {
                        canvas.DrawColor(OverlayColor);
                    }
                }
                else if (Name != null)
                {
                    string avatarName = AvatarName;

                    Paint p = new Paint()
                    {
                        Color = GetAvatarBackgroundColor(avatarName),
                    };

                    p.Flags |= PaintFlags.AntiAlias;

                    //
                    p.SetStyle(Paint.Style.Fill);
                    canvas.DrawPaint(p);

                    //  Set 
                    p.Color = Color.White;
                    p.SetTypeface(Typeface.SansSerif);
                    p.TextSize = 60;

                    //
                    var metrics = p.GetFontMetrics();

                    float width = p.MeasureText(avatarName);
                    canvas.DrawText(avatarName, centerX - width / 2F, centerY + metrics.Bottom, p);

                }
            }

            if (BorderRadius > 0)
            {
                canvas.Restore();

                //  
                var cPaint = new Paint();
                cPaint.Flags |= PaintFlags.AntiAlias;
                cPaint.Color = BorderColor;
                cPaint.StrokeWidth = BorderRadius;
                cPaint.SetStyle(Paint.Style.Stroke);

                if (DrawShadow && !canvas.IsHardwareAccelerated)
                {
                    cPaint.SetShadowLayer(ShadowRadius, ShadowOffsetX, ShadowOffsetY, ShadowColor);
                }

                //
                switch (shape)
                {
                    case AvatarShape.Circle:
                        canvas.DrawCircle(centerX, centerY, uniformWidth - (BorderRadius * 2F), cPaint);
                        break;
                    case AvatarShape.RoundedRect:
                        {
                            canvas.DrawRoundRect(0, 0, Width, Height, 5, 5, cPaint);
                        }
                        break;
                }
            }
        }
    }
}