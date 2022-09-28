
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public struct DrawPen
    {
        public Color4 mColor4;
        public float Width;

        public Pen GdiPen
        {
            get
            {
                return new Pen(Color4Util.ToGDIColor(mColor4), Width);
            }
        }

        public static DrawPen NullPen = new DrawPen(0, 0);

        public bool IsNullPen
        {
            get => mColor4.A == 0.0;
        }

        public void Dispose()
        {
        }

        public Color4 Color4()
        {
            return mColor4;
        }

        public DrawPen(Pen pen)
        {
            mColor4 = Color4Util.FromArgb(pen.Color.ToArgb());
            Width = pen.Width;
        }

        public DrawPen(int argb, float width)
        {
            mColor4 = Color4Util.FromArgb(argb);
            Width = width;
        }

        public DrawPen(Color4 color, float width)
        {
            mColor4 = color;
            Width = width;
        }
    }
}
