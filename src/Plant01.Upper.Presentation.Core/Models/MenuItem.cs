namespace Plant01.Upper.Presentation.Core.Models
{
    public class MenuItem
    {
        // 实现该类的属性和方法
        public string Name { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string IconChar { get; set; } = string.Empty;
        public string IconPlacement { get; set; } = "Left";
        public string Title { get; set; } = string.Empty;
        public Type ViewModelType { get; set; } = default!;
    }
}
