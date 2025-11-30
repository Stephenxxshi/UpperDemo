using System;
using System.ComponentModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Media3D;

namespace Plant01.WpfUI.Controls
{
    public class AntFlipNumber : Viewport3D
    {
        private bool _isLoaded;

        private TextBlock? _page1TextDown;

        private TextBlock? _page2TextUp;

        private TextBlock? _page2TextDown;

        private TextBlock? _page3TextUp;

        private ContainerUIElement3D? _page1;

        private ContainerUIElement3D? _page2;

        private ContainerUIElement3D? _page3;

        private ContainerUIElement3D? _content;

        private readonly AxisAngleRotation3D _pageRotation3D;

        private readonly DoubleAnimation _animation;

        private bool _isAnimating;

        public static readonly DependencyProperty CornerRadiusProperty = DependencyProperty.Register(
            nameof(CornerRadius), typeof(CornerRadius), typeof(AntFlipNumber), new PropertyMetadata(new CornerRadius(4)));

        public CornerRadius CornerRadius
        {
            get => (CornerRadius)GetValue(CornerRadiusProperty);
            set => SetValue(CornerRadiusProperty, value);
        }

        public static readonly DependencyProperty BackgroundProperty = DependencyProperty.Register(
            nameof(Background), typeof(Brush), typeof(AntFlipNumber), new PropertyMetadata(default(Brush)));

        public Brush Background
        {
            get => (Brush)GetValue(BackgroundProperty);
            set => SetValue(BackgroundProperty, value);
        }

        public static readonly DependencyProperty ForegroundProperty = DependencyProperty.Register(
            nameof(Foreground), typeof(Brush), typeof(AntFlipNumber), new PropertyMetadata(default(Brush)));

        public Brush Foreground
        {
            get => (Brush)GetValue(ForegroundProperty);
            set => SetValue(ForegroundProperty, value);
        }

        public static readonly DependencyProperty FontSizeProperty = DependencyProperty.Register(
            nameof(FontSize), typeof(double), typeof(AntFlipNumber), new PropertyMetadata(70.0));

        [TypeConverter(typeof(FontSizeConverter))]
        public double FontSize
        {
            get => (double)GetValue(FontSizeProperty);
            set => SetValue(FontSizeProperty, value);
        }

        public static readonly DependencyProperty FontFamilyProperty = DependencyProperty.Register(
            nameof(FontFamily), typeof(FontFamily), typeof(AntFlipNumber), new PropertyMetadata(new FontFamily("Segoe UI")));

        public FontFamily FontFamily
        {
            get => (FontFamily)GetValue(FontFamilyProperty);
            set => SetValue(FontFamilyProperty, value);
        }

        public static readonly DependencyProperty FontWeightProperty = DependencyProperty.Register(
            nameof(FontWeight), typeof(FontWeight), typeof(AntFlipNumber), new PropertyMetadata(FontWeights.Normal));

        public FontWeight FontWeight
        {
            get => (FontWeight)GetValue(FontWeightProperty);
            set => SetValue(FontWeightProperty, value);
        }

        public static readonly DependencyProperty FontStyleProperty = DependencyProperty.Register(
            nameof(FontStyle), typeof(FontStyle), typeof(AntFlipNumber), new PropertyMetadata(FontStyles.Normal));

        public FontStyle FontStyle
        {
            get => (FontStyle)GetValue(FontStyleProperty);
            set => SetValue(FontStyleProperty, value);
        }

        public static readonly DependencyProperty NumberProperty = DependencyProperty.Register(
            nameof(Number), typeof(int), typeof(AntFlipNumber), new PropertyMetadata(0, OnNumberChanged));

        private static void OnNumberChanged(DependencyObject s, DependencyPropertyChangedEventArgs e) =>
            ((AntFlipNumber)s).OnNumberChanged();

        public int Number
        {
            get => (int)GetValue(NumberProperty);
            set => SetValue(NumberProperty, value);
        }

        public AntFlipNumber()
        {
            var visual3D = new ModelVisual3D
            {
                Content = new DirectionalLight()
            };
            Children.Add(visual3D);

            _pageRotation3D = new AxisAngleRotation3D
            {
                Angle = 0,
                Axis = new Vector3D(1, 0, 0)
            };

            _animation = new DoubleAnimation(0, 180, new Duration(TimeSpan.FromSeconds(0.8)))
            {
                FillBehavior = FillBehavior.Stop
            };
            _animation.Completed += Animation_Completed;

            Loaded += (s, e) =>
            {
                if (_isLoaded) return;
                _isLoaded = true;
                InitNumber();
                var transform3D = new RotateTransform3D(_pageRotation3D);
                if (_page2 != null) _page2.Transform = transform3D;
            };
        }

