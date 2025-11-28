using System.Windows;
using System.Windows.Controls;

namespace Plant01.WpfUI.Controls
{
    public enum TransitionMode
    {
        Fade,
        SlideLeft,
        SlideRight,
        SlideTop,
        SlideBottom,
        Zoom
    }

    [TemplatePart(Name = PreviousContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = CurrentContentPresentationSitePartName, Type = typeof(ContentPresenter))]
    public class TransitioningContentControl : ContentControl
    {
        private const string PreviousContentPresentationSitePartName = "PreviousContentPresentationSite";
        private const string CurrentContentPresentationSitePartName = "CurrentContentPresentationSite";

        private ContentPresenter? _previousContentPresentationSite;
        private ContentPresenter? _currentContentPresentationSite;

        static TransitioningContentControl()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(TransitioningContentControl), new FrameworkPropertyMetadata(typeof(TransitioningContentControl)));
        }

        public static readonly DependencyProperty TransitionModeProperty = DependencyProperty.Register(
            nameof(TransitionMode), typeof(TransitionMode), typeof(TransitioningContentControl), new PropertyMetadata(TransitionMode.Fade));

        public TransitionMode TransitionMode
        {
            get => (TransitionMode)GetValue(TransitionModeProperty);
            set => SetValue(TransitionModeProperty, value);
        }

        public static readonly DependencyProperty RunTransitionOnLoadedProperty = DependencyProperty.Register(
            nameof(RunTransitionOnLoaded), typeof(bool), typeof(TransitioningContentControl), new PropertyMetadata(false));

        /// <summary>
        /// 获取或设置是否在控件加载时播放过渡动画。
        /// </summary>
        public bool RunTransitionOnLoaded
        {
            get => (bool)GetValue(RunTransitionOnLoadedProperty);
            set => SetValue(RunTransitionOnLoadedProperty, value);
        }

        public override void OnApplyTemplate()
        {
            base.OnApplyTemplate();
            _previousContentPresentationSite = GetTemplateChild(PreviousContentPresentationSitePartName) as ContentPresenter;
            _currentContentPresentationSite = GetTemplateChild(CurrentContentPresentationSitePartName) as ContentPresenter;

            if (_currentContentPresentationSite != null)
            {
                if (RunTransitionOnLoaded)
                {
                    _currentContentPresentationSite.Content = Content;
                    StartTransition(null, Content);
                }
                else
                {
                    _currentContentPresentationSite.Content = Content;
                    VisualStateManager.GoToState(this, "Normal", false);
                }
            }
        }

        protected override void OnContentChanged(object oldContent, object newContent)
        {
            base.OnContentChanged(oldContent, newContent);
            StartTransition(oldContent, newContent);
        }

        private void StartTransition(object oldContent, object newContent)
        {
            if (_previousContentPresentationSite != null && _currentContentPresentationSite != null)
            {
                _previousContentPresentationSite.Content = oldContent;
                _currentContentPresentationSite.Content = newContent;

                string stateName = TransitionMode.ToString();
                VisualStateManager.GoToState(this, "Normal", false);
                VisualStateManager.GoToState(this, stateName, true);
            }
        }
    }
}
