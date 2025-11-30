using System.Windows;
using System.Windows.Controls;
using System.Windows.Controls.Primitives;
using System.Windows.Input;
using System.Windows.Interop;

namespace Plant01.WpfUI.Controls
{
    public enum VirtualKeyboardMode
    {
        Full,
        Numeric
    }

    public class AntVirtualKeyboard : Control
    {
        static AntVirtualKeyboard()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntVirtualKeyboard), new FrameworkPropertyMetadata(typeof(AntVirtualKeyboard)));
        }

        public static readonly DependencyProperty ModeProperty =
            DependencyProperty.Register(nameof(Mode), typeof(VirtualKeyboardMode), typeof(AntVirtualKeyboard), new PropertyMetadata(VirtualKeyboardMode.Full));

        public VirtualKeyboardMode Mode
        {
            get => (VirtualKeyboardMode)GetValue(ModeProperty);
            set => SetValue(ModeProperty, value);
        }

        public static readonly DependencyProperty TargetElementProperty =
            DependencyProperty.Register(nameof(TargetElement), typeof(UIElement), typeof(AntVirtualKeyboard), new PropertyMetadata(null));

        public UIElement TargetElement
        {
            get => (UIElement)GetValue(TargetElementProperty);
            set => SetValue(TargetElementProperty, value);
        }

        public static readonly DependencyProperty IsShiftEnabledProperty =
            DependencyProperty.Register(nameof(IsShiftEnabled), typeof(bool), typeof(AntVirtualKeyboard), new PropertyMetadata(false));

        public bool IsShiftEnabled
        {
            get => (bool)GetValue(IsShiftEnabledProperty);
            set => SetValue(IsShiftEnabledProperty, value);
        }

        public static readonly DependencyProperty IsCapsLockProperty =
            DependencyProperty.Register(nameof(IsCapsLock), typeof(bool), typeof(AntVirtualKeyboard), new PropertyMetadata(false));

        public bool IsCapsLock
        {
            get => (bool)GetValue(IsCapsLockProperty);
            set => SetValue(IsCapsLockProperty, value);
        }

        public static readonly DependencyProperty KeyHeightProperty =
            DependencyProperty.Register(nameof(KeyHeight), typeof(double), typeof(AntVirtualKeyboard), new PropertyMetadata(40.0));

        public double KeyHeight
        {
            get => (double)GetValue(KeyHeightProperty);
            set => SetValue(KeyHeightProperty, value);
        }

        public static readonly DependencyProperty KeyWidthProperty =
            DependencyProperty.Register(nameof(KeyWidth), typeof(double), typeof(AntVirtualKeyboard), new PropertyMetadata(double.NaN));

        public double KeyWidth
        {
            get => (double)GetValue(KeyWidthProperty);
            set => SetValue(KeyWidthProperty, value);
        }

        public static readonly DependencyProperty KeyMarginProperty =
            DependencyProperty.Register(nameof(KeyMargin), typeof(Thickness), typeof(AntVirtualKeyboard), new PropertyMetadata(new Thickness(2)));

        public Thickness KeyMargin
        {
            get => (Thickness)GetValue(KeyMarginProperty);
            set => SetValue(KeyMarginProperty, value);
        }

        public static readonly DependencyProperty KeyFontSizeProperty =
            DependencyProperty.Register(nameof(KeyFontSize), typeof(double), typeof(AntVirtualKeyboard), new PropertyMetadata(14.0));

        public double KeyFontSize
        {
            get => (double)GetValue(KeyFontSizeProperty);
            set => SetValue(KeyFontSizeProperty, value);
        }

        public ICommand KeyCommand { get; private set; }
        public ICommand SwitchModeCommand { get; private set; }
        public ICommand BackspaceCommand { get; private set; }
        public ICommand EnterCommand { get; private set; }
        public ICommand CloseCommand { get; private set; }
        public ICommand ShiftCommand { get; private set; }
        public ICommand CapsLockCommand { get; private set; }

        public AntVirtualKeyboard()
        {
            KeyCommand = new RelayCommand<string>(OnKeyPressed);
            SwitchModeCommand = new RelayCommand<object>(OnSwitchMode);
            BackspaceCommand = new RelayCommand<object>(OnBackspace);
            EnterCommand = new RelayCommand<object>(OnEnter);
            CloseCommand = new RelayCommand<object>(OnClose);
            ShiftCommand = new RelayCommand<object>(OnShift);
            CapsLockCommand = new RelayCommand<object>(OnCapsLock);
        }

        private void OnShift(object obj)
        {
            IsShiftEnabled = !IsShiftEnabled;
        }

        private void OnCapsLock(object obj)
        {
            IsCapsLock = !IsCapsLock;
        }

        private void OnKeyPressed(string key)
        {
            string textToInsert = key;

            // Apply Shift/Caps logic
            bool isShift = IsShiftEnabled;
            bool isCaps = IsCapsLock;
            bool isUpper = isShift ^ isCaps;

            if (key.Length == 1 && char.IsLetter(key[0]))
            {
                textToInsert = isUpper ? key.ToUpper() : key.ToLower();
            }
            else if (isShift)
            {
                textToInsert = key switch
                {
                    "1" => "!",
                    "2" => "@",
                    "3" => "#",
                    "4" => "$",
                    "5" => "%",
                    "6" => "^",
                    "7" => "&",
                    "8" => "*",
                    "9" => "(",
                    "0" => ")",
                    "-" => "_",
                    "=" => "+",
                    "[" => "{",
                    "]" => "}",
                    "\\" => "|",
                    ";" => ":",
                    "'" => "\"",
                    "," => "<",
                    "." => ">",
                    "/" => "?",
                    "`" => "~",
                    _ => key
                };
            }

            if (TargetElement is TextBox textBox)
            {
                int caretIndex = textBox.CaretIndex;
                textBox.Text = textBox.Text.Insert(caretIndex, textToInsert);
                textBox.CaretIndex = caretIndex + textToInsert.Length;
                textBox.Focus();
            }
            else if (TargetElement is PasswordBox passwordBox)
            {
                // PasswordBox support omitted for brevity
            }

            // Reset Shift after a key press if it was enabled (Sticky Shift behavior)
            if (IsShiftEnabled)
            {
                IsShiftEnabled = false;
            }
        }

        private void OnBackspace(object obj)
        {
            if (TargetElement is TextBox textBox && textBox.Text.Length > 0)
            {
                int caretIndex = textBox.CaretIndex;
                if (caretIndex > 0)
                {
                    textBox.Text = textBox.Text.Remove(caretIndex - 1, 1);
                    textBox.CaretIndex = caretIndex - 1;
                    textBox.Focus();
                }
            }
        }

        private void OnEnter(object obj)
        {
            // Trigger default enter behavior or close
             if (TargetElement is TextBox textBox)
            {
                 // Maybe insert newline?
                 // int caretIndex = textBox.CaretIndex;
                 // textBox.Text = textBox.Text.Insert(caretIndex, Environment.NewLine);
                 // textBox.CaretIndex = caretIndex + Environment.NewLine.Length;
            }
        }

        private void OnSwitchMode(object obj)
        {
            Mode = Mode == VirtualKeyboardMode.Full ? VirtualKeyboardMode.Numeric : VirtualKeyboardMode.Full;
        }

        private void OnClose(object obj)
        {
            // Logic to close the popup containing this keyboard
            // This might need to find the parent Popup
            var parent = this.Parent;
            while (parent != null)
            {
                if (parent is Popup popup)
                {
                    popup.IsOpen = false;
                    break;
                }
                parent = System.Windows.Media.VisualTreeHelper.GetParent(parent);
            }
        }
    }

    // Simple RelayCommand implementation if not available in the project
    public class RelayCommand<T> : ICommand
    {
        private readonly Action<T> _execute;
        private readonly Predicate<T>? _canExecute;

        public RelayCommand(Action<T> execute, Predicate<T>? canExecute = null)
        {
            _execute = execute;
            _canExecute = canExecute;
        }

        public bool CanExecute(object? parameter) => _canExecute == null || _canExecute((T)parameter!);

        public void Execute(object? parameter) => _execute((T)parameter!);

        public event EventHandler? CanExecuteChanged
        {
            add { CommandManager.RequerySuggested += value; }
            remove { CommandManager.RequerySuggested -= value; }
        }
    }
}
