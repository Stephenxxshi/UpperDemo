using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Shapes;

namespace Plant01.WpfUI.Controls
{
    [TemplatePart(Name = ElementColorPanel, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementColorThumb, Type = typeof(FrameworkElement))]
    [TemplatePart(Name = ElementHueSlider, Type = typeof(Slider))]
    public class AntColorPicker : Control
    {
        private const string ElementColorPanel = "PART_ColorPanel";
        private const string ElementColorThumb = "PART_ColorThumb";
        private const string ElementHueSlider = "PART_HueSlider";

        private FrameworkElement? _colorPanel;
        private FrameworkElement? _colorThumb;
        private Slider? _hueSlider;
        private bool _isUpdating;

        static AntColorPicker()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntColorPicker), new FrameworkPropertyMetadata(typeof(AntColorPicker)));
        }

        public AntColorPicker()
        {
            SelectColorCommand = new SimpleCommand(OnSelectColor);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();

            if (_colorPanel != null)
            {
                _colorPanel.MouseLeftButtonDown -= OnColorPanelMouseDown;
                _colorPanel.MouseMove -= OnColorPanelMouseMove;
                _colorPanel.MouseLeftButtonUp -= OnColorPanelMouseUp;
            }

            if (_hueSlider != null)
            {
                _hueSlider.ValueChanged -= OnHueSliderValueChanged;
            }

            _colorPanel = GetTemplateChild(ElementColorPanel) as FrameworkElement;
            _colorThumb = GetTemplateChild(ElementColorThumb) as FrameworkElement;
            _hueSlider = GetTemplateChild(ElementHueSlider) as Slider;

            if (_colorPanel != null)
            {
                _colorPanel.MouseLeftButtonDown += OnColorPanelMouseDown;
                _colorPanel.MouseMove += OnColorPanelMouseMove;
                _colorPanel.MouseLeftButtonUp += OnColorPanelMouseUp;
            }

            if (_hueSlider != null)
            {
                _hueSlider.ValueChanged += OnHueSliderValueChanged;
            }

            UpdateVisuals();
        }

        #region Dependency Properties

        public static readonly DependencyProperty SelectedColorProperty = DependencyProperty.Register(
            nameof(SelectedColor), typeof(Color), typeof(AntColorPicker), new PropertyMetadata(Colors.Red, OnSelectedColorChanged));

        public Color SelectedColor
        {
            get => (Color)GetValue(SelectedColorProperty);
            set => SetValue(SelectedColorProperty, value);
        }

        public static readonly DependencyProperty HueColorProperty = DependencyProperty.Register(
            nameof(HueColor), typeof(Color), typeof(AntColorPicker), new PropertyMetadata(Colors.Red));

        public Color HueColor
        {
            get => (Color)GetValue(HueColorProperty);
            private set => SetValue(HueColorProperty, value);
        }

        public static readonly DependencyProperty IsDropDownOpenProperty = DependencyProperty.Register(
            nameof(IsDropDownOpen), typeof(bool), typeof(AntColorPicker), new PropertyMetadata(false));

        public bool IsDropDownOpen
        {
            get => (bool)GetValue(IsDropDownOpenProperty);
            set => SetValue(IsDropDownOpenProperty, value);
        }

        #endregion

        #region Events

        public static readonly RoutedEvent SelectedColorChangedEvent = EventManager.RegisterRoutedEvent(
            nameof(SelectedColorChanged), RoutingStrategy.Bubble, typeof(RoutedPropertyChangedEventHandler<Color>), typeof(AntColorPicker));

        public event RoutedPropertyChangedEventHandler<Color> SelectedColorChanged
        {
            add => AddHandler(SelectedColorChangedEvent, value);
            remove => RemoveHandler(SelectedColorChangedEvent, value);
        }

        #endregion

        #region Logic

        private double _hue = 0;
        private double _saturation = 1;
        private double _value = 1;
        private bool _isDraggingPanel = false;

        private static void OnSelectedColorChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var picker = (AntColorPicker)d;
            if (!picker._isUpdating)
            {
                picker.UpdateHsvFromColor((Color)e.NewValue);
                picker.UpdateVisuals();
            }
            picker.RaiseEvent(new RoutedPropertyChangedEventArgs<Color>((Color)e.OldValue, (Color)e.NewValue, SelectedColorChangedEvent));
        }

        private void OnHueSliderValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            if (_isUpdating) return;
            _hue = e.NewValue;
            UpdateColorFromHsv();
        }

        private void OnColorPanelMouseDown(object sender, MouseButtonEventArgs e)
        {
            _isDraggingPanel = true;
            _colorPanel?.CaptureMouse();
            UpdateColorFromMouse(e.GetPosition(_colorPanel));
        }

        private void OnColorPanelMouseMove(object sender, MouseEventArgs e)
        {
            if (_isDraggingPanel)
            {
                UpdateColorFromMouse(e.GetPosition(_colorPanel));
            }
        }

        private void OnColorPanelMouseUp(object sender, MouseButtonEventArgs e)
        {
            _isDraggingPanel = false;
            _colorPanel?.ReleaseMouseCapture();
        }

        private void UpdateColorFromMouse(Point p)
        {
            if (_colorPanel == null) return;

            double width = _colorPanel.ActualWidth;
            double height = _colorPanel.ActualHeight;

            double x = Math.Max(0, Math.Min(width, p.X));
            double y = Math.Max(0, Math.Min(height, p.Y));

            _saturation = x / width;
            _value = 1 - (y / height);

            UpdateColorFromHsv();
        }

        private void UpdateColorFromHsv()
        {
            _isUpdating = true;
            var color = ColorFromHsv(_hue, _saturation, _value);
            SelectedColor = color;
            HueColor = ColorFromHsv(_hue, 1, 1);
            UpdateThumbPosition();
            _isUpdating = false;
        }

        private void UpdateHsvFromColor(Color color)
        {
            // Simple RGB to HSV conversion
            double r = color.R / 255.0;
            double g = color.G / 255.0;
            double b = color.B / 255.0;

            double max = Math.Max(r, Math.Max(g, b));
            double min = Math.Min(r, Math.Min(g, b));
            double delta = max - min;

            _value = max;
            _saturation = max == 0 ? 0 : delta / max;

            if (delta == 0)
            {
                _hue = 0; // Undefined, keep previous or 0
            }
            else
            {
                if (max == r)
                {
                    _hue = 60 * (((g - b) / delta) % 6);
                }
                else if (max == g)
                {
                    _hue = 60 * (((b - r) / delta) + 2);
                }
                else
                {
                    _hue = 60 * (((r - g) / delta) + 4);
                }
            }

            if (_hue < 0) _hue += 360;
        }

        private void UpdateVisuals()
        {
            if (_hueSlider != null)
            {
                _hueSlider.Value = _hue;
            }
            HueColor = ColorFromHsv(_hue, 1, 1);
            UpdateThumbPosition();
        }

        private void UpdateThumbPosition()
        {
            if (_colorPanel == null || _colorThumb == null) return;

            // Wait for layout if needed, but usually ActualWidth is available if visible
            if (_colorPanel.ActualWidth == 0) return;

            double x = _saturation * _colorPanel.ActualWidth;
            double y = (1 - _value) * _colorPanel.ActualHeight;

            // Center the thumb
            Canvas.SetLeft(_colorThumb, x - (_colorThumb.ActualWidth / 2));
            Canvas.SetTop(_colorThumb, y - (_colorThumb.ActualHeight / 2));
        }

        private static Color ColorFromHsv(double hue, double saturation, double value)
        {
            int hi = Convert.ToInt32(Math.Floor(hue / 60)) % 6;
            double f = hue / 60 - Math.Floor(hue / 60);

            value = value * 255;
            byte v = Convert.ToByte(value);
            byte p = Convert.ToByte(value * (1 - saturation));
            byte q = Convert.ToByte(value * (1 - f * saturation));
            byte t = Convert.ToByte(value * (1 - (1 - f) * saturation));

            if (hi == 0) return Color.FromRgb(v, t, p);
            else if (hi == 1) return Color.FromRgb(q, v, p);
            else if (hi == 2) return Color.FromRgb(p, v, t);
            else if (hi == 3) return Color.FromRgb(p, q, v);
            else if (hi == 4) return Color.FromRgb(t, p, v);
            else return Color.FromRgb(v, p, q);
        }

        #endregion

        public ICommand SelectColorCommand { get; }

        private void OnSelectColor(object parameter)
        {
            if (parameter is string colorCode)
            {
                try
                {
                    SelectedColor = (Color)ColorConverter.ConvertFromString(colorCode);
                    // IsDropDownOpen = false; // Don't close on preset click, maybe? Or yes? AntDesign usually keeps open.
                }
                catch { }
            }
            else if (parameter is Color color)
            {
                SelectedColor = color;
            }
        }

        private class SimpleCommand : ICommand
        {
            private readonly Action<object> _action;
            public SimpleCommand(Action<object> action) => _action = action;
            public event EventHandler? CanExecuteChanged;
            public bool CanExecute(object? parameter) => true;
            public void Execute(object? parameter) => _action(parameter!);
        }
    }
}
