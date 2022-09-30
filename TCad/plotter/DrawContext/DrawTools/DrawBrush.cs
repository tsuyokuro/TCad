
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter;

public readonly struct DrawBrush
{
    public readonly Color4 mColor4;

    public readonly SolidBrush GdiBrush
    {
        get => GDIToolManager.Instance.Brush(this);
    }

    public static DrawBrush NullBrush = new DrawBrush(Color.FromArgb(0, 0, 0, 0));

    public bool IsNullBrush
    {
        get => mColor4.A == 0;
    }

    public void Dispose()
    {
    }

    public Color4 Color4()
    {
        return mColor4;
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