        private void Animation_Completed(object? sender, EventArgs e)
        {
            _isAnimating = false;
            UpdateNumber();
        }

        private void InitNumber()
        {
            _page1 = new ContainerUIElement3D();
            var num1 = Number > 8 ? 0 : Number + 1;
            var page1NumDown = CreateNumber(num1, false, out _page1TextDown);
            _page1.Children.Add(page1NumDown);

            _page2 = new ContainerUIElement3D();
            var page2NumUp = CreateNumber(num1, true, out _page2TextUp);
            var page2NumDown = CreateNumber(Number, false, out _page2TextDown);
            _page2.Children.Add(page2NumUp);
            _page2.Children.Add(page2NumDown);

            _page3 = new ContainerUIElement3D();
            var page3NumUp = CreateNumber(Number, true, out _page3TextUp);
            _page3.Children.Add(page3NumUp);
            var transform3D = new RotateTransform3D(new AxisAngleRotation3D
            {
                Angle = 180,
                Axis = new Vector3D(1, 0, 0)
            });
            _page3.Transform = transform3D;

            _content = new ContainerUIElement3D
            {
                Children =
            {
                _page1,
                _page2,
                _page3
            }
            };
            Children.Add(_content);
        }

        private bool CheckNull() => _page1TextDown != null && _page2TextUp != null && _page2TextDown != null && _page3TextUp != null;

        private void OnNumberChanged()
        {
            if (!CheckNull()) return;

            InitNewNumber();

            if (_isAnimating)
            {
                _isAnimating = false;
                UpdateNumber();
                return;
            }

            _isAnimating = true;
            _pageRotation3D.BeginAnimation(AxisAngleRotation3D.AngleProperty, _animation);
        }

        private void InitNewNumber()
        {
            var num1 = Number.ToString();
            if (_page1TextDown != null) _page1TextDown.Text = num1;
            if (_page2TextUp != null) _page2TextUp.Text = num1;
        }

        private void UpdateNumber()
        {
            _pageRotation3D.BeginAnimation(AxisAngleRotation3D.AngleProperty, null);
            _pageRotation3D.Angle = 0;

            var num = Number.ToString();
            if (_page2TextDown != null) _page2TextDown.Text = num;
            if (_page3TextUp != null) _page3TextUp.Text = num;

            _isAnimating = false;
        }

        private Viewport2DVisual3D CreateNumber(int num, bool isUp, out TextBlock textBlock)
        {
            int flag;
            var rotateTransform = new RotateTransform();

            if (isUp)
            {
                flag = -1;
                rotateTransform.Angle = 180;
            }
            else
            {
                flag = 1;
            }
            var halfWidth = ActualWidth / 2;
            var quarterWidth = ActualWidth / 4;
            var quarterHeight = ActualHeight / 4;

            var meMaterial = new DiffuseMaterial();
            Viewport2DVisual3D.SetIsVisualHostMaterial(meMaterial, true);

            textBlock = new TextBlock
            {
                RenderTransformOrigin = new Point(0.5, 0.5),
                Foreground = Foreground,
                FontSize = FontSize,
                FontFamily = FontFamily,
                FontWeight = FontWeight,
                FontStyle = FontStyle,
                VerticalAlignment = VerticalAlignment.Center,
                HorizontalAlignment = HorizontalAlignment.Center,
                TextAlignment = TextAlignment.Center,
                Text = num.ToString(),
                RenderTransform = rotateTransform,
                Margin = new Thickness(0, 0, 0, -quarterHeight)
            };

            var border = new Border
            {
                ClipToBounds = true,
                CornerRadius = new CornerRadius(CornerRadius.TopLeft, CornerRadius.TopRight, 0, 0),
                Background = Background,
                Width = halfWidth,
                Height = quarterHeight,
                Child = textBlock
            };

            var positions = new Point3DCollection
            {
                new(-quarterWidth * flag, quarterHeight, 0),
                new(-quarterWidth * flag, 0, 0),
                new(quarterWidth * flag, 0, 0),
                new(quarterWidth * flag, quarterHeight, 0)
            };

            var triangleIndices = new Int32Collection
            {
                0,
                1,
                2,
                0,
                2,
                3
            };

            var textureCoordinates = new PointCollection
            {
                new(0, 0),
                new(0, 1),
                new(1, 1),
                new(1, 0)
            };

            var geometry3D = new MeshGeometry3D
            {
                Positions = positions,
                TriangleIndices = triangleIndices,
                TextureCoordinates = textureCoordinates
            };

            var child = new Viewport2DVisual3D
            {
                Geometry = geometry3D,
                Visual = border,
                Material = meMaterial
            };

            return child;
        }
    }
}
