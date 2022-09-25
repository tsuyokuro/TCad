
using OpenTK.Mathematics;
using System.Drawing;

namespace Plotter
{
    public class DrawPen
    {
        public int Argb;

        public Color4 mColor4;

        public float Width;

        public Pen mGdiPen;
        public Pen GdiPen
        {
            get => mGdiPen;
        }

        public static DrawPen NullPen = new DrawPen(Color.FromArgb(0, 0, 0, 0), 0);

        public bool IsNullPen
        {
            get => ((uint)Argb & 0xff000000) == 0;
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

        public Color GdiColor()
        {
            return Color.FromArgb(Argb);
        }

        public DrawPen(Pen pen)
        {
            mGdiPen = pen;
            Argb = pen.Color.ToArgb();
            mColor4 = Color4Util.FromArgb(Argb);
            Width = pen.Width;
        }

        public DrawPen(Color color, float width)
        {
            mGdiPen = null;
            Argb = color.ToArgb();
            mColor4 = Color4Util.FromArgb(Argb);
            Width = width;
        }

        public DrawPen(Color4 color, float width)
        {
            mGdiPen = null;
            Argb = color.ToArgb();
            mColor4 = color;
            Width = width;
        }
    }
}
