using System.Windows;

namespace Plant01.WpfUI.Themes;

public static class DesignTokenKeys
{
    private static ComponentResourceKey CreateKey(string id)
    {
        return new ComponentResourceKey(typeof(DesignTokenKeys), id);
    }

    // Brand Colors
    /// <summary>
    /// 品牌主色，用于主要按钮、激活状态、高亮等。
    /// </summary>
    public static ComponentResourceKey PrimaryColor => CreateKey(nameof(PrimaryColor));
    /// <summary>
    /// 品牌主色的原始颜色值（非画笔），用于需要 Color 结构的场景。
    /// </summary>
    public static ComponentResourceKey PrimaryColorValue => CreateKey(nameof(PrimaryColorValue)); // Raw Color value
    /// <summary>
    /// 品牌主色的悬停状态颜色。
    /// </summary>
    public static ComponentResourceKey PrimaryColorHover => CreateKey(nameof(PrimaryColorHover));
    /// <summary>
    /// 品牌主色的按下/激活状态颜色。
    /// </summary>
    public static ComponentResourceKey PrimaryColorActive => CreateKey(nameof(PrimaryColorActive));
    /// <summary>
    /// 品牌主色的轮廓/描边颜色，常用于 Focus 状态的光晕。
    /// </summary>
    public static ComponentResourceKey PrimaryOutline => CreateKey(nameof(PrimaryOutline));

    // Ant Design 5.x Map Tokens
    /// <summary>
    /// 品牌主色背景 (Index 1)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryBg => CreateKey(nameof(ColorPrimaryBg));
    /// <summary>
    /// 品牌主色背景悬停 (Index 2)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryBgHover => CreateKey(nameof(ColorPrimaryBgHover));
    /// <summary>
    /// 品牌主色边框 (Index 3)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryBorder => CreateKey(nameof(ColorPrimaryBorder));
    /// <summary>
    /// 品牌主色边框悬停 (Index 4)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryBorderHover => CreateKey(nameof(ColorPrimaryBorderHover));
    /// <summary>
    /// 品牌主色悬停 (Index 5)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryHover => CreateKey(nameof(ColorPrimaryHover));
    // ColorPrimary is Index 6
    /// <summary>
    /// 品牌主色激活 (Index 7)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryActive => CreateKey(nameof(ColorPrimaryActive));
    /// <summary>
    /// 品牌主色文本悬停 (Index 8)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryTextHover => CreateKey(nameof(ColorPrimaryTextHover));
    /// <summary>
    /// 品牌主色文本 (Index 9)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryText => CreateKey(nameof(ColorPrimaryText));
    /// <summary>
    /// 品牌主色文本激活 (Index 10)
    /// </summary>
    public static ComponentResourceKey ColorPrimaryTextActive => CreateKey(nameof(ColorPrimaryTextActive));

    // Functional Colors
    /// <summary>
    /// 成功状态颜色（如成功提示、完成图标）。
    /// </summary>
    public static ComponentResourceKey SuccessColor => CreateKey(nameof(SuccessColor));
    /// <summary>
    /// 警告状态颜色（如警告提示、注意图标）。
    /// </summary>
    public static ComponentResourceKey WarningColor => CreateKey(nameof(WarningColor));
    /// <summary>
    /// 错误状态颜色（如错误提示、删除按钮）。
    /// </summary>
    public static ComponentResourceKey ErrorColor => CreateKey(nameof(ErrorColor));
    /// <summary>
    /// 错误状态的悬停颜色。
    /// </summary>
    public static ComponentResourceKey ErrorColorHover => CreateKey(nameof(ErrorColorHover));
    /// <summary>
    /// 错误状态的按下颜色。
    /// </summary>
    public static ComponentResourceKey ErrorColorActive => CreateKey(nameof(ErrorColorActive));
    /// <summary>
    /// 信息状态颜色（如信息提示、链接）。
    /// </summary>
    public static ComponentResourceKey InfoColor => CreateKey(nameof(InfoColor));

    // Neutral Colors
    /// <summary>
    /// 页面整体背景色。
    /// </summary>
    public static ComponentResourceKey BodyBackground => CreateKey(nameof(BodyBackground));
    /// <summary>
    /// 组件容器背景色（如卡片、弹窗、输入框背景）。
    /// </summary>
    public static ComponentResourceKey ComponentBackground => CreateKey(nameof(ComponentBackground));
    /// <summary>
    /// 浮层容器背景色（如下拉菜单、Tooltip）。
    /// </summary>
    public static ComponentResourceKey PopoverBackground => CreateKey(nameof(PopoverBackground));
    
