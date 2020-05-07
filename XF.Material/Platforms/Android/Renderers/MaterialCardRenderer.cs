using System.ComponentModel;
using Android.Content;
using Android.Graphics;
using Android.Graphics.Drawables;
using Android.Util;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XF.Material.Droid.Renderers;
using XF.Material.Forms.UI;
using static Android.Views.View;

[assembly: ExportRenderer(typeof(MaterialCard), typeof(MaterialCardRenderer))]

namespace XF.Material.Droid.Renderers
{
    public class MaterialCardRenderer : Xamarin.Forms.Platform.Android.AppCompat.FrameRenderer, IOnTouchListener
    {
        private Bitmap maskBitmap;
        private Paint paint, maskPaint;
        private MaterialCard _materialCard;

        public MaterialCardRenderer(Context context) : base(context)
        {
            paint = new Paint(PaintFlags.AntiAlias);

            maskPaint = new Paint(PaintFlags.AntiAlias | PaintFlags.FilterBitmap);
            maskPaint.SetXfermode(new PorterDuffXfermode(PorterDuff.Mode.Clear));

            SetWillNotDraw(false);
        }


        public bool OnTouch(Android.Views.View v, MotionEvent e)
        {
            if (_materialCard.GestureRecognizers.Count <= 0 || Control.Foreground == null)
            {
                return false;
            }

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    Control.Foreground.SetHotspot(e.GetX(), e.GetY());
                    Control.Pressed = true;
                    break;
                case MotionEventActions.Up:
                case MotionEventActions.Cancel:
                case MotionEventActions.Outside:
                    Control.Pressed = false;
                    break;
            }
            return false;
        }

        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement == null)
            {
                return;
            }

            _materialCard = Element as MaterialCard;

            UpdateStrokeColor();
            UpdateCornerRadiaus();
            Control.Elevate(_materialCard.Elevation);
            SetClickable();
            Control.SetOnTouchListener(this);
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            switch (e?.PropertyName)
            {
                case nameof(MaterialCard.Elevation):
                    Control.Elevate(_materialCard.Elevation);
                    break;
                case nameof(MaterialCard.IsClickable):
                    SetClickable();
                    break;
                case nameof(Frame.BackgroundColor):
                    UpdateStrokeColor();
                    break;
                case nameof(MaterialCard.ExtendedCornerRadius):
                    UpdateCornerRadiaus();
                    break;
            }
        }

        private void SetClickable()
        {
            var clickable = _materialCard.IsClickable;
            if (clickable && Control.Foreground == null)
            {
                var outValue = new TypedValue();
                Context.Theme.ResolveAttribute(
                    Android.Resource.Attribute.SelectableItemBackground, outValue, true);
                Control.Foreground = Context.GetDrawable(outValue.ResourceId);
            }

            Control.Focusable = clickable;
            Control.Clickable = clickable;
        }


        protected override void DispatchDraw(Canvas canvas)
        {
            base.DispatchDraw(canvas);

            var card = Element as MaterialCard;

            if (card != null && card.CornerRadius == 0)
            {
                var cornerRadius = card.ExtendedCornerRadius;
                var path = new Path();

                var topLeftCorner = Context.ToPixels(cornerRadius.TopLeft);
                var topRightCorner = Context.ToPixels(cornerRadius.TopRight);
                var bottomLeftCorner = Context.ToPixels(cornerRadius.BottomLeft);
                var bottomRightCorner = Context.ToPixels(cornerRadius.BottomRight);

                var cornerRadii = new[]
                {
                    topLeftCorner,
                    topLeftCorner,

                    topRightCorner,
                    topRightCorner,

                    bottomRightCorner,
                    bottomRightCorner,

                    bottomLeftCorner,
                    bottomLeftCorner,
                };

                path.AddRoundRect(new RectF(0, 0, Width, Height), cornerRadii, Path.Direction.Cw);

                canvas.ClipPath(path);
            }
        }


        private void UpdateCornerRadiaus()
        {
            var drawable = (GradientDrawable)Control.Background;
            var card = Element as MaterialCard;

            if (card != null && card.CornerRadius == 0)
            {
                var cornerRadius = card.ExtendedCornerRadius;

                var topLeftCorner = Context.ToPixels(cornerRadius.TopLeft);
                var topRightCorner = Context.ToPixels(cornerRadius.TopRight);
                var bottomLeftCorner = Context.ToPixels(cornerRadius.BottomLeft);
                var bottomRightCorner = Context.ToPixels(cornerRadius.BottomRight);

                var cornerRadii = new[]
                {
                    topLeftCorner,
                    topLeftCorner,

                    topRightCorner,
                    topRightCorner,

                    bottomRightCorner,
                    bottomRightCorner,

                    bottomLeftCorner,
                    bottomLeftCorner,
                };

                drawable.SetCornerRadii(cornerRadii);
            }
        }

        private void UpdateStrokeColor()
        {
            var borderColor = _materialCard.BorderColor.IsDefault ? _materialCard.BackgroundColor : _materialCard.BorderColor;
            var drawable = (GradientDrawable)Control.Background;
            drawable.SetStroke((int)MaterialHelper.ConvertDpToPx(1), borderColor.ToAndroid());
        }
    }
}
