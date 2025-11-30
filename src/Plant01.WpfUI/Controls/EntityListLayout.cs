using Plant01.WpfUI.Controls;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Plant01.WpfUI.Controls
{
    [TemplatePart(Name = FilterAreaPartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = ActionAreaPartName, Type = typeof(ContentPresenter))]
    [TemplatePart(Name = PaginationPartName, Type = typeof(AntPagination))]
    public class EntityListLayout : ContentControl
    {
        private const string FilterAreaPartName = "PART_FilterArea";
        private const string ActionAreaPartName = "PART_ActionArea";
        private const string PaginationPartName = "PART_Pagination";

        static EntityListLayout()
        {
            DefaultStyleKeyProperty.OverrideMetadata(typeof(EntityListLayout), new FrameworkPropertyMetadata(typeof(EntityListLayout)));
        }

        public static readonly DependencyProperty FilterContentProperty = DependencyProperty.Register(
            nameof(FilterContent), typeof(object), typeof(EntityListLayout), new PropertyMetadata(null));

        public object FilterContent
        {
            get => GetValue(FilterContentProperty);
            set => SetValue(FilterContentProperty, value);
        }

        public static readonly DependencyProperty ActionContentProperty = DependencyProperty.Register(
            nameof(ActionContent), typeof(object), typeof(EntityListLayout), new PropertyMetadata(null));

        public object ActionContent
        {
            get => GetValue(ActionContentProperty);
            set => SetValue(ActionContentProperty, value);
        }

        public static readonly DependencyProperty IsLoadingProperty = DependencyProperty.Register(
            nameof(IsLoading), typeof(bool), typeof(EntityListLayout), new PropertyMetadata(false));

        public bool IsLoading
        {
            get => (bool)GetValue(IsLoadingProperty);
            set => SetValue(IsLoadingProperty, value);
        }

        public static readonly DependencyProperty TotalCountProperty = DependencyProperty.Register(
            nameof(TotalCount), typeof(int), typeof(EntityListLayout), new PropertyMetadata(0));
        public int TotalCount { get => (int)GetValue(TotalCountProperty); set => SetValue(TotalCountProperty, value); }

        public static readonly DependencyProperty PageIndexProperty = DependencyProperty.Register(
            nameof(PageIndex), typeof(int), typeof(EntityListLayout), new PropertyMetadata(1));
        public int PageIndex { get => (int)GetValue(PageIndexProperty); set => SetValue(PageIndexProperty, value); }

        public static readonly DependencyProperty PageSizeProperty = DependencyProperty.Register(
            nameof(PageSize), typeof(int), typeof(EntityListLayout), new PropertyMetadata(10));
        public int PageSize { get => (int)GetValue(PageSizeProperty); set => SetValue(PageSizeProperty, value); }

        public static readonly DependencyProperty AdvancedFilterContentProperty = DependencyProperty.Register(
            nameof(AdvancedFilterContent), typeof(object), typeof(EntityListLayout), new PropertyMetadata(null));

        public object AdvancedFilterContent
        {
            get => GetValue(AdvancedFilterContentProperty);
            set => SetValue(AdvancedFilterContentProperty, value);
        }

        public static readonly DependencyProperty TitleProperty = DependencyProperty.Register(
            nameof(Title), typeof(string), typeof(EntityListLayout), new PropertyMetadata(null));

        public string Title
        {
            get => (string)GetValue(TitleProperty);
            set => SetValue(TitleProperty, value);
        }

        public static readonly DependencyProperty SelectedCountProperty = DependencyProperty.Register(
            nameof(SelectedCount), typeof(int), typeof(EntityListLayout), new PropertyMetadata(0));

        public int SelectedCount
        {
            get => (int)GetValue(SelectedCountProperty);
            set => SetValue(SelectedCountProperty, value);
        }

        public static readonly DependencyProperty BatchActionContentProperty = DependencyProperty.Register(
            nameof(BatchActionContent), typeof(object), typeof(EntityListLayout), new PropertyMetadata(null));

        public object BatchActionContent
        {
            get => GetValue(BatchActionContentProperty);
            set => SetValue(BatchActionContentProperty, value);
        }

        public static readonly DependencyProperty RefreshCommandProperty = DependencyProperty.Register(
            nameof(RefreshCommand), typeof(ICommand), typeof(EntityListLayout), new PropertyMetadata(null));

        public ICommand RefreshCommand
        {
            get => (ICommand)GetValue(RefreshCommandProperty);
            set => SetValue(RefreshCommandProperty, value);
        }

        public static readonly DependencyProperty ViewSettingsContentProperty = DependencyProperty.Register(
            nameof(ViewSettingsContent), typeof(object), typeof(EntityListLayout), new PropertyMetadata(null));

        public object ViewSettingsContent
        {
            get => GetValue(ViewSettingsContentProperty);
            set => SetValue(ViewSettingsContentProperty, value);
        }
    }
}
