using System.ComponentModel;
using CoreAnimation;
using CoreGraphics;
using UIKit;
using Xamarin.Forms;
using Xamarin.Forms.Platform.iOS;
using XF.Material.Forms.UI;
using XF.Material.iOS.GestureRecognizers;
using XF.Material.iOS.Renderers;

[assembly: ExportRenderer(typeof(MaterialCard), typeof(MaterialCardRenderer))]
namespace XF.Material.iOS.Renderers
{
    public class MaterialCardRenderer : FrameRenderer
    {
        private MaterialCard _materialCard;

        private UIColor _rippleColor;
        private UITapGestureRecognizer _rippleGestureRecognizerDelegate = null;


        protected override void OnElementChanged(ElementChangedEventArgs<Frame> e)
        {
            base.OnElementChanged(e);

            if (e?.NewElement == null)
            {
                return;
            }


            _materialCard = Element as MaterialCard;
            if (_materialCard != null)
            {
                this.Elevate(_materialCard.Elevation);
            }

            SetupColors();
            SetIsClickable();
        }

        private CGRect frame;

        public override CGRect Frame
        {
            get
            {
                return base.Frame;
            }
            set
            {
                base.Frame = value;

                if (frame != value)
                {
                    frame = value;
                    DrawRoundCorners();
                }
            }
        }

        private void DrawRoundCorners()
        {
            var card = Element as MaterialCard;

            if (card == null)
                return;

            if (card.CornerRadius != 0)
                return;

            var radius = RetrieveCommonCornerRadius(card.ExtendedCornerRadius);

            if (radius <= 0)
                return;

            if (Layer.Bounds.IsEmpty)
                return;


            var roundedCorners = RetrieveRoundedCorners(card.ExtendedCornerRadius);

            UIBezierPath mPath = UIBezierPath.FromRoundedRect(Layer.Bounds, roundedCorners, new CGSize(width: radius, height: radius));
            CAShapeLayer maskLayer = new CAShapeLayer();
            maskLayer.Frame = Layer.Bounds;
            maskLayer.Path = mPath.CGPath;
            Layer.Mask = maskLayer;
        }


        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            base.OnElementPropertyChanged(sender, e);

            if (e?.PropertyName == nameof(MaterialCard.Elevation) || e?.PropertyName == nameof(VisualElement.BackgroundColor))
            {
                this.Elevate(_materialCard.Elevation);
            }

            switch (e?.PropertyName)
            {
                // For some reason the Elevation will get messed up when the background
                // color is modified. So this fixes it.
                //
                case nameof(MaterialCard.BackgroundColor):
                    SetupColors();
                    SetIsClickable();
                    this.Elevate(_materialCard.Elevation);
                    break;
                case nameof(MaterialCard.IsClickable):
                    SetIsClickable();
                    break;
            }
        }

        private void SetupColors()
        {
            _rippleColor = BackgroundColor.IsColorDark() ? Color.FromHex("#52FFFFFF").ToUIColor() : Color.FromHex("#52000000").ToUIColor();
        }

        private void SetIsClickable()
        {
            var clickable = _materialCard.IsClickable;
            if (clickable)
            {
                if (_rippleGestureRecognizerDelegate == null)
                {
                    _rippleGestureRecognizerDelegate = new MaterialRippleGestureRecognizer(_rippleColor.CGColor, this);
                }
                else
                {
                    RemoveGestureRecognizer(_rippleGestureRecognizerDelegate);
                }

                AddGestureRecognizer(_rippleGestureRecognizerDelegate);
            }
            else if (_rippleGestureRecognizerDelegate != null)
            {
                RemoveGestureRecognizer(_rippleGestureRecognizerDelegate);
            }
        }



        // A very basic way of retrieving same one value for all of the corners
        private double RetrieveCommonCornerRadius(CornerRadius cornerRadius)
        {
            var commonCornerRadius = cornerRadius.TopLeft;
            if (commonCornerRadius <= 0)
            {
                commonCornerRadius = cornerRadius.TopRight;
                if (commonCornerRadius <= 0)
                {
                    commonCornerRadius = cornerRadius.BottomLeft;
                    if (commonCornerRadius <= 0)
                    {
                        commonCornerRadius = cornerRadius.BottomRight;
                    }
                }
            }

            return commonCornerRadius;
        }


        private UIRectCorner RetrieveRoundedCorners(CornerRadius cornerRadius)
        {
            var roundedCorners = default(UIRectCorner);

            if (cornerRadius.TopLeft > 0)
            {
                roundedCorners |= UIRectCorner.TopLeft;
            }

            if (cornerRadius.TopRight > 0)
            {
                roundedCorners |= UIRectCorner.TopRight;
            }

            if (cornerRadius.BottomLeft > 0)
            {
                roundedCorners |= UIRectCorner.BottomLeft;
            }

            if (cornerRadius.BottomRight > 0)
            {
                roundedCorners |= UIRectCorner.BottomRight;
            }

            return roundedCorners;
        }
    }
}