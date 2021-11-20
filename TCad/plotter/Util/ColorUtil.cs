using System;

namespace Plotter
{
    public struct HSV
    {
        public float H; // 色相 ( 0<= H < 360 )
        public float S; // 彩度 ( 0<= H = 1.0 )
        public float V; // 明度 ( 0<= H = 1.0 )

        public HSV(float h, float s, float v)
        {
            H = h;
            S = s;
            V = v;
        }
    }

    public struct RGB
    {
        public float R;
        public float G;
        public float B;

        public RGB(float r, float g, float b)
        {
            R = r;
            G = g;
            B = b;
        }
    }

    class ColorUtil
    {
        public static RGB Brightness(RGB rgb, float a)
        {
            rgb.R *= a;
            rgb.G *= a;
            rgb.B *= a;

            if (rgb.R > 1.0f)
            {
                rgb.R = 1.0f;
            }

            if (rgb.G > 1.0f)
            {
                rgb.G = 1.0f;
            }

            if (rgb.B > 1.0f)
            {
                rgb.B = 1.0f;
            }

            return rgb;
        }


        public static HSV RgbToHsv(RGB rgb)
        {
            float r = rgb.R;
            float g = rgb.G;
            float b = rgb.B;


            float max = Math.Max(r, Math.Max(g, b));
            float min = Math.Min(r, Math.Min(g, b));

            float brightness = max;

            float hue, saturation;
            if (max == min)
            {
                hue = 0f;
                saturation = 0f;
            }
            else
            {
                float c = max - min;

                if (max == r)
                {
                    hue = (g - b) / c;
                }
                else if (max == g)
                {
                    hue = (b - r) / c + 2f;
                }
                else
                {
                    hue = (r - g) / c + 4f;
                }
                hue *= 60f;
                if (hue < 0f)
                {
                    hue += 360f;
                }

                saturation = c / max;
            }

            HSV hsv = new HSV();

            hsv.H = hue;
            hsv.S = saturation;
            hsv.V = brightness;

            return hsv;
        }

        public static RGB HsvToRgb(HSV hsv)
        {
            float v = hsv.V;
            float s = hsv.S;

            float r, g, b;
            if (s == 0)
            {
                r = v;
                g = v;
                b = v;
            }
            else
            {
                float h = hsv.H / 60f;
                int i = (int)Math.Floor(h);
                float f = h - i;
                float p = v * (1f - s);
                float q;
                if (i % 2 == 0)
                {
                    q = v * (1f - (1f - f) * s);
                }
                else
                {
                    q = v * (1f - f * s);
                }

                switch (i)
                {
                    case 0:
                        r = v;
                        g = q;
                        b = p;
                        break;
                    case 1:
                        r = q;
                        g = v;
                        b = p;
                        break;
                    case 2:
                        r = p;
                        g = v;
                        b = q;
                        break;
                    case 3:
                        r = p;
                        g = q;
                        b = v;
                        break;
                    case 4:
                        r = q;
                        g = p;
                        b = v;
                        break;
                    case 5:
                        r = v;
                        g = p;
                        b = q;
                        break;
                    default:
                        throw new ArgumentException("bad hue", "hsv");
                }
            }

            RGB rgb = new RGB();

            rgb.R = r;
            rgb.G = g;
            rgb.B = b;

            return rgb;
        }
    }
}
