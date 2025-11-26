using System.Windows;
using System.Windows.Media;
using Plant01.WpfUI.Themes;

namespace Plant01.WpfUI.Helpers
{
    public enum ThemeType
    {
        Light,
        Dark
    }

    public static class ThemeManager
    {
        public static void ApplyTheme(Color primarySeed, ThemeType themeType)
        {
            if (Application.Current == null) return;
            ApplyThemeToDictionary(Application.Current.Resources, primarySeed, themeType);
        }

        public static void ApplyThemeToDictionary(ResourceDictionary resources, Color primarySeed, ThemeType themeType)
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
            }
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
