namespace Plant01.Upper.Presentation.Core.Models
{
    /// <summary>
    /// 导航菜单项
    /// </summary>
    public class NavigateItem
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconChar { get; set; } = string.Empty;
        public string IconPlacement { get; set; } = "Top";
        public Type ViewModelType { get; set; } = default!;
    }
}
