﻿using System;
using System.ComponentModel;
using System.Diagnostics;
using System.Linq;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;
using Xamarin.Essentials;
using Xamarin.Forms;

namespace XF.Material.Forms.UI.Dialogs
{
    /// <summary>
    /// Base class of Material modal dialogs.
    /// </summary>
    public abstract class BaseMaterialModalPage : PopupPage, IMaterialModalPage
    {
        private bool _disposed;
        private bool _dismissing;

        protected BaseMaterialModalPage()
        {
            Animation = new ScaleAnimation()
            {
                DurationIn = 150,
                DurationOut = 100,
                EasingIn = Easing.SinOut,
                EasingOut = Easing.Linear,
                HasBackgroundAnimation = true,
                PositionIn = Rg.Plugins.Popup.Enums.MoveAnimationOptions.Center,
                PositionOut = Rg.Plugins.Popup.Enums.MoveAnimationOptions.Center,
                ScaleIn = 0.9,
                ScaleOut = 1
            };

            DisplayOrientation = DeviceDisplay.MainDisplayInfo.Orientation;
        }

        public virtual bool Dismissable => true;

        public virtual string MessageText
        {
            get { return ""; }
            set { }
        }

        protected DisplayOrientation DisplayOrientation { get; private set; }

        /// <summary>
        /// Dismisses this modal dialog asynchronously.
        /// </summary>
        public async Task DismissAsync()
        {
            try
            {
                await PopupNavigation.Instance.RemovePageAsync(this, true);
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }

        /// <summary>
        /// For internal use.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// For internal use only when running on the Android platform.
        /// </summary>
        [EditorBrowsable(EditorBrowsableState.Never)]
        public void RaiseOnBackButtonDismissed()
        {
            if (_dismissing || Device.RuntimePlatform == Device.iOS)
            {
                return;
            }

            _dismissing = true;

            OnBackButtonDismissed();
        }

        protected virtual void OnBackButtonDismissed() { }

        protected virtual void Dispose(bool disposing)
        {
            if (_disposed || !disposing)
            {
                return;
            }

            Device.BeginInvokeOnMainThread(async () =>
            {
                try
                {
                    await DismissAsync();
                    Content = null;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(ex);
                }
            });

            _disposed = true;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();

            DeviceDisplay.MainDisplayInfoChanged += DeviceDisplay_MainDisplayInfoChanged;
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();

            DeviceDisplay.MainDisplayInfoChanged -= DeviceDisplay_MainDisplayInfoChanged;
        }

        protected override void OnDisappearingAnimationEnd()
        {
            base.OnDisappearingAnimationEnd();
            Dispose();
        }

        protected virtual void OnOrientationChanged(DisplayOrientation orientation)
        {
        }

        /// <summary>
        /// Shows this modal dialog.
        /// </summary>
        protected virtual async Task ShowAsync()
        {
            await PopupNavigation.Instance.PushAsync(this, true);
        }



        private void DeviceDisplay_MainDisplayInfoChanged(object sender, DisplayInfoChangedEventArgs e)
        {
            if (DisplayOrientation == e.DisplayInfo.Orientation)
            {
                return;
            }

            DisplayOrientation = e.DisplayInfo.Orientation;
            OnOrientationChanged(DisplayOrientation);
        }
    }
}
