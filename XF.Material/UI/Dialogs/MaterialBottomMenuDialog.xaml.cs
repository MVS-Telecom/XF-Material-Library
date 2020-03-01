using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Rg.Plugins.Popup.Enums;
using Xamarin.Essentials;
using Xamarin.Forms;
using Xamarin.Forms.Xaml;
using XF.Material.Forms.Resources.Typography;
using XF.Material.Forms.UI;
using XF.Material.Forms.UI.Dialogs;

namespace XF.Material.UI.Dialogs
{
    public class Option
    {
        /// <summary>
        /// 
        /// </summary>
        public object Id { get; set; }

        /// <summary>
        /// Текст опции
        /// </summary>
        public string Title { get; set; }

        /// <summary>
        /// Имя иконки
        /// </summary>
        public string IconSource { get; set; }

        /// <summary>
        /// Цвет накладываемый на иконку
        /// </summary>
        public Color? IconTint { get; set; }

        /// <summary>
        /// Не применять закрашивание иконки (не использовать дефолтный)
        /// </summary>
        public bool IgnoreTint { get; set; }

        /// <summary>
        /// Прозрачность иконки
        /// </summary>
        public double? IconOpacity { get; set; }

        /// <summary>
        /// Показать разделитель
        /// </summary>
        public bool? ShowDividerAbove { get; set; }
    }


    [XamlCompilation(XamlCompilationOptions.Compile)]
    public partial class MaterialBottomMenuDialog : BaseMaterialModalPage, IMaterialAwaitableDialog<Option>
    {
        internal MaterialBottomMenuDialog(
            List<Option> options,
            string title = null,
            BottomSheetDialogConfiguration configuration = null) : this(configuration)
        {
            StackLayout content = new StackLayout();

            options.ForEach(x =>
            {
                if (x.ShowDividerAbove == true)
                {
                    content.Children.Add(new MaterialDivider()
                    {
                        Margin = new Thickness(0)
                    });
                }


                var container = new MaterialCard()
                {
                    CornerRadius = 0,
                    Elevation = 0,
                    IsClickable = true,
                    Padding = new Thickness(8, 12),
                    Margin = new Thickness(0),
                };

                container.Clicked += async (s, e) =>
                {
                    await DismissAsync();
                    InputTaskCompletionSource?.SetResult(x);
                };

                var view = new StackLayout();
                view.Orientation = StackOrientation.Horizontal;
                view.Padding = new Thickness(12, 1, 12, 1);


                #region Иконка

                if (!string.IsNullOrEmpty(x.IconSource))
                {
                    var icon = new MaterialIcon()
                    {
                        WidthRequest = 24,
                        HeightRequest = 24,
                        Source = x.IconSource,
                        Opacity = x.IconOpacity ?? 1
                    };

                    if (!x.IgnoreTint)
                        icon.TintColor = x.IconTint ?? (Color)Application.Current.Resources["IconTint"];

                    view.Children.Add(new MaterialCard()
                    {
                        Elevation = 0,
                        CornerRadius = 19,
                        Margin = 0,
                        Padding = new Thickness(8),
                        VerticalOptions = LayoutOptions.Center,
                        BackgroundColor = (Color)Application.Current.Resources["IconBackground"],
                        Content = icon
                    });
                }

                #endregion


                #region Текст

                view.Children.Add(new MaterialLabel()
                {
                    Padding = new Thickness(10, 1),
                    TypeScale = MaterialTypeScale.Body1,
                    TextColor = Color.Black,
                    Text = x.Title,
                    VerticalOptions = LayoutOptions.Center
                });

                #endregion

                container.Content = view;
                content.Children.Add(container);
            });



            InputTaskCompletionSource = new TaskCompletionSource<Option>();

            Content.Content = content;

            Title.Text = title;
            Title.IsVisible = !string.IsNullOrEmpty(Title.Text);

            ///Скрываем хедер, если на нем нет заголовка и кнопки закрытия
            Header.IsVisible = Title.IsVisible || Close.IsVisible;

            ///Важный паддинг для iOS (из-за нижней "брови")
            if (Device.RuntimePlatform == Device.iOS)
                HasSystemPadding = false;

            Close.ClickCommand = new Command(async () =>
            {
                await DismissAsync();
                InputTaskCompletionSource?.SetResult(null);
            });

            Animation = new MaterialBottomSheetAnimation(MoveAnimationOptions.Bottom, MoveAnimationOptions.Bottom);
        }

        internal MaterialBottomMenuDialog(BottomSheetDialogConfiguration configuration = null)
        {
            InitializeComponent();
            Configure(configuration);
        }

        public TaskCompletionSource<Option> InputTaskCompletionSource { get; set; }

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
        public static async Task<Option> ShowAsync(
            List<Option> options,
            string title = null,
            BottomSheetDialogConfiguration configuration = null)
        {
            var dialog = new MaterialBottomMenuDialog(options, title, configuration);
            await dialog.ShowAsync();

            return await dialog.InputTaskCompletionSource.Task;
        }

        protected override void OnBackButtonDismissed()
        {
            InputTaskCompletionSource?.SetResult(null);
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            ChangeLayout();
        }

        protected override bool OnBackgroundClicked()
        {
            InputTaskCompletionSource?.SetResult(null);

            return base.OnBackgroundClicked();
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

            preferredConfig.HideActionButton = true;
            preferredConfig.HideCloseButton = false;

            Close.IsVisible = !preferredConfig.HideCloseButton;
            CloseWhenBackgroundIsClicked = preferredConfig.CloseWhenBackgroundIsClicked ?? true;
        }

    }
}
