using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Enums;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.UI.Dialogs;
using Rg.Plugins.Popup.Animations;
using Rg.Plugins.Popup.Interfaces.Animations;
using Rg.Plugins.Popup.Pages;
using Rg.Plugins.Popup.Services;

namespace XF.Material.UI.Dialogs
{
    public class MaterialBottomSheetAnimation : FadeAnimation
    {
        private double _defaultScale;
        private double _defaultOpacity;
        private double _defaultTranslationX;
        private double _defaultTranslationY;

        public MoveAnimationOptions PositionIn { get; set; }
        public MoveAnimationOptions PositionOut { get; set; }

        public MaterialBottomSheetAnimation() : this(MoveAnimationOptions.Center, MoveAnimationOptions.Center) { }

        public MaterialBottomSheetAnimation(MoveAnimationOptions positionIn, MoveAnimationOptions positionOut)
        {
            PositionIn = positionIn;
            PositionOut = positionOut;
            EasingIn = Easing.CubicOut;
            EasingOut = Easing.CubicOut;

            if (PositionIn != MoveAnimationOptions.Center) DurationIn = 350;
            if (PositionOut != MoveAnimationOptions.Center) DurationOut = 350;
        }

        public override void Preparing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation) base.Preparing(content, page);

            HidePage(page);

            if (content == null) return;

            UpdateDefaultProperties(content);

