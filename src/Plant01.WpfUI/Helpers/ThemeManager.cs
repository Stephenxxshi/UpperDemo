using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Effects;
using Plant01.WpfUI.Themes;

namespace Plant01.WpfUI.Helpers
{
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
        public static void ApplyTheme(Color primarySeed, ThemeType themeType, DensityType density = DensityType.Default)
        {
            if (Application.Current == null) return;
            ApplyThemeToDictionary(Application.Current.Resources, primarySeed, themeType, density);
        }

        public static void ApplyThemeToDictionary(ResourceDictionary resources, Color primarySeed, ThemeType themeType, DensityType density = DensityType.Default)
        {
            bool isDark = themeType == ThemeType.Dark;

            // 1. Generate Palette
            var palette = ColorAlgorithm.GeneratePalette(primarySeed, isDark);

            // 2. Map Palette to Semantic Tokens
            // Palette is 0-indexed (0 = Step 1, 5 = Step 6 Base)
            
            // Primary
            SetBrush(resources, DesignTokenKeys.PrimaryColor, palette[5]);       // Base (6)
            SetBrush(resources, DesignTokenKeys.PrimaryColorHover, palette[4]);  // Hover (5)
            SetBrush(resources, DesignTokenKeys.PrimaryColorActive, palette[6]); // Active (7)
            SetBrush(resources, DesignTokenKeys.PrimaryOutline, palette[0]);     // Outline (1)

            // Functional Colors (Fixed seeds for now)
            SetBrush(resources, DesignTokenKeys.SuccessColor, Color.FromRgb(0x52, 0xc4, 0x1a));
            SetBrush(resources, DesignTokenKeys.WarningColor, Color.FromRgb(0xfa, 0xad, 0x14));
            SetBrush(resources, DesignTokenKeys.ErrorColor, Color.FromRgb(0xff, 0x4d, 0x4f));
            SetBrush(resources, DesignTokenKeys.InfoColor, Color.FromRgb(0x16, 0x77, 0xff));

            // 3. Neutrals
            if (isDark)
            {
                SetBrush(resources, DesignTokenKeys.BodyBackground, Color.FromRgb(0x00, 0x00, 0x00));
                SetBrush(resources, DesignTokenKeys.ComponentBackground, Color.FromRgb(0x14, 0x14, 0x14));
                SetBrush(resources, DesignTokenKeys.PopoverBackground, Color.FromRgb(0x1f, 0x1f, 0x1f));
                
                SetBrush(resources, DesignTokenKeys.BorderColor, Color.FromRgb(0x42, 0x42, 0x42));
                SetBrush(resources, DesignTokenKeys.BorderColorSplit, Color.FromRgb(0x30, 0x30, 0x30));

                SetBrush(resources, DesignTokenKeys.TextPrimary, Color.FromArgb(217, 255, 255, 255)); // 85%
                SetBrush(resources, DesignTokenKeys.TextSecondary, Color.FromArgb(115, 255, 255, 255)); // 45%
                SetBrush(resources, DesignTokenKeys.TextTertiary, Color.FromArgb(77, 255, 255, 255)); // 30%
                SetBrush(resources, DesignTokenKeys.TextQuaternary, Color.FromArgb(64, 255, 255, 255)); // 25%
                
                SetBrush(resources, DesignTokenKeys.MaskColor, Color.FromArgb(115, 0, 0, 0));
                
                // Dark Shadows (Subtler or different)
                SetShadow(resources, DesignTokenKeys.BoxShadowSmall, 2, 0.2);
                SetShadow(resources, DesignTokenKeys.BoxShadow, 6, 0.3);
                SetShadow(resources, DesignTokenKeys.BoxShadowLarge, 10, 0.4);
            }
            else
            {
                SetBrush(resources, DesignTokenKeys.BodyBackground, Colors.White);
                SetBrush(resources, DesignTokenKeys.ComponentBackground, Colors.White);
                SetBrush(resources, DesignTokenKeys.PopoverBackground, Colors.White);

                SetBrush(resources, DesignTokenKeys.BorderColor, Color.FromRgb(0xd9, 0xd9, 0xd9));
                SetBrush(resources, DesignTokenKeys.BorderColorSplit, Color.FromRgb(0xf0, 0xf0, 0xf0));

                SetBrush(resources, DesignTokenKeys.TextPrimary, Color.FromArgb(224, 0, 0, 0)); // 88%
                SetBrush(resources, DesignTokenKeys.TextSecondary, Color.FromArgb(166, 0, 0, 0)); // 65%
                SetBrush(resources, DesignTokenKeys.TextTertiary, Color.FromArgb(115, 0, 0, 0)); // 45%
                SetBrush(resources, DesignTokenKeys.TextQuaternary, Color.FromArgb(64, 0, 0, 0)); // 25%
                
                SetBrush(resources, DesignTokenKeys.MaskColor, Color.FromArgb(115, 0, 0, 0));

                // Light Shadows
                SetShadow(resources, DesignTokenKeys.BoxShadowSmall, 2, 0.1);
                SetShadow(resources, DesignTokenKeys.BoxShadow, 6, 0.15);
                SetShadow(resources, DesignTokenKeys.BoxShadowLarge, 10, 0.2);
            }

            // 4. Density & Sizing
            ApplyDensity(resources, density);
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
            
            SetValue(resources, DesignTokenKeys.BorderRadius, new CornerRadius(borderRadius));
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
}
