using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Text;
using Android.Content;
using Android.Runtime;
using Android.Views;
using Xamarin.Forms;
using Xamarin.Forms.Platform.Android;
using XF.Material.Platforms.Android.Renderers.Internals;
using XF.Material.UI.Dialogs;

[assembly: ExportRenderer(typeof(DialogAnchor), typeof(DialogAnchorRenderer))]
namespace XF.Material.Platforms.Android.Renderers.Internals
{
    public class DialogAnchorRenderer : VisualElementRenderer<Xamarin.Forms.View>
    {
        float originalX;
        float originalY;
        float dX;
        float dY;
        bool firstTime = true;
        bool touchedDown = false;


        public DialogAnchorRenderer(Context context) : base(context)
        {

        }

        protected override void OnElementChanged(ElementChangedEventArgs<Xamarin.Forms.View> e)
        {
            base.OnElementChanged(e);


            if (e.OldElement != null)
            {
                LongClick -= HandleLongClick;
            }
            if (e.NewElement != null)
            {
                LongClick += HandleLongClick;
                var dragView = Element as DialogAnchor;
                //dragView.RestorePositionCommand = new Command(() =>
                //{
                //    if (!firstTime)
                //    {
                //        SetX(originalX);
                //        SetY(originalY);
                //    }

                //});
            }

        }

        private void HandleLongClick(object sender, LongClickEventArgs e)
        {
            var dragView = Element as DialogAnchor;
            if (firstTime)
            {
                originalX = dragX;
                originalY = dragY;
                firstTime = false;
            }
            dragView._DragStarted();
            touchedDown = true;
        }

        protected override void OnElementPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            var dragView = Element as DialogAnchor;
            base.OnElementPropertyChanged(sender, e);

        }
        protected override void OnVisibilityChanged(global::Android.Views.View changedView, [GeneratedEnum] ViewStates visibility)
        {
            base.OnVisibilityChanged(changedView, visibility);
            if (visibility == ViewStates.Visible)
            {
            }
        }

        public override bool OnTouchEvent(MotionEvent e)
        {
            float x = e.RawX;
            float y = e.RawY;

            var dragView = Element as DialogAnchor;

            this.RequestDisallowInterceptTouchEvent(true);

            switch (e.Action)
            {
                case MotionEventActions.Down:
                    if (dragView.DragMode == DragMode.Touch)
                    {
                        if (!touchedDown)
                        {
                            if (firstTime)
                            {
                                originalX = dragX;
                                originalY = dragY;
                                firstTime = false;
                            }
                            dragView._DragStarted();
                        }

                        touchedDown = true;
                    }
                    dX = x - dragX;
                    dY = y - dragY;
                    break;
                case MotionEventActions.Move:
                    if (touchedDown)
                    {
                        if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Horizontal)
                        {
                            dragX = x - dX;
                            dragView._DragX(dragX);
                        }

                        if (dragView.DragDirection == DragDirectionType.All || dragView.DragDirection == DragDirectionType.Vertical)
                        {
                            dragY = Math.Max(0, y - dY);
                            dragView._DragY(dragY / 2f);
                        }

                    }
                    break;
                case MotionEventActions.Up:
                    touchedDown = false;
                    dragView._DragEnded();
                    break;
                case MotionEventActions.Cancel:
                    touchedDown = false;
                    break;
            }
            return base.OnTouchEvent(e);
        }



        float dragX = 0;
        float dragY = 0;

        public override bool OnInterceptTouchEvent(MotionEvent e)
        {

            BringToFront();
            return true;
        }

    }
}
