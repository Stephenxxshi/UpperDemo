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
    public static ComponentResourceKey PrimaryColorHover => CreateKey(nameof(PrimaryColorHover));
    public static ComponentResourceKey PrimaryColorActive => CreateKey(nameof(PrimaryColorActive));
    public static ComponentResourceKey PrimaryOutline => CreateKey(nameof(PrimaryOutline));

    // Functional Colors
    public static ComponentResourceKey SuccessColor => CreateKey(nameof(SuccessColor));
    public static ComponentResourceKey WarningColor => CreateKey(nameof(WarningColor));
    public static ComponentResourceKey ErrorColor => CreateKey(nameof(ErrorColor));
    public static ComponentResourceKey InfoColor => CreateKey(nameof(InfoColor));

    // Neutral Colors
    public static ComponentResourceKey BodyBackground => CreateKey(nameof(BodyBackground));
    public static ComponentResourceKey ComponentBackground => CreateKey(nameof(ComponentBackground));
    public static ComponentResourceKey PopoverBackground => CreateKey(nameof(PopoverBackground));
    
    public static ComponentResourceKey BorderColor => CreateKey(nameof(BorderColor));
    public static ComponentResourceKey BorderColorSplit => CreateKey(nameof(BorderColorSplit));

    public static ComponentResourceKey TextPrimary => CreateKey(nameof(TextPrimary));
    public static ComponentResourceKey TextSecondary => CreateKey(nameof(TextSecondary));
    public static ComponentResourceKey TextTertiary => CreateKey(nameof(TextTertiary));
    public static ComponentResourceKey TextQuaternary => CreateKey(nameof(TextQuaternary));
    
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
}
