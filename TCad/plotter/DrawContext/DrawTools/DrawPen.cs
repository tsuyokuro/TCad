
using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace Plotter;

public struct DrawPen : IEquatable<DrawPen>
{
    public static DrawPen Invalid;

    private static float InvalidValue = -1.0f; 

    static DrawPen()
    {
        Invalid = new()
        {
            Color4 = new Color4(0, 0, 0, InvalidValue),
            Width = float.MinValue,
        };
    }

    public Color4 mColor4;
    public float Width;

    public Pen GdiPen
    {
        get => GDIToolManager.Instance.Pen(this);
    }

    public int Argb
    {
        get => ColorUtil.ToArgb(mColor4);
    }

    public ColorPack ColorPack
    {
        get => new ColorPack(Argb);
    }

    public bool IsInvalid
    {
        get => mColor4.A < 0f;
    }

    public Color4 Color4
    {
        get => mColor4;
        set => mColor4 = value;
    }

    public DrawPen(int argb, float width)
    {
        mColor4 = ColorUtil.FromArgb(argb);
        Width = width;
    }

    public DrawPen(Color4 color, float width)
    {
        mColor4 = color;
        Width = width;
    }


    public static bool operator == (DrawPen pen1, DrawPen pen2)
    {
        return (pen1.Color4 == pen1.Color4) && (pen1.Width == pen2.Width);
    }

    public static bool operator != (DrawPen pen1, DrawPen pen2)
    {
        return !((pen1.Color4 == pen1.Color4) && (pen1.Width == pen2.Width));
    }

    public bool Equals(DrawPen other)
    {
        return Color4 == other.Color4 && Width == other.Width;
    }

    public override bool Equals(object obj)
    {
        return obj is DrawPen other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Color4.A, Color4.R, Color4.G, Color4.B,
            Width
            );
    }
}
