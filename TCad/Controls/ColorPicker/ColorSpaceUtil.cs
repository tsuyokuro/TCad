using System;

namespace TCad.Controls;

public class ColorSpaceUtil
{
    /// <summary>
    /// 
    /// </summary>
    /// <param name="r">0~1.0</param>
    /// <param name="g">0~1.0</param>
    /// <param name="b">0~1.0</param>
    /// 
    /// <param name="h">0~360</param>
    /// <param name="s">0~1.0</param>
    /// <param name="v">0~1.0</param>
    public static void RgbToHsv(double r, double g, double b, out double h, out double s, out double v)
    {
        var max = Math.Max(r, Math.Max(g, b));

        if (max == 0.0)
        {
            h = s = v = 0.0;
            return;
        }

        var min = Math.Min(r, Math.Min(g, b));
        double delta = max - min;

        if (max == min)
        {
            h = 0;
        }
        else if (max == r)
        {
            h = (60 * (g - b) / delta) % 360;
        }
        else if (max == g)
        {
            h = 60 * (b - r) / delta + 120;
        }
        else
        {
            h = 60 * (r - g) / delta + 240;
        }

        if (h < 0.0)
            h += 360;

        h = Math.Round(h, MidpointRounding.AwayFromZero);
        s = delta / max;

        v = max;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="h">0~360</param>
    /// <param name="s">0~1.0</param>
    /// <param name="v">0~1.0</param>
    /// 
    /// <param name="r">0~1.0</param>
    /// <param name="g">0~1.0</param>
    /// <param name="b">0~1.0</param>
    public static void HsvToRgb(double h, double s, double v, out double r, out double g, out double b)
    {
        if (s == 0.0)
        {
            r = v;
            g = v;
            b = v;

            return;
        }

        h = h % 360;
        int hi = (int)(h / 60) % 6;
        var f = h / 60 - (int)(h / 60);
        var p = v * (1 - s);
        var q = v * (1 - f * s);
        var t = v * (1 - (1 - f) * s);

        switch (hi)
        {
            case 0: r = v; g = t; b = p; break;
            case 1: r = q; g = v; b = p; break;
            case 2: r = p; g = v; b = t; break;
            case 3: r = p; g = q; b = v; break;
            case 4: r = t; g = p; b = v; break;
            default: r = v; g = p; b = q; break;
        }
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="r">0~1.0</param>
    /// <param name="g">0~1.0</param>
    /// <param name="b">0~1.0</param>
    /// 
    /// <param name="hue">0~360</param>
    /// <param name="saturation">0~1.0</param>
    /// <param name="lightness">0~1.0</param>
    public static void RgbToHsl(double r, double g, double b, out double hue, out double saturation, out double lightness)
    {
        double h, s, l;

        double maxColor = Math.Max(r, Math.Max(g, b));
        double minColor = Math.Min(r, Math.Min(g, b));

        if (r == g && r == b)
        {
            h = 0.0;
            s = 0.0;
            l = r;
        }
        else
        {
            l = (minColor + maxColor) / 2;
            if (l < 0.5)
                s = (maxColor - minColor) / (maxColor + minColor);
            else
                s = (maxColor - minColor) / (2.0 - maxColor - minColor);

            if (r == maxColor)
                h = (g - b) / (maxColor - minColor);
            else if (g == maxColor)
                h = 2.0 + (b - r) / (maxColor - minColor);
            else
                h = 4.0 + (r - g) / (maxColor - minColor);

            h /= 6;

            if (h < 0)
                ++h;
        }

        if (h < 0) h = 0;
        if (h > 1) h = 1;
        if (s < 0) s = 0;
        if (s > 1) s = 1;
        if (l < 0) l = 0;
        if (l > 1) l = 1;

        hue = h * 360.0;
        saturation = s;
        lightness = l;
    }

    /// <summary>
    /// 
    /// </summary>
    /// <param name="hue">0~360</param>
    /// <param name="saturation">0~1.0</param>
    /// <param name="lightness">0~1.0</param>
    /// 
    /// <param name="r">0~1.0</param>
    /// <param name="g">0~1.0</param>
    /// <param name="b">0~1.0</param>
    public static void HslToRgb(double hue, double saturation, double lightness, out double r, out double g, out double b)
    {
        double h, s, l, s1, s2, r1, g1, b1;

        h = hue / 360.0;
        s = saturation;
        l = lightness;

        if (s == 0)
        {
            r = g = b = l;
        }
        else
        {
            if (l < 0.5)
            {
                s2 = l * (1 + s);
            }
            else
            {
                s2 = (l + s) - (l * s);
            }

            s1 = 2 * l - s2;
            r1 = h + 1.0 / 3.0;

            if (r1 > 1)
            {
                --r1;
            }

            g1 = h;
            b1 = h - 1.0 / 3.0;

            if (b1 < 0)
                ++b1;

            // R
            if (r1 < 1.0 / 6.0)
                r = s1 + (s2 - s1) * 6.0 * r1;
            else if (r1 < 0.5)
                r = s2;
            else if (r1 < 2.0 / 3.0)
                r = s1 + (s2 - s1) * ((2.0 / 3.0) - r1) * 6.0;
            else
                r = s1;

            // G
            if (g1 < 1.0 / 6.0)
                g = s1 + (s2 - s1) * 6.0 * g1;
            else if (g1 < 0.5)
                g = s2;
            else if (g1 < 2.0 / 3.0)
                g = s1 + (s2 - s1) * ((2.0 / 3.0) - g1) * 6.0;
            else g = s1;

            // B
            if (b1 < 1.0 / 6.0)
                b = s1 + (s2 - s1) * 6.0 * b1;
            else if (b1 < 0.5)
                b = s2;
            else if (b1 < 2.0 / 3.0)
                b = s1 + (s2 - s1) * ((2.0 / 3.0) - b1) * 6.0;
            else
                b = s1;
        }

        if (r < 0) r = 0;
        if (r > 1) r = 1;
        if (g < 0) g = 0;
        if (g > 1) g = 1;
        if (b < 0) b = 0;
        if (b > 1) b = 1;
    }
}
