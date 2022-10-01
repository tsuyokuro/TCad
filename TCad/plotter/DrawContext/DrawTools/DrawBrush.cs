
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter;

public struct DrawBrush
{
    public Color4 mColor4;

    public readonly SolidBrush GdiBrush
    {
        get => GDIToolManager.Instance.Brush(this);
    }

    public int Argb
    {
        get => ColorUtil.ToArgb(mColor4);
    }

    public ColorPack ColorPack
    {
        get => new ColorPack(Argb);
    }

    public static DrawBrush NullBrush = new DrawBrush(0);

    public bool IsNullBrush
    {
        get => mColor4.A == 0;
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
}
