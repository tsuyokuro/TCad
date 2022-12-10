//#define DEFAULT_DATA_TYPE_DOUBLE

using OpenTK.Mathematics;
using System;
using System.Drawing;



#if DEFAULT_DATA_TYPE_DOUBLE
using vcompo_t = System.Double;
using vector3_t = OpenTK.Mathematics.Vector3d;
using vector4_t = OpenTK.Mathematics.Vector4d;
using matrix4_t = OpenTK.Mathematics.Matrix4d;
#else
using vcompo_t = System.Single;
using vector3_t = OpenTK.Mathematics.Vector3;
using vector4_t = OpenTK.Mathematics.Vector4;
using matrix4_t = OpenTK.Mathematics.Matrix4;
#endif


namespace Plotter;

public struct DrawPen : IEquatable<DrawPen>
{
    public static DrawPen InvalidPen;

    static DrawPen()
    {
        InvalidPen = new()
        {
            Color4 = Color4Ext.Invalid,
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

    public bool IsNull
    {
        get => mColor4.A == 0f;
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
