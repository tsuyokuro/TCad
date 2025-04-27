using OpenTK.Mathematics;
using TCad.Plotter;
using System;
using System.Drawing;

namespace TCad.Plotter.DrawToolSet;

public struct DrawBrush : IEquatable<DrawBrush>
{
    public static DrawBrush InvalidBrush;

    static DrawBrush()
    {
        InvalidBrush = new()
        {
            Color4 = Color4Ext.Invalid,
        };
    }

    public Color4 mColor4;

    public readonly SolidBrush GdiBrush
    {
        get => GDIToolManager.Provider.Get().Brush(this);
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

    public bool IsNull
    {
        get => mColor4.A == 0f;
    }

    public Color4 Color4
    {
        get => mColor4;
        set => mColor4 = value;
    }

    public DrawBrush(int argb)
    {
        mColor4 = ColorUtil.FromArgb(argb);
    }

    public DrawBrush(Color4 color)
    {
        mColor4 = color;
    }

    public static bool operator ==(DrawBrush brush1, DrawBrush brush2)
    {
        return (brush1.Color4 == brush2.Color4);
    }

    public static bool operator !=(DrawBrush brush1, DrawBrush brush2)
    {
        return !(brush1.Color4 == brush2.Color4);
    }

    public bool Equals(DrawBrush other)
    {
        return Color4 == other.Color4;
    }

    public override bool Equals(object obj)
    {
        return obj is DrawBrush other && Equals(other);
    }

    public override int GetHashCode()
    {
        return HashCode.Combine(
            Color4.A, Color4.R, Color4.G, Color4.B
            );
    }


}

