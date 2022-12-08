using OpenTK.Mathematics;
using System;
using System.Drawing;
using System.Runtime.InteropServices;


using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;

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

    public static Color4 Mix(Color4 c1, Color4 c2, float strengthC1)
    {
        float strengthC2 = 1.0f - strengthC1;

        Color4 rc = default;

        rc.R = ((c1.R * strengthC1) + (c2.R * strengthC2));
        rc.G = ((c1.G * strengthC1) + (c2.G * strengthC2));
        rc.B = ((c1.B * strengthC1) + (c2.B * strengthC2));

        rc.R = Math.Min(1.0f, rc.R);
        rc.G = Math.Min(1.0f, rc.G);
        rc.B = Math.Min(1.0f, rc.B);

        rc.A = 1.0f;

        return rc;
    }
}

public static class Color4Ext
{
    public static bool IsInvalid(this Color4 v)
    {
        return v.A < (vcompo_t)(0.0);
    }

    public static readonly Color4 Invalid = new Color4(0, 0, 0, -1.0f);
}
