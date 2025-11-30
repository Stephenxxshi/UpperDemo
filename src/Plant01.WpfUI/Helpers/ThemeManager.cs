using Plant01.WpfUI.Themes;

using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;

namespace Plant01.WpfUI.Helpers;

public enum ThemeType
{
    Light,
    Dark
}

public enum DensityType
{
    Compact,
    Default,
    Touch
}

public static class ThemeManager
{
    public static void ApplyTheme(Color primarySeed, ThemeType themeType, DensityType density = DensityType.Default, Color? bodyBackground = null)
    {
        if (Application.Current == null) return;
        ApplyThemeToDictionary(Application.Current.Resources, primarySeed, themeType, density, bodyBackground);
    }

    public static void ApplyThemeToDictionary(ResourceDictionary resources, Color primarySeed, ThemeType themeType, DensityType density = DensityType.Default, Color? bodyBackground = null)
    {
        bool isDark = themeType == ThemeType.Dark;

        // 1. Generate Palette
        var palette = ColorAlgorithm.GeneratePalette(primarySeed, isDark);

        // 2. Map Palette to Semantic Tokens
        // Palette is 0-indexed (0 = Step 1, 5 = Step 6 Base)

        // Ant Design 5.x Algorithm Mapping
        SetBrush(resources, DesignTokenKeys.ColorPrimaryBg, palette[0]);          // Step 1
        SetBrush(resources, DesignTokenKeys.ColorPrimaryBgHover, palette[1]);     // Step 2
        SetBrush(resources, DesignTokenKeys.ColorPrimaryBorder, palette[2]);      // Step 3
        SetBrush(resources, DesignTokenKeys.ColorPrimaryBorderHover, palette[3]); // Step 4
        SetBrush(resources, DesignTokenKeys.ColorPrimaryHover, palette[4]);       // Step 5
        // Step 6 is Base
        SetBrush(resources, DesignTokenKeys.ColorPrimaryActive, palette[6]);      // Step 7
        SetBrush(resources, DesignTokenKeys.ColorPrimaryTextHover, palette[7]);   // Step 8
        SetBrush(resources, DesignTokenKeys.ColorPrimaryText, palette[8]);        // Step 9
        SetBrush(resources, DesignTokenKeys.ColorPrimaryTextActive, palette[9]);  // Step 10

        // Legacy / Semantic Mapping
        SetBrush(resources, DesignTokenKeys.PrimaryColor, palette[5]);       // Base (6)
        SetValue(resources, DesignTokenKeys.PrimaryColorValue, palette[5]);  // Raw Color
        SetBrush(resources, DesignTokenKeys.PrimaryColorHover, palette[4]);  // Hover (5)
        SetBrush(resources, DesignTokenKeys.PrimaryColorActive, palette[6]); // Active (7)
        SetBrush(resources, DesignTokenKeys.PrimaryOutline, palette[0]);     // Outline (1)

        // Functional Colors (Fixed seeds for now) ����ɫ
        SetBrush(resources, DesignTokenKeys.SuccessColor, Color.FromRgb(0x52, 0xc4, 0x1a));
        SetBrush(resources, DesignTokenKeys.WarningColor, Color.FromRgb(0xfa, 0xad, 0x14));
        SetBrush(resources, DesignTokenKeys.ErrorColor, Color.FromRgb(0xff, 0x4d, 0x4f));
        SetBrush(resources, DesignTokenKeys.ErrorColorHover, Color.FromRgb(0xff, 0x78, 0x75));
        SetBrush(resources, DesignTokenKeys.ErrorColorActive, Color.FromRgb(0xd9, 0x36, 0x3e));
        SetBrush(resources, DesignTokenKeys.InfoColor, Color.FromRgb(0x16, 0x77, 0xff));

        // 3. Neutrals ����ɫ
        if (isDark)
        {
            SetBrush(resources, DesignTokenKeys.BodyBackground, bodyBackground ?? Color.FromRgb(0x00, 0x00, 0x00));
            SetBrush(resources, DesignTokenKeys.ComponentBackground, Color.FromRgb(0x14, 0x14, 0x14));
            SetBrush(resources, DesignTokenKeys.PopoverBackground, Color.FromRgb(0x1f, 0x1f, 0x1f));

            SetBrush(resources, DesignTokenKeys.BorderColor, Color.FromRgb(0x42, 0x42, 0x42));
            SetBrush(resources, DesignTokenKeys.BorderColorSplit, Color.FromRgb(0x30, 0x30, 0x30));
            SetBrush(resources, DesignTokenKeys.FillColor, Color.FromRgb(0x6b, 0x6b, 0x6b)); // Dark Mode Neutral Fill
            SetBrush(resources, DesignTokenKeys.ColorFillAlter, Color.FromRgb(0x1D, 0x1D, 0x1D)); // Dark Mode Fill Alter

            SetBrush(resources, DesignTokenKeys.TextPrimary, Color.FromArgb(217, 255, 255, 255)); // 85%
            SetBrush(resources, DesignTokenKeys.TextSecondary, Color.FromArgb(115, 255, 255, 255)); // 45%
            SetBrush(resources, DesignTokenKeys.TextTertiary, Color.FromArgb(77, 255, 255, 255)); // 30%
            SetBrush(resources, DesignTokenKeys.TextQuaternary, Color.FromArgb(64, 255, 255, 255)); // 25%
            SetBrush(resources, DesignTokenKeys.TextOnPrimary, Colors.White);
            SetBrush(resources, DesignTokenKeys.PlaceholderColor, Color.FromArgb(100, 255, 255, 255)); // Dark Mode Placeholder

            SetBrush(resources, DesignTokenKeys.MaskColor, Color.FromArgb(115, 0, 0, 0));

            // Dark Shadows (Subtler or different)
            SetShadow(resources, DesignTokenKeys.BoxShadowSmall, 2, 0.2);
            SetShadow(resources, DesignTokenKeys.BoxShadow, 6, 0.3);
            SetShadow(resources, DesignTokenKeys.BoxShadowLarge, 10, 0.4);
        }
        else
        {
            SetBrush(resources, DesignTokenKeys.BodyBackground, bodyBackground ?? Colors.White);
            SetBrush(resources, DesignTokenKeys.ComponentBackground, Colors.White);
            SetBrush(resources, DesignTokenKeys.PopoverBackground, Colors.White);

            SetBrush(resources, DesignTokenKeys.BorderColor, Color.FromRgb(0xd9, 0xd9, 0xd9));
            SetBrush(resources, DesignTokenKeys.BorderColorSplit, Color.FromRgb(0xf0, 0xf0, 0xf0));
            SetBrush(resources, DesignTokenKeys.ColorFillAlter, Color.FromRgb(0xFA, 0xFA, 0xFA)); // Light Mode Fill Alter
            SetBrush(resources, DesignTokenKeys.FillColor, Color.FromRgb(0x80, 0x80, 0x80)); // Light Mode Neutral Fill (Visible Gray)

            SetBrush(resources, DesignTokenKeys.TextPrimary, Color.FromArgb(224, 0, 0, 0)); // 88%
            SetBrush(resources, DesignTokenKeys.TextSecondary, Color.FromArgb(166, 0, 0, 0)); // 65%
            SetBrush(resources, DesignTokenKeys.TextTertiary, Color.FromArgb(115, 0, 0, 0)); // 45%
            SetBrush(resources, DesignTokenKeys.TextQuaternary, Color.FromArgb(64, 0, 0, 0)); // 25%
            SetBrush(resources, DesignTokenKeys.TextOnPrimary, Colors.White);
            SetBrush(resources, DesignTokenKeys.PlaceholderColor, Color.FromRgb(191, 191, 191)); // #BFBFBF

            SetBrush(resources, DesignTokenKeys.MaskColor, Color.FromArgb(115, 0, 0, 0));

            // Light Shadows
            SetShadow(resources, DesignTokenKeys.BoxShadowSmall, 2, 0.1);
            SetShadow(resources, DesignTokenKeys.BoxShadow, 6, 0.15);
            SetShadow(resources, DesignTokenKeys.BoxShadowLarge, 10, 0.2);
        }

        // 4. Density & Sizing
        ApplyDensity(resources, density);

        // 5. Control Item Colors
        SetControlItemColors(resources, palette, isDark);

        // 6. Table Colors
        SetTableColors(resources, palette, isDark);
    }

