/* 
 * Xamarin C# Port of hdodenhof/CircleImageView
 * Created by Bradley Locke May 2015 - brad.locke@gmail.com
 * https://github.com/blocke79/CircleImageView.Xamarin
 * 
 * 
 * Originally Java version created by hdodenhof
 * https://github.com/hdodenhof/CircleImageView
 * 
 */

using System;
using System.IO;
using Android.App;
using Android.Content;
using Android.Content.Res;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Runtime;
using Android.Util;
using Android.Widget;
using Uri = Android.Net.Uri;

namespace CircleImageView.Xamarin
{
    public class CircleImageView : ImageView
    {
        private const int ColordrawableDimension = 2;

        private const int DEFAULT_BORDER_WIDTH = 0;
        private const bool DEFAULT_BORDER_OVERLAY = false;
        private static readonly ScaleType SCALE_TYPE = ScaleType.CenterCrop;

        private static readonly Bitmap.Config BITMAP_CONFIG = Bitmap.Config.Argb8888;
        private static readonly int DEFAULT_BORDER_COLOR = Color.Black;

        private readonly Paint mBitmapPaint = new Paint();
        private readonly Paint mBorderPaint = new Paint();
        private readonly RectF mBorderRect = new RectF();
        private readonly RectF mDrawableRect = new RectF();
        private readonly Matrix mShaderMatrix = new Matrix();

        private Bitmap mBitmap;
        private int mBitmapHeight;
        private BitmapShader mBitmapShader;
        private int mBitmapWidth;
        private int mBorderColor = DEFAULT_BORDER_COLOR;
        private bool mBorderOverlay;
        private float mBorderRadius;
        private int mBorderWidth = DEFAULT_BORDER_WIDTH;

        private ColorFilter mColorFilter;
        private float mDrawableRadius;

        private bool mReady;
        private bool mSetupPending;

        protected CircleImageView(IntPtr javaReference, JniHandleOwnership transfer) : base(javaReference, transfer)
        {
        }

        public CircleImageView(Context context) : base(context)
        {
            Init();
        }

        public CircleImageView(Context context, IAttributeSet attrs) : base(context, attrs)
        {
            Init();
        }

        public CircleImageView(Context context, IAttributeSet attrs, int defStyleAttr)
            : base(context, attrs, defStyleAttr)
        {
            TypedArray a = context.ObtainStyledAttributes(attrs, Resource.Styleable.CircleImageView, defStyleAttr, 0);

            mBorderWidth = a.GetDimensionPixelSize(Resource.Styleable.CircleImageView_border_width, DEFAULT_BORDER_WIDTH);
            mBorderColor = a.GetColor(Resource.Styleable.CircleImageView_border_color, DEFAULT_BORDER_COLOR);
            mBorderOverlay = a.GetBoolean(Resource.Styleable.CircleImageView_border_overlay, DEFAULT_BORDER_OVERLAY);

            a.Recycle();
            Init();
        }

        public CircleImageView(Context context, IAttributeSet attrs, int defStyleAttr, int defStyleRes)
            : base(context, attrs, defStyleAttr, defStyleRes)
        {
        }

        private void Init()
        {
            base.SetScaleType(SCALE_TYPE);
            mReady = true;

            if (!mSetupPending) return;
            Setup();
            mSetupPending = false;
        }

        public override ScaleType GetScaleType()
        {
            base.GetScaleType();
            return SCALE_TYPE;
        }

        public override void SetScaleType(ScaleType scaleType)
        {
            {
                if (scaleType != SCALE_TYPE)
                {
                    throw new ArgumentException(string.Format("ScaleType {0} not supported.", scaleType));
                }
            }
        }

        public override void SetAdjustViewBounds(Boolean adjustViewBounds)
        {
            {
                if (adjustViewBounds)
                {
                    throw new ArgumentException("adjustViewBounds not supported.");
                }
            }
        }


        protected override void OnDraw(Canvas canvas)
        {
            if (Drawable == null)
            {
                return;
            }

            canvas.DrawCircle(Width/2, Height/2, mDrawableRadius, mBitmapPaint);
            if (mBorderWidth != 0)
            {
                canvas.DrawCircle(Width/2, Height/2, mBorderRadius, mBorderPaint);
            }
        }

        protected override void OnSizeChanged(int w, int h, int oldw, int oldh)
        {
            base.OnSizeChanged(w, h, oldw, oldh);
            Setup();
        }

        public int BorderColor()
        {
            return mBorderColor;
        }

        public void SetBorderColor(int borderColor)
        {
            {
                if (borderColor == mBorderColor)
                {
                    return;
                }

                mBorderColor = borderColor;
                mBorderPaint.Color = new Color(mBorderColor);
                Invalidate();
            }
        }

        public void SetBorderColorResource(int borderColorRes)
        {
            {
                SetBorderColor(Context.Resources.GetColor(borderColorRes));
            }
        }

        public int BorderWidth()
        {
            return mBorderWidth;
        }

