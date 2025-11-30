using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using Plant01.WpfUI.Controls;

namespace Plant01.WpfUI.Helpers
{
    public class VirtualKeyboardHelper
    {
        public static readonly DependencyProperty IsEnabledProperty =
            DependencyProperty.RegisterAttached("IsEnabled", typeof(bool), typeof(VirtualKeyboardHelper), new PropertyMetadata(false, OnIsEnabledChanged));

        public static bool GetIsEnabled(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsEnabledProperty);
        }

        public static void SetIsEnabled(DependencyObject obj, bool value)
        {
            obj.SetValue(IsEnabledProperty, value);
        }

        public static readonly DependencyProperty KeyboardModeProperty =
            DependencyProperty.RegisterAttached("KeyboardMode", typeof(VirtualKeyboardMode), typeof(VirtualKeyboardHelper), new PropertyMetadata(VirtualKeyboardMode.Full, OnVisualPropertyChanged));

        public static VirtualKeyboardMode GetKeyboardMode(DependencyObject obj)
        {
            return (VirtualKeyboardMode)obj.GetValue(KeyboardModeProperty);
        }

        public static void SetKeyboardMode(DependencyObject obj, VirtualKeyboardMode value)
        {
            obj.SetValue(KeyboardModeProperty, value);
        }

        public static readonly DependencyProperty KeyHeightProperty =
            DependencyProperty.RegisterAttached("KeyHeight", typeof(double), typeof(VirtualKeyboardHelper), new PropertyMetadata(40.0, OnVisualPropertyChanged));

        public static double GetKeyHeight(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyHeightProperty);
        }

        public static void SetKeyHeight(DependencyObject obj, double value)
        {
            obj.SetValue(KeyHeightProperty, value);
        }

        public static readonly DependencyProperty KeyWidthProperty =
            DependencyProperty.RegisterAttached("KeyWidth", typeof(double), typeof(VirtualKeyboardHelper), new PropertyMetadata(double.NaN, OnVisualPropertyChanged));

        public static double GetKeyWidth(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyWidthProperty);
        }

        public static void SetKeyWidth(DependencyObject obj, double value)
        {
            obj.SetValue(KeyWidthProperty, value);
        }

        public static readonly DependencyProperty KeyMarginProperty =
            DependencyProperty.RegisterAttached("KeyMargin", typeof(Thickness), typeof(VirtualKeyboardHelper), new PropertyMetadata(new Thickness(2), OnVisualPropertyChanged));

        public static Thickness GetKeyMargin(DependencyObject obj)
        {
            return (Thickness)obj.GetValue(KeyMarginProperty);
        }

        public static void SetKeyMargin(DependencyObject obj, Thickness value)
        {
            obj.SetValue(KeyMarginProperty, value);
        }

        public static readonly DependencyProperty KeyFontSizeProperty =
            DependencyProperty.RegisterAttached("KeyFontSize", typeof(double), typeof(VirtualKeyboardHelper), new PropertyMetadata(14.0, OnVisualPropertyChanged));

        public static double GetKeyFontSize(DependencyObject obj)
        {
            return (double)obj.GetValue(KeyFontSizeProperty);
        }

        public static void SetKeyFontSize(DependencyObject obj, double value)
        {
            obj.SetValue(KeyFontSizeProperty, value);
        }

        private static Popup? _keyboardPopup;
        private static AntVirtualKeyboard? _virtualKeyboard;

        private static void OnVisualPropertyChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (_virtualKeyboard != null && _keyboardPopup != null && _keyboardPopup.IsOpen && _virtualKeyboard.TargetElement == d)
            {
                if (e.Property == KeyHeightProperty) _virtualKeyboard.KeyHeight = (double)e.NewValue;
                else if (e.Property == KeyWidthProperty) _virtualKeyboard.KeyWidth = (double)e.NewValue;
                else if (e.Property == KeyMarginProperty) _virtualKeyboard.KeyMargin = (Thickness)e.NewValue;
                else if (e.Property == KeyFontSizeProperty) _virtualKeyboard.KeyFontSize = (double)e.NewValue;
                else if (e.Property == KeyboardModeProperty) _virtualKeyboard.Mode = (VirtualKeyboardMode)e.NewValue;
            }
        }

        private static void OnIsEnabledChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            if (d is Control control) // TextBox or PasswordBox
            {
                if ((bool)e.NewValue)
                {
                    control.PreviewMouseLeftButtonUp += Control_GotFocus;
                    // control.GotFocus += Control_GotFocus; // Optional: Open on focus
                }
                else
                {
                    control.PreviewMouseLeftButtonUp -= Control_GotFocus;
                    // control.GotFocus -= Control_GotFocus;
                }
            }
        }

        private static void Control_GotFocus(object sender, RoutedEventArgs e)
        {
            if (sender is UIElement element)
            {
                ShowKeyboard(element);
            }
        }

        private static void ShowKeyboard(UIElement target)
        {
            EnsurePopupCreated();

            if (_virtualKeyboard != null && _keyboardPopup != null)
            {
                _virtualKeyboard.TargetElement = target;
                
                if (target is DependencyObject dep)
                {
                    _virtualKeyboard.Mode = GetKeyboardMode(dep);
                    _virtualKeyboard.KeyHeight = GetKeyHeight(dep);
                    _virtualKeyboard.KeyWidth = GetKeyWidth(dep);
                    _virtualKeyboard.KeyMargin = GetKeyMargin(dep);
                    _virtualKeyboard.KeyFontSize = GetKeyFontSize(dep);
                }

                _keyboardPopup.PlacementTarget = target;
                _keyboardPopup.Placement = PlacementMode.Bottom;
                _keyboardPopup.IsOpen = true;
            }
        }

        private static void EnsurePopupCreated()
        {
            if (_keyboardPopup == null)
            {
                _virtualKeyboard = new AntVirtualKeyboard();
                
                _keyboardPopup = new Popup
                {
                    Child = _virtualKeyboard,
                    StaysOpen = true, // Keep open so we can type
                    AllowsTransparency = true,
                    PopupAnimation = PopupAnimation.Slide
                };
            }
        }
    }
}