    private static void ApplyDensity(ResourceDictionary resources, DensityType density)
    {
        double baseHeight = 32;
        double baseFontSize = 14;
        double basePadding = 15;
        double borderRadius = 6;

        switch (density)
        {
            case DensityType.Compact:
                baseHeight = 24;
                baseFontSize = 12;
                basePadding = 7;
                borderRadius = 4;
                break;
            case DensityType.Default:
                baseHeight = 32;
                baseFontSize = 14;
                basePadding = 15;
                borderRadius = 6;
                break;
            case DensityType.Touch:
                baseHeight = 44; // Minimum touch target
                baseFontSize = 16;
                basePadding = 20;
                borderRadius = 8;
                break;
        }

        SetValue(resources, DesignTokenKeys.ControlHeight, baseHeight);
        SetValue(resources, DesignTokenKeys.ControlHeightSM, baseHeight * 0.75);
        SetValue(resources, DesignTokenKeys.ControlHeightLG, baseHeight * 1.25);

        SetValue(resources, DesignTokenKeys.FontSize, baseFontSize);
        SetValue(resources, DesignTokenKeys.FontSizeSM, baseFontSize - 2);
        SetValue(resources, DesignTokenKeys.FontSizeLG, baseFontSize + 2);

        SetValue(resources, DesignTokenKeys.PaddingMD, new Thickness(basePadding, 4, basePadding, 4));
        SetValue(resources, DesignTokenKeys.PaddingSM, new Thickness(basePadding / 2, 2, basePadding / 2, 2));

        // Modal Paddings (Ant Design 5.x Defaults)
        SetValue(resources, DesignTokenKeys.ModalContentPadding, new Thickness(24));
        SetValue(resources, DesignTokenKeys.ModalHeaderPadding, new Thickness(24, 16, 24, 16)); // Left, Top, Right, Bottom
        SetValue(resources, DesignTokenKeys.ModalFooterPadding, new Thickness(16, 10, 16, 10));

        SetValue(resources, DesignTokenKeys.BorderRadius, new CornerRadius(borderRadius));
        SetValue(resources, DesignTokenKeys.BorderRadiusBase, borderRadius);
    }

