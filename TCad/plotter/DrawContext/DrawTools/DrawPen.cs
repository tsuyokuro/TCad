
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public struct DrawPen
    {
        public Color4 mColor4;

        public float Width;

        private Pen mGdiPen;
        public Pen GdiPen
        {
            get => mGdiPen;
        }

        public static DrawPen NullPen = new DrawPen(Color.FromArgb(0, 0, 0, 0), 0);

        public bool IsNullPen
        {
            get => mColor4.A == 0.0;
        }

        public void Dispose()
        {
            if (mGdiPen != null)
            {
                mGdiPen.Dispose();
                mGdiPen = null;
            }
        }

        public Color4 Color4()
        {
            return mColor4;
        }

        public DrawPen(Pen pen)
        {
            mGdiPen = pen;
            mColor4 = Color4Util.FromArgb(pen.Color.ToArgb());
            Width = pen.Width;
        }

        public DrawPen(Color color, float width)
        {
            mGdiPen = null;
            mColor4 = Color4Util.FromArgb(color.ToArgb());
            Width = width;
        }

        public DrawPen(Color4 color, float width)
        {
            mGdiPen = null;
            mColor4 = color;
            Width = width;
        }
    }
}