    /// <summary>
    /// 标准边框颜色（如输入框、按钮边框）。
    /// </summary>
    public static ComponentResourceKey BorderColor => CreateKey(nameof(BorderColor));
    /// <summary>
    /// 分割线颜色（如列表分割线、表格边框）。
    /// </summary>
    public static ComponentResourceKey BorderColorSplit => CreateKey(nameof(BorderColorSplit));

    /// <summary>
    /// 填充颜色，常用于禁用状态、未选中状态的背景。
    /// </summary>
    public static ComponentResourceKey FillColor => CreateKey(nameof(FillColor)); // New Token for neutral fills (Switch off, etc.)

    /// <summary>
    /// 主要文本颜色（标题、正文）。
    /// </summary>
    public static ComponentResourceKey TextPrimary => CreateKey(nameof(TextPrimary));
    /// <summary>
    /// 次要文本颜色（辅助说明、次级标题）。
    /// </summary>
    public static ComponentResourceKey TextSecondary => CreateKey(nameof(TextSecondary));
    /// <summary>
    /// 第三级文本颜色（失效文本、占位符）。
    /// </summary>
    public static ComponentResourceKey TextTertiary => CreateKey(nameof(TextTertiary));
    /// <summary>
    /// 第四级文本颜色（极弱文本、图标）。
    /// </summary>
    public static ComponentResourceKey TextQuaternary => CreateKey(nameof(TextQuaternary));
    /// <summary>
    /// 在主色背景上的文本颜色（通常为白色）。
    /// </summary>
    public static ComponentResourceKey TextOnPrimary => CreateKey(nameof(TextOnPrimary)); // Always White usually
    /// <summary>
    /// 输入框占位符颜色。
    /// </summary>
    public static ComponentResourceKey PlaceholderColor => CreateKey(nameof(PlaceholderColor));

    /// <summary>
    /// 遮罩层颜色（如弹窗背后的半透明遮罩）。
    /// </summary>
    public static ComponentResourceKey MaskColor => CreateKey(nameof(MaskColor));

    // Shadows
    /// <summary>
    /// 小号阴影（如按钮 Hover）。
    /// </summary>
    public static ComponentResourceKey BoxShadowSmall => CreateKey(nameof(BoxShadowSmall));
    /// <summary>
    /// 中号阴影（如下拉菜单、卡片）。
    /// </summary>
    public static ComponentResourceKey BoxShadow => CreateKey(nameof(BoxShadow));
    /// <summary>
    /// 大号阴影（如弹窗、Drawer）。
    /// </summary>
    public static ComponentResourceKey BoxShadowLarge => CreateKey(nameof(BoxShadowLarge));

    // Sizing & Density
    /// <summary>
    /// 控件标准高度（默认 32px）。
    /// </summary>
    public static ComponentResourceKey ControlHeight => CreateKey(nameof(ControlHeight));
    /// <summary>
    /// 控件大号高度（默认 40px）。
    /// </summary>
    public static ComponentResourceKey ControlHeightLG => CreateKey(nameof(ControlHeightLG));
    /// <summary>
    /// 控件小号高度（默认 24px）。
    /// </summary>
    public static ComponentResourceKey ControlHeightSM => CreateKey(nameof(ControlHeightSM));
    
    /// <summary>
    /// 标准字体大小（默认 14px）。
    /// </summary>
    public static ComponentResourceKey FontSize => CreateKey(nameof(FontSize));
    /// <summary>
    /// 大号字体大小（默认 16px）。
    /// </summary>
    public static ComponentResourceKey FontSizeLG => CreateKey(nameof(FontSizeLG));
    /// <summary>
    /// 小号字体大小（默认 12px）。
    /// </summary>
    public static ComponentResourceKey FontSizeSM => CreateKey(nameof(FontSizeSM));
    /// <summary>
    /// 标题1字体大小。
    /// </summary>
    public static ComponentResourceKey FontSizeHeading1 => CreateKey(nameof(FontSizeHeading1));
    /// <summary>
    /// 标题2字体大小。
    /// </summary>
    public static ComponentResourceKey FontSizeHeading2 => CreateKey(nameof(FontSizeHeading2));
    /// <summary>
    /// 标题3字体大小。
    /// </summary>
    public static ComponentResourceKey FontSizeHeading3 => CreateKey(nameof(FontSizeHeading3));
    /// <summary>
    /// 标题4字体大小。
    /// </summary>
    public static ComponentResourceKey FontSizeHeading4 => CreateKey(nameof(FontSizeHeading4));
    /// <summary>
    /// 标题5字体大小。
    /// </summary>
    public static ComponentResourceKey FontSizeHeading5 => CreateKey(nameof(FontSizeHeading5));

