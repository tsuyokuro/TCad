using OpenTK.Mathematics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plotter;

public static class ColorUtil
{
    [StructLayout(LayoutKind.Explicit)]
    private struct ColorPack
    {
        [FieldOffset(0)]
        public int Argb;

        [FieldOffset(3)]
        public byte A;

        [FieldOffset(2)]
        public byte R;

        [FieldOffset(1)]
        public byte G;

        [FieldOffset(0)]
        public byte B;

        public ColorPack(byte a, byte r, byte g, byte b)
        {
            Argb = 0;
            A = a;
            R = r;
            G = g;
            B = b;
        }
    }

    public static Color4 FromArgb(int argb)
    {
        ColorPack c = default;
        c.Argb = argb;

        return new Color4(
                c.R,
                c.G,
                c.B,
                c.A
            );
    }

    public static int ARGB(byte a, byte r, byte g, byte b)
    {
        return new ColorPack(a, r, g, b).Argb;
    }

    public static Color ToGDIColor(Color4 c)
    {
        return Color.FromArgb(
            (int)(c.A * 255f),
            (int)(c.R * 255f),
            (int)(c.G * 255f),
            (int)(c.B * 255f));
    }
}
