
using OpenTK.Graphics;
using System.Drawing;

namespace Plotter
{
    public class DrawPen
    {
        public int ID;

        public int Argb;

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
            return Color4Util.FromArgb(Argb);
        }

        public Color GdiColor()
        {
            return Color.FromArgb(Argb);
        }

        public DrawPen(Pen pen)
        {
            ID = 0;
            mGdiPen = pen;
            Argb = pen.Color.ToArgb();
            Width = pen.Width;
        }

        public DrawPen(Color color, float width)
        {
            ID = 0;
            mGdiPen = null;
            Argb = color.ToArgb();
            Width = width;
        }

        public DrawPen(Color4 color, float width)
        {
            ID = 0;
            mGdiPen = null;
            Argb = color.ToArgb();
            Width = width;
        }
    }
}
