using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;

namespace Plant01.WpfUI.Controls
{
    [TemplatePart(Name = "PART_HourLeft", Type = typeof(AntFlipNumber))]
    [TemplatePart(Name = "PART_HourRight", Type = typeof(AntFlipNumber))]
    [TemplatePart(Name = "PART_MinuteLeft", Type = typeof(AntFlipNumber))]
    [TemplatePart(Name = "PART_MinuteRight", Type = typeof(AntFlipNumber))]
    [TemplatePart(Name = "PART_SecondLeft", Type = typeof(AntFlipNumber))]
    [TemplatePart(Name = "PART_SecondRight", Type = typeof(AntFlipNumber))]
    public class AntFlipClock : Control
    {
        static AntFlipClock()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(AntFlipClock), new FrameworkPropertyMetadata(typeof(AntFlipClock)));
        }

        private DispatcherTimer? _timer;
        private AntFlipNumber? _hourLeft;
        private AntFlipNumber? _hourRight;
        private AntFlipNumber? _minuteLeft;
        private AntFlipNumber? _minuteRight;
        private AntFlipNumber? _secondLeft;
        private AntFlipNumber? _secondRight;

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _hourLeft = GetTemplateChild("PART_HourLeft") as AntFlipNumber;
            _hourRight = GetTemplateChild("PART_HourRight") as AntFlipNumber;
            _minuteLeft = GetTemplateChild("PART_MinuteLeft") as AntFlipNumber;
            _minuteRight = GetTemplateChild("PART_MinuteRight") as AntFlipNumber;
            _secondLeft = GetTemplateChild("PART_SecondLeft") as AntFlipNumber;
            _secondRight = GetTemplateChild("PART_SecondRight") as AntFlipNumber;

            StartTimer();
        }

        private void StartTimer()
        {
            if (_timer != null) return;
            
            _timer = new DispatcherTimer();
            _timer.Interval = TimeSpan.FromSeconds(1);
            _timer.Tick += (s, e) => UpdateTime();
            _timer.Start();
            UpdateTime();
        }

        private void UpdateTime()
        {
            var now = DateTime.Now;
            UpdateNumber(_hourLeft, now.Hour / 10);
            UpdateNumber(_hourRight, now.Hour % 10);
            UpdateNumber(_minuteLeft, now.Minute / 10);
            UpdateNumber(_minuteRight, now.Minute % 10);
            UpdateNumber(_secondLeft, now.Second / 10);
            UpdateNumber(_secondRight, now.Second % 10);
        }

        private void UpdateNumber(AntFlipNumber? number, int value)
        {
            if (number != null)
            {
                number.Number = value;
            }
        }
    }
}