        public void SetBorderWidth(int borderWidth)
        {
            {
                if (borderWidth == mBorderWidth)
                {
                    return;
                }

                mBorderWidth = borderWidth;
                Setup();
            }
        }

        public Boolean BorderOverlay()
        {
            return mBorderOverlay;
        }

        public void SetBorderOverlay(Boolean borderOverlay)
        {
            {
                if (borderOverlay == mBorderOverlay)
                {
                    return;
                }

                mBorderOverlay = borderOverlay;
                Setup();
            }
        }

        public override void SetImageBitmap(Bitmap bm)
        {
            {
                base.SetImageBitmap(bm);
                mBitmap = bm;
                Setup();
            }
        }

        public override void SetImageDrawable(Drawable drawable)
        {
            {
                base.SetImageDrawable(drawable);
                mBitmap = GetBitmapFromDrawable(drawable);
                Setup();
            }
        }

        public override void SetImageResource(int resId)
        {
            {
                base.SetImageResource(resId);
                mBitmap = GetBitmapFromDrawable(Resources.GetDrawable(resId));
                Setup();
            }
        }

        public override void SetImageURI(Uri uri)
        {
            base.SetImageURI(uri);
            Stream stream = Application.Context.ContentResolver.OpenInputStream(uri);
            Drawable drawable = Drawable.CreateFromStream(stream, uri.ToString());
            mBitmap = GetBitmapFromDrawable(drawable);
            Setup();
        }

        public override void SetColorFilter(ColorFilter cf)
        {
            base.SetColorFilter(cf);

            if (cf == mColorFilter)
            {
                return;
            }

            mColorFilter = cf;
            mBitmapPaint.SetColorFilter(mColorFilter);
            Invalidate();
        }

        private Bitmap GetBitmapFromDrawable(Drawable drawable)
        {
            if (drawable == null)
            {
                return null;
            }

            var bitmapDrawable = drawable as BitmapDrawable;
            if (bitmapDrawable != null)
            {
                return bitmapDrawable.Bitmap;
            }

            try
            {
                Bitmap bitmap;

                if (drawable is ColorDrawable)
                {
                    bitmap = Bitmap.CreateBitmap(ColordrawableDimension, ColordrawableDimension, BITMAP_CONFIG);
                }
                else
                {
                    bitmap = Bitmap.CreateBitmap(drawable.IntrinsicWidth, drawable.IntrinsicHeight, BITMAP_CONFIG);
                }

                var canvas = new Canvas(bitmap);
                drawable.SetBounds(0, 0, canvas.Width, canvas.Height);
                drawable.Draw(canvas);
                return bitmap;
            }
            catch (OutOfMemoryException)
            {
                return null;
            }
        }

        private void Setup()
        {
            if (!mReady)
            {
                mSetupPending = true;
                return;
            }

            if (mBitmap == null)
            {
                return;
            }

            mBitmapShader = new BitmapShader(mBitmap, Shader.TileMode.Clamp, Shader.TileMode.Clamp);

            mBitmapPaint.AntiAlias = true;
            mBitmapPaint.SetShader(mBitmapShader);
            mBorderPaint.SetStyle(Paint.Style.Stroke);
            mBorderPaint.AntiAlias = true;
            mBorderPaint.Color = new Color(mBorderColor);
            mBorderPaint.StrokeWidth = mBorderWidth;

            mBitmapHeight = mBitmap.Height;
            mBitmapWidth = mBitmap.Width;

            mBorderRect.Set(0, 0, Width, Height);
            mBorderRadius = Math.Min((mBorderRect.Height() - mBorderWidth)/2, (mBorderRect.Width() - mBorderWidth)/2);

            mDrawableRect.Set(mBorderRect);
            if (!mBorderOverlay)
            {
                mDrawableRect.Inset(mBorderWidth, mBorderWidth);
            }
            mDrawableRadius = Math.Min(mDrawableRect.Height()/2, mDrawableRect.Width()/2);

            UpdateShaderMatrix();
            Invalidate();
        }

        private void UpdateShaderMatrix()
        {
            float scale;
            float dx = 0;
            float dy = 0;

            mShaderMatrix.Set(null);

            if (mBitmapWidth*mDrawableRect.Height() > mDrawableRect.Width()*mBitmapHeight)
            {
                scale = mDrawableRect.Height()/mBitmapHeight;
                dx = (mDrawableRect.Width() - mBitmapWidth*scale)*0.5f;
            }
            else
            {
                scale = mDrawableRect.Width()/mBitmapWidth;
                dy = (mDrawableRect.Height() - mBitmapHeight*scale)*0.5f;
            }

            mShaderMatrix.SetScale(scale, scale);
            mShaderMatrix.PostTranslate((int) (dx + 0.5f) + mDrawableRect.Left, (int) (dy + 0.5f) + mDrawableRect.Top);

            mBitmapShader.SetLocalMatrix(mShaderMatrix);
        }
    }
}