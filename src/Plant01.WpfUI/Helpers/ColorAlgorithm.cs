using System.Windows.Media;

namespace Plant01.WpfUI.Helpers;

public static class ColorAlgorithm
{
    public struct Hsv
    {
        public double H;
        public double S;
        public double V;

        public Hsv(double h, double s, double v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    public static List<Color> GeneratePalette(Color seed, bool isDark)
    {
        var palette = new List<Color>();
        var hsv = ColorToHsv(seed);

        for (int i = 1; i <= 10; i++)
        {
            // 暗色模式下，大致反转渐变方向
            // 亮色：1(浅) -> 10(深)
            // 暗色：1(深) -> 10(浅)
            // 但基准色(6)保持相对稳定

            // 简单策略：使用相同算法，但暗色模式下反转索引
            // 这是一种启发式做法，真实的 Ant Design 使用更复杂的混合
            int effectiveIndex = isDark ? 11 - i : i;

            // 暗色模式通常需要降低饱和度以减少视觉疲劳
            Hsv adjustedHsv = hsv;
            if (isDark)
            {
                adjustedHsv.S *= 0.8; // 暗色模式下轻微降低饱和度
            }

            palette.Add(GetAlgorithmColor(adjustedHsv, effectiveIndex));
        }
        return palette;
    }

    private static Color GetAlgorithmColor(Hsv hsv, int index)
    {
        double h = hsv.H;
        double s = hsv.S;
        double v = hsv.V;

        // 索引6为基准色
        if (index != 6)
        {
            // 色相旋转
            double step = index - 6;
            h += step * 2; // 简单的色相旋转
            if (h < 0) h += 360;
            if (h >= 360) h -= 360;

            // 饱和度与明度调整
            if (index < 6) // 越靠近1越浅
            {
                s -= (6 - index) * 0.16;
                v += (6 - index) * 0.05;
            }
            else // 越靠近10越深
            {
                s += (index - 6) * 0.05;
                v -= (index - 6) * 0.15;
            }
        }

        s = Math.Clamp(s, 0, 1);
        v = Math.Clamp(v, 0, 1);

        return HsvToColor(new Hsv(h, s, v));
    }

    public static Hsv ColorToHsv(Color color)
    {
        double r = color.R / 255.0;
        double g = color.G / 255.0;
        double b = color.B / 255.0;

        double max = Math.Max(r, Math.Max(g, b));
        double min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        double h = 0;
        if (delta > 0)
        {
            if (max == r) h = ((g - b) / delta) % 6;
            else if (max == g) h = (b - r) / delta + 2;
            else h = (r - g) / delta + 4;
            h *= 60;
            if (h < 0) h += 360;
        }

        double s = max == 0 ? 0 : delta / max;
        double v = max;

        return new Hsv(h, s, v);
    }

    public static Color HsvToColor(Hsv hsv)
    {
        double c = hsv.V * hsv.S;
        double x = c * (1 - Math.Abs((hsv.H / 60) % 2 - 1));
        double m = hsv.V - c;

        double r = 0, g = 0, b = 0;
        if (hsv.H >= 0 && hsv.H < 60) { r = c; g = x; b = 0; }
        else if (hsv.H >= 60 && hsv.H < 120) { r = x; g = c; b = 0; }
        else if (hsv.H >= 120 && hsv.H < 180) { r = 0; g = c; b = x; }
        else if (hsv.H >= 180 && hsv.H < 240) { r = 0; g = x; b = c; }
        else if (hsv.H >= 240 && hsv.H < 300) { r = x; g = 0; b = c; }
        else if (hsv.H >= 300 && hsv.H < 360) { r = c; g = 0; b = x; }

        return Color.FromRgb(
            (byte)((r + m) * 255),
            (byte)((g + m) * 255),
            (byte)((b + m) * 255));
    }
}