    /// <summary>
    /// 极小内边距。
    /// </summary>
    public static ComponentResourceKey PaddingXS => CreateKey(nameof(PaddingXS));
    /// <summary>
    /// 小号内边距。
    /// </summary>
    public static ComponentResourceKey PaddingSM => CreateKey(nameof(PaddingSM));
    /// <summary>
    /// 中号内边距。
    /// </summary>
    public static ComponentResourceKey PaddingMD => CreateKey(nameof(PaddingMD));
    /// <summary>
    /// 大号内边距。
    /// </summary>
    public static ComponentResourceKey PaddingLG => CreateKey(nameof(PaddingLG));
    /// <summary>
    /// 弹窗内容内边距 (24px)。
    /// </summary>
    public static ComponentResourceKey ModalContentPadding => CreateKey(nameof(ModalContentPadding));
    /// <summary>
    /// 弹窗头部内边距 (16px 24px)。
    /// </summary>
    public static ComponentResourceKey ModalHeaderPadding => CreateKey(nameof(ModalHeaderPadding));
    /// <summary>
    /// 弹窗底部内边距 (10px 16px)。
    /// </summary>
    public static ComponentResourceKey ModalFooterPadding => CreateKey(nameof(ModalFooterPadding));
    
    /// <summary>
    /// 标准圆角半径。
    /// </summary>
    public static ComponentResourceKey BorderRadius => CreateKey(nameof(BorderRadius));
    /// <summary>
    /// 基础圆角数值（double 类型）。
    /// </summary>
    public static ComponentResourceKey BorderRadiusBase => CreateKey(nameof(BorderRadiusBase));

    // Control Item States (Menu, List, etc.)
    /// <summary>
    /// 控件项悬停背景（如菜单项、列表项 Hover）。
    /// </summary>
    public static ComponentResourceKey ControlItemBgHover => CreateKey(nameof(ControlItemBgHover));
    /// <summary>
    /// 控件项激活/选中背景（如菜单项选中）。
    /// </summary>
    public static ComponentResourceKey ControlItemBgActive => CreateKey(nameof(ControlItemBgActive));
    /// <summary>
    /// 控件项激活并悬停的背景。
    /// </summary>
    public static ComponentResourceKey ControlItemBgActiveHover => CreateKey(nameof(ControlItemBgActiveHover));
    /// <summary>
    /// 控件项按下背景。
    /// </summary>
    public static ComponentResourceKey ControlItemBgPressed => CreateKey(nameof(ControlItemBgPressed));

    // Table
    /// <summary>
    /// 表头背景色。
    /// </summary>
    public static ComponentResourceKey TableHeaderBg => CreateKey(nameof(TableHeaderBg));
    /// <summary>
    /// 表头排序状态背景色。
    /// </summary>
    public static ComponentResourceKey TableHeaderSortBg => CreateKey(nameof(TableHeaderSortBg));
    /// <summary>
    /// 表头文字颜色。
    /// </summary>
    public static ComponentResourceKey TableHeaderColor => CreateKey(nameof(TableHeaderColor));
    /// <summary>
    /// 表格行悬停背景色。
    /// </summary>
    public static ComponentResourceKey TableRowHoverBg => CreateKey(nameof(TableRowHoverBg));
    /// <summary>
    /// 表格斑马纹背景色。
    /// </summary>
    public static ComponentResourceKey TableStripedRowBg => CreateKey(nameof(TableStripedRowBg));
    /// <summary>
    /// 表格行选中背景色。
    /// </summary>
    public static ComponentResourceKey TableSelectedRowBg => CreateKey(nameof(TableSelectedRowBg));
}
