using System;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using Xamarin.Forms;

namespace XF.Material.Forms.UI
{
    /// <summary>
    /// A view that shows an image icon that can be tinted.
    /// </summary>
    public class MaterialIcon : Image, IMaterialTintableControl
    {
        /// <summary>
        /// Backing field for the bindable property <see cref="TintColor"/>.
        /// </summary>
        public static readonly BindableProperty TintColorProperty = BindableProperty.Create(nameof(TintColor), typeof(Color), typeof(MaterialIcon), Color.Default);


        /// <summary>
        /// Backing field for the bindable property <see cref="ClickCommandParameter"/>.
        /// </summary>
        public static readonly BindableProperty ClickCommandParameterProperty = BindableProperty.Create(nameof(ClickCommandParameter), typeof(object), typeof(MaterialIcon));

        /// <summary>
        /// Backing field for the bindable property <see cref="ClickCommand"/>.
        /// </summary>
        public static readonly BindableProperty ClickCommandProperty = BindableProperty.Create(nameof(ClickCommand), typeof(ICommand), typeof(MaterialIcon));


        /// <summary>
        /// Backing field for the bindable property <see cref="IsClickable"/>.
        /// </summary>
        public static readonly BindableProperty IsClickableProperty = BindableProperty.Create(nameof(IsClickable), typeof(bool), typeof(MaterialIcon), false);



        /// <summary>
        /// When the property <see cref="IsClickable"/> is set to true, this event is raised when this card was clicked.
        /// </summary>
        public event EventHandler Clicked;

        /// <summary>
        /// When the property <see cref="IsClickable"/> is set to true, this command will be executed when this card was clicked.
        /// </summary>
        public ICommand ClickCommand
        {
            get => (ICommand)GetValue(ClickCommandProperty);
            set => SetValue(ClickCommandProperty, value);
        }

        /// <summary>
        /// Gets or sets the tint color of the image icon.
        /// </summary>
        public Color TintColor
        {
            get => (Color)GetValue(TintColorProperty);
            set => SetValue(TintColorProperty, value);
        }


        /// <summary>
        /// The parameter to pass when <see cref="ClickCommand"/> executes.
        /// </summary>
        public object ClickCommandParameter
        {
            get => GetValue(ClickCommandParameterProperty);
            set => SetValue(ClickCommandParameterProperty, value);
        }



        /// <summary>
        /// Gets or sets the flag indicating of the card is clickable with a ripple effect.
        /// </summary>
        public bool IsClickable
        {
            get => (bool)GetValue(IsClickableProperty);
            set => SetValue(IsClickableProperty, value);
        }

        protected virtual void OnClick()
        {
            Clicked?.Invoke(this, EventArgs.Empty);
            ClickCommand?.Execute(ClickCommandParameter);
        }


        protected override void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            base.OnPropertyChanged(propertyName);

            if (propertyName == nameof(IsClickable))
            {
                OnIsClickableChanged(IsClickable);
            }
        }

        private TapGestureRecognizer _tapGestureRecognizer;

        private void OnIsClickableChanged(bool isClickable)
        {
            if (isClickable)
            {
                if (_tapGestureRecognizer == null)
                {
                    _tapGestureRecognizer = new TapGestureRecognizer
                    {
                        Command = new Command(OnClick)
                    };
                }

                GestureRecognizers.Add(_tapGestureRecognizer);
            }
            else
            {
                GestureRecognizers.Remove(_tapGestureRecognizer);
            }
        }
    }
}
