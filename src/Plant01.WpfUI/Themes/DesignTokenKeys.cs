using System.Windows;

namespace Plant01.WpfUI.Themes;

public static class DesignTokenKeys
{
    private static ComponentResourceKey CreateKey(string id)
    {
        return new ComponentResourceKey(typeof(DesignTokenKeys), id);
    }

    // Brand Colors
    public static ComponentResourceKey PrimaryColor => CreateKey(nameof(PrimaryColor));
    public static ComponentResourceKey PrimaryColorValue => CreateKey(nameof(PrimaryColorValue)); // Raw Color value
    public static ComponentResourceKey PrimaryColorHover => CreateKey(nameof(PrimaryColorHover));
    public static ComponentResourceKey PrimaryColorActive => CreateKey(nameof(PrimaryColorActive));
    public static ComponentResourceKey PrimaryOutline => CreateKey(nameof(PrimaryOutline));

    // Functional Colors
    public static ComponentResourceKey SuccessColor => CreateKey(nameof(SuccessColor));
    public static ComponentResourceKey WarningColor => CreateKey(nameof(WarningColor));
    public static ComponentResourceKey ErrorColor => CreateKey(nameof(ErrorColor));
    public static ComponentResourceKey ErrorColorHover => CreateKey(nameof(ErrorColorHover));
    public static ComponentResourceKey ErrorColorActive => CreateKey(nameof(ErrorColorActive));
    public static ComponentResourceKey InfoColor => CreateKey(nameof(InfoColor));

    // Neutral Colors
    public static ComponentResourceKey BodyBackground => CreateKey(nameof(BodyBackground));
    public static ComponentResourceKey ComponentBackground => CreateKey(nameof(ComponentBackground));
    public static ComponentResourceKey PopoverBackground => CreateKey(nameof(PopoverBackground));
    
    public static ComponentResourceKey BorderColor => CreateKey(nameof(BorderColor));
    public static ComponentResourceKey BorderColorSplit => CreateKey(nameof(BorderColorSplit));

    public static ComponentResourceKey FillColor => CreateKey(nameof(FillColor)); // New Token for neutral fills (Switch off, etc.)

    public static ComponentResourceKey TextPrimary => CreateKey(nameof(TextPrimary));
    public static ComponentResourceKey TextSecondary => CreateKey(nameof(TextSecondary));
    public static ComponentResourceKey TextTertiary => CreateKey(nameof(TextTertiary));
    public static ComponentResourceKey TextQuaternary => CreateKey(nameof(TextQuaternary));
    public static ComponentResourceKey TextOnPrimary => CreateKey(nameof(TextOnPrimary)); // Always White usually
    public static ComponentResourceKey PlaceholderColor => CreateKey(nameof(PlaceholderColor));

    public static ComponentResourceKey MaskColor => CreateKey(nameof(MaskColor));

    // Shadows
    public static ComponentResourceKey BoxShadowSmall => CreateKey(nameof(BoxShadowSmall));
    public static ComponentResourceKey BoxShadow => CreateKey(nameof(BoxShadow));
    public static ComponentResourceKey BoxShadowLarge => CreateKey(nameof(BoxShadowLarge));

    // Sizing & Density
    public static ComponentResourceKey ControlHeight => CreateKey(nameof(ControlHeight));
    public static ComponentResourceKey ControlHeightLG => CreateKey(nameof(ControlHeightLG));
    public static ComponentResourceKey ControlHeightSM => CreateKey(nameof(ControlHeightSM));
    
    public static ComponentResourceKey FontSize => CreateKey(nameof(FontSize));
    public static ComponentResourceKey FontSizeLG => CreateKey(nameof(FontSizeLG));
    public static ComponentResourceKey FontSizeSM => CreateKey(nameof(FontSizeSM));

    public static ComponentResourceKey PaddingXS => CreateKey(nameof(PaddingXS));
    public static ComponentResourceKey PaddingSM => CreateKey(nameof(PaddingSM));
    public static ComponentResourceKey PaddingMD => CreateKey(nameof(PaddingMD));
    public static ComponentResourceKey PaddingLG => CreateKey(nameof(PaddingLG));
    
    public static ComponentResourceKey BorderRadius => CreateKey(nameof(BorderRadius));
    public static ComponentResourceKey BorderRadiusBase => CreateKey(nameof(BorderRadiusBase));

    // Control Item States (Menu, List, etc.)
    public static ComponentResourceKey ControlItemBgHover => CreateKey(nameof(ControlItemBgHover));
    public static ComponentResourceKey ControlItemBgActive => CreateKey(nameof(ControlItemBgActive));
    public static ComponentResourceKey ControlItemBgActiveHover => CreateKey(nameof(ControlItemBgActiveHover));
    public static ComponentResourceKey ControlItemBgPressed => CreateKey(nameof(ControlItemBgPressed));

    // Table
    public static ComponentResourceKey TableHeaderBg => CreateKey(nameof(TableHeaderBg));
    public static ComponentResourceKey TableHeaderSortBg => CreateKey(nameof(TableHeaderSortBg));
    public static ComponentResourceKey TableHeaderColor => CreateKey(nameof(TableHeaderColor));
    public static ComponentResourceKey TableRowHoverBg => CreateKey(nameof(TableRowHoverBg));
    public static ComponentResourceKey TableStripedRowBg => CreateKey(nameof(TableStripedRowBg));
    public static ComponentResourceKey TableSelectedRowBg => CreateKey(nameof(TableSelectedRowBg));
}