    private static void SetControlItemColors(ResourceDictionary resources, List<Color> palette, bool isDark)
    {
        if (isDark)
        {
            // Dark Mode
            SetBrush(resources, DesignTokenKeys.ControlItemBgHover, Color.FromArgb(20, 255, 255, 255)); // White 8%
            SetBrush(resources, DesignTokenKeys.ControlItemBgPressed, Color.FromArgb(30, 255, 255, 255)); // White 12%
            SetBrush(resources, DesignTokenKeys.ControlItemBgActive, palette[5]); // Primary Base (Solid for Dark Menu usually)
            SetBrush(resources, DesignTokenKeys.ControlItemBgActiveHover, palette[4]);
        }
        else
        {
            // Light Mode
            SetBrush(resources, DesignTokenKeys.ControlItemBgHover, Color.FromRgb(0xF5, 0xF5, 0xF5)); // Neutral hover
            SetBrush(resources, DesignTokenKeys.ControlItemBgPressed, Color.FromRgb(0xE6, 0xE6, 0xE6)); // Neutral pressed
            SetBrush(resources, DesignTokenKeys.ControlItemBgActive, palette[0]); // Primary-1 (Light Blue)
            SetBrush(resources, DesignTokenKeys.ControlItemBgActiveHover, palette[1]); // Primary-2
        }
    }

    /// <summary>
    /// ����DataGrid���������ɫ
    /// </summary>
    /// <param name="resources"></param>
    /// <param name="palette"></param>
    /// <param name="isDark"></param>
    private static void SetTableColors(ResourceDictionary resources, List<Color> palette, bool isDark)
    {
        if (isDark)
        {
            SetBrush(resources, DesignTokenKeys.TableHeaderBg, Color.FromRgb(0x1D, 0x1D, 0x1D));
            SetBrush(resources, DesignTokenKeys.TableHeaderSortBg, Color.FromRgb(0x26, 0x26, 0x26));
            SetBrush(resources, DesignTokenKeys.TableHeaderColor, Color.FromArgb(217, 255, 255, 255)); // TextPrimary
            SetBrush(resources, DesignTokenKeys.TableRowHoverBg, Color.FromArgb(20, 255, 255, 255)); // White 8%
            SetBrush(resources, DesignTokenKeys.TableStripedRowBg, Color.FromArgb(10, 255, 255, 255)); // White 4%
            SetBrush(resources, DesignTokenKeys.TableSelectedRowBg, palette[5]); // Primary Base
        }
        else
        {
            SetBrush(resources, DesignTokenKeys.TableHeaderBg, Color.FromRgb(0xFA, 0xFA, 0xFA));
            SetBrush(resources, DesignTokenKeys.TableHeaderSortBg, Color.FromRgb(0xF5, 0xF5, 0xF5));
            SetBrush(resources, DesignTokenKeys.TableHeaderColor, Color.FromArgb(224, 0, 0, 0)); // TextPrimary
            SetBrush(resources, DesignTokenKeys.TableRowHoverBg, Color.FromRgb(0xFA, 0xFA, 0xFA));
            SetBrush(resources, DesignTokenKeys.TableStripedRowBg, Color.FromRgb(0xFA, 0xFA, 0xFA));
            SetBrush(resources, DesignTokenKeys.TableSelectedRowBg, palette[0]); // Primary-1
        }
    }

    private static void SetValue(ResourceDictionary resources, ComponentResourceKey key, object value)
    {
        if (resources.Contains(key))
        {
            resources[key] = value;
        }
        else
        {
            resources.Add(key, value);
        }
    }

    private static void SetShadow(ResourceDictionary resources, ComponentResourceKey key, double blur, double opacity)
    {
        var shadow = new DropShadowEffect
        {
            BlurRadius = blur,
            ShadowDepth = blur / 3,
            Opacity = opacity,
            Color = Colors.Black,
            Direction = 270
        };
        shadow.Freeze();
        SetValue(resources, key, shadow);
    }

    private static void SetBrush(ResourceDictionary resources, ComponentResourceKey key, Color color)
    {
        var brush = new SolidColorBrush(color);
        brush.Freeze();

        if (resources.Contains(key))
        {
            resources[key] = brush;
        }
        else
        {
            resources.Add(key, brush);
        }
    }
}
