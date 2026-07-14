using OpenTK.Mathematics;
using System;
using System.Drawing;

namespace TCad.Plotter.DrawToolSet;

public struct DrawBrush : IEquatable<DrawBrush>
{
    public readonly static DrawBrush InvalidBrush = new()
    {
        Color4 = Color4Ext.Invalid,
    };

    static DrawBrush()
    {
    }

    public Color4 mColor4;

    public readonly SolidBrush GdiBrush
    {
        get => GDIToolManager.Provider.Get().Brush(this);
    }

    public readonly int Argb
    {
        get => ColorUtil.ToArgb(mColor4);
    }

    public readonly ColorPack ColorPack
    {
        get => new(Argb);
    }

    public readonly bool IsInvalid
    {
        get => mColor4.A < 0f;
    }

    public readonly bool IsNull
    {
        get => mColor4.A == 0f;
    }

    public Color4 Color4
    {
        readonly get => mColor4;
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

    public readonly bool Equals(DrawBrush other)
    {
        return Color4 == other.Color4;
    }

    public override readonly bool Equals(object obj)
    {
        return obj is DrawBrush other && Equals(other);
    }

    public override readonly int GetHashCode()
    {
        return HashCode.Combine(
            Color4.A, Color4.R, Color4.G, Color4.B
            );
    }


}