            if (!HasBackgroundAnimation) content.Opacity = 0;
        }

        public override void Disposing(View content, PopupPage page)
        {
            if (HasBackgroundAnimation) base.Disposing(content, page);

            ShowPage(page);

            if (content == null) return;

            content.Scale = _defaultScale;
            content.Opacity = _defaultOpacity;
            content.TranslationX = _defaultTranslationX;
            content.TranslationY = _defaultTranslationY;
        }

        public async override Task Appearing(View content, PopupPage page)
        {
            var taskList = new List<Task>();

            taskList.Add(base.Appearing(content, page));

            if (content != null)
            {
                var topOffset = GetTopOffset(content, page);
                var leftOffset = GetLeftOffset(content, page);

                if (PositionIn == MoveAnimationOptions.Top)
                {
                    content.TranslationY = -topOffset;
                    taskList.Add(content.TranslateTo(_defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
                }
                else if (PositionIn == MoveAnimationOptions.Bottom)
                {
                    content.TranslationY = topOffset;
                    taskList.Add(content.TranslateTo(_defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
                }
                else if (PositionIn == MoveAnimationOptions.Left)
                {
                    content.TranslationX = -leftOffset;
                    taskList.Add(content.TranslateTo(_defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
                }
                else if (PositionIn == MoveAnimationOptions.Right)
                {
                    content.TranslationX = leftOffset;
                    taskList.Add(content.TranslateTo(_defaultTranslationX, _defaultTranslationY, DurationIn, EasingIn));
                }
            }

            ShowPage(page);

            await Task.WhenAll(taskList);
        }

        public async override Task Disappearing(View content, PopupPage page)
        {
            var taskList = new List<Task>();

            taskList.Add(base.Disappearing(content, page));

            if (content != null)
            {
                UpdateDefaultProperties(content);

                var topOffset = GetTopOffset(content, page);
                var leftOffset = GetLeftOffset(content, page);

                if (PositionOut == MoveAnimationOptions.Top)
                {
                    taskList.Add(content.TranslateTo(_defaultTranslationX, -topOffset, DurationOut, EasingOut));
                }
                else if (PositionOut == MoveAnimationOptions.Bottom)
                {
                    taskList.Add(content.TranslateTo(_defaultTranslationX, topOffset, DurationOut, EasingOut));
                }
                else if (PositionOut == MoveAnimationOptions.Left)
                {
                    taskList.Add(content.TranslateTo(-leftOffset, _defaultTranslationY, DurationOut, EasingOut));
                }
                else if (PositionOut == MoveAnimationOptions.Right)
                {
                    taskList.Add(content.TranslateTo(leftOffset, _defaultTranslationY, DurationOut, EasingOut));
                }
            }

            await Task.WhenAll(taskList);
        }

        private void UpdateDefaultProperties(View content)
        {
            _defaultScale = content.Scale;
            _defaultOpacity = content.Opacity;
            _defaultTranslationX = content.TranslationX;
            _defaultTranslationY = content.TranslationY;
        }
    }


    public class BottomSheetDialogConfiguration
    {
        public bool HideCloseButton { get; set; }
        public bool HideActionButton { get; set; }
        public Color? ActionButtonColor { get; set; }
        public bool? CloseWhenBackgroundIsClicked { get; set; }
        public bool? TransparentBackground { get; set; }
        public bool? DisableContentPadding { get; set; }
        public bool? IsDraggable { get; set; }
    }


    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MaterialBottomDialog : BaseMaterialModalPage, IMaterialAwaitableDialog<bool>
    {
        internal MaterialBottomDialog(
            View content,
            FormattedString title = null,
            string actionText = null,
            Func<Task<bool>> onAction = null,
            BottomSheetDialogConfiguration configuration = null) : this(configuration)
        {
            InputTaskCompletionSource = new TaskCompletionSource<bool>();

            if (string.IsNullOrEmpty(actionText))
                actionText = "DONE";// app.GetString("common_done");

            text.Text = actionText.ToUpper();
            //ActionButton.IsVisible = !string.IsNullOrEmpty(text.Text);

            ActionButton.ClickCommand = new Command(async () =>
            {
                if (onAction == null)
                {
                    await DismissAsync();
                    InputTaskCompletionSource?.SetResult(true);
                }

                ActionButton.IsClickable = ActionButton.IsEnabled = false;
                activityIndicator.IsVisible = true;
                text.IsVisible = false;

                try
                {
                    if (await onAction())
                    {
                        await DismissAsync();
                        InputTaskCompletionSource?.SetResult(true);
                    }
                }
                finally
                {
                    ActionButton.IsClickable = ActionButton.IsEnabled = true;
                    text.IsVisible = true;
                    activityIndicator.IsVisible = false;
                }
            });

            Content.Content = content;
            Title.FormattedText = title;
            Title.IsVisible = !string.IsNullOrEmpty(Title.FormattedText?.ToString());

            ///Скрываем хедер, если на нем нет заголовка и кнопки закрытия
            Header.IsVisible = Title.IsVisible || Close.IsVisible;

            ///Важный паддинг для iOS (из-за нижней "брови")
            if (Device.RuntimePlatform == Device.iOS)
                HasSystemPadding = false;

            Close.ClickCommand = new Command(async () =>
            {
                await DismissAsync();
                InputTaskCompletionSource?.SetResult(false);
            });

            Animation = new MaterialBottomSheetAnimation(MoveAnimationOptions.Bottom, MoveAnimationOptions.Bottom);
        }

        internal MaterialBottomDialog(BottomSheetDialogConfiguration configuration = null)
        {
            InitializeComponent();
            Configure(configuration);
        }

        public TaskCompletionSource<bool> InputTaskCompletionSource { get; set; }

        internal static BottomSheetDialogConfiguration GlobalConfiguration { get; set; }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="actionText"></param>
        /// <param name="onAction"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<bool> ShowAsync(
            View content,
            FormattedString title = null,
            string actionText = null,
            Func<Task<bool>> onAction = null,
            BottomSheetDialogConfiguration configuration = null)
        {
            var dialog = new MaterialBottomDialog(content, title, actionText, onAction, configuration);
            await dialog.ShowAsync();

            return await dialog.InputTaskCompletionSource.Task;
        }


        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="actionText"></param>
        /// <param name="onAction"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<bool> ShowAsync(
            View content,
            FormattedString title = null,
            string actionText = null,
            Func<bool> onAction = null,
            BottomSheetDialogConfiguration configuration = null)
        {
            var dialog = new MaterialBottomDialog(content, title, actionText, () => Device.InvokeOnMainThreadAsync(() => onAction?.Invoke() ?? true), configuration);
            await dialog.ShowAsync();

            return await dialog.InputTaskCompletionSource.Task;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="content"></param>
        /// <param name="title"></param>
        /// <param name="configuration"></param>
        /// <returns></returns>
        public static async Task<MaterialBottomDialog> Show(
            View content,
            FormattedString title = null,
            BottomSheetDialogConfiguration configuration = null)
        {
            var dialog = new MaterialBottomDialog(content, title, actionText: null, onAction: null, configuration);
            await dialog.ShowAsync();

            Task.Run(async () =>
            {
                await dialog.InputTaskCompletionSource.Task;
            });

            return dialog;
        }


        protected override void OnAppearing()
        {
            base.OnAppearing();
            ChangeLayout();
        }


        protected override void OnDisappearing()
        {
            base.OnDisappearing();
        }

        protected override void OnOrientationChanged(DisplayOrientation orientation)
        {
            base.OnOrientationChanged(orientation);

            ChangeLayout();
        }


        protected override void OnPropertyChanged(string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(SystemPadding))
                systemPadding.HeightRequest = SystemPadding.Bottom;
        }

        private void ChangeLayout()
        {
            switch (DisplayOrientation)
            {
                case DisplayOrientation.Landscape when Device.Idiom == TargetIdiom.Phone:
                    Container.WidthRequest = 560;
                    Container.HorizontalOptions = LayoutOptions.Center;
                    break;
                case DisplayOrientation.Portrait when Device.Idiom == TargetIdiom.Phone:
                    Container.WidthRequest = -1;
                    Container.HorizontalOptions = LayoutOptions.FillAndExpand;
                    break;
            }

        }

        private void Configure(BottomSheetDialogConfiguration configuration)
        {
            var preferredConfig = configuration ?? GlobalConfiguration ?? new BottomSheetDialogConfiguration();

            ActionButton.IsVisible = !preferredConfig.HideActionButton;
            Close.IsVisible = !preferredConfig.HideCloseButton;
            ActionButton.BackgroundColor = preferredConfig.ActionButtonColor ?? ActionButton.BackgroundColor;
            CloseWhenBackgroundIsClicked = preferredConfig.CloseWhenBackgroundIsClicked ?? true;
            BackgroundInputTransparent = preferredConfig.TransparentBackground ?? false;

            if (preferredConfig.DisableContentPadding == true)
                Content.Padding = new Thickness(0, 0, 0, 0);

            if (preferredConfig.TransparentBackground == true)
                BackgroundColor = Color.Transparent;

            if (preferredConfig.IsDraggable == true)
                DragAnchor.IsVisible = true;
        }

    }
}
