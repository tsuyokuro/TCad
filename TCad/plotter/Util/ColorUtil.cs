using OpenTK.Mathematics;
using System.Drawing;
using System.Runtime.InteropServices;

namespace Plotter;

[StructLayout(LayoutKind.Explicit)]
public struct ColorPack
{
    [FieldOffset(0)]
    public int Argb = 0;

    [FieldOffset(3)]
    public byte A = 0;

    [FieldOffset(2)]
    public byte R = 0;

    [FieldOffset(1)]
    public byte G = 0;

    [FieldOffset(0)]
    public byte B = 0;

    public ColorPack(byte a, byte r, byte g, byte b)
    {
        A = a;
        R = r;
        G = g;
        B = b;
    }

    public ColorPack(int argb)
    {
        Argb = argb;
    }
}

public static class ColorUtil
{
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

    public static int Argb(byte a, byte r, byte g, byte b)
    {
        return new ColorPack(a, r, g, b).Argb;
    }

    public static int ToArgb(Color4 c)
    {
        return Argb(
                (byte)(c.A * 255f),
                (byte)(c.R * 255f),
                (byte)(c.G * 255f),
                (byte)(c.B * 255f));
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
